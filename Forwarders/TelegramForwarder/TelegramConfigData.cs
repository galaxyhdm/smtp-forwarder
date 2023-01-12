using Newtonsoft.Json;

namespace SmtpForwarder.TelegramForwarder;

public class TelegramConfigData
{
    [JsonProperty(PropertyName = "telegramChatIds")]
    public List<string> TelegramChatIds { get; set; }

    [JsonProperty(PropertyName = "telegramBotToken")]
    public string TelegramBotToken { get; set; }

    [JsonProperty(PropertyName = "telegramApiPrefix")]
    public string TelegramApiPrefix { get; set; }

    [JsonProperty(PropertyName = "telegramApiTimeoutSeconds")]
    public int TelegramApiTimeoutSeconds { get; set; }

    [JsonProperty(PropertyName = "messageTemplate")]
    public string MessageTemplate { get; set; }


    [JsonProperty(PropertyName = "forwardedAttachmentMaxSize")]
    public string ForwardedAttachmentMaxSize { get; set; }

    [JsonProperty(PropertyName = "forwardedAttachmentMaxPhotoSize")]
    public string ForwardedAttachmentMaxPhotoSize { get; set; }

    [JsonProperty(PropertyName = "messageLengthToSendAsFile")]
    public int MessageLengthToSendAsFile { get; set; }
}