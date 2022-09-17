namespace SmtpForwarder.DataLayer.Events;

public class SavingEventArgs<TEntity> : EventArgs where TEntity : class
{
    public TEntity Entity { get; }

    public SavingEventArgs(TEntity entity) => Entity = entity;
}

public class SavingFailedEventArgs<TEntity> : EventArgs where TEntity : class
{
    public TEntity Entity { get; }
    public Exception Exception { get; }

    public SavingFailedEventArgs(TEntity entity, Exception exception)
    {
        Entity = entity;
        Exception = exception;
    }
}