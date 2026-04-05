using System.Diagnostics;

namespace CsjAds.Internal;

/// <summary>
/// Ensures callbacks are dispatched to the MAUI main/UI thread.
/// </summary>
internal static class MainThreadDispatcher
{
    public static void Dispatch(Action action)
    {
        void RunSafe()
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] MainThreadDispatcher callback error: {ex}");
            }
        }

        if (MainThread.IsMainThread)
            RunSafe();
        else
            MainThread.BeginInvokeOnMainThread(RunSafe);
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
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });
        return tcs.Task;
    }
}
