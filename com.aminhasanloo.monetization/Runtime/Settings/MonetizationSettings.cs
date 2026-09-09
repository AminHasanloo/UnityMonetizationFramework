// SPDX-License-Identifier: MIT
// Author: Amin Hasanloo

using System;
using UnityEngine;

namespace AminHasanloo.Monetization.Settings
{
    public enum StoreProvider
    {
        Mock,
        GooglePlay,
        CafeBazaar,
        Myket,
        ZarinpalLegacy
    }

    public enum IapProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    [Serializable]
    public class IapProductDefinition
    {
        [Tooltip("Stable product ID used by game code.")]
        public string id;
        public IapProductType type = IapProductType.Consumable;

        [Header("Optional store-specific IDs")]
        public string googlePlayId;
        public string cafeBazaarId;
        public string myketId;

        public string GetStoreId(StoreProvider store)
        {
            switch (store)
            {
                case StoreProvider.GooglePlay:
                    return string.IsNullOrWhiteSpace(googlePlayId) ? id : googlePlayId;
                case StoreProvider.CafeBazaar:
                    return string.IsNullOrWhiteSpace(cafeBazaarId) ? id : cafeBazaarId;
                case StoreProvider.Myket:
                    return string.IsNullOrWhiteSpace(myketId) ? id : myketId;
                default:
                    return id;
            }
        }
    }

    [Serializable]
    public class AdMobSettings
    {
        public bool enabled;
        [Header("AdMob")]
        public string androidAppId;
        public string bannerAdUnitId;
        public string interstitialAdUnitId;
        public string rewardedAdUnitId;
    }

    [Serializable]
    public class TapsellSettings
    {
        public bool enabled;
        public string appIdOrKey;
        public string bannerZoneId;
        public string interstitialZoneId;
        public string rewardedZoneId;
    }

    [Serializable]
    public class LevelPlaySettings
    {
        public bool enabled;
        public string appKey;
        [Tooltip("LevelPlay Ad Unit ID for banner")]
        public string bannerAdUnitId;
        [Tooltip("LevelPlay Ad Unit ID for interstitial")]
        public string interstitialAdUnitId;
        [Tooltip("LevelPlay Ad Unit ID for rewarded")]
        public string rewardedAdUnitId;
        [Tooltip("Optional user ID passed during LevelPlay initialization")]
        public string userId;
    }

    [Serializable]
    public class GooglePlayIapSettings
    {
        public bool enabled = true;
    }

    [Serializable]
    public class CafeBazaarIapSettings
    {
        public bool enabled;
        [Tooltip("RSA public key from Cafe Bazaar developer panel. Used by Poolakey security check.")]
        public string publicKey;
        [Tooltip("Automatically consume consumable products after a successful local fulfillment callback.")]
        public bool autoConsumeConsumables = true;
    }

    [Serializable]
    public class MyketIapSettings
    {
        public bool enabled;
        [Tooltip("Reserved for the official Myket Billing adapter. The v2 migration adapter is on the roadmap.")]
        public string publicKey;
    }

    [Serializable]
    public class ZarinpalSettings
    {
        public bool enabled;
        [Tooltip("Legacy client-side settings are retained for migration only. Production payment verification must be server-side.")]
        public string merchantId;
        public string callbackUrl = "myapp://zarinpal";
        public string baseApiUrl = "https://payment.zarinpal.com";
    }

    [CreateAssetMenu(menuName = "Amin Hasanloo/Monetization Settings", fileName = "MonetizationSettings")]
    public class MonetizationSettings : ScriptableObject
    {
        [Header("General")]
        public bool initializeOnStartup = true;
        [Tooltip("Lets the package work in the Unity Editor without any third-party SDK or store account.")]
        public bool useMockServicesInEditor = true;

        [Header("Active IAP Store")]
        [Tooltip("Exactly one IAP provider is active at runtime. Mock is recommended while developing in the Editor.")]
        public StoreProvider activeStore = StoreProvider.Mock;

        [Header("Store Configuration")]
        public GooglePlayIapSettings googlePlay = new GooglePlayIapSettings();
        public CafeBazaarIapSettings cafeBazaar = new CafeBazaarIapSettings();
        public MyketIapSettings myket = new MyketIapSettings();
        public ZarinpalSettings zarinpal = new ZarinpalSettings();

        [Header("Ad Networks")]
        public AdMobSettings adMob = new AdMobSettings();
        public TapsellSettings tapsell = new TapsellSettings();
        public LevelPlaySettings levelPlay = new LevelPlaySettings();

        [Header("Product Catalog")]
        public IapProductDefinition[] products = Array.Empty<IapProductDefinition>();

        [HideInInspector]
        [Tooltip("Legacy v1 catalog. Migrate these IDs to Products.")]
        public string[] productIds = Array.Empty<string>();

        public IapProductDefinition[] GetProducts()
        {
            if (products != null && products.Length > 0)
                return products;

            if (productIds == null || productIds.Length == 0)
                return Array.Empty<IapProductDefinition>();

            var migrated = new IapProductDefinition[productIds.Length];
            for (var i = 0; i < productIds.Length; i++)
            {
                migrated[i] = new IapProductDefinition
                {
                    id = productIds[i],
                    type = IapProductType.Consumable
                };
            }
            return migrated;
        }

        public static MonetizationSettings Load()
        {
            var settings = Resources.Load<MonetizationSettings>("MonetizationSettings");
            if (settings != null)
                return settings;

#if UNITY_EDITOR
            // Safe transient defaults for explicit Editor tests. Auto-start is disabled so
            // merely importing the package does not execute a fake purchase environment.
            settings = CreateInstance<MonetizationSettings>();
            settings.name = "Transient Monetization Settings (Editor Mock)";
            settings.initializeOnStartup = false;
            settings.useMockServicesInEditor = true;
            settings.activeStore = StoreProvider.Mock;
            return settings;
#else
            // Never silently fall back to Mock purchases in a player build.
            throw new InvalidOperationException(
                "MonetizationSettings.asset is missing. Create Assets/Resources/MonetizationSettings.asset before building a player.");
#endif
        }
    }
}
