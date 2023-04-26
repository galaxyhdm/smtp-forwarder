namespace SmtpForwarder.Domain.Settings;

public class Settings
{
    public string? ConnectionString { get; set; }
    public string InternalDomain { get; set; } = "test.lab";

    public bool AllowSmtpForward { get; set; } = false;
    public List<string> AllowedForwardDomains { get; set; } = new();
}