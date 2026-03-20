namespace CsjAds.Sample;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("rewarded", typeof(Pages.RewardedVideoPage));
        Routing.RegisterRoute("interstitial", typeof(Pages.InterstitialPage));
        Routing.RegisterRoute("banner", typeof(Pages.BannerPage));
        Routing.RegisterRoute("splash", typeof(Pages.SplashPage));
    }
}
