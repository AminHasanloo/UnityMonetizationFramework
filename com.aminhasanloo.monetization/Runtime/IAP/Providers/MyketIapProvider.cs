// SPDX-License-Identifier: MIT
#if STORE_MYKET
using System;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP.Providers
{
    public class MyketIapProvider : IIapProvider
    {
        private readonly string publicKey;
        public MyketIapProvider(string publicKey) { this.publicKey = publicKey; }
        public bool IsInitialized { get; private set; }
        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public void Initialize(string[] productIds)
        {
            try
            {
                MyketIAB.enableLogging(true);
                MyketIAB.init(publicKey);
                MyketIABEventManager.billingSupportedEvent += () => { IsInitialized = true; MyketIAB.queryInventory(productIds); };
                MyketIABEventManager.billingNotSupportedEvent += (err) => Debug.LogError("Myket billing not supported: " + err);
                MyketIABEventManager.purchaseSucceededEvent += (purchase) => OnPurchaseSucceeded?.Invoke(purchase.ProductId);
                MyketIABEventManager.purchaseFailedEvent += (err) => OnPurchaseFailed?.Invoke("", err);
            }
            catch (Exception e) { Debug.LogError(e); }
        }

        public void Purchase(string productId) => MyketIAB.purchaseProduct(productId);
        public void Restore() => MyketIAB.queryPurchases();
    }

    // Dummy placeholders if plugin absent
    public static class MyketIAB
    {
        public static void enableLogging(bool v) { }
        public static void init(string key) { }
        public static void queryInventory(string[] skus) { }
        public static void queryPurchases() { }
        public static void purchaseProduct(string sku) { }
    }
    public static class MyketIABEventManager
    {
        public static event Action billingSupportedEvent;
        public static event Action<string> billingNotSupportedEvent;
        public static event Action<MyketPurchase> purchaseSucceededEvent;
        public static event Action<string> purchaseFailedEvent;
    }
    public class MyketPurchase { public string ProductId; }
}
#endif
