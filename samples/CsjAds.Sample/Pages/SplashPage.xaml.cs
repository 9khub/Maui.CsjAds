namespace CsjAds.Sample.Pages;

public partial class SplashPage : ContentPage
{
    private readonly ICsjAdService _adService;

    public SplashPage(ICsjAdService adService)
    {
        InitializeComponent();
        _adService = adService;
    }

    private async void OnLoadAndShow(object sender, EventArgs e)
    {
        var slotId = SlotIdEntry.Text?.Trim();
        if (string.IsNullOrEmpty(slotId))
        {
            LogLabel.Text = "Please enter a slot ID";
            return;
        }

        using var splashAd = _adService.CreateSplashAd(slotId);
        splashAd.TimeoutMilliseconds = 3000;

        splashAd.OnAdLoaded += (_, _) => Log("Splash loaded");
        splashAd.OnAdFailed += (_, args) => Log($"Splash failed: {args.Error}");
        splashAd.OnAdShown += (_, _) => Log("Splash shown");
        splashAd.OnAdClosed += (_, _) => Log("Splash closed");

        Log("Loading splash ad...");
        try
        {
            await splashAd.LoadAsync();
            splashAd.Show();
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
    }

    private void Log(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            LogLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
