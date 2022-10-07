namespace SmtpForwarder.Application.Enums;

public enum IncomingMessageResponse
{
    Ok,
    MailboxNameNotAllowed,
    MailboxUnavailable,
    NoValidRecipientsGiven,
    SizeLimitExceeded,
    Error,
}