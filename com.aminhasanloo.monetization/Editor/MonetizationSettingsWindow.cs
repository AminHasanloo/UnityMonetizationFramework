// SPDX-License-Identifier: MIT
// Author: Amin Hasanloo
#if UNITY_EDITOR
using System.Collections.Generic;
using AminHasanloo.Monetization.Settings;
using UnityEditor;
using UnityEngine;

namespace AminHasanloo.Monetization.Editor
{
    public class MonetizationSettingsWindow : EditorWindow
    {
        MonetizationSettings settings;
        Vector2 scroll;

        [MenuItem("Window/Monetization/Settings")]
        public static void Open() => GetWindow<MonetizationSettingsWindow>("Monetization Settings").Show();

        void OnEnable() => settings = Resources.Load<MonetizationSettings>("MonetizationSettings");

        void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a persistent settings asset at Assets/Resources/MonetizationSettings.asset. Runtime defaults are mock-only and are not saved.",
                    MessageType.Info);

                if (GUILayout.Button("Create Settings Asset"))
                    CreateSettingsAsset();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            var so = new SerializedObject(settings);
            so.Update();

            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("initializeOnStartup"));
            EditorGUILayout.PropertyField(so.FindProperty("useMockServicesInEditor"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("IAP Store", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("activeStore"));
            EditorGUILayout.HelpBox(
                "v2 activates exactly one store at runtime. This prevents the v1 bug where multiple compile symbols could overwrite the active provider.",
                MessageType.Info);

            switch (settings.activeStore)
            {
                case StoreProvider.GooglePlay:
                    EditorGUILayout.PropertyField(so.FindProperty("googlePlay"), true);
                    EditorGUILayout.HelpBox("Requires Unity In-App Purchasing 5.x before enabling STORE_GOOGLEPLAY.", MessageType.Info);
                    break;
                case StoreProvider.CafeBazaar:
                    EditorGUILayout.PropertyField(so.FindProperty("cafeBazaar"), true);
                    EditorGUILayout.HelpBox("Requires the official Cafe Bazaar Poolakey Unity SDK before enabling STORE_CAFEBAZAAR.", MessageType.Info);
                    break;
                case StoreProvider.Myket:
                    EditorGUILayout.PropertyField(so.FindProperty("myket"), true);
                    EditorGUILayout.HelpBox("Myket v1 fake shim was removed. The official adapter is planned for v2.1 and is not enabled by this window.", MessageType.Warning);
                    break;
                case StoreProvider.ZarinpalLegacy:
                    EditorGUILayout.PropertyField(so.FindProperty("zarinpal"), true);
                    EditorGUILayout.HelpBox("Direct client-side Zarinpal verification is intentionally disabled. Use a backend checkout flow.", MessageType.Error);
                    break;
                default:
                    EditorGUILayout.HelpBox("Mock IAP works in Editor without a store SDK.", MessageType.None);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Product Catalog", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("products"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ads", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("adMob"), true);
            EditorGUILayout.PropertyField(so.FindProperty("tapsell"), true);
            EditorGUILayout.PropertyField(so.FindProperty("levelPlay"), true);
            EditorGUILayout.HelpBox(
                "AdMob is the validated v2 adapter path. Tapsell is waiting for a response-ID lifecycle migration, and LevelPlay is waiting for the 9+ Ad Unit API adapter. Neither legacy symbol is enabled automatically.",
                MessageType.Info);

            so.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(settings);

            EditorGUILayout.Space(12);
            if (GUILayout.Button("Apply Android Scripting Define Symbols", GUILayout.Height(32)))
                ApplyDefines(settings);

            if (GUILayout.Button("Ping Settings Asset"))
                EditorGUIUtility.PingObject(settings);

            EditorGUILayout.EndScrollView();
        }

        void CreateSettingsAsset()
        {
            const string resources = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resources))
                AssetDatabase.CreateFolder("Assets", "Resources");

            settings = CreateInstance<MonetizationSettings>();
            AssetDatabase.CreateAsset(settings, resources + "/MonetizationSettings.asset");
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(settings);
        }

        static void ApplyDefines(MonetizationSettings s)
        {
            var raw = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            var defines = new HashSet<string>(raw.Split(';'));
            defines.Remove(string.Empty);

            SetDefine(defines, "AD_ADMOB", s.adMob.enabled);

            // Retired legacy ad symbols are deliberately removed until clean v2 adapters land.
            defines.Remove("AD_TAPSELL");
            defines.Remove("AD_IRONSOURCE");
            defines.Remove("AD_LEVELPLAY");

            SetDefine(defines, "STORE_GOOGLEPLAY",
                s.activeStore == StoreProvider.GooglePlay && s.googlePlay.enabled);
            SetDefine(defines, "STORE_CAFEBAZAAR",
                s.activeStore == StoreProvider.CafeBazaar && s.cafeBazaar.enabled);

            // Unsupported/unsafe v1 IAP adapters are never enabled by the editor UI.
            defines.Remove("STORE_MYKET");
            defines.Remove("PAY_ZARINPAL");

            var symbols = string.Join(";", defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);
            Debug.Log("[Monetization] Applied Android define symbols: " + symbols);
        }

        static void SetDefine(HashSet<string> set, string symbol, bool enabled)
        {
            if (enabled) set.Add(symbol);
            else set.Remove(symbol);
        }
    }
}
#endif
