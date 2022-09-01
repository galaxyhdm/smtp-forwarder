namespace DataLayer.Interfaces;

public interface IAppContext
{
    Task<bool> CanConnectAsync();
}