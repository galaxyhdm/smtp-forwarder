using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NLog;
using SmtpForwarder.Application.Interfaces.Security;
using SmtpForwarder.Application.Utils;

namespace SmtpForwarder.SmtpReceiverServer.Security;

internal class SelfSignedCertificateProvider : ICertificateProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    // ----- Env-Keys ------
    private const string CertSubjectNameSuffix = "CERT_SUBJECT_NAME";
    private const string CertDnsNameSuffix = "CERT_DNS_NAME";
    private const string CertKeyStrengthSuffix = "CERT_KEY_STRENGTH";
    private const string CertValidDaysSuffix = "CERT_VALID_DAYS";
    
    // ----- Env ------
    private readonly string _envPrefix;
    private readonly string? _subjectName;
    private readonly string? _dnsName;
    private readonly int _keyStrength;
    private readonly int _validDays;
    
    public SelfSignedCertificateProvider(string envPrefix) {
        if (envPrefix.Trim().Length == 0)
            throw new ArgumentException(
                $"{nameof(envPrefix)} is not valid, please supply a string with one or more characters.");
        _envPrefix = envPrefix.Trim().ToUpper();

        var prefix = $"{_envPrefix}_";
        _subjectName = Env.GetStringDefault($"{prefix}{CertSubjectNameSuffix}", "SmtpToTelegram");
        _dnsName = Env.GetStringDefault($"{prefix}{CertDnsNameSuffix}", null);
        _keyStrength = Env.GetIntDefault($"{prefix}{CertKeyStrengthSuffix}", 2048, i => i % 1024 == 0);
        _validDays = Env.GetIntDefault($"{prefix}{CertValidDaysSuffix}", 90, i => i > 0);
    }
    
    public X509Certificate2? GetCertificate(out bool unsecureAuthentication)
    {
        unsecureAuthentication = false;
        return Create();
    }
    
    private X509Certificate2 Create() {
        Log.Debug(
            $"Generating certificate with {_keyStrength} bits, valid for {_validDays} day{(_validDays == 1 ? "" : "s")}.");
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddDnsName(Environment.MachineName);
        if (!string.IsNullOrEmpty(_dnsName) && !string.IsNullOrWhiteSpace(_dnsName))
            sanBuilder.AddDnsName(_dnsName);

        var distinguishedName = new X500DistinguishedName($"CN={_subjectName}");
        using var rsa = RSA.Create(_keyStrength);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection {new("1.3.6.1.5.5.7.3.1")}, true));
        request.CertificateExtensions.Add(sanBuilder.Build());

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1),
            new DateTimeOffset(DateTime.UtcNow.AddDays(_validDays)));

        return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
    }
    
}