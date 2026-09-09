// SPDX-License-Identifier: MIT
#if PAY_ZARINPAL
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Settings;

namespace AminHasanloo.Monetization.IAP
{
    /// <summary>
    /// Migration guard for the legacy v1 direct Zarinpal client flow.
    /// Merchant credentials, authoritative pricing and payment verification do not belong
    /// in a shipped game client. v2 deliberately refuses to run that insecure flow.
    /// </summary>
    public sealed class ZarinpalProvider : IIapProvider
    {
        public bool IsInitialized => false;
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public ZarinpalProvider(ZarinpalSettings settings) { }

        public Task InitializeAsync(IReadOnlyList<IapProductDefinition> products)
        {
            throw new NotSupportedException(
                "The v1 client-side Zarinpal flow was retired for security. Use a server-authoritative checkout endpoint; see the README roadmap.");
        }

        public void Purchase(string productId) =>
            PurchaseFailed?.Invoke(productId, "Zarinpal direct-client checkout is disabled in v2.0.");

        public Task RestoreAsync() => Task.CompletedTask;
        public void Dispose() { }
    }
}
#endif
