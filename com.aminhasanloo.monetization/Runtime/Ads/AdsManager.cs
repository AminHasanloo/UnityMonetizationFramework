// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;

namespace AminHasanloo.Monetization.Ads
{
    public static class Ads
    {
        static List<IAdNetwork> networks = new List<IAdNetwork>();
        static bool initialized;

        public static async Task InitializeAsync()
        {
            if (initialized) return;
            var s = MonetizationSettings.Load();

#if AD_ADMOB
            networks.Add(new AdMobAdapter(s.adMob));
#endif
#if AD_TAPSELL
            networks.Add(new TapsellAdapter(s.tapsell));
#endif
#if AD_IRONSOURCE
            networks.Add(new IronSourceAdapter(s.ironSource));
#endif
            foreach (var n in networks) await n.InitializeAsync();
            initialized = true;
        }

        public static bool IsRewardedReady(string placement)
        {
            foreach (var n in networks)
                if (n.IsReady(AdType.Rewarded, placement)) return true;
            return false;
        }

        public static void ShowRewarded(string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            foreach (var n in networks)
            {
                if (n.IsReady(AdType.Rewarded, placement))
                {
                    n.Show(AdType.Rewarded, placement, onReward, onClosed);
                    return;
                }
            }
            UnityEngine.Debug.LogWarning($"No rewarded ad ready for placement: {placement}");
        }

        public static void ShowInterstitial(string placement)
        {
            foreach (var n in networks)
                if (n.IsReady(AdType.Interstitial, placement)) { n.Show(AdType.Interstitial, placement); return; }
        }

        public static void ShowBanner(string placement) 
        {
            foreach (var n in networks)
                if (n.IsReady(AdType.Banner, placement)) { n.Show(AdType.Banner, placement); return; }
        }
    }
}
