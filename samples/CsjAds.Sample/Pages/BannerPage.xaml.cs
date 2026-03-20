namespace CsjAds.Sample.Pages;

public partial class BannerPage : ContentPage
{
    public BannerPage()
    {
        InitializeComponent();
    }

    private void OnBannerLoaded(object sender, AdEventArgs e)
    {
        LogLabel.Text = $"[{DateTime.Now:HH:mm:ss}] Banner loaded";
    }

    private void OnBannerFailed(object sender, AdErrorEventArgs e)
    {
        LogLabel.Text = $"[{DateTime.Now:HH:mm:ss}] Banner failed: {e.Error}";
    }

    private void OnBannerClicked(object sender, AdEventArgs e)
    {
        LogLabel.Text = $"[{DateTime.Now:HH:mm:ss}] Banner clicked";
    }
}
