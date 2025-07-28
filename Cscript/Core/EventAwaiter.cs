using System;
using System.Threading.Tasks;

public static class EventAwaiter<T>
{
    public static Task<T> WaitFor(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe, Predicate<T> predicate)
    {
        var tcs = new TaskCompletionSource<T>();

        Action<T> handler = null;
        handler = (data) =>
        {
            if (predicate(data))
            {
                unsubscribe(handler);
                tcs.TrySetResult(data);
            }
        };

        subscribe(handler);
        return tcs.Task;
    }
}

