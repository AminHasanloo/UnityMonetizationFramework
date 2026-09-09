// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Development provider that makes the full purchase flow testable in Editor
    /// without any store SDK or developer account.
    /// </summary>
    public sealed class MockIapProvider : IIapProvider
    {
        readonly HashSet<string> knownProducts = new HashSet<string>();

        public bool IsInitialized { get; private set; }
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            knownProducts.Clear();
            if (products != null)
            {
                foreach (var product in products)
                    if (product != null && !string.IsNullOrWhiteSpace(product.id))
                        knownProducts.Add(product.id);
            }

            IsInitialized = true;
            Debug.Log($"[Monetization/MockIAP] Ready with {knownProducts.Count} product(s).");
            return Task.CompletedTask;
        }

        public void Purchase(string productId)
        {
            if (!IsInitialized)
            {
                PurchaseFailed?.Invoke(productId, "Mock IAP is not initialized.");
                return;
            }

            if (!knownProducts.Contains(productId))
            {
                PurchaseFailed?.Invoke(productId, "Unknown product ID.");
                return;
            }

            Debug.Log($"[Monetization/MockIAP] Purchase succeeded: {productId}");
            PurchaseSucceeded?.Invoke(productId);
        }

        public Task RestoreAsync()
        {
            Debug.Log("[Monetization/MockIAP] Restore completed.");
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
