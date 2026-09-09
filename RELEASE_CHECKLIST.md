# v2.0.0 Release Checklist

Use this checklist for the first stable Unity Monetization Framework v2 release.

A stable tag should represent **tested behavior**, not just merged code.

---

## Core package

- [ ] `main` contains the intended v2.0 code.
- [ ] `package.json` version is `2.0.0`.
- [ ] Unity minimum version matches the README.
- [ ] Unity IAP dependency matches the README and validation docs.
- [ ] Package Sanity GitHub Action is green on the release commit.
- [ ] Package installs through Git URL with `?path=/com.aminhasanloo.monetization`.
- [ ] No compile errors with no optional ad/store SDK symbols enabled.

## Editor / mock validation

- [ ] `AminHasanloo.Monetization.EditorTests` passes in Unity Test Runner.
- [ ] Validation Dashboard sample imports correctly.
- [ ] Validation scene can be generated from `Tools > Monetization > Create Validation Scene`.
- [ ] Mock purchase success path passes.
- [ ] Mock unknown-product failure path passes.
- [ ] Mock rewarded callback passes.
- [ ] Mock interstitial close callback passes.
- [ ] Mock banner load/show/hide/destroy passes.
- [ ] Force reinitialize does not duplicate callbacks.

## Google Play

- [ ] Internal test build installs from Google Play.
- [ ] Product metadata loads.
- [ ] Consumable purchase succeeds.
- [ ] User cancellation does not grant content.
- [ ] Non-consumable purchase succeeds.
- [ ] Non-consumable restore/relaunch path succeeds.
- [ ] Subscription flow validated if advertised for this release.
- [ ] Failure/network scenarios do not grant content.
- [ ] Validation issue linked below.

Validation issue: _TBD_

## Cafe Bazaar / Poolakey

- [ ] Official Poolakey Unity SDK installed.
- [ ] Poolakey connection succeeds on a Bazaar-compatible device.
- [ ] Consumable purchase succeeds.
- [ ] Consumable consume succeeds when configured.
- [ ] Cancellation/failure does not grant content.
- [ ] Non-consumable restore succeeds.
- [ ] Subscription restore validated if advertised.
- [ ] Validation issue linked below.

Validation issue: _TBD_

## AdMob

- [ ] Official Google Mobile Ads Unity plugin installed.
- [ ] Official test IDs are used during validation.
- [ ] Rewarded load / ready / show / reward / close / reload passes.
- [ ] Interstitial load / ready / show / close / reload passes.
- [ ] Banner load readiness, show, hide and destroy pass.
- [ ] Failed load/show cannot leave a stale ready state.
- [ ] Validation issue linked below.

Validation issue: _TBD_

## Security / correctness

- [ ] Production build cannot silently fall back to Mock when settings are missing.
- [ ] Fake Myket/Bazaar v1 placeholder implementations remain removed.
- [ ] Legacy client-side Zarinpal verification remains removed.
- [ ] Legacy IronSource/Tapsell code is not presented as production-ready.
- [ ] Valuable virtual currency is not described as server-secure unless backend verification is actually implemented.
- [ ] No private keys, merchant secrets, production purchase tokens or private receipts are committed.

## Documentation

- [ ] Root README provider matrix matches actual validation evidence.
- [ ] `VALIDATION.md` reflects the tested Unity, SDK and device versions.
- [ ] `CHANGELOG.md` includes all v2.0 changes.
- [ ] Installation snippet points to the correct package path.
- [ ] Roadmap clearly separates implemented features from planned features.
- [ ] Known limitations are explicit.

## Release packaging

- [ ] Decide: stable `v2.0.0` or pre-release `v2.0.0-preview.1` based on validation evidence.
- [ ] Create annotated Git tag from the exact validated commit.
- [ ] Create GitHub Release with concise release notes.
- [ ] Test installing the package from the tag, not only `main`.
- [ ] Update README install example to show the stable tag after release.
- [ ] Verify the release page, package metadata and changelog all show the same version.

---

## Release decision

**Stable release rule:** Google Play, Cafe Bazaar and AdMob device/store validation reports are linked and passing.

If one or more advertised provider paths are still unverified on a real environment, use a GitHub **pre-release** instead of a stable tag.
