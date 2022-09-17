namespace SmtpForwarder.DataLayer.Interfaces;

public interface IAppContext
{
    Task<bool> CanConnectAsync();
}