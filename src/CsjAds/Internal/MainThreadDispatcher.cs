namespace CsjAds.Internal;

/// <summary>
/// Ensures callbacks are dispatched to the MAUI main/UI thread.
/// </summary>
internal static class MainThreadDispatcher
{
    public static void Dispatch(Action action)
    {
        if (MainThread.IsMainThread)
            action();
        else
            MainThread.BeginInvokeOnMainThread(action);
    }

    public static Task DispatchAsync(Func<Task> action)
    {
        if (MainThread.IsMainThread)
            return action();

        var tcs = new TaskCompletionSource();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}
