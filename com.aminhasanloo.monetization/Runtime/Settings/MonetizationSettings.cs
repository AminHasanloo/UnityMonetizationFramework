// SPDX-License-Identifier: MIT
// Author: Amin Hasanloo

using UnityEngine;

namespace AminHasanloo.Monetization.Settings
{
    public enum StoreProvider
    {
        GooglePlay,
        CafeBazaar,
        Myket,
        Zarinpal
    }

    [System.Serializable]
    public class AdMobSettings
    {
        public bool enabled;
        [Header("AdMob")]
        public string androidAppId;
        public string bannerAdUnitId;
        public string interstitialAdUnitId;
        public string rewardedAdUnitId;
    }

    [System.Serializable]
    public class TapsellSettings
    {
        public bool enabled;
        public string appIdOrKey;
        public string bannerZoneId;
        public string interstitialZoneId;
        public string rewardedZoneId;
    }

    [System.Serializable]
    public class IronSourceSettings
    {
        public bool enabled;
        public string appKey;
        public string bannerPlacement = "DefaultBanner";
        public string interstitialPlacement = "DefaultInterstitial";
        public string rewardedPlacement = "DefaultRewardedVideo";
    }

    [System.Serializable]
    public class GooglePlayIapSettings
    {
        public bool enabled = true;
    }

    [System.Serializable]
    public class CafeBazaarIapSettings
    {
        public bool enabled;
        [Tooltip("RSA Public Key from CafeBazaar developer panel")]
        public string publicKey;
    }

    [System.Serializable]
    public class MyketIapSettings
    {
        public bool enabled;
        [Tooltip("RSA Public Key from Myket developer panel")]
        public string publicKey;
    }

    [System.Serializable]
    public class ZarinpalSettings
    {
        public bool enabled;
        public string merchantId;
        [Tooltip("Callback URL (deep-link to your app or your backend endpoint)")]
        public string callbackUrl = "myapp://zarinpal";
        [Tooltip("Optional base URL override (for sandbox)")]
        public string baseApiUrl = "https://payment.zarinpal.com";
    }

    [CreateAssetMenu(menuName = "Amin Hasanloo/Monetization Settings", fileName = "MonetizationSettings")]
    public class MonetizationSettings : ScriptableObject
    {
        [Header("General")]
        public bool initializeOnStartup = true;

        [Header("Stores")]
        public GooglePlayIapSettings googlePlay = new GooglePlayIapSettings();
        public CafeBazaarIapSettings cafeBazaar = new CafeBazaarIapSettings();
        public MyketIapSettings myket = new MyketIapSettings();
        public ZarinpalSettings zarinpal = new ZarinpalSettings();

        [Header("Ads")]
        public AdMobSettings adMob = new AdMobSettings();
        public TapsellSettings tapsell = new TapsellSettings();
        public IronSourceSettings ironSource = new IronSourceSettings();

        [Header("Catalogs")]
        [Tooltip("Product IDs shared across stores (override per store in provider if needed).")]
        public string[] productIds;

        public static MonetizationSettings Load()
        {
            var settings = Resources.Load<MonetizationSettings>("MonetizationSettings");
            if (settings == null)
            {
                settings = CreateInstance<MonetizationSettings>();
            }
            return settings;
        }
    }
}
