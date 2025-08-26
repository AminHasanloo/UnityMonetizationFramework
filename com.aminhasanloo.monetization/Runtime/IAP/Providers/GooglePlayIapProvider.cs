// SPDX-License-Identifier: MIT
#if STORE_GOOGLEPLAY
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace AminHasanloo.Monetization.IAP.Providers
{
    public class GooglePlayIapProvider : IIapProvider, IStoreListener
    {
        private IStoreController controller;
        private IExtensionProvider extensions;

        public bool IsInitialized => controller != null && extensions != null;

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public void Initialize(string[] productIds)
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var id in productIds)
                builder.AddProduct(id, ProductType.Consumable, new IDs { { id, GooglePlay.Name } });
            UnityPurchasing.Initialize(this, builder);
        }

        public void Purchase(string productId)
        {
            if (!IsInitialized) { Debug.LogWarning("Unity IAP not initialized"); return; }
            var product = controller.products.WithID(productId);
            if (product != null && product.availableToPurchase) controller.InitiatePurchase(product);
            else Debug.LogWarning("Product not found or not available: " + productId);
        }

        public void Restore()
        {
            if (!IsInitialized) return;
#if UNITY_ANDROID
            var google = extensions.GetExtension<IGooglePlayStoreExtensions>();
            google.RestoreTransactions(result => Debug.Log("RestoreTransactions: " + result));
#endif
        }

        // IStoreListener
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.controller = controller; this.extensions = extensions;
            Debug.Log("Unity IAP Initialized");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError("IAP Init Failed: " + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            Debug.Log("Purchase Success: " + e.purchasedProduct.definition.id);
            OnPurchaseSucceeded?.Invoke(e.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed?.Invoke(product.definition.id, failureReason.ToString());
        }
    }
}
#endif
