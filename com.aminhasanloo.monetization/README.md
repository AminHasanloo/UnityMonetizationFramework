# Unity Monetization Framework v2

A modular IAP + Ads layer for **Unity 6** by **Amin Hasanloo**.

## v2.0 status

- ✅ Editor-safe **Mock IAP + Mock Ads**
- ✅ **Google Play** via Unity IAP **5.0.4**
- ✅ **Cafe Bazaar** via the official Poolakey API
- ✅ **AdMob** current full-screen ad lifecycle
- ✅ Single active IAP store, async initialization, typed product catalog
- 🧭 Tapsell Plus, Myket and LevelPlay are roadmap integrations, not fake/stale adapters
- 🔒 Direct client-side Zarinpal verification has been removed in favor of a future backend flow

## Quick start

1. Open **Window > Monetization > Settings**.
2. Create `Assets/Resources/MonetizationSettings.asset`.
3. Keep `Active Store = Mock` and `Use Mock Services In Editor = true`.
4. Add at least one product to `Products`.
5. Import the **Monetization Demo** sample or initialize from code.

```csharp
using AminHasanloo.Monetization;
using AminHasanloo.Monetization.Ads;
using AminHasanloo.Monetization.IAP;
using UnityEngine;

public class Demo : MonoBehaviour
{
    async void Start()
    {
        Iap.OnPurchaseSucceeded += id => Debug.Log($"Purchased: {id}");
        Iap.OnPurchaseFailed += (id, error) => Debug.LogError(error);

        await Monetization.InitializeAsync();
        Ads.Load(AdType.Rewarded, "reward_default");
    }

    public void Buy() => Iap.Purchase("coins_100");

    public void Rewarded() => Ads.ShowRewarded(
        "reward_default",
        reward => Debug.Log($"Reward: {reward.Amount} {reward.Type}"));
}
```

The default Editor mock lets you build and test shop UI, purchase callbacks and rewarded-ad gameplay without external SDK accounts.

## External SDKs

Only install what you use:

- **Google Play:** Unity IAP 5.0.4 is declared as a package dependency.
- **Cafe Bazaar:** official [Poolakey Unity SDK](https://github.com/cafebazaar/PoolakeyUnitySdk).
- **AdMob:** current official Google Mobile Ads Unity plugin.

After installing an external SDK, configure it in **Window > Monetization > Settings** and apply Android scripting symbols.

## Intentionally not enabled in v2.0

- **Tapsell Plus:** requires a clean current response-ID adapter.
- **Myket:** requires a real official Billing adapter.
- **Unity LevelPlay:** requires the LevelPlay 9+ Init + Ad Unit API.
- **Zarinpal:** should use a server-owned checkout and verification flow.

## Production note

Official SDK integrations still need provider dashboard configuration, valid test accounts/IDs and real-device/store testing. For valuable currencies or competitive economies, make purchase fulfillment server-authoritative and idempotent.

## Full docs & roadmap

See the repository root README:

**https://github.com/AminHasanloo/UnityMonetizationFramework**

## License

MIT
