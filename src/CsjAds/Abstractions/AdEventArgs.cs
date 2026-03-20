namespace CsjAds;

/// <summary>
/// Base event args for ad lifecycle events.
/// </summary>
public class AdEventArgs : EventArgs
{
}

/// <summary>
/// Event args when an ad fails to load or show.
/// </summary>
public sealed class AdErrorEventArgs : AdEventArgs
{
    public AdErrorEventArgs(AdError error)
    {
        Error = error;
    }

    public AdError Error { get; }
}

/// <summary>
/// Event args when a rewarded ad verifies the reward.
/// </summary>
public sealed class RewardEventArgs : AdEventArgs
{
    public RewardEventArgs(RewardInfo reward)
    {
        Reward = reward;
    }

    public RewardInfo Reward { get; }
}
