using MimeKit;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Extensions;

namespace SmtpForwarder.Application.Filter;

public static class RecipientFilter
{
    public static Dictionary<MailAddressType, List<MailboxAddress>> SortRecipients(HashSet<MailboxAddress>? recipients, 
        string internalDomain, bool allowSmtpForward, List<string> allowedForwardAddresses)
    {
        var dictionary = new Dictionary<MailAddressType, List<MailboxAddress>>();
        if (recipients is null || recipients.Count == 0)
            return dictionary;

        foreach (var address in recipients)
        {
            var mailAddressType = DesignateType(address, internalDomain, allowSmtpForward, allowedForwardAddresses);
            dictionary.AddToDictionary(mailAddressType, address);
        }

        return dictionary;
    }

    public static MailAddressType DesignateType(MailboxAddress address, string internalDomain, bool allowSmtpForward,
        List<string> allowedForwardAddresses)
    {
        if (address.Domain.Equals(internalDomain)) return MailAddressType.Internal;
        if (allowSmtpForward && (allowedForwardAddresses.Contains("*") ||
                                 allowedForwardAddresses.Contains(address.Domain)))
            return MailAddressType.ForwardExternal;
        return MailAddressType.Blocked;
    }
    
}