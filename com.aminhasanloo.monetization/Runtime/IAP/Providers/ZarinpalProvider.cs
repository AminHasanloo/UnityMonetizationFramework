// SPDX-License-Identifier: MIT
#if PAY_ZARINPAL
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AminHasanloo.Monetization.IAP.Providers
{
    /// <summary>
    /// Minimal REST client for Zarinpal v4 request/verify.
    /// ⚠️ توصیه: Verify را روی سرور خودتان انجام دهید.
    /// </summary>
    public class ZarinpalProvider : IIapProvider
    {
        private readonly string merchantId;
        private readonly string callbackUrl;
        private readonly string baseApiUrl;
        public ZarinpalProvider(string merchantId, string callbackUrl, string baseApiUrl = "https://payment.zarinpal.com")
        {
            this.merchantId = merchantId;
            this.callbackUrl = callbackUrl;
            this.baseApiUrl = baseApiUrl;
        }

        public bool IsInitialized { get; private set; }
        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        private string pendingProduct;

        public void Initialize(string[] productIds)
        {
            Application.deepLinkActivated += OnDeepLink;
            IsInitialized = true;
        }

        public void Purchase(string productId)
        {
            pendingProduct = productId;
            var amount = 1000; // TODO: map productId -> amount (Rials) via your own catalog
            var description = $"Purchase {productId}";
            var req = new PaymentRequest { merchant_id = merchantId, amount = amount, callback_url = callbackUrl, description = description };
            CoroutineRunner.Run(RequestPayment(req, (ok, authority, msg) =>
            {
                if (ok)
                {
                    var url = $"{baseApiUrl}/pg/StartPay/{authority}";
                    Application.OpenURL(url);
                }
                else
                {
                    OnPurchaseFailed?.Invoke(productId, msg);
                }
            }));
        }

        public void Restore() { /* N/A for direct gateway */ }

        private void OnDeepLink(string url)
        {
            // ex: myapp://zarinpal?Authority=xxx&Status=OK
            var uri = new Uri(url);
            var query = UnityEngine.WWW.UnEscapeURL(uri.Query);
            var status = GetQuery(query, "Status");
            var authority = GetQuery(query, "Authority");
            if (status != "OK")
            {
                OnPurchaseFailed?.Invoke(pendingProduct, "Status=" + status);
                return;
            }
            CoroutineRunner.Run(VerifyPayment(new VerifyRequest { merchant_id = merchantId, amount = 1000, authority = authority }, (ok, refId, msg) =>
            {
                if (ok) OnPurchaseSucceeded?.Invoke(pendingProduct);
                else OnPurchaseFailed?.Invoke(pendingProduct, msg);
            }));
        }

        static string GetQuery(string query, string key)
        {
            foreach (var part in query.TrimStart('?').Split('&'))
            {
                var kv = part.Split('=');
                if (kv.Length == 2 && kv[0] == key) return kv[1];
            }
            return null;
        }

        [Serializable] class PaymentRequest { public string merchant_id; public int amount; public string callback_url; public string description; }
        [Serializable] class VerifyRequest { public string merchant_id; public int amount; public string authority; }

        private IEnumerator RequestPayment(PaymentRequest req, Action<bool, string, string> cb)
        {
            var json = JsonUtility.ToJson(req);
            var url = $"{baseApiUrl}/pg/v4/payment/request.json";
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("Accept", "application/json");
                yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    cb(false, null, uwr.error);
                }
                else
                {
                    var text = uwr.downloadHandler.text;
                    // naive parse
                    if (text.Contains("\"code\": 100"))
                    {
                        var idx = text.IndexOf("\"authority\":");
                        var auth = "";
                        if (idx >= 0)
                        {
                            var start = text.IndexOf('"', idx + 12) + 1;
                            var end = text.IndexOf('"', start);
                            auth = text.Substring(start, end - start);
                        }
                        cb(true, auth, null);
                    }
                    else cb(false, null, text);
                }
            }
        }

        private IEnumerator VerifyPayment(VerifyRequest req, Action<bool, long, string> cb)
        {
            var json = JsonUtility.ToJson(req);
            var url = $"{baseApiUrl}/pg/v4/payment/verify.json";
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("Accept", "application/json");
                yield return uwr.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    cb(false, 0, uwr.error);
                }
                else
                {
                    var text = uwr.downloadHandler.text;
                    if (text.Contains("\"code\": 100"))
                    {
                        // parse ref_id
                        long refId = 0;
                        var idx = text.IndexOf("\"ref_id\":");
                        if (idx >= 0)
                        {
                            var start = text.IndexOf(':', idx) + 1;
                            var end = text.IndexOf(',', start);
                            long.TryParse(text.Substring(start, end - start), out refId);
                        }
                        cb(true, refId, null);
                    }
                    else cb(false, 0, text);
                }
            }
        }
    }

    /// <summary>Utility to run coroutines from non-Mono classes.</summary>
    public class CoroutineRunner : MonoBehaviour
    {
        static CoroutineRunner _instance;
        public static void Ensure()
        {
            if (_instance == null)
            {
                var go = new GameObject("[Monetization.CoroutineRunner]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CoroutineRunner>();
            }
        }
        public static void Run(IEnumerator routine)
        {
            Ensure(); _instance.StartCoroutine(routine);
        }
    }
}
#endif
