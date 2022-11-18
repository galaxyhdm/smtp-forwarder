using System.Security.Cryptography.X509Certificates;

namespace SmtpForwarder.Application.Interfaces.Security;

public interface ICertificateProvider
{
    X509Certificate2? GetCertificate(out bool unsecureAuthentication);
}