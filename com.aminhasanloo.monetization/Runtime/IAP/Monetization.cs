// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Ads;
using AminHasanloo.Monetization.IAP;
using AminHasanloo.Monetization.Settings;
using UnityEngine;

namespace AminHasanloo.Monetization
{
    public static class Monetization
    {
        static Task initializationTask;
        static bool initialized;

        public static bool IsInitialized => initialized;

        public static Task InitializeAsync(bool force = false)
        {
            if (initialized && !force)
                return Task.CompletedTask;

            if (initializationTask != null && !initializationTask.IsCompleted && !force)
                return initializationTask;

            initializationTask = InitializeInternalAsync(force);
            return initializationTask;
        }

        static async Task InitializeInternalAsync(bool force)
        {
            if (force)
            {
                Iap.Shutdown();
                Ads.Reset();
                initialized = false;
            }

            var settings = MonetizationSettings.Load();
            var products = settings.GetProducts();

            await Ads.InitializeAsync();

            IIapProvider provider;
            var selectedStore = settings.activeStore;

#if UNITY_EDITOR
            if (settings.useMockServicesInEditor)
                selectedStore = StoreProvider.Mock;
#endif

            switch (selectedStore)
            {
                case StoreProvider.Mock:
                    provider = new MockIapProvider();
                    break;

                case StoreProvider.GooglePlay:
#if STORE_GOOGLEPLAY
                    provider = new GooglePlayIapProvider(settings.googlePlay, selectedStore);
#else
                    throw MissingSdk("Google Play", "STORE_GOOGLEPLAY", "Unity IAP 5.x");
#endif
                    break;

                case StoreProvider.CafeBazaar:
#if STORE_CAFEBAZAAR
                    provider = new CafeBazaarIapProvider(settings.cafeBazaar, selectedStore);
#else
                    throw MissingSdk("Cafe Bazaar", "STORE_CAFEBAZAAR", "Poolakey Unity SDK");
#endif
                    break;

                case StoreProvider.Myket:
#if STORE_MYKET
                    provider = new MyketIapProvider(settings.myket, selectedStore);
#else
                    throw MissingSdk("Myket", "STORE_MYKET", "official Myket Billing Unity adapter");
#endif
                    break;

                case StoreProvider.ZarinpalLegacy:
#if PAY_ZARINPAL
                    provider = new ZarinpalProvider(settings.zarinpal);
#else
                    throw MissingSdk("Zarinpal legacy client flow", "PAY_ZARINPAL", "the legacy adapter");
#endif
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            await Iap.InitializeAsync(provider, products);
            initialized = true;
            Debug.Log($"[Monetization] Initialized. IAP={Iap.ProviderName}, Ads={Ads.NetworkCount}");
        }

        static InvalidOperationException MissingSdk(string provider, string symbol, string dependency)
        {
            return new InvalidOperationException(
                $"{provider} is selected, but its adapter is not compiled. Install {dependency}, then enable scripting symbol {symbol} from Monetization Settings.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void AutoInitialize()
        {
            try
            {
                var settings = MonetizationSettings.Load();
                if (!settings.initializeOnStartup)
                    return;

                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Monetization] Auto initialization failed: {ex}");
            }
        }
    }
}
