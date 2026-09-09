# Unity Monetization Framework v2.0 Validation Pack

This document is the release-validation protocol for **v2.0.0**.

The project deliberately separates three kinds of confidence:

- ✅ **Passed**: evidence exists in CI or a recorded Unity/device run.
- 🟦 **Ready to run**: the test or tool is included, but has not been executed in the current environment.
- 🟡 **External validation required**: an official SDK, provider account, store track, dashboard configuration or physical device is required.
- 🔒 **Not part of v2.0**: intentionally disabled until a future production implementation exists.

A successful compile is useful evidence, but it is **not** proof that money, entitlements or ad rewards behave correctly on a production device.

---

## Current validation matrix

| Area | Automated/static | Unity Editor | Real device / store | v2.0 release gate |
|---|---:|---:|---:|---:|
| UPM metadata + migration guards | ✅ GitHub Action | N/A | N/A | Required |
| Mock IAP | 🟦 EditMode tests included | 🟦 Dashboard included | N/A | Required |
| Mock Ads | 🟦 EditMode tests included | 🟦 Dashboard included | N/A | Required |
| Google Play IAP | ✅ API contract reviewed for Unity IAP 5.4.3 | 🟦 Compile/run locally | 🟡 Internal test track required | Required for stable release |
| Cafe Bazaar | ✅ Adapter reviewed against official Poolakey API | 🟦 Compile/run with Poolakey installed | 🟡 Bazaar-compatible device/account required | Required for stable release |
| AdMob | ✅ Adapter reviewed against current load/show API | 🟦 Compile/run with official plugin | 🟡 Test ad IDs on device required | Required for stable release |
| Myket | 🔒 Fake v1 shim removed | 🔒 | 🔒 v2.1 roadmap | Not advertised as ready |
| Tapsell Plus | 🔒 Old adapter retired | 🔒 | 🔒 v2.1 roadmap | Not advertised as ready |
| Unity LevelPlay | 🔒 Legacy IronSource adapter retired | 🔒 | 🔒 v2.1 roadmap | Not advertised as ready |
| Zarinpal | 🔒 Unsafe client flow removed | 🔒 | 🔒 Backend roadmap | Not advertised as ready |

> Update this table only when evidence exists. For device/store validation, link the corresponding GitHub validation issue or test report.

---

## 1. Run the automated repository checks

The repository includes `.github/workflows/package-sanity.yml`.

It checks that:

- package metadata stays on the intended Unity 6 / Unity IAP v2 baseline;
- retired fake store shims do not return;
- unsafe legacy Zarinpal client-payment code does not return;
- obsolete proxy Android manifest code stays removed;
- the Validation Pack files remain present;
- the package exposes the Validation Dashboard sample;
- README security and roadmap language stays present.

A green result is the first gate, not the final gate.

---

## 2. Run EditMode tests in Unity

The package includes:

```text
com.aminhasanloo.monetization/Tests/Editor/
```

The tests cover the SDK-independent core:

- canonical and store-specific product IDs;
- legacy v1 product-catalog migration;
- Mock IAP successful purchase;
- Mock IAP unknown-product failure;
- Mock rewarded-ad reward + close callbacks.

### Make package tests visible

For a Git/UPM-installed package, add the package name to the **project** `Packages/manifest.json` testables list:

```json
{
  "testables": [
    "com.aminhasanloo.monetization"
  ]
}
```

Keep your existing `dependencies` object unchanged.

Then open:

```text
Window > General > Test Runner
```

Run all **EditMode** tests in `AminHasanloo.Monetization.EditorTests`.

### Pass criteria

- all included EditMode tests are green;
- there are no unexpected exceptions in the Console;
- no external provider SDK is required for these tests.

---

## 3. Import the Validation Dashboard sample

In Unity Package Manager, select **Amin Hasanloo Monetization (IAP + Ads)** and import:

```text
Validation Dashboard
```

The sample contains a zero-art validation panel and a scene-builder utility.

Use:

```text
Tools > Monetization > Create Validation Scene
```

The command creates an empty scene with a `MonetizationValidationDashboard` GameObject. Save it anywhere inside your project.

The dashboard exposes buttons for:

- Initialize / Force Reinitialize
- Purchase
- Restore
- Load Rewarded / Interstitial / Banner
- Show Rewarded / Interstitial / Banner
- Hide / Destroy Banner
- clear the validation event log

It also shows:

- framework initialized state;
- active IAP provider;
- ad-network count;
- last successful purchase;
- last purchase error;
- rewarded-ad callback count;
- recent lifecycle events.

---

## 4. Mock Editor validation

Recommended first run:

```text
Active Store = Mock
Use Mock Services In Editor = true
```

Create these products:

| ID | Type |
|---|---|
| `coins_100` | Consumable |
| `remove_ads` | NonConsumable |
| `vip_monthly` | Subscription |

### Checklist

- [ ] Initialize completes without exception.
- [ ] Provider name reports `MockIapProvider`.
- [ ] Purchase of `coins_100` fires success exactly once.
- [ ] Purchase of an unknown ID fires failure exactly once.
- [ ] Restore completes without exception.
- [ ] Rewarded load becomes ready.
- [ ] Rewarded show fires one reward callback and one close callback.
- [ ] Interstitial show fires close callback.
- [ ] Banner load/show/hide/destroy executes without exception.
- [ ] Force Reinitialize does not duplicate purchase callbacks.

If any item fails, do not start provider-device validation until the core regression is fixed.

---

## 5. Google Play validation

### Required environment

- Unity 6 project
- package dependency `com.unity.purchasing` pinned to the version declared by this package
- Android application configured in Google Play Console
- active products matching the Monetization catalog
- licensed tester / internal test track
- physical Android device with Google Play

### Scenarios

- [ ] Connect to store.
- [ ] Fetch product catalog.
- [ ] Consumable purchase success.
- [ ] User-cancel purchase failure path.
- [ ] Non-consumable purchase success.
- [ ] Subscription purchase success if subscriptions are part of the release claim.
- [ ] Relaunch and fetch existing purchases.
- [ ] Non-consumable/subscription entitlement is delivered idempotently.
- [ ] Unknown product is rejected by the framework.
- [ ] Network/store failure produces a useful error and does not grant content.

For valuable currency, also validate the future server-authoritative flow before using it in a competitive economy.

---

## 6. Cafe Bazaar / Poolakey validation

### Required environment

- official Cafe Bazaar Poolakey Unity SDK installed
- Android package configured for Bazaar
- Bazaar developer/test account
- matching product IDs
- RSA public key if Poolakey security checking is enabled
- Bazaar-compatible Android device/environment

### Scenarios

- [ ] Poolakey connects successfully.
- [ ] Consumable purchase succeeds.
- [ ] Consumable token is consumed when `autoConsumeConsumables` is enabled.
- [ ] Purchase cancellation/failure does not grant content.
- [ ] Non-consumable restore works.
- [ ] Subscription restore works if used by the game.
- [ ] Relaunch does not double-grant an entitlement.
- [ ] Invalid/missing SDK fails clearly.

---

## 7. AdMob validation

### Required environment

- current official Google Mobile Ads Unity plugin
- Android app configured for AdMob
- **official test ad unit IDs** during validation
- physical Android device

### Scenarios

- [ ] SDK initializes.
- [ ] Rewarded ad loads and `IsReady` becomes true.
- [ ] Reward is granted only from the rewarded callback.
- [ ] Closing a rewarded ad triggers close once.
- [ ] Used rewarded ad is destroyed/reloaded.
- [ ] Interstitial loads, shows, closes and reloads.
- [ ] Banner is not reported ready before load success.
- [ ] Banner show/hide/destroy works.
- [ ] Load/show failure does not leave a stale ready state.
- [ ] App resume does not duplicate callbacks.

Never use live ad IDs while repeatedly testing clicks/impressions.

---

## 8. Record evidence with a GitHub issue

Use the repository issue form:

```text
Provider validation report
```

Create one issue per provider + environment combination. Attach screenshots or short recordings where useful, but never post private keys, merchant secrets, purchase tokens or sensitive receipts.

Suggested issue naming:

```text
[Validation] Google Play / Unity 6000.x / Pixel 7 / PASS
[Validation] Cafe Bazaar / Unity 6000.x / Samsung A52 / PASS
[Validation] AdMob / GMA 11.x / Android / PASS
```

---

## Stable v2.0.0 exit criteria

Before creating the stable `v2.0.0` GitHub Release:

- [ ] Package Sanity workflow is green on the release commit.
- [ ] All package EditMode tests pass in Unity 6.
- [ ] Validation Dashboard imports and runs.
- [ ] Mock validation checklist passes.
- [ ] Google Play required scenarios have a linked PASS report.
- [ ] Cafe Bazaar required scenarios have a linked PASS report.
- [ ] AdMob required scenarios have a linked PASS report.
- [ ] README provider matrix matches actual evidence.
- [ ] No provider is called production-ready without real environment evidence.
- [ ] `RELEASE_CHECKLIST.md` is complete.

If device/store validation is incomplete, publish a pre-release such as `v2.0.0-preview.1` instead of labeling the package stable.
