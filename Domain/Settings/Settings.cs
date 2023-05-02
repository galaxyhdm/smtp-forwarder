namespace SmtpForwarder.Domain.Settings;

public class Settings
{
    public string? ConnectionString { get; set; }
    public string InternalDomain { get; set; } = "test.lab";

    public bool AllowSmtpForward { get; set; } = false;
    public List<string> AllowedForwardDomains { get; set; } = new();

    public MailSettings MailSettings { get; set; } = new();
}

public class MailSettings
{
    public string? DisplayName { get; set; }
    public string? From { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public bool UseStartTls { get; set; }
}
