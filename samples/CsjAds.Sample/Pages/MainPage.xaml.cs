namespace CsjAds.Sample.Pages;

public partial class MainPage : ContentPage
{
    private readonly ICsjAdService _adService;

    public MainPage(ICsjAdService adService)
    {
        InitializeComponent();
        _adService = adService;
    }

    private async void OnInitializeSdk(object sender, EventArgs e)
    {
        StatusLabel.Text = "SDK Status: Initializing...";

        try
        {
            var success = await _adService.StartAsync();
            StatusLabel.Text = success
                ? "SDK Status: Initialized"
                : "SDK Status: Failed to initialize";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"SDK Status: Error - {ex.Message}";
        }
    }

    private async void OnRewardedVideo(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("rewarded");
    }

    private async void OnInterstitial(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("interstitial");
    }

    private async void OnBanner(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("banner");
    }

    private async void OnSplash(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("splash");
    }
}
