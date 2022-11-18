using System.Security.Cryptography.X509Certificates;
using NLog;
using SmtpForwarder.Application.Interfaces.Security;
using SmtpForwarder.Application.Utils;

namespace SmtpForwarder.SmtpReceiverServer.Security;

public class TlsCertificateProvider : ICertificateProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    // ----- Env-Keys ------
    private const string CertFilePathSuffix = "CERT_FILE_PATH";
    private const string KeyFilePathSuffix = "KEY_FILE_PATH";
    private const string ChainFilePathSuffix = "CHAIN_FILE_PATH";

    // ----- Env ------
    private readonly string _envPrefix;
    private readonly string? _certFilePath;
    private readonly string? _keyFilePath;
    private readonly string? _chainFilePath;
    
    public TlsCertificateProvider(string envPrefix) {
        if (envPrefix.Trim().Length == 0)
            throw new ArgumentException(
                $"{nameof(envPrefix)} is not valid, please supply a string with one or more characters.");
        _envPrefix = envPrefix.Trim().ToUpper();

        var prefix = $"{_envPrefix}_";
        _certFilePath = Env.GetStringDefault($"{prefix}{CertFilePathSuffix}", "cert.pem");
        _keyFilePath = Env.GetStringDefault($"{prefix}{KeyFilePathSuffix}", "privkey.pem");
        _chainFilePath = Env.GetStringDefault($"{prefix}{ChainFilePathSuffix}", "chain.pem");
    }
    
    public X509Certificate2? GetCertificate(out bool unsecureAuthentication) {
        unsecureAuthentication = false;
        if (!CheckFiles()) return null;

        var certificate = X509Certificate2.CreateFromPemFile(
            _certFilePath ?? throw new InvalidOperationException(), 
            _keyFilePath);

        var chain = new X509Certificate2Collection(certificate);
        chain.ImportFromPemFile(_chainFilePath ?? throw new InvalidOperationException());

        var pfxExport = chain.Export(X509ContentType.Pfx);

        return new X509Certificate2(pfxExport ?? Array.Empty<byte>());
    }
    
    private bool CheckFiles()
    {
        var error = false; 
        if (!File.Exists(_certFilePath)) {
            Log.Warn($"Cert-File not found. ({_certFilePath})");
            error = true;
        }

        if (!File.Exists(_keyFilePath)) {
            Log.Warn($"Key-File not found. ({_keyFilePath})");
            error = true;
        }

        if (!File.Exists(_chainFilePath)) {
            Log.Warn($"Chain-File not found. ({_chainFilePath})");
            error = true;
        }

        return !error;
    }
    
}