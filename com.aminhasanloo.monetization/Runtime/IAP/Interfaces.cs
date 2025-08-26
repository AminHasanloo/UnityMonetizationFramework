// SPDX-License-Identifier: MIT
using System;

namespace AminHasanloo.Monetization.IAP
{
    public interface IIapProvider
    {
        void Initialize(string[] productIds);
        bool IsInitialized { get; }
        void Purchase(string productId);
        void Restore();
        event Action<string> OnPurchaseSucceeded;
        event Action<string, string> OnPurchaseFailed;
    }

    public static class Iap
    {
        static IIapProvider provider;
        public static void Initialize(IIapProvider p, string[] productIds) { provider = p; provider.Initialize(productIds); }
        public static bool IsInitialized => provider != null && provider.IsInitialized;
        public static void Purchase(string productId) => provider?.Purchase(productId);
        public static void Restore() => provider?.Restore();
        public static event Action<string> OnPurchaseSucceeded { add { if (provider != null) provider.OnPurchaseSucceeded += value; } remove { if (provider != null) provider.OnPurchaseSucceeded -= value; } }
        public static event Action<string, string> OnPurchaseFailed { add { if (provider != null) provider.OnPurchaseFailed += value; } remove { if (provider != null) provider.OnPurchaseFailed -= value; } }
    }
}
