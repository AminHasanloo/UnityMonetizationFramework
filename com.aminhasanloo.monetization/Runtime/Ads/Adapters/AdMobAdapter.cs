// SPDX-License-Identifier: MIT
#if AD_ADMOB
using System;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using GoogleMobileAds.Api;
using UnityEngine;

namespace AminHasanloo.Monetization.Ads
{
    /// <summary>
    /// Google Mobile Ads adapter using the current Load / CanShowAd / Show lifecycle.
    /// The implementation deliberately owns ad destruction/reload so full-screen ad
    /// objects are not leaked or accidentally shown twice.
    /// </summary>
    public sealed class AdMobAdapter : IAdNetwork
    {
        readonly AdMobSettings settings;
        RewardedAd rewarded;
        InterstitialAd interstitial;
        BannerView banner;
        bool bannerLoaded;

        public AdMobAdapter(AdMobSettings settings) => this.settings = settings;

        public Task InitializeAsync()
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var tcs = new TaskCompletionSource<bool>();
            MobileAds.Initialize(_ => tcs.TrySetResult(true));
            return tcs.Task;
        }

        public bool IsReady(AdType type, string placement)
        {
            switch (type)
            {
                case AdType.Rewarded:
                    return rewarded != null && rewarded.CanShowAd();
                case AdType.Interstitial:
                    return interstitial != null && interstitial.CanShowAd();
                case AdType.Banner:
                    return banner != null && bannerLoaded;
                default:
                    return false;
            }
        }

        public void Load(AdType type, string placement)
        {
            switch (type)
            {
                case AdType.Rewarded:
                    LoadRewarded();
                    break;
                case AdType.Interstitial:
                    LoadInterstitial();
                    break;
                case AdType.Banner:
                    LoadBanner();
                    break;
            }
        }

        void LoadRewarded()
        {
            rewarded?.Destroy();
            rewarded = null;

            if (string.IsNullOrWhiteSpace(settings.rewardedAdUnitId))
            {
                Debug.LogWarning("[Monetization/AdMob] Rewarded Ad Unit ID is empty.");
                return;
            }

            RewardedAd.Load(settings.rewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Monetization/AdMob] Rewarded load failed: {error}");
                    return;
                }

                rewarded = ad;
            });
        }

        void LoadInterstitial()
        {
            interstitial?.Destroy();
            interstitial = null;

            if (string.IsNullOrWhiteSpace(settings.interstitialAdUnitId))
            {
                Debug.LogWarning("[Monetization/AdMob] Interstitial Ad Unit ID is empty.");
                return;
            }

            InterstitialAd.Load(settings.interstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Monetization/AdMob] Interstitial load failed: {error}");
                    return;
                }

                interstitial = ad;
            });
        }

        void LoadBanner()
        {
            DestroyBanner("default");

            if (string.IsNullOrWhiteSpace(settings.bannerAdUnitId))
            {
                Debug.LogWarning("[Monetization/AdMob] Banner Ad Unit ID is empty.");
                return;
            }

            bannerLoaded = false;
            banner = new BannerView(settings.bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
            banner.OnBannerAdLoaded += () => bannerLoaded = true;
            banner.OnBannerAdLoadFailed += error =>
            {
                bannerLoaded = false;
                Debug.LogWarning($"[Monetization/AdMob] Banner load failed: {error}");
            };
            banner.LoadAd(new AdRequest());
        }

        public void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            if (type == AdType.Rewarded && rewarded != null && rewarded.CanShowAd())
            {
                var ad = rewarded;
                rewarded = null; // prevents a second Show call while this ad is on screen

                Action closedHandler = null;
                Action<AdError> failedHandler = null;

                closedHandler = () =>
                {
                    DetachRewardedHandlers(ad, closedHandler, failedHandler);
                    ad.Destroy();
                    try { onClosed?.Invoke(); }
                    finally { LoadRewarded(); }
                };

                failedHandler = error =>
                {
                    DetachRewardedHandlers(ad, closedHandler, failedHandler);
                    ad.Destroy();
                    Debug.LogWarning($"[Monetization/AdMob] Rewarded show failed: {error}");
                    LoadRewarded();
                };

                ad.OnAdFullScreenContentClosed += closedHandler;
                ad.OnAdFullScreenContentFailed += failedHandler;
                ad.Show(reward => onReward?.Invoke(new Reward
                {
                    Type = reward.Type,
                    Amount = reward.Amount
                }));
                return;
            }

            if (type == AdType.Interstitial && interstitial != null && interstitial.CanShowAd())
            {
                var ad = interstitial;
                interstitial = null; // prevents duplicate show attempts

                Action closedHandler = null;
                Action<AdError> failedHandler = null;

                closedHandler = () =>
                {
                    DetachInterstitialHandlers(ad, closedHandler, failedHandler);
                    ad.Destroy();
                    try { onClosed?.Invoke(); }
                    finally { LoadInterstitial(); }
                };

                failedHandler = error =>
                {
                    DetachInterstitialHandlers(ad, closedHandler, failedHandler);
                    ad.Destroy();
                    Debug.LogWarning($"[Monetization/AdMob] Interstitial show failed: {error}");
                    LoadInterstitial();
                };

                ad.OnAdFullScreenContentClosed += closedHandler;
                ad.OnAdFullScreenContentFailed += failedHandler;
                ad.Show();
                return;
            }

            if (type == AdType.Banner && banner != null && bannerLoaded)
            {
                banner.Show();
                return;
            }

            Debug.LogWarning($"[Monetization/AdMob] {type} is not ready.");
        }

        static void DetachRewardedHandlers(
            RewardedAd ad,
            Action closedHandler,
            Action<AdError> failedHandler)
        {
            if (ad == null) return;
            if (closedHandler != null) ad.OnAdFullScreenContentClosed -= closedHandler;
            if (failedHandler != null) ad.OnAdFullScreenContentFailed -= failedHandler;
        }

        static void DetachInterstitialHandlers(
            InterstitialAd ad,
            Action closedHandler,
            Action<AdError> failedHandler)
        {
            if (ad == null) return;
            if (closedHandler != null) ad.OnAdFullScreenContentClosed -= closedHandler;
            if (failedHandler != null) ad.OnAdFullScreenContentFailed -= failedHandler;
        }

        public void HideBanner(string placement) => banner?.Hide();

        public void DestroyBanner(string placement)
        {
            bannerLoaded = false;
            banner?.Destroy();
            banner = null;
        }
    }
}
#endif
