# Monetization Validation Dashboard

This sample is a zero-art, runtime validation panel for **Unity Monetization Framework v2**.

It is intentionally simple: no prefab, no custom fonts, no UI package dependency. The dashboard uses `OnGUI` so it can be imported into almost any Unity 6 test project and exercised immediately.

## Setup

1. Create `Assets/Resources/MonetizationSettings.asset` from **Window > Monetization > Settings**.
2. For the first run, keep:
   - `Active Store = Mock`
   - `Use Mock Services In Editor = true`
3. Add at least one product, for example `coins_100`.
4. Use **Tools > Monetization > Create Validation Scene**.
5. Save the new scene.
6. Enter Play Mode.

## What the dashboard exposes

- initialize / force reinitialize
- purchase / restore
- rewarded load / show
- interstitial load / show
- banner load / show / hide / destroy
- current framework and provider state
- ad readiness
- last purchase / last error
- reward callback count
- recent event log

## External provider testing

The same dashboard can be used in a device build after the relevant official SDK and provider settings are installed.

Do not call a provider validated until its real test environment passes the relevant checklist in the repository root `VALIDATION.md`.
