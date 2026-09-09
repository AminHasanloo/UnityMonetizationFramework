# Unity Monetization Framework v2

A modular IAP + Ads layer for Unity by **Amin Hasanloo**.

## v2.0 at a glance

- Editor-safe **Mock IAP + Mock Ads** for development without store/ad accounts.
- **Google Play** through Unity IAP 5.x `StoreController`.
- **Cafe Bazaar** through the official Poolakey Unity SDK.
- **AdMob** through the current full-screen ad lifecycle.
- **Tapsell Plus** adapter retained behind an optional scripting symbol.
- Exactly **one active IAP store** at runtime.
- Async initialization and restore APIs.
- Typed product catalog with store-specific ID overrides.
- Legacy fake Myket/Bazaar shims and unsafe direct-client Zarinpal flow removed.

> Myket, LevelPlay 9+ and secure server-backed Zarinpal are tracked in the root README roadmap rather than being presented as finished integrations.

## Quick start

1. Open **Window > Monetization > Settings**.
2. Create `Assets/Resources/MonetizationSettings.asset`.
3. Keep `Active Store = Mock` and `Use Mock Services In Editor = true`.
4. Add at least one product to the `Products` catalog.
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

## External SDKs

Only install the SDKs you actually enable:

- Google Play: Unity IAP 5.x is already declared as a package dependency.
- Cafe Bazaar: official [Poolakey Unity SDK](https://github.com/cafebazaar/PoolakeyUnitySdk).
- AdMob: current Google Mobile Ads Unity plugin.
- Tapsell: current official Tapsell Plus Unity plugin.

After installing an external provider SDK, configure the provider in **Window > Monetization > Settings** and click **Apply Android Scripting Define Symbols**.

## Security

For valuable currencies and server-authoritative economies, verify purchases on a backend and make fulfillment idempotent. Do not put payment gateway verification secrets or authoritative prices inside the shipped client.

## Full documentation & roadmap

See the repository root README:

**https://github.com/AminHasanloo/UnityMonetizationFramework**

## License

MIT
