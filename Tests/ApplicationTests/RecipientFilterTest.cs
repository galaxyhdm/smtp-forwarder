using MimeKit;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Filter;

namespace ApplicationTests;

public class RecipientFilterTest
{

    [TestCaseSource(nameof(FilterTestCases))]
    public void TestSort(RecipientFilterTestCase filterTestCase)
    {
        var designateType = RecipientFilter.DesignateType(
            filterTestCase.Address,
            filterTestCase.InternalDomain,
            filterTestCase.AllowSmtpForward,
            filterTestCase.AllowedForwardAddresses);

        Assert.AreEqual(
            filterTestCase.ExpectedResult,
            designateType,
            $"{filterTestCase.Address} | {filterTestCase.InternalDomain} | {filterTestCase.AllowSmtpForward} | {string.Join(", ", filterTestCase.AllowedForwardAddresses)}");
    }


    private static RecipientFilterTestCase[] FilterTestCases =
    {
        new("test@test.lab", "test.lab", false, MailAddressType.Internal),
        new("to@hallo.lab", "test.lab", false, MailAddressType.Blocked),
        new("to@markusk.dev", "test.lab", false, MailAddressType.Blocked),
        
        new("test@test.lab", "hallo.lab", false, MailAddressType.Blocked),
        new("to@hallo.lab", "hallo.lab", false, MailAddressType.Internal),
        new("to@markusk.dev", "hallo.lab", false, MailAddressType.Blocked),
        
        new("test@test.lab", "test.lab", true, MailAddressType.Internal, "hallo.lab"),
        new("to@hallo.lab", "test.lab", true, MailAddressType.ForwardExternal, "hallo.lab"),
        new("to@markusk.dev", "test.lab", true, MailAddressType.Blocked, "hallo.lab"),
        
        new("test@test.lab", "test.lab", true, MailAddressType.Internal, "hallo.lab", "markusk.dev"),
        new("to@hallo.lab", "test.lab", true, MailAddressType.ForwardExternal, "hallo.lab", "markusk.dev"),
        new("to@markusk.dev", "test.lab", true, MailAddressType.ForwardExternal, "hallo.lab", "markusk.dev"),
        
        new("test@test.lab", "test.lab", true, MailAddressType.Internal, "*"),
        new("to@hallo.lab", "test.lab", true, MailAddressType.ForwardExternal, "*"),
        new("to@markusk.dev", "test.lab", true, MailAddressType.ForwardExternal, "*"),
    };

    public class RecipientFilterTestCase
    {
        public RecipientFilterTestCase(string address, string internalDomain, bool allowSmtpForward,
            MailAddressType expectedResult,
            params string[] allowedForwardAddresses)
        {
            InternalDomain = internalDomain;
            AllowSmtpForward = allowSmtpForward;
            AllowedForwardAddresses = allowedForwardAddresses.ToList();
            ExpectedResult = expectedResult;
            Address = new MailboxAddress("", address);
        }

        public MailboxAddress Address { get; }
        public string InternalDomain { get; }
        public bool AllowSmtpForward { get; }
        public List<string> AllowedForwardAddresses { get; }

        public MailAddressType ExpectedResult { get; }

    }
}