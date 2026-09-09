# Changelog

## 2.0.0 - Production-focused rewrite

### Added
- Editor-safe Mock IAP provider and Mock Ads network so purchase/ad gameplay can be tested without store or ad accounts.
- Async IAP lifecycle with `InitializeAsync` and `RestoreAsync`.
- Persistent facade-level purchase event subscriptions, including listeners registered before provider initialization.
- Explicit single-store routing with `StoreProvider`.
- Typed product catalog with Consumable, NonConsumable, Subscription and optional store-specific IDs.
- Real runtime auto-bootstrap when `initializeOnStartup` is enabled.
- Ad load/readiness/show helpers.

### Updated
- Package target moved to **Unity 6 / 6000.0+**.
- Unity IAP dependency updated to **5.0.4**.
- Google Play adapter migrated from v4 `IStoreListener` / `ConfigurationBuilder` to the Unity IAP 5 `StoreController` flow.
- Google Play now registers store, product-fetch, purchase-fetch and purchase-failure callbacks before requests are made.
- AdMob adapter migrated to the current static `Load()`, `CanShowAd()` and rewarded `Show(Action<Reward>)` lifecycle.
- Cafe Bazaar replaced with a real binding to the official Poolakey `Payment` API.
- Settings window rebuilt around one active store and conservative scripting-symbol management.
- Demo sample updated to the v2 async API and mock-first Editor workflow.
- Root and package READMEs rewritten with setup, migration notes, security guidance and roadmap.

### Security / correctness
- Removed v1 fake Cafe Bazaar and Myket placeholder APIs.
- Removed the old direct client-side Zarinpal request/verify flow and hardcoded price behavior.
- Retired legacy `IronSource.Agent` integration because modern Unity LevelPlay uses the new Init and Ad Unit APIs.
- Retired the old Tapsell adapter because the current SDK requires a request response ID before showing an ad.
- Removed obsolete Android manifest proxy activities, Myket/Bazaar legacy permissions and Zarinpal deep-link shims. Official provider SDKs now own their manifest/dependency entries.

### Migration notes
- Legacy `productIds` is retained as a hidden field and is converted to consumable products when the new `products` catalog is empty.
- `STORE_MYKET`, `PAY_ZARINPAL`, `AD_TAPSELL`, `AD_IRONSOURCE` and `AD_LEVELPLAY` are intentionally not enabled by the v2.0 settings window.
- Google Play, Cafe Bazaar and AdMob still require provider-side configuration and real-device/store testing before release.

## 1.0.0 - Initial release
- ScriptableObject settings for Ads & IAP.
- Initial AdMob, Tapsell and legacy IronSource adapter concepts.
- Initial Google Play, Cafe Bazaar, Myket and Zarinpal provider concepts.
- Editor settings window and sample scripts.
