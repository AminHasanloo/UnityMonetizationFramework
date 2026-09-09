// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AminHasanloo.Monetization.Ads;
using AminHasanloo.Monetization.IAP;
using UnityEngine;
using MonetizationFacade = AminHasanloo.Monetization.Monetization;

namespace AminHasanloo.Monetization.Samples.Validation
{
    /// <summary>
    /// Zero-art runtime dashboard for validating the public monetization facade in Editor
    /// and on provider test builds. It intentionally avoids UGUI/prefab dependencies.
    /// </summary>
    public sealed class MonetizationValidationDashboard : MonoBehaviour
    {
        [Header("Validation Inputs")]
        public string productId = "coins_100";
        public string placement = "validation";
        public bool initializeOnStart;

        readonly Queue<string> eventLog = new Queue<string>();
        Vector2 scroll;
        string lastPurchase = "-";
        string lastError = "-";
        int rewardCallbacks;
        bool subscribed;
        bool busy;

        void OnEnable() => Subscribe();
        void OnDisable() => Unsubscribe();

        void Start()
        {
            Append("Dashboard ready. Configure MonetizationSettings, then initialize.");
            if (initializeOnStart)
                _ = InitializeFrameworkAsync(false);
        }

        void Subscribe()
        {
            if (subscribed) return;
            Iap.OnPurchaseSucceeded += OnPurchaseSucceeded;
            Iap.OnPurchaseFailed += OnPurchaseFailed;
            subscribed = true;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            Iap.OnPurchaseSucceeded -= OnPurchaseSucceeded;
            Iap.OnPurchaseFailed -= OnPurchaseFailed;
            subscribed = false;
        }

        void OnPurchaseSucceeded(string id)
        {
            lastPurchase = id;
            Append($"IAP SUCCESS: {id}");
        }

        void OnPurchaseFailed(string id, string error)
        {
            lastError = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error;
            Append($"IAP FAIL: {id} | {lastError}");
        }

        async Task InitializeFrameworkAsync(bool force)
        {
            if (busy) return;
            busy = true;
            try
            {
                Append(force ? "Force reinitialize..." : "Initialize...");
                await MonetizationFacade.InitializeAsync(force);
                Append($"Initialized. IAP={Iap.ProviderName}, Ads={Ads.NetworkCount}");
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Append("INIT FAIL: " + ex.Message);
                Debug.LogException(ex);
            }
            finally
            {
                busy = false;
            }
        }

        async Task RestoreAsync()
        {
            try
            {
                Append("Restore requested...");
                await Iap.RestoreAsync();
                Append("Restore call completed.");
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Append("RESTORE FAIL: " + ex.Message);
                Debug.LogException(ex);
            }
        }

        void OnGUI()
        {
            var width = Mathf.Min(620f, Screen.width - 24f);
            var height = Mathf.Min(760f, Screen.height - 24f);
            GUILayout.BeginArea(new Rect(12f, 12f, width, height), "Monetization v2 Validation", GUI.skin.window);

            GUILayout.Label($"Framework: {(MonetizationFacade.IsInitialized ? "READY" : "NOT INITIALIZED")}");
            GUILayout.Label($"IAP Provider: {Iap.ProviderName} | IAP Ready: {Iap.IsInitialized}");
            GUILayout.Label($"Ad Networks: {Ads.NetworkCount} | Ads Ready: {Ads.IsInitialized}");
            GUILayout.Label($"Last Purchase: {lastPurchase}");
            GUILayout.Label($"Last Error: {lastError}");
            GUILayout.Label($"Reward Callbacks: {rewardCallbacks}");

            GUILayout.Space(8);
            GUILayout.Label("Inputs", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Product", GUILayout.Width(70));
            productId = GUILayout.TextField(productId ?? string.Empty);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Placement", GUILayout.Width(70));
            placement = GUILayout.TextField(placement ?? string.Empty);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("Lifecycle", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUI.enabled = !busy;
            if (GUILayout.Button("Initialize")) _ = InitializeFrameworkAsync(false);
            if (GUILayout.Button("Force Reinitialize")) _ = InitializeFrameworkAsync(true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("IAP", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Purchase"))
            {
                Append("Purchase requested: " + productId);
                Iap.Purchase(productId);
            }
            if (GUILayout.Button("Restore")) _ = RestoreAsync();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("Ads", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Rewarded")) { Ads.Load(AdType.Rewarded, placement); Append("Load Rewarded"); }
            if (GUILayout.Button("Show Rewarded"))
            {
                Ads.ShowRewarded(
                    placement,
                    reward =>
                    {
                        rewardCallbacks++;
                        Append($"REWARD: {reward.Amount} {reward.Type}");
                    },
                    () => Append("Rewarded closed"));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Interstitial")) { Ads.Load(AdType.Interstitial, placement); Append("Load Interstitial"); }
            if (GUILayout.Button("Show Interstitial")) Ads.ShowInterstitial(placement, () => Append("Interstitial closed"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Banner")) { Ads.Load(AdType.Banner, placement); Append("Load Banner"); }
            if (GUILayout.Button("Show Banner")) Ads.ShowBanner(placement);
            if (GUILayout.Button("Hide Banner")) Ads.HideBanner(placement);
            if (GUILayout.Button("Destroy Banner")) Ads.DestroyBanner(placement);
            GUILayout.EndHorizontal();

            GUILayout.Label(
                $"Ready: Rewarded={Ads.IsReady(AdType.Rewarded, placement)} | Interstitial={Ads.IsReady(AdType.Interstitial, placement)} | Banner={Ads.IsReady(AdType.Banner, placement)}");

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Event Log", GUI.skin.box);
            if (GUILayout.Button("Clear", GUILayout.Width(80))) eventLog.Clear();
            GUILayout.EndHorizontal();

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            foreach (var entry in eventLog)
                GUILayout.Label(entry);
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        void Append(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            eventLog.Enqueue(line);
            while (eventLog.Count > 24)
                eventLog.Dequeue();
            Debug.Log("[Monetization/Validation] " + message);
        }
    }
}
