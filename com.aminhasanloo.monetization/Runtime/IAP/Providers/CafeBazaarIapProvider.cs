// SPDX-License-Identifier: MIT
#if STORE_CAFEBAZAAR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;
using UnityEngine;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Cafe Bazaar adapter for the official Poolakey Unity SDK.
    /// Poolakey currently ships as source without an assembly definition in common installs,
    /// so the adapter binds to its public API at runtime. This keeps the UPM runtime asmdef
    /// isolated while still using the real Payment/Connect/Purchase/Consume/GetPurchases API.
    /// </summary>
    public sealed class CafeBazaarIapProvider : IIapProvider
    {
        readonly CafeBazaarIapSettings settings;
        readonly StoreProvider store;
        readonly Dictionary<string, IapProductDefinition> productsByCanonical = new Dictionary<string, IapProductDefinition>();
        readonly Dictionary<string, string> canonicalToStore = new Dictionary<string, string>();
        readonly Dictionary<string, string> storeToCanonical = new Dictionary<string, string>();

        object payment;
        Type paymentType;
        Type skuTypeEnum;

        public bool IsInitialized { get; private set; }
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public CafeBazaarIapProvider(CafeBazaarIapSettings settings, StoreProvider store)
        {
            this.settings = settings;
            this.store = store;
        }

        public async Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            if (!settings.enabled)
                throw new InvalidOperationException("Cafe Bazaar is selected but disabled in Monetization Settings.");

#if !UNITY_ANDROID && !UNITY_EDITOR
            throw new PlatformNotSupportedException("Cafe Bazaar Poolakey is supported on Android builds.");
#else
            BuildCatalog(products);
            BindPoolakey();

            var securityType = FindType("Bazaar.Poolakey.SecurityCheck");
            var paymentConfigurationType = FindType("Bazaar.Poolakey.PaymentConfiguration");

            var security = string.IsNullOrWhiteSpace(settings.publicKey)
                ? securityType.GetMethod("Disable", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null)
                : securityType.GetMethod("Enable", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { settings.publicKey });

            var configuration = Activator.CreateInstance(paymentConfigurationType, security);
            payment = Activator.CreateInstance(paymentType, configuration);

            var connect = FindMethod(paymentType, "Connect", 1);
            var result = await InvokeTaskResult(connect, payment, new object[] { null });
            if (!IsSuccess(result))
                throw new InvalidOperationException("Poolakey connection failed: " + GetMessage(result));

            IsInitialized = true;
            Debug.Log($"[Monetization/Bazaar] Poolakey connected with {productsByCanonical.Count} product(s).");
#endif
        }

        public async void Purchase(string productId)
        {
            if (!IsInitialized || payment == null)
            {
                PurchaseFailed?.Invoke(productId, "Poolakey is not initialized.");
                return;
            }

            if (!canonicalToStore.TryGetValue(productId, out var storeId))
            {
                PurchaseFailed?.Invoke(productId, "Product is not present in the monetization catalog.");
                return;
            }

            try
            {
                var definition = productsByCanonical[productId];
                var skuType = Enum.Parse(skuTypeEnum,
                    definition.type == IapProductType.Subscription ? "subscription" : "inApp");

                var purchaseMethod = FindMethod(paymentType, "Purchase", 6);
                var result = await InvokeTaskResult(purchaseMethod, payment,
                    new object[] { storeId, skuType, null, null, "", null });

                if (!IsSuccess(result))
                {
                    PurchaseFailed?.Invoke(productId, GetMessage(result));
                    return;
                }

                var purchaseInfo = GetField(result, "data");
                if (purchaseInfo == null)
                {
                    PurchaseFailed?.Invoke(productId, "Poolakey returned an empty purchase result.");
                    return;
                }

                if (definition.type == IapProductType.Consumable && settings.autoConsumeConsumables)
                {
                    var token = GetField(purchaseInfo, "purchaseToken") as string;
                    var consumeMethod = FindMethod(paymentType, "Consume", 2);
                    var consume = await InvokeTaskResult(consumeMethod, payment, new object[] { token, null });
                    if (!IsSuccess(consume))
                    {
                        PurchaseFailed?.Invoke(productId, "Purchase succeeded but consume failed: " + GetMessage(consume));
                        return;
                    }
                }

                PurchaseSucceeded?.Invoke(productId);
            }
            catch (Exception ex)
            {
                PurchaseFailed?.Invoke(productId, Unwrap(ex).Message);
            }
        }

        public async Task RestoreAsync()
        {
            if (!IsInitialized || payment == null) return;

            await RestoreType("inApp");
            await RestoreType("subscription");
        }

        async Task RestoreType(string typeName)
        {
            var skuType = Enum.Parse(skuTypeEnum, typeName);
            var getPurchases = FindMethod(paymentType, "GetPurchases", 2);
            var result = await InvokeTaskResult(getPurchases, payment, new object[] { skuType, null });
            if (!IsSuccess(result)) return;

            if (!(GetField(result, "data") is IEnumerable purchases)) return;
            foreach (var purchase in purchases)
            {
                var storeId = GetField(purchase, "productId") as string;
                if (string.IsNullOrWhiteSpace(storeId) || !storeToCanonical.TryGetValue(storeId, out var canonicalId))
                    continue;

                if (productsByCanonical.TryGetValue(canonicalId, out var definition) &&
                    definition.type != IapProductType.Consumable)
                    PurchaseSucceeded?.Invoke(canonicalId);
            }
        }

        void BuildCatalog(IReadOnlyList<IapProductDefinition> products)
        {
            productsByCanonical.Clear();
            canonicalToStore.Clear();
            storeToCanonical.Clear();

            if (products == null) return;
            foreach (var product in products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.id)) continue;
                var storeId = product.GetStoreId(store);
                productsByCanonical[product.id] = product;
                canonicalToStore[product.id] = storeId;
                storeToCanonical[storeId] = product.id;
            }
        }

        void BindPoolakey()
        {
            paymentType = FindType("Bazaar.Poolakey.Payment");
            skuTypeEnum = FindType("Bazaar.Poolakey.Data.SKUDetails+Type");
        }

        static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            throw new InvalidOperationException(
                $"Required Poolakey type '{fullName}' was not found. Install the official Cafe Bazaar Poolakey Unity SDK first.");
        }

        static MethodInfo FindMethod(Type type, string name, int parameterCount)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                if (method.Name == name && method.GetParameters().Length == parameterCount)
                    return method;
            throw new MissingMethodException(type.FullName, name);
        }

        static async Task<object> InvokeTaskResult(MethodInfo method, object instance, object[] args)
        {
            try
            {
                var task = method.Invoke(instance, args) as Task;
                if (task == null) throw new InvalidOperationException($"{method.Name} did not return a Task.");
                await task;
                return task.GetType().GetProperty("Result")?.GetValue(task);
            }
            catch (Exception ex)
            {
                throw Unwrap(ex);
            }
        }

        static bool IsSuccess(object result)
        {
            var status = GetField(result, "status");
            return status != null && string.Equals(status.ToString(), "Success", StringComparison.OrdinalIgnoreCase);
        }

        static string GetMessage(object result) => GetField(result, "message") as string ?? "Unknown Poolakey error.";

        static object GetField(object instance, string fieldName)
        {
            if (instance == null) return null;
            return instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance);
        }

        static Exception Unwrap(Exception ex) => ex is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : ex;

        public void Dispose()
        {
            if (payment != null)
            {
                try { FindMethod(paymentType, "Disconnect", 0).Invoke(payment, null); }
                catch (Exception ex) { Debug.LogWarning("[Monetization/Bazaar] Disconnect: " + Unwrap(ex).Message); }
            }
            payment = null;
            IsInitialized = false;
        }
    }
}
#endif
