using System.Security.Cryptography.X509Certificates;
using NLog;
using SmtpForwarder.Application.Interfaces.Security;
using SmtpForwarder.Application.Utils;

namespace SmtpForwarder.SmtpReceiverServer.Security;

public class CertificateProvider : ICertificateProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<CertMode, ICertificateProvider> _certificateProviders;

    // ----- Env-Keys ------
    private const string CertModeSuffix = "CERT_MODE";
    private const string TrustSuffix = "I_KNOW_WHAT_I_AM_DOING_TRUST_ME";

    // ----- Env ------
    private readonly string _envPrefix;
    private readonly string _certMode;
    private readonly bool _trust;

    public CertificateProvider(string envPrefix)
    {
        if (envPrefix.Trim().Length == 0)
            throw new ArgumentException(
                $"{nameof(envPrefix)} is not valid, please supply a string with one or more characters.");
        _envPrefix = envPrefix.Trim().ToUpper();
        Log.Debug($"Certificate env prefix: {_envPrefix}");

        _certificateProviders = new Dictionary<CertMode, ICertificateProvider>();
        InitProviders();

        var prefix = $"{_envPrefix}_";
        _certMode = Env.GetStringDefault($"{prefix}{CertModeSuffix}", CertMode.SelfSigned.ToString())
                    ?? CertMode.SelfSigned.ToString();
        _trust = Env.GetBoolDefault($"{TrustSuffix}");
    }
    
    public X509Certificate2? GetCertificate(out bool unsecureAuthentication) {
        if (TryGetProvider(_certMode, out var certMode, out var provider))
            return provider.GetCertificate(out unsecureAuthentication);
        if (certMode == CertMode.Disabled && _trust) {
            unsecureAuthentication = _trust;
            return null;
        }

        Log.Warn($"Could not found provider for '{certMode}'!");
        unsecureAuthentication = false;
        return null;
    }

    private void InitProviders() {
        _certificateProviders.Add(CertMode.SelfSigned, new SelfSignedCertificateProvider(_envPrefix));
        _certificateProviders.Add(CertMode.External, new TlsCertificateProvider(_envPrefix));
    }

    private bool TryGetProvider(string key, out CertMode parsedCertMode, out ICertificateProvider provider) {
        parsedCertMode = TryGetCertMode(key, CertMode.SelfSigned);
        return _certificateProviders.TryGetValue(parsedCertMode, out provider);
    }

    private static CertMode TryGetCertMode(string key, CertMode defaultCertMode) =>
        Enum.TryParse(key, true, out CertMode certMode) ? certMode : defaultCertMode;
}