// SPDX-License-Identifier: MIT
#if AD_TAPSELL
using System;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
// Assuming TapsellPlus SDK namespace:
using TapsellPlusSDK;

namespace AminHasanloo.Monetization.Ads
{
    public class TapsellAdapter : IAdNetwork
    {
        private readonly TapsellSettings _s;
        public TapsellAdapter(TapsellSettings s) { _s = s; }

        public Task InitializeAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            TapsellPlus.Initialize(_s.appIdOrKey, (init) => { tcs.SetResult(true); });
            return tcs.Task;
        }

        public bool IsReady(AdType type, string placement)
        {
            if (type == AdType.Rewarded) return TapsellPlus.IsRewardedAdReady(_s.rewardedZoneId);
            if (type == AdType.Interstitial) return TapsellPlus.IsInterstitialAdReady(_s.interstitialZoneId);
            if (type == AdType.Banner) return true; // Banners show on-demand
            return false;
        }

        public void Load(AdType type, string placement)
        {
            if (type == AdType.Rewarded) TapsellPlus.RequestRewardedVideo(_s.rewardedZoneId);
            else if (type == AdType.Interstitial) TapsellPlus.RequestInterstitial(_s.interstitialZoneId);
        }

        public void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            if (type == AdType.Rewarded)
            {
                TapsellPlus.ShowRewardedVideo(_s.rewardedZoneId, (r) => onReward?.Invoke(new Reward { Type = "reward", Amount = 1 }));
            }
            else if (type == AdType.Interstitial)
            {
                TapsellPlus.ShowInterstitial(_s.interstitialZoneId);
            }
            else if (type == AdType.Banner)
            {
                TapsellPlus.ShowBanner(_s.bannerZoneId, TapsellPlusBannerType.Banner320x50, TapsellPlusHorizontalGravity.Center, TapsellPlusVerticalGravity.Bottom, 0, "default");
            }
        }

        public void HideBanner(string placement) => TapsellPlus.HideBanner();
        public void DestroyBanner(string placement) => TapsellPlus.DestroyBanner();
    }
}
#endif
