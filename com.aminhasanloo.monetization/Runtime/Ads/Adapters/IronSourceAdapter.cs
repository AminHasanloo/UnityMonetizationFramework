// SPDX-License-Identifier: MIT
#if AD_IRONSOURCE
using System;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
// Requires LevelPlay / IronSource Unity SDK
public class IronSourceAdapter : AminHasanloo.Monetization.Ads.IAdNetwork
{
    private readonly IronSourceSettings _s;
    public IronSourceAdapter(IronSourceSettings s) { _s = s; }

    public Task InitializeAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(_s.appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
        tcs.SetResult(true);
        return tcs.Task;
    }

    public bool IsReady(AminHasanloo.Monetization.Ads.AdType type, string placement)
    {
        if (type == AminHasanloo.Monetization.Ads.AdType.Rewarded) return IronSource.Agent.isRewardedVideoAvailable();
        if (type == AminHasanloo.Monetization.Ads.AdType.Interstitial) return IronSource.Agent.isInterstitialReady();
        if (type == AminHasanloo.Monetization.Ads.AdType.Banner) return true;
        return false;
    }

    public void Load(AminHasanloo.Monetization.Ads.AdType type, string placement)
    {
        if (type == AminHasanloo.Monetization.Ads.AdType.Interstitial) IronSource.Agent.loadInterstitial();
        // banners & rewarded are auto-managed in most setups
    }

    public void Show(AminHasanloo.Monetization.Ads.AdType type, string placement, Action<AminHasanloo.Monetization.Ads.Reward> onReward = null, Action onClosed = null)
    {
        if (type == AminHasanloo.Monetization.Ads.AdType.Rewarded && IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSourceEvents.onRewardedVideoAdRewardedEvent += (placementName, reward) =>
            {
                onReward?.Invoke(new AminHasanloo.Monetization.Ads.Reward { Type = reward.getName(), Amount = reward.getAmount() });
            };
            IronSource.Agent.showRewardedVideo(_s.rewardedPlacement);
        }
        else if (type == AminHasanloo.Monetization.Ads.AdType.Interstitial && IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial(_s.interstitialPlacement);
        }
        else if (type == AminHasanloo.Monetization.Ads.AdType.Banner)
        {
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
            IronSource.Agent.displayBanner();
        }
    }

    public void HideBanner(string placement) => IronSource.Agent.hideBanner();
    public void DestroyBanner(string placement) => IronSource.Agent.destroyBanner();
}
#endif
