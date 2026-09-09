// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AminHasanloo.Monetization.Ads
{
    public sealed class MockAdNetwork : IAdNetwork
    {
        bool initialized;
        public Task InitializeAsync() { initialized = true; Debug.Log("[Monetization/MockAds] Ready."); return Task.CompletedTask; }
        public bool IsReady(AdType type, string placement) => initialized;
        public void Load(AdType type, string placement) => Debug.Log($"[Monetization/MockAds] Load {type} ({placement})");
        public void Show(AdType type, string placement, Action<Reward> onReward = null, Action onClosed = null)
        {
            Debug.Log($"[Monetization/MockAds] Show {type} ({placement})");
            if (type == AdType.Rewarded) onReward?.Invoke(new Reward { Type = "mock_reward", Amount = 1 });
            onClosed?.Invoke();
        }
        public void HideBanner(string placement) => Debug.Log($"[Monetization/MockAds] Hide banner ({placement})");
        public void DestroyBanner(string placement) => Debug.Log($"[Monetization/MockAds] Destroy banner ({placement})");
    }
}
