// SPDX-License-Identifier: MIT
using System;
using AminHasanloo.Monetization.Ads;
using AminHasanloo.Monetization.IAP;
using UnityEngine;

namespace AminHasanloo.Monetization.Samples
{
    /// <summary>
    /// Drop this component on a GameObject and wire UI buttons to the public methods.
    /// In Editor, the default v2 settings use mock IAP + ads, so this demo works without SDK accounts.
    /// </summary>
    public sealed class MonetizationDemoUI : MonoBehaviour
    {
        [SerializeField] string demoProductId = "coins_100";
        [SerializeField] string rewardedPlacement = "reward_default";
        [SerializeField] string interstitialPlacement = "interstitial_default";

        async void Start()
        {
            Iap.OnPurchaseSucceeded += OnPurchaseSucceeded;
            Iap.OnPurchaseFailed += OnPurchaseFailed;

            try
            {
                await Monetization.InitializeAsync();
                Ads.Load(AdType.Rewarded, rewardedPlacement);
                Ads.Load(AdType.Interstitial, interstitialPlacement);
                Debug.Log($"[Monetization Demo] Ready. IAP={Iap.ProviderName}, ad networks={Ads.NetworkCount}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Monetization Demo] Initialization failed: {ex}");
            }
        }

        void OnDestroy()
        {
            Iap.OnPurchaseSucceeded -= OnPurchaseSucceeded;
            Iap.OnPurchaseFailed -= OnPurchaseFailed;
        }

        public void BuyDemoProduct() => Iap.Purchase(demoProductId);

        public async void RestorePurchases() => await Iap.RestoreAsync();

        public void ShowRewarded()
        {
            if (!Ads.IsRewardedReady(rewardedPlacement))
            {
                Ads.Load(AdType.Rewarded, rewardedPlacement);
                Debug.Log("[Monetization Demo] Rewarded ad requested. Try again when ready.");
                return;
            }

            Ads.ShowRewarded(
                rewardedPlacement,
                reward => Debug.Log($"[Monetization Demo] Reward: {reward.Amount} {reward.Type}"),
                () => Debug.Log("[Monetization Demo] Rewarded ad closed."));
        }

        public void ShowInterstitial()
        {
            if (!Ads.IsReady(AdType.Interstitial, interstitialPlacement))
            {
                Ads.Load(AdType.Interstitial, interstitialPlacement);
                return;
            }

            Ads.ShowInterstitial(interstitialPlacement,
                () => Debug.Log("[Monetization Demo] Interstitial closed."));
        }

        void OnPurchaseSucceeded(string productId) =>
            Debug.Log($"[Monetization Demo] Purchase/restore success: {productId}");

        void OnPurchaseFailed(string productId, string error) =>
            Debug.LogError($"[Monetization Demo] Purchase failed: {productId} | {error}");
    }
}
