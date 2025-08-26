// SPDX-License-Identifier: MIT
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using AminHasanloo.Monetization;

public class MonetizationDemoUI : MonoBehaviour
{
    public Button initButton;
    public Button rewardedButton;
    public Button interstitialButton;
    public Button bannerButton;
    public Button iapButton;

    async void Start()
    {
        if (initButton) initButton.onClick.AddListener(async () => await Init());
        if (rewardedButton) rewardedButton.onClick.AddListener(() => Ads.Ads.ShowRewarded("reward_default"));
        if (interstitialButton) interstitialButton.onClick.AddListener(() => Ads.Ads.ShowInterstitial("interstitial_default"));
        if (bannerButton) bannerButton.onClick.AddListener(() => Ads.Ads.ShowBanner("banner_default"));
        if (iapButton) iapButton.onClick.AddListener(() => AminHasanloo.Monetization.IAP.Iap.Purchase("coins_100"));
    }

    async Task Init()
    {
        await Monetization.InitializeAsync();
        Debug.Log("Monetization Initialized");
    }
}
