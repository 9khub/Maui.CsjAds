namespace CsjAds.Sample.Pages;

public partial class InterstitialPage : ContentPage
{
    private readonly ICsjAdService _adService;
    private ICsjInterstitialAd? _interstitialAd;

    public InterstitialPage(ICsjAdService adService)
    {
        InitializeComponent();
        _adService = adService;
    }

    private async void OnLoadAd(object sender, EventArgs e)
    {
        var slotId = SlotIdEntry.Text?.Trim();
        if (string.IsNullOrEmpty(slotId))
        {
            LogLabel.Text = "Please enter a slot ID";
            return;
        }

        _interstitialAd?.Dispose();
        _interstitialAd = _adService.CreateInterstitialAd(slotId);

        _interstitialAd.OnAdLoaded += (_, _) => Log("Ad loaded");
        _interstitialAd.OnAdFailed += (_, args) => Log($"Ad failed: {args.Error}");
        _interstitialAd.OnAdShown += (_, _) => Log("Ad shown");
        _interstitialAd.OnAdClicked += (_, _) => Log("Ad clicked");
        _interstitialAd.OnAdClosed += (_, _) =>
        {
            Log("Ad closed");
            ShowButton.IsEnabled = false;
        };

        Log("Loading ad...");
        try
        {
            await _interstitialAd.LoadAsync();
            ShowButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Log($"Load error: {ex.Message}");
        }
    }

    private void OnShowAd(object sender, EventArgs e)
    {
        if (_interstitialAd?.IsLoaded == true)
        {
            _interstitialAd.Show();
        }
        else
        {
            Log("Ad not ready");
        }
    }

    private void Log(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            LogLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _interstitialAd?.Dispose();
        _interstitialAd = null;
    }
}
