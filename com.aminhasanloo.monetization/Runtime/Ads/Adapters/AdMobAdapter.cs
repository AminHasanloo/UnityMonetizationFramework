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
    /// Google Mobile Ads adapter using the current static Load()/CanShowAd()/Show(Action&lt;Reward&gt;) API.
    /// </summary>
    public sealed class AdMobAdapter : IAdNetwork
    {
        readonly AdMobSettings settings;
        RewardedAd rewarded;
        InterstitialAd interstitial;
        BannerView banner;

        public AdMobAdapter(AdMobSettings settings) => this.settings = settings;

        public Task InitializeAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            MobileAds.Initialize(_ => tcs.TrySetResult(true));
            return tcs.Task;
        }

        public bool IsReady(AdType type, string placement)
        {
            switch (type)
            {
                case AdType.Rewarded: return rewarded != null && rewarded.CanShowAd();
                case AdType.Interstitial: return interstitial != null && interstitial.CanShowAd();
                case AdType.Banner: return banner != null;
                default: return false;
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

            RewardedAd.Load(settings.rewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Monetization/AdMob] Rewarded load failed: {error}");
                    return;
                }

                rewarded = ad;
                ad.OnAdFullScreenContentClosed += () =>
                {
                    rewarded = null;
                    LoadRewarded();
                };
                ad.OnAdFullScreenContentFailed += errorInfo =>
                {
                    Debug.LogWarning($"[Monetization/AdMob] Rewarded show failed: {errorInfo}");
                    rewarded = null;
                    LoadRewarded();
                };
            });
        }

        void LoadInterstitial()
        {
            interstitial?.Destroy();
            interstitial = null;

            InterstitialAd.Load(settings.interstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Monetization/AdMob] Interstitial load failed: {error}");
                    return;
                }

                interstitial = ad;
                ad.OnAdFullScreenContentClosed += () =>
                {
                    interstitial = null;
                    LoadInterstitial();
                };
                ad.OnAdFullScreenContentFailed += errorInfo =>
                {
                    Debug.LogWarning($"[Monetization/AdMob] Interstitial show failed: {errorInfo}");
                    interstitial = null;
                    LoadInterstitial();
                };
            });
        }

        void LoadBanner()
        {
            DestroyBanner("default");
            banner = new BannerView(settings.bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
            banner.LoadAd(new AdRequest());
        }

        public void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            if (type == AdType.Rewarded && rewarded != null && rewarded.CanShowAd())
            {
                var ad = rewarded;
                Action closeHandler = null;
                closeHandler = () =>
                {
                    ad.OnAdFullScreenContentClosed -= closeHandler;
                    onClosed?.Invoke();
                };
                ad.OnAdFullScreenContentClosed += closeHandler;
                ad.Show(reward => onReward?.Invoke(new Reward { Type = reward.Type, Amount = reward.Amount }));
                return;
            }

            if (type == AdType.Interstitial && interstitial != null && interstitial.CanShowAd())
            {
                var ad = interstitial;
                Action closeHandler = null;
                closeHandler = () =>
                {
                    ad.OnAdFullScreenContentClosed -= closeHandler;
                    onClosed?.Invoke();
                };
                ad.OnAdFullScreenContentClosed += closeHandler;
                ad.Show();
                return;
            }

            if (type == AdType.Banner && banner != null)
            {
                banner.Show();
                return;
            }

            Debug.LogWarning($"[Monetization/AdMob] {type} is not ready.");
        }

        public void HideBanner(string placement) => banner?.Hide();

        public void DestroyBanner(string placement)
        {
            banner?.Destroy();
            banner = null;
        }
    }
}
#endif
