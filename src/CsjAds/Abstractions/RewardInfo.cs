namespace CsjAds;

/// <summary>
/// Information about the reward granted by a rewarded video ad.
/// </summary>
public sealed class RewardInfo
{
    public RewardInfo(string rewardName, int rewardAmount, bool isVerified)
    {
        RewardName = rewardName;
        RewardAmount = rewardAmount;
        IsVerified = isVerified;
    }

    public string RewardName { get; }
    public int RewardAmount { get; }
    public bool IsVerified { get; }
}
