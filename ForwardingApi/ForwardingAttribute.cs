namespace SmtpForwarder.ForwardingApi;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ForwardingAttribute : Attribute
{
    private readonly string _name;

    public ForwardingAttribute(string name)
    {
        _name = name;
    }

    public string Name => _name;
}