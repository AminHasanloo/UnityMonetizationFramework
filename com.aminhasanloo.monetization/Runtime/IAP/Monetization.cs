// SPDX-License-Identifier: MIT
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;

namespace AminHasanloo.Monetization
{
    public static class Monetization
    {
        public static async Task InitializeAsync()
        {
            var s = MonetizationSettings.Load();
            if (s.initializeOnStartup)
            {
                // Ads
                await Ads.Ads.InitializeAsync();

                // IAP
#if STORE_GOOGLEPLAY
                var gp = new IAP.Providers.GooglePlayIapProvider();
                IAP.Iap.Initialize(gp, s.productIds);
#endif
#if STORE_CAFEBAZAAR
                var bazaar = new IAP.Providers.CafeBazaarIapProvider(s.cafeBazaar.publicKey);
                IAP.Iap.Initialize(bazaar, s.productIds);
#endif
#if STORE_MYKET
                var myket = new IAP.Providers.MyketIapProvider(s.myket.publicKey);
                IAP.Iap.Initialize(myket, s.productIds);
#endif
#if PAY_ZARINPAL
                var zarin = new IAP.Providers.ZarinpalProvider(s.zarinpal.merchantId, s.zarinpal.callbackUrl, s.zarinpal.baseApiUrl);
                IAP.Iap.Initialize(zarin, s.productIds);
#endif
            }
        }
    }
}
