// SPDX-License-Identifier: MIT
#if STORE_CAFEBAZAAR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using Bazaar.Data;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Real Cafe Bazaar adapter based on the official Poolakey Unity SDK.
    /// v1 shipped no-op placeholder classes; v2 removes those shims entirely.
    /// </summary>
    public sealed class CafeBazaarIapProvider : IIapProvider
    {
        readonly CafeBazaarIapSettings settings;
        readonly StoreProvider store;
        readonly Dictionary<string, IapProductDefinition> productsByCanonical = new Dictionary<string, IapProductDefinition>();
        readonly Dictionary<string, string> canonicalToStore = new Dictionary<string, string>();
        readonly Dictionary<string, string> storeToCanonical = new Dictionary<string, string>();

        Payment payment;

        public bool IsInitialized { get; private set; }
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public CafeBazaarIapProvider(CafeBazaarIapSettings settings, StoreProvider store)
        {
            this.settings = settings;
            this.store = store;
        }

        public async Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            if (!settings.enabled)
                throw new InvalidOperationException("Cafe Bazaar is selected but disabled in Monetization Settings.");

#if !UNITY_ANDROID
            throw new PlatformNotSupportedException("Cafe Bazaar Poolakey is supported on Android builds.");
#else
            productsByCanonical.Clear();
            canonicalToStore.Clear();
            storeToCanonical.Clear();

            if (products != null)
            {
                foreach (var product in products)
                {
                    if (product == null || string.IsNullOrWhiteSpace(product.id)) continue;
                    var storeId = product.GetStoreId(store);
                    productsByCanonical[product.id] = product;
                    canonicalToStore[product.id] = storeId;
                    storeToCanonical[storeId] = product.id;
                }
            }

            var security = string.IsNullOrWhiteSpace(settings.publicKey)
                ? SecurityCheck.Disable()
                : SecurityCheck.Enable(settings.publicKey);

            payment = new Payment(new PaymentConfiguration(security));
            var result = await payment.Connect();
            if (result.status != Status.Success)
                throw new InvalidOperationException($"Poolakey connection failed: {result.message}");

            IsInitialized = true;
            Debug.Log($"[Monetization/Bazaar] Poolakey connected with {productsByCanonical.Count} product(s).");
#endif
        }

        public async void Purchase(string productId)
        {
            if (!IsInitialized || payment == null)
            {
                PurchaseFailed?.Invoke(productId, "Poolakey is not initialized.");
                return;
            }

            if (!canonicalToStore.TryGetValue(productId, out var storeId))
            {
                PurchaseFailed?.Invoke(productId, "Product is not present in the monetization catalog.");
                return;
            }

            try
            {
                var definition = productsByCanonical[productId];
                var skuType = definition.type == IapProductType.Subscription ? SKUDetails.Type.subscription : SKUDetails.Type.inApp;
                var result = await payment.Purchase(storeId, skuType);

                if (result.status != Status.Success || result.data == null)
                {
                    PurchaseFailed?.Invoke(productId, result.message ?? "Purchase failed.");
                    return;
                }

                if (definition.type == IapProductType.Consumable && settings.autoConsumeConsumables)
                {
                    var consume = await payment.Consume(result.data.purchaseToken);
                    if (consume.status != Status.Success)
                    {
                        PurchaseFailed?.Invoke(productId, $"Purchase succeeded but consume failed: {consume.message}");
                        return;
                    }
                }

                PurchaseSucceeded?.Invoke(productId);
            }
            catch (Exception ex)
            {
                PurchaseFailed?.Invoke(productId, ex.Message);
            }
        }

        public async Task RestoreAsync()
        {
            if (!IsInitialized || payment == null) return;

            var inApps = await payment.GetPurchases(SKUDetails.Type.inApp);
            if (inApps.status == Status.Success && inApps.data != null)
            {
                foreach (var purchase in inApps.data)
                    RaiseRestoredPurchase(purchase);
            }

            var subscriptions = await payment.GetPurchases(SKUDetails.Type.subscription);
            if (subscriptions.status == Status.Success && subscriptions.data != null)
            {
                foreach (var purchase in subscriptions.data)
                    RaiseRestoredPurchase(purchase);
            }
        }

        void RaiseRestoredPurchase(PurchaseInfo purchase)
        {
            if (purchase == null || string.IsNullOrWhiteSpace(purchase.productId)) return;
            if (!storeToCanonical.TryGetValue(purchase.productId, out var canonicalId)) return;

            if (productsByCanonical.TryGetValue(canonicalId, out var definition) &&
                definition.type != IapProductType.Consumable)
            {
                PurchaseSucceeded?.Invoke(canonicalId);
            }
        }

        public void Dispose()
        {
            if (payment != null)
            {
                try { payment.Disconnect(); }
                catch (Exception ex) { Debug.LogWarning($"[Monetization/Bazaar] Disconnect: {ex.Message}"); }
            }
            payment = null;
            IsInitialized = false;
        }
    }
}
#endif
