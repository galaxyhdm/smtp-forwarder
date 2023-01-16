using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ByteSizeLib;
using Flurl.Http;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using SmtpForwarder.ForwardingApi;

namespace SmtpForwarder.TelegramForwarder;

[Forwarding("telegram_forwarder")]
public class TelegramForwarder : IForwarder
{
    public string Name
    {
        get
        {
            if(string.IsNullOrWhiteSpace(_name))
                _name = (GetType().GetCustomAttribute(typeof(ForwardingAttribute)) as ForwardingAttribute)!.Name;
            return _name;
        }
    }

    private TelegramConfig _telegramConfig;

    private const string BodyTruncated = "\n\n[...]";
    private const string MainFolder = "files";

    private string? _name;
    
    public Task InitializeAsync(string forwarderConfig)
    {
        var telegramConfigData = JsonConvert.DeserializeObject<TelegramConfigData>(forwarderConfig);
        _telegramConfig = new TelegramConfig(telegramConfigData ??
                                             throw new JsonSerializationException(
                                                 "Could not load 'telegramConfigData'"));
        return Task.CompletedTask;
    }

    public async Task ForwardMessage(MimeMessage message, List<string> attachmentIds, Guid requestId)
    {
        var formattedEmail = FormatEmail(message, attachmentIds, requestId);
        await SendToTelegram(formattedEmail);
    }

    private async Task<bool> SendToTelegram(FormattedEmail formattedEmail)
    {
        foreach (var telegramConfigTelegramChatId in _telegramConfig.TelegramChatIds)
        {
            var message = await SendMessageToChat(formattedEmail, telegramConfigTelegramChatId);
            if (message is null) continue;
            foreach (var formattedEmailAttachment in formattedEmail.Attachments)
            {
                await SendAttachmentToChat(formattedEmailAttachment, telegramConfigTelegramChatId, message.Value);
            }
        }

        return true;
    }

    private async Task<TelegramAPIMessage?> SendMessageToChat(FormattedEmail message, string chatId)
    {
        var response =
            await
                $"{_telegramConfig.TelegramApiPrefix}bot{_telegramConfig.TelegramBotToken}/sendMessage"
                    .WithTimeout(_telegramConfig.TelegramApiTimeoutSeconds)
                    .SetQueryParam("disable_web_page_preview", true)
                    .PostMultipartAsync(content => content
                        .AddString("chat_id", chatId)
                        .AddString("text", message.Text));

        if (response.StatusCode != 200)
        {
            //todo add error log
            return null;
        }

        var result = await response.GetJsonAsync<TelegramAPIMessageResult>();
        return result.Ok ? result.Result : null;
    }

    private async Task<bool> SendAttachmentToChat(FormattedAttachment attachment, string chatId,
        TelegramAPIMessage sentMessage)
    {
        string partName;
        string method;

        switch (attachment.FileType)
        {
            case 1:
                partName = "photo";
                method = "sendPhoto";
                break;
            default:
                partName = "document";
                method = "sendDocument";
                break;
        }

        var request =
            $"{_telegramConfig.TelegramApiPrefix}bot{_telegramConfig.TelegramBotToken}/{method}?disable_notification=true"
                .WithTimeout(_telegramConfig.TelegramApiTimeoutSeconds);

        IFlurlResponse? result;
        if (!string.IsNullOrWhiteSpace(attachment.FilePath) || attachment.FileId is not null)
        {
            result = await request.PostMultipartAsync(content =>
            {
                (attachment.FileId is null
                        ? content.AddFile(partName, attachment.FilePath, fileName: attachment.Filename)
                        : content.AddString(partName, attachment.FileId))
                    .AddString("chat_id", chatId)
                    .AddString("reply_to_message_id", sentMessage.MessageId.ToString())
                    .AddString("caption", attachment.Caption);
            });
        }
        else if (attachment.Content is not null && attachment.Content.Length > 0)
        {
            using var memoryStream = new MemoryStream(attachment.Content);
            result = await request.PostMultipartAsync(content =>
            {
                content.AddFile(partName, memoryStream, attachment.Filename)
                    .AddString("chat_id", chatId)
                    .AddString("reply_to_message_id", sentMessage.MessageId.ToString())
                    .AddString("caption", attachment.Caption);
            });
        }
        else
            result = null;

        if (result is null || result.StatusCode != 200)
            return false;

        if (attachment.FileId is not null) return true;

        var messageResult = await result.GetJsonAsync<TelegramAPIMessageResult>();
        if (!messageResult.Ok) return false;
        if (messageResult.Result?.Document is null) return false;
        var fileId = messageResult.Result?.Document?.FileId;
        attachment.FileId = fileId;
        return true;
    }
    
    private static string HtmlToPlainText(string html)
    {
        const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
        const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
        const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
        var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
        var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
        var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

        var text = html;
        //Decode html specific characters
        text = System.Net.WebUtility.HtmlDecode(text); 
        //Remove tag whitespace/line breaks
        text = tagWhiteSpaceRegex.Replace(text, "><");
        //Replace <br /> with line breaks
        text = lineBreakRegex.Replace(text, Environment.NewLine);
        //Strip formatting
        text = stripFormattingRegex.Replace(text, string.Empty);

        return text;
    }
    
    
    private FormattedEmail FormatEmail(MimeMessage message, List<string> attachmentIds, Guid requestId)
    {

        var text = message.TextBody;
        
        if (string.IsNullOrWhiteSpace(text))
        {
            var html = message.GetTextBody(TextFormat.Html);
            text = HtmlToPlainText(html);
            //text = new HtmlToText().Convert(html);
        }

        if (string.IsNullOrWhiteSpace(text))
            text = message.GetTextBody(TextFormat.Plain);

        var attachmentDetails = new List<string>();
        var attachments = new List<FormattedAttachment>();

        var doParts = (string emoji, IEnumerable<MimeEntity> entities) =>
        {
            foreach (var part in entities)
            {
                var partContentId = part.ContentId;
                var filename = part.ContentDisposition?.FileName ?? part.ContentType.Name;
                if (string.IsNullOrWhiteSpace(text)
                    && part.ContentType.IsMimeType("text", "plain")
                    && string.IsNullOrWhiteSpace(filename))
                {
                    if (part is MessagePart messagePart)
                    {
                        text = messagePart.Message.TextBody;
                        continue;
                    }
                }

                var action = "discarded";
                var contentTypeMediaType = part.ContentType.MimeType;
                var contentType = GuessContentType(contentTypeMediaType, filename);

                if (!attachmentIds.Contains(partContentId)) continue;

                var fileInfo = new FileInfo(Path.Combine(MainFolder, requestId.ToString(), partContentId));
                if (FileIsImage(contentType) && fileInfo.Length <= _telegramConfig.ForwardedAttachmentMaxPhotoSize)
                {
                    action = "sending...";
                    attachments.Add(new FormattedAttachment
                    {
                        Filename = filename,
                        Caption = filename,
                        //Content = File.ReadAllBytes(fileInfo.FullName),
                        FileType = 1, //photo
                        FilePath = fileInfo.FullName
                    });
                }
                else if (fileInfo.Length <= _telegramConfig.ForwardedAttachmentMaxSize)
                {
                    action = "sending...";
                    attachments.Add(new FormattedAttachment
                    {
                        Filename = filename,
                        Caption = filename,
                        //Content = File.ReadAllBytes(fileInfo.FullName),
                        FileType = 2, //documents
                        FilePath = fileInfo.FullName
                    });
                }

                var line =
                    $"- {emoji} {filename} ({contentType}) {ByteSize.FromBytes(fileInfo.Length).ToBinaryString()}, {action}";

                attachmentDetails.Add(line);
            }
        };

        //doParts("🔗", message.BodyParts);
        doParts("📎", message.Attachments);

        var formattedAttachmentsDetails = attachmentDetails.Count > 0
            ? $"Attachments:\n{string.Join("\n", attachmentDetails)}"
            : "";

        var (fullMessageText, truncatedMessageText) = FormatMessage(
            message.From.ToString(),
            message.Subject,
            text,
            formattedAttachmentsDetails);

        if (string.IsNullOrWhiteSpace(truncatedMessageText))
        {
            return new FormattedEmail
            {
                Text = fullMessageText,
                Attachments = attachments
            };
        }

        var bytes = Encoding.UTF8.GetBytes(fullMessageText);
        //Encoding.GetEncoding(message.Body.ContentType.Charset).GetBytes(fullMessageText);
        if (bytes.Length > _telegramConfig.ForwardedAttachmentMaxSize)
            throw new FormatException(
                $"The message lenght ({fullMessageText.Length}) is larger than 'ForwardedAttachmentMaxSize' ({_telegramConfig.ForwardedAttachmentMaxSize})");

        var formattedAttachment = new FormattedAttachment
        {
            Filename = "full_message.txt",
            Caption = "Full message",
            Content = bytes,
            FileType = 2
        };

        attachments.Insert(0, formattedAttachment);
        return new FormattedEmail
        {
            Text = truncatedMessageText,
            Attachments = attachments
        };
    }

    private (string, string) FormatMessage(string from, string subject, string text, string formattedAttachmentsDetails)
    {
        var fullMessageText = new StringBuilder(_telegramConfig.MessageTemplate)
            .Replace("\\n", "\n")
            .Replace("{from}", from)
            .Replace("{subject}", subject)
            .Replace("{body}", text.Trim())
            .Replace("{attachments_details}", formattedAttachmentsDetails)
            .ToString();

        if (fullMessageText.Length <= _telegramConfig.MessageLengthToSendAsFile)
            return (fullMessageText, "");

        var emptyMessageText = new StringBuilder(_telegramConfig.MessageTemplate)
            .Replace("\\n", "\n")
            .Replace("{from}", from)
            .Replace("{subject}", subject)
            .Replace("{body}", BodyTruncated.Trim())
            .Replace("{attachments_details}", formattedAttachmentsDetails)
            .ToString();

        if (emptyMessageText.Length >= _telegramConfig.MessageLengthToSendAsFile)
            return (fullMessageText, fullMessageText[.._telegramConfig.MessageLengthToSendAsFile]);

        var maxBodyLenght = _telegramConfig.MessageLengthToSendAsFile - emptyMessageText.Length;
        var truncatedMessageText = new StringBuilder(_telegramConfig.MessageTemplate)
            .Replace("\\n", "\n")
            .Replace("{from}", from)
            .Replace("{subject}", subject)
            .Replace("{body}", $"{text.Trim()[..maxBodyLenght]}{BodyTruncated.Trim()}".Trim())
            .Replace("{attachments_details}", formattedAttachmentsDetails)
            .ToString();

        if (truncatedMessageText.Length > _telegramConfig.MessageLengthToSendAsFile)
            throw new FormatException(
                $"Unexpected lenght of truncated message: \n{maxBodyLenght}\n{truncatedMessageText}");

        return (fullMessageText, truncatedMessageText);
    }

    private string GuessContentType(string? contentType, string filename)
    {
        if (contentType is not null && !contentType.Equals("application/octet-stream"))
            return contentType;

        var mimeType = MimeTypes.GetMimeType(filename);
        return (string.IsNullOrWhiteSpace(mimeType) ? contentType : mimeType) ?? string.Empty;
    }

    private bool FileIsImage(string contentType)
    {
        switch (contentType)
        {
            case "image/jpeg":
            case "image/png":
                return true;
            default:
                return false;
        }
    }

    private class TelegramConfig
    {
        public List<string> TelegramChatIds { get; }
        public string TelegramBotToken { get; }
        public string TelegramApiPrefix { get; }
        public int TelegramApiTimeoutSeconds { get; }

        public string MessageTemplate { get; }
        public double ForwardedAttachmentMaxSize { get; }
        public double ForwardedAttachmentMaxPhotoSize { get; }
        public int MessageLengthToSendAsFile { get; }

        public TelegramConfig(TelegramConfigData forwarderConfig)
        {
            TelegramChatIds = forwarderConfig.TelegramChatIds;
            TelegramBotToken = forwarderConfig.TelegramBotToken;
            TelegramApiPrefix = forwarderConfig.TelegramApiPrefix;
            TelegramApiTimeoutSeconds = forwarderConfig.TelegramApiTimeoutSeconds;
            MessageTemplate = forwarderConfig.MessageTemplate;

            ForwardedAttachmentMaxSize = ByteSize.Parse(forwarderConfig.ForwardedAttachmentMaxSize).Bytes;
            ForwardedAttachmentMaxPhotoSize = ByteSize.Parse(forwarderConfig.ForwardedAttachmentMaxPhotoSize).Bytes;
            MessageLengthToSendAsFile = forwarderConfig.MessageLengthToSendAsFile;
        }
    }

    private class FormattedEmail
    {
        public string Text { get; set; }
        public List<FormattedAttachment> Attachments { get; set; }
    }

    private class FormattedAttachment
    {
        public string? Filename { get; set; }
        public string Caption { get; set; }
        public byte[]? Content { get; set; }
        public int FileType { get; set; }
        public string? FilePath { get; set; }

        public string? FileId { get; set; }
    }

    private struct TelegramApiDocument
    {
        [JsonProperty(PropertyName = "file_id")]
        public string FileId { get; set; }
    }

    private struct TelegramAPIMessage
    {
        [JsonProperty(PropertyName = "message_id")]
        public int MessageId { get; set; }

        [JsonProperty(PropertyName = "document")]
        public TelegramApiDocument? Document { get; set; }
    }

    private struct TelegramAPIMessageResult
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "result")]
        public TelegramAPIMessage? Result { get; set; }
    }
}