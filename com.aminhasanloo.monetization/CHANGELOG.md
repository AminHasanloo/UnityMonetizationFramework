# Changelog

## 2.0.0 - Production-focused rewrite

### Added
- Editor-safe Mock IAP provider and Mock Ads network so the package can be exercised without store/ad SDK accounts.
- Async IAP lifecycle (`InitializeAsync`, `RestoreAsync`) and retained event subscriptions registered before initialization.
- Explicit single-store routing with `StoreProvider` to prevent providers overwriting one another.
- Typed product catalog with consumable, non-consumable, subscription and optional store-specific IDs.
- Automatic runtime bootstrap when `initializeOnStartup` is enabled.
- Ad loading/readiness helpers and deterministic network initialization.

### Updated
- Google Play adapter migrated from Unity IAP v4 `IStoreListener` / `ConfigurationBuilder` to Unity IAP 5.x `StoreController`.
- AdMob adapter migrated to the current static `Load()`, `CanShowAd()` and rewarded `Show(Action<Reward>)` lifecycle.
- Cafe Bazaar provider replaced with a real adapter for the official Poolakey Unity SDK.
- Settings window rebuilt around one active store and safer scripting define management.
- Demo sample updated to the v2 async API and mock-first Editor workflow.
- UPM metadata updated and Unity IAP 5.x added as a package dependency.

### Security / correctness
- Removed the v1 fake Cafe Bazaar and Myket placeholder APIs that could compile without performing real billing.
- Retired the direct client-side Zarinpal payment verification path. Production gateway verification must be server-authoritative.
- Retired the legacy `IronSource.Agent` adapter because LevelPlay 9+ requires the newer Init and Ad Unit APIs.

### Migration notes
- `productIds` is retained as a hidden legacy field and is converted to consumable products when the new `products` catalog is empty.
- Myket and LevelPlay are intentionally not auto-enabled in v2.0; clean adapters are listed in the README roadmap.
- Store/ad SDK integrations still require their official vendor packages, dashboard configuration and real-device testing before release.

## 1.0.0 - Initial release
- ScriptableObject settings for Ads & IAP.
- Initial AdMob, Tapsell and legacy IronSource adapters.
- Initial Google Play, Cafe Bazaar, Myket and Zarinpal provider concepts.
- Editor settings window and sample scripts.
