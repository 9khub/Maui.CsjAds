namespace CsjAds.Sample.Pages;

public partial class RewardedVideoPage : ContentPage
{
    private readonly ICsjAdService _adService;
    private ICsjRewardedVideoAd? _rewardedAd;

    public RewardedVideoPage(ICsjAdService adService)
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

        _rewardedAd?.Dispose();
        _rewardedAd = _adService.CreateRewardedVideoAd(slotId);

        _rewardedAd.OnAdLoaded += (_, _) => Log("Ad loaded successfully");
        _rewardedAd.OnAdFailed += (_, args) => Log($"Ad failed: {args.Error}");
        _rewardedAd.OnAdShown += (_, _) => Log("Ad shown");
        _rewardedAd.OnAdClicked += (_, _) => Log("Ad clicked");
        _rewardedAd.OnAdClosed += (_, _) =>
        {
            Log("Ad closed");
            ShowButton.IsEnabled = false;
        };
        _rewardedAd.OnRewardVerified += (_, args) =>
            Log($"Reward: {args.Reward.RewardName} x{args.Reward.RewardAmount} (verified: {args.Reward.IsVerified})");

        Log("Loading ad...");
        try
        {
            await _rewardedAd.LoadAsync();
            ShowButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Log($"Load error: {ex.Message}");
        }
    }

    private void OnShowAd(object sender, EventArgs e)
    {
        if (_rewardedAd?.IsLoaded == true)
        {
            _rewardedAd.Show();
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
        _rewardedAd?.Dispose();
        _rewardedAd = null;
    }
}
