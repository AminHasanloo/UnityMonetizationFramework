// SPDX-License-Identifier: MIT
#if STORE_CAFEBAZAAR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP.Providers
{
    public class CafeBazaarIapProvider : IIapProvider
    {
        private readonly string publicKey;
        public CafeBazaarIapProvider(string publicKey) { this.publicKey = publicKey; }
        public bool IsInitialized { get; private set; }

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public void Initialize(string[] productIds)
        {
            try
            {
                BazaarIAB.enableLogging(true);
                BazaarIAB.init(publicKey);
                IABEventManager.billingSupportedEvent += () =>
                {
                    IsInitialized = true;
                    BazaarIAB.queryInventory(productIds);
                };
                IABEventManager.billingNotSupportedEvent += (err) => Debug.LogError("Bazaar billing not supported: " + err);
                IABEventManager.purchaseSucceededEvent += (purchase) => OnPurchaseSucceeded?.Invoke(purchase.ProductId);
                IABEventManager.purchaseFailedEvent += (err) => OnPurchaseFailed?.Invoke("", err);
            }
            catch (Exception e) { Debug.LogError(e); }
        }

        public void Purchase(string productId) => BazaarIAB.purchaseProduct(productId);
        public void Restore() => BazaarIAB.queryPurchases();
    }

    // Dummy types to avoid compile errors if plugin not imported
    public static class BazaarIAB
    {
        public static void enableLogging(bool v) { }
        public static void init(string key) { }
        public static void queryInventory(string[] skus) { }
        public static void queryPurchases() { }
        public static void purchaseProduct(string sku) { }
    }
    public static class IABEventManager
    {
        public static event Action billingSupportedEvent;
        public static event Action<string> billingNotSupportedEvent;
        public static event Action<BazaarPurchase> purchaseSucceededEvent;
        public static event Action<string> purchaseFailedEvent;
    }
    public class BazaarPurchase { public string ProductId; }
}
#endif
