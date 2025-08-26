// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

namespace AminHasanloo.Monetization.Ads
{
    public enum AdType { Banner, Interstitial, Rewarded }
    public interface IAdNetwork
    {
        Task InitializeAsync();
        bool IsReady(AdType type, string placement);
        void Load(AdType type, string placement);
        void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null);
        void HideBanner(string placement);
        void DestroyBanner(string placement);
    }

    public struct Reward
    {
        public string Type;
        public double Amount;
    }
}
