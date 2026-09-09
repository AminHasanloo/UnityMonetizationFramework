// SPDX-License-Identifier: MIT
// v2.0 migration note:
// The legacy IronSource.Agent integration was removed because Unity LevelPlay 9+
// requires the newer LevelPlay Init API and Ad Unit objects.
//
// This file intentionally contains no SDK references so projects carrying the old
// AD_IRONSOURCE symbol do not compile against stale APIs. Remove AD_IRONSOURCE and
// follow the LevelPlay item in the README roadmap for the new adapter.

namespace AminHasanloo.Monetization.Ads
{
    internal static class LegacyIronSourceAdapterRetired
    {
        public const string Message =
            "Legacy IronSource.Agent adapter retired in Monetization v2.0. Use the future LevelPlay Ad Unit adapter.";
    }
}
