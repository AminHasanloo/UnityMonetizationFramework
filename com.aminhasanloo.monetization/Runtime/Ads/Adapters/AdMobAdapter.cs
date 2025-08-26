// SPDX-License-Identifier: MIT
#if AD_ADMOB
using System;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using GoogleMobileAds.Api; // Requires GoogleMobileAds Unity plugin

namespace AminHasanloo.Monetization.Ads
{
    public class AdMobAdapter : IAdNetwork
    {
        private readonly AdMobSettings _s;
        private RewardedAd rewarded;
        private InterstitialAd interstitial;
        private BannerView banner;

        public AdMobAdapter(AdMobSettings s) { _s = s; }

        public Task InitializeAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            MobileAds.Initialize(initStatus => tcs.SetResult(true));
            return tcs.Task;
        }

        public bool IsReady(AdType type, string placement)
        {
            switch (type)
            {
                case AdType.Rewarded: return rewarded != null && rewarded.CanShowAd();
                case AdType.Interstitial: return interstitial != null && interstitial.CanShowAd();
                case AdType.Banner: return banner != null;
            }
            return false;
        }

        public void Load(AdType type, string placement)
        {
            var request = new AdRequest.Builder().Build();
            if (type == AdType.Rewarded)
            {
                RewardedAd.Load(_s.rewardedAdUnitId, request, (ad, err) =>
                {
                    if (err != null) UnityEngine.Debug.LogError(err);
                    rewarded = ad;
                });
            }
            else if (type == AdType.Interstitial)
            {
                InterstitialAd.Load(_s.interstitialAdUnitId, request, (ad, err) =>
                {
                    if (err != null) UnityEngine.Debug.LogError(err);
                    interstitial = ad;
                });
            }
            else if (type == AdType.Banner)
            {
                banner = new BannerView(_s.bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
                banner.LoadAd(request);
            }
        }

        public void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            if (type == AdType.Rewarded && rewarded != null)
            {
                rewarded.OnUserEarnedReward += (o, r) => onReward?.Invoke(new Reward { Type = r.Type, Amount = r.Amount });
                rewarded.Show();
            }
            else if (type == AdType.Interstitial && interstitial != null) interstitial.Show();
            else if (type == AdType.Banner && banner != null) { /* already showing */ }
        }

        public void HideBanner(string placement) => banner?.Hide();
        public void DestroyBanner(string placement) { banner?.Destroy(); banner = null; }
    }
}
#endif
