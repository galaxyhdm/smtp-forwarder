using MimeKit;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Filter;

public class RecipientFilterAddress
{
    public MailboxAddress MailboxAddress { get; }
    public string LocalPart => MailboxAddress.LocalPart;
    
    public ForwardingAddress? ForwardingAddress { get; private set; }

    public bool InDatabase => ForwardingAddress is not null;
    public bool HasPermission { get;  set; }

    public RecipientFilterAddress(MailboxAddress mailboxAddress)
    {
        MailboxAddress = mailboxAddress;
    }

    public RecipientFilterAddress SetForwardingAddress(ForwardingAddress? forwardingAddress)
    {
        ForwardingAddress = forwardingAddress;
        return this;
    }
    
}