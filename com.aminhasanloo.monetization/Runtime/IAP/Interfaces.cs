// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP
{
    public interface IIapProvider : IDisposable
    {
        bool IsInitialized { get; }
        Task InitializeAsync(IReadOnlyList<IapProductDefinition> products);
        void Purchase(string productId);
        Task RestoreAsync();
        event Action<string> PurchaseSucceeded;
        event Action<string, string> PurchaseFailed;
    }

    public static class Iap
    {
        static IIapProvider provider;
        static event Action<string> purchaseSucceeded;
        static event Action<string, string> purchaseFailed;

        public static bool IsInitialized => provider != null && provider.IsInitialized;
        public static string ProviderName => provider?.GetType().Name ?? "None";

        /// <summary>
        /// Subscriptions are retained even when registered before provider initialization.
        /// This fixes a v1 issue where early subscriptions were silently lost.
        /// </summary>
        public static event Action<string> OnPurchaseSucceeded
        {
            add => purchaseSucceeded += value;
            remove => purchaseSucceeded -= value;
        }

        public static event Action<string, string> OnPurchaseFailed
        {
            add => purchaseFailed += value;
            remove => purchaseFailed -= value;
        }

        internal static async Task InitializeAsync(IIapProvider newProvider, IReadOnlyList<IapProductDefinition> products)
        {
            if (newProvider == null)
                throw new ArgumentNullException(nameof(newProvider));

            DetachProvider();
            provider = newProvider;
            provider.PurchaseSucceeded += HandlePurchaseSucceeded;
            provider.PurchaseFailed += HandlePurchaseFailed;

            try
            {
                await provider.InitializeAsync(products);
            }
            catch
            {
                DetachProvider();
                throw;
            }
        }

        public static void Purchase(string productId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[Monetization/IAP] Purchase called before IAP initialization.");
                purchaseFailed?.Invoke(productId, "IAP is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(productId))
            {
                purchaseFailed?.Invoke(productId, "Product ID is empty.");
                return;
            }

            provider.Purchase(productId);
        }

        public static Task RestoreAsync()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Monetization/IAP] Restore called before initialization.");
                return Task.CompletedTask;
            }

            return provider.RestoreAsync();
        }

        [Obsolete("Use RestoreAsync() and await it instead.")]
        public static void Restore() => _ = RestoreAsync();

        internal static void Shutdown()
        {
            DetachProvider();
        }

        static void HandlePurchaseSucceeded(string id) => purchaseSucceeded?.Invoke(id);
        static void HandlePurchaseFailed(string id, string error) => purchaseFailed?.Invoke(id, error);

        static void DetachProvider()
        {
            if (provider == null) return;
            provider.PurchaseSucceeded -= HandlePurchaseSucceeded;
            provider.PurchaseFailed -= HandlePurchaseFailed;
            provider.Dispose();
            provider = null;
        }
    }
}
