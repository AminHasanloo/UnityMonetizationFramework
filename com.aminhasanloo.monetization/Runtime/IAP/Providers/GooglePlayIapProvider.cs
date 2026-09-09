// SPDX-License-Identifier: MIT
#if STORE_GOOGLEPLAY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;
using UnityEngine.Purchasing;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Unity IAP 5.x adapter using StoreController.
    /// </summary>
    public sealed class GooglePlayIapProvider : IIapProvider
    {
        readonly GooglePlayIapSettings settings;
        readonly StoreProvider store;
        readonly Dictionary<string, string> canonicalToStore = new Dictionary<string, string>();
        readonly Dictionary<string, string> storeToCanonical = new Dictionary<string, string>();

        StoreController controller;
        TaskCompletionSource<bool> productsFetched;
        bool disposed;

        public bool IsInitialized { get; private set; }
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public GooglePlayIapProvider(GooglePlayIapSettings settings, StoreProvider store)
        {
            this.settings = settings;
            this.store = store;
        }

        public async Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            if (!settings.enabled)
                throw new InvalidOperationException("Google Play IAP is selected but disabled in Monetization Settings.");

            canonicalToStore.Clear();
            storeToCanonical.Clear();

            var definitions = new List<ProductDefinition>();
            if (products != null)
            {
                foreach (var product in products)
                {
                    if (product == null || string.IsNullOrWhiteSpace(product.id)) continue;
                    var storeId = product.GetStoreId(store);
                    canonicalToStore[product.id] = storeId;
                    storeToCanonical[storeId] = product.id;
                    definitions.Add(new ProductDefinition(storeId, ToUnityProductType(product.type)));
                }
            }

            controller = UnityIAPServices.StoreController();

            // Unity IAP 5 expects both success and failure callbacks to be registered.
            controller.OnStoreConnected += () => Debug.Log("[Monetization/GooglePlay] Store connected.");
            controller.OnStoreDisconnected += description =>
                Debug.LogWarning($"[Monetization/GooglePlay] Store disconnected: {description.message}");

            controller.OnPurchasePending += OnPurchasePending;
            controller.OnPurchaseFailed += OnPurchaseFailedInternal;

            productsFetched = new TaskCompletionSource<bool>();
            controller.OnProductsFetched += fetchedProducts =>
            {
                Debug.Log($"[Monetization/GooglePlay] Fetched {fetchedProducts.Count} product(s).");
                productsFetched.TrySetResult(true);
            };
            controller.OnProductsFetchFailed += failure =>
                productsFetched.TrySetException(new InvalidOperationException(
                    $"Unity IAP product fetch failed: {failure.FailureReason}"));

            controller.OnPurchasesFetched += OnPurchasesFetched;
            controller.OnPurchasesFetchFailed += failure =>
                Debug.LogWarning($"[Monetization/GooglePlay] Purchase restore failed: {failure.FailureReason} | {failure.Message}");

            await controller.Connect();

            if (definitions.Count > 0)
            {
                controller.FetchProducts(definitions);
                await productsFetched.Task;
            }

            IsInitialized = true;
            controller.FetchPurchases();
            Debug.Log($"[Monetization/GooglePlay] Unity IAP v5 ready with {definitions.Count} product(s).");
        }

        public void Purchase(string productId)
        {
            if (!IsInitialized || controller == null)
            {
                PurchaseFailed?.Invoke(productId, "Unity IAP is not initialized.");
                return;
            }

            if (!canonicalToStore.TryGetValue(productId, out var storeId))
            {
                PurchaseFailed?.Invoke(productId, "Product is not present in the monetization catalog.");
                return;
            }

            controller.PurchaseProduct(storeId);
        }

        public Task RestoreAsync()
        {
            if (controller == null) return Task.CompletedTask;
            controller.FetchPurchases();
            return Task.CompletedTask;
        }

        void OnPurchasePending(PendingOrder order)
        {
            try
            {
                foreach (var item in order.CartOrdered.Items())
                    PurchaseSucceeded?.Invoke(TryGetCanonicalId(item.Product.definition.id));

                // Local-fulfilment default. Server-authoritative games should verify and grant
                // idempotently before confirming the pending order.
                controller.ConfirmPurchase(order);
            }
            catch (Exception ex)
            {
                PurchaseFailed?.Invoke(GetFirstProductId(order), $"Purchase fulfillment failed: {ex.Message}");
            }
        }

        void OnPurchaseFailedInternal(FailedOrder order)
        {
            var storeId = GetFirstProductId(order);
            var message = $"{order.FailureReason}: {order.Details}";
            PurchaseFailed?.Invoke(TryGetCanonicalId(storeId), message);
        }

        void OnPurchasesFetched(Orders orders)
        {
            // Confirmed non-consumables/subscriptions are re-delivered as entitlements.
            // Game-side entitlement handling should be idempotent.
            foreach (var confirmedOrder in orders.ConfirmedOrders)
            {
                foreach (var item in confirmedOrder.CartOrdered.Items())
                {
                    if (item.Product.definition.type == ProductType.Consumable) continue;
                    PurchaseSucceeded?.Invoke(TryGetCanonicalId(item.Product.definition.id));
                }
            }
        }

        static string GetFirstProductId(Order order)
        {
            var item = order?.CartOrdered?.Items()?.FirstOrDefault();
            return item?.Product?.definition?.id ?? string.Empty;
        }

        string TryGetCanonicalId(string storeId)
        {
            if (string.IsNullOrWhiteSpace(storeId)) return storeId;
            return storeToCanonical.TryGetValue(storeId, out var id) ? id : storeId;
        }

        static ProductType ToUnityProductType(IapProductType type)
        {
            switch (type)
            {
                case IapProductType.NonConsumable: return ProductType.NonConsumable;
                case IapProductType.Subscription: return ProductType.Subscription;
                default: return ProductType.Consumable;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            IsInitialized = false;
            controller = null;
        }
    }
}
#endif
