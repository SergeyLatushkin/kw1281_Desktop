public sealed class DataSender
{
    private static readonly DataSender _instance = new ();
    public event Action<IBaseResult>? DataReceived;

    private DataSender()
    {
    }

    public static DataSender Instance => _instance;

    public void Send<T>(T data)
    {
        Send(data, null!);
    }

    public void Error(Exception error)
    {
        Send<Exception>(default!, error);
    }

    public void Error(string error)
    {
        Error(new Exception(error));
    }

    public void Send<T>(T data, Exception error)
    {
        DataReceived?.Invoke(new Result<T> (data, error));
    }
}

public record Result<T>(T Data, Exception Error) : IBaseResult
{
    public bool Ok => Error == null;

    public string Content
    {
        get
        {
            if (Data is IEnumerable<object> data)
            {
                return string.Join('\n', data.Cast<object>().Select(x => x.ToString()));
            }
            else
            {
                return Data?.ToString()!;
            }
        }
    }
}

public interface IBaseResult
{
    public bool Ok { get; }
    public string Content { get; }
}