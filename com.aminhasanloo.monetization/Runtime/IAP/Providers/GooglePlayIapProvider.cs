// SPDX-License-Identifier: MIT
#if STORE_GOOGLEPLAY
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;
using UnityEngine.Purchasing;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Unity IAP 5.x adapter. The v1 IStoreListener/ConfigurationBuilder implementation
    /// was removed because those APIs were replaced by StoreController in IAP v5.
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
            controller.OnPurchasePending += OnPurchasePending;
            controller.OnPurchaseFailed += failedOrder =>
                PurchaseFailed?.Invoke(TryGetCanonicalId(failedOrder.ToString()), failedOrder.ToString());

            productsFetched = new TaskCompletionSource<bool>();
            controller.OnProductsFetched += fetchedProducts => productsFetched.TrySetResult(true);
            controller.OnProductsFetchFailed += failure =>
                productsFetched.TrySetException(new InvalidOperationException($"Unity IAP product fetch failed: {failure}"));

            controller.OnPurchasesFetched += orders =>
            {
                foreach (var confirmedOrder in orders.ConfirmedOrders)
                {
                    foreach (var item in confirmedOrder.CartOrdered.Items())
                    {
                        if (item.Product.definition.type == ProductType.Consumable) continue;
                        PurchaseSucceeded?.Invoke(TryGetCanonicalId(item.Product.definition.id));
                    }
                }
            };

            await controller.Connect();

            if (definitions.Count > 0)
            {
                controller.FetchProducts(definitions);
                await productsFetched.Task;
            }

            IsInitialized = true;
            controller.FetchPurchases();
            Debug.Log($"[Monetization/GooglePlay] Unity IAP v5 connected with {definitions.Count} product(s).");
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

                // Basic local-fulfilment mode. For server-authoritative economies, replace this
                // with receipt verification before ConfirmPurchase (see README roadmap/security notes).
                controller.ConfirmPurchase(order);
            }
            catch (Exception ex)
            {
                PurchaseFailed?.Invoke(string.Empty, $"Purchase fulfillment failed: {ex.Message}");
            }
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
