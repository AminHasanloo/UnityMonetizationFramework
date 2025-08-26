// SPDX-License-Identifier: MIT
using UnityEngine;

namespace AminHasanloo.Monetization.Utils
{
    /// <summary>Optional: attach to a bootstrap scene to log incoming deep links (useful for Zarinpal).</summary>
    public class DeepLinkLogger : MonoBehaviour
    {
        void Awake()
        {
            Application.deepLinkActivated += url => Debug.Log("DeepLink: " + url);
        }
    }
}
