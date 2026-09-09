// SPDX-License-Identifier: MIT
using AminHasanloo.Monetization.Ads;
using AminHasanloo.Monetization.IAP;
using AminHasanloo.Monetization.Settings;
using NUnit.Framework;
using UnityEngine;

namespace AminHasanloo.Monetization.Tests
{
    public sealed class MonetizationCoreTests
    {
        [Test]
        public void ProductDefinition_UsesStoreSpecificIdAndFallsBackToCanonicalId()
        {
            var product = new IapProductDefinition
            {
                id = "coins_100",
                googlePlayId = "coins_100_gp",
                cafeBazaarId = "coins_100_bazaar"
            };

            Assert.AreEqual("coins_100_gp", product.GetStoreId(StoreProvider.GooglePlay));
            Assert.AreEqual("coins_100_bazaar", product.GetStoreId(StoreProvider.CafeBazaar));
            Assert.AreEqual("coins_100", product.GetStoreId(StoreProvider.Myket));
            Assert.AreEqual("coins_100", product.GetStoreId(StoreProvider.Mock));
        }

        [Test]
        public void LegacyCatalog_IsMigratedToConsumablesWhenV2CatalogIsEmpty()
        {
            var settings = ScriptableObject.CreateInstance<MonetizationSettings>();
            try
            {
                settings.products = System.Array.Empty<IapProductDefinition>();
                settings.productIds = new[] { "legacy_coins", "legacy_gems" };

                var migrated = settings.GetProducts();

                Assert.AreEqual(2, migrated.Length);
                Assert.AreEqual("legacy_coins", migrated[0].id);
                Assert.AreEqual(IapProductType.Consumable, migrated[0].type);
                Assert.AreEqual("legacy_gems", migrated[1].id);
                Assert.AreEqual(IapProductType.Consumable, migrated[1].type);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void MockIap_KnownProduct_FiresSuccessExactlyOnce()
        {
            var provider = new MockIapProvider();
            var successCount = 0;
            string purchasedId = null;

            provider.PurchaseSucceeded += id =>
            {
                successCount++;
                purchasedId = id;
            };

            provider.InitializeAsync(new[]
            {
                new IapProductDefinition { id = "coins_100", type = IapProductType.Consumable }
            }).GetAwaiter().GetResult();

            provider.Purchase("coins_100");

            Assert.AreEqual(1, successCount);
            Assert.AreEqual("coins_100", purchasedId);
            provider.Dispose();
        }

        [Test]
        public void MockIap_UnknownProduct_FiresFailureAndNeverSuccess()
        {
            var provider = new MockIapProvider();
            var successCount = 0;
            var failureCount = 0;
            string failedId = null;
            string error = null;

            provider.PurchaseSucceeded += _ => successCount++;
            provider.PurchaseFailed += (id, message) =>
            {
                failureCount++;
                failedId = id;
                error = message;
            };

            provider.InitializeAsync(new[]
            {
                new IapProductDefinition { id = "coins_100" }
            }).GetAwaiter().GetResult();

            provider.Purchase("missing_product");

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(1, failureCount);
            Assert.AreEqual("missing_product", failedId);
            StringAssert.Contains("Unknown product", error);
            provider.Dispose();
        }

        [Test]
        public void MockRewardedAd_FiresOneRewardAndOneCloseCallback()
        {
            var network = new MockAdNetwork();
            var rewardCount = 0;
            var closeCount = 0;
            Reward reward = default;

            network.InitializeAsync().GetAwaiter().GetResult();
            network.Load(AdType.Rewarded, "validation_reward");

            Assert.IsTrue(network.IsReady(AdType.Rewarded, "validation_reward"));

            network.Show(
                AdType.Rewarded,
                "validation_reward",
                value =>
                {
                    rewardCount++;
                    reward = value;
                },
                () => closeCount++);

            Assert.AreEqual(1, rewardCount);
            Assert.AreEqual(1, closeCount);
            Assert.AreEqual("mock_reward", reward.Type);
            Assert.AreEqual(1d, reward.Amount);
        }
    }
}
