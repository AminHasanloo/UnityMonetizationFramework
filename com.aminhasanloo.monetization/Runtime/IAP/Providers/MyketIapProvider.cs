// SPDX-License-Identifier: MIT
#if STORE_MYKET
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// v1 included no-op placeholder Myket classes which could report a false sense of integration.
    /// Those shims are removed in v2. This adapter intentionally fails fast until the official
    /// Myket Billing Unity API is wired and device-tested.
    /// </summary>
    public sealed class MyketIapProvider : IIapProvider
    {
        public bool IsInitialized => false;
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public MyketIapProvider(MyketIapSettings settings, StoreProvider store) { }

        public Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            throw new NotSupportedException(
                "The fake v1 Myket shim was removed. Install the official Myket Billing Unity SDK and use the v2.1 Myket adapter from the roadmap before enabling STORE_MYKET.");
        }

        public void Purchase(string productId) =>
            PurchaseFailed?.Invoke(productId, "Myket adapter is not implemented in v2.0.");

        public Task RestoreAsync() => Task.CompletedTask;
        public void Dispose() { }
    }
}
#endif
