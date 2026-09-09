// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;

namespace AminHasanloo.Monetization.Ads
{
    public static class Ads
    {
        static readonly List<IAdNetwork> networks = new List<IAdNetwork>();
        static bool initialized;

        public static bool IsInitialized => initialized;
        public static int NetworkCount => networks.Count;

        public static async Task InitializeAsync()
        {
            if (initialized) return;

            networks.Clear();
            var s = MonetizationSettings.Load();

#if UNITY_EDITOR
            if (s.useMockServicesInEditor)
                networks.Add(new MockAdNetwork());
#endif

#if AD_ADMOB
            if (s.adMob.enabled) networks.Add(new AdMobAdapter(s.adMob));
#endif
#if AD_TAPSELL
            if (s.tapsell.enabled) networks.Add(new TapsellAdapter(s.tapsell));
#endif

            // LevelPlay's legacy IronSource.Agent integration from v1 is intentionally not
            // registered in v2. Unity requires the newer LevelPlay Init + Ad Unit APIs.
            // A clean AD_LEVELPLAY adapter is tracked in the roadmap instead of pretending
            // the legacy adapter is production-safe.

            foreach (var network in networks)
            {
                try { await network.InitializeAsync(); }
                catch (Exception ex) { Debug.LogError($"[Monetization/Ads] {network.GetType().Name} init failed: {ex.Message}"); }
            }

            initialized = true;
        }

        internal static void Reset()
        {
            networks.Clear();
            initialized = false;
        }

        public static void Load(AdType type, string placement = "default")
        {
            foreach (var network in networks) network.Load(type, placement);
        }

        public static bool IsReady(AdType type, string placement = "default")
        {
            foreach (var network in networks)
                if (network.IsReady(type, placement)) return true;
            return false;
        }

        public static bool IsRewardedReady(string placement) => IsReady(AdType.Rewarded, placement);

        public static void ShowRewarded(string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            foreach (var network in networks)
            {
                if (!network.IsReady(AdType.Rewarded, placement)) continue;
                network.Show(AdType.Rewarded, placement, onReward, onClosed);
                return;
            }
            Debug.LogWarning($"[Monetization/Ads] No rewarded ad ready for placement: {placement}");
        }

        public static void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            foreach (var network in networks)
            {
                if (!network.IsReady(AdType.Interstitial, placement)) continue;
                network.Show(AdType.Interstitial, placement, null, onClosed);
                return;
            }
            Debug.LogWarning($"[Monetization/Ads] No interstitial ready for placement: {placement}");
        }

        public static void ShowBanner(string placement = "default")
        {
            foreach (var network in networks)
            {
                if (!network.IsReady(AdType.Banner, placement)) continue;
                network.Show(AdType.Banner, placement);
                return;
            }
            Debug.LogWarning($"[Monetization/Ads] No banner ready for placement: {placement}");
        }

        public static void HideBanner(string placement = "default")
        {
            foreach (var network in networks) network.HideBanner(placement);
        }

        public static void DestroyBanner(string placement = "default")
        {
            foreach (var network in networks) network.DestroyBanner(placement);
        }
    }
}
