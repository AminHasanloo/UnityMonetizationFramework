// SPDX-License-Identifier: MIT
// v2.0 migration note:
// The old adapter used obsolete/non-existent convenience methods and treated a zone ID
// as if it were the response ID returned by Tapsell Plus. Current Tapsell Plus requires
// Request*Ad(zoneId) -> responseId -> Show*Ad(responseId).
//
// To avoid shipping a misleading adapter, v2.0 disables Tapsell until the response-ID
// lifecycle is implemented and device-tested. See the root README roadmap.

namespace AminHasanloo.Monetization.Ads
{
    internal static class LegacyTapsellAdapterRetired
    {
        public const string Message =
            "Tapsell v1 adapter retired in Monetization v2.0; response-ID API adapter is planned for v2.1.";
    }
}
