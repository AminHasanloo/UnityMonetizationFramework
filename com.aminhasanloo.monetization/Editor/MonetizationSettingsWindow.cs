// SPDX-License-Identifier: MIT
// Author: Amin Hasanloo
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AminHasanloo.Monetization.Settings;
using System.Collections.Generic;

namespace AminHasanloo.Monetization.Editor
{
    public class MonetizationSettingsWindow : EditorWindow
    {
        private MonetizationSettings settings;

        [MenuItem("Window/Monetization/Settings")]
        public static void Open()
        {
            GetWindow<MonetizationSettingsWindow>("Monetization Settings").Show();
        }

        private void OnEnable()
        {
            settings = MonetizationSettings.Load();
        }

        private Vector2 scroll;
        private void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Create a MonetizationSettings asset via Create > Amin Hasanloo > Monetization Settings and place it under a Resources/ folder as 'MonetizationSettings'.", MessageType.Info);
                if (GUILayout.Button("Create In Project/Resources"))
                {
                    var path = "Assets/Resources";
                    if (!AssetDatabase.IsValidFolder(path))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    var asset = CreateInstance<MonetizationSettings>();
                    AssetDatabase.CreateAsset(asset, $"{path}/MonetizationSettings.asset");
                    AssetDatabase.SaveAssets();
                    settings = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            SerializedObject so = new SerializedObject(settings);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("initializeOnStartup"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stores", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("googlePlay"), true);
            EditorGUILayout.PropertyField(so.FindProperty("cafeBazaar"), true);
            EditorGUILayout.PropertyField(so.FindProperty("myket"), true);
            EditorGUILayout.PropertyField(so.FindProperty("zarinpal"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ads", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("adMob"), true);
            EditorGUILayout.PropertyField(so.FindProperty("tapsell"), true);
            EditorGUILayout.PropertyField(so.FindProperty("ironSource"), true);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(so.FindProperty("productIds"), true);

            so.ApplyModifiedProperties();

            if (GUILayout.Button("Apply Scripting Define Symbols"))
            {
                ApplyDefines(settings);
            }

            EditorGUILayout.EndScrollView();
        }

        static void ApplyDefines(MonetizationSettings s)
        {
            var defines = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).Split(';'));
            // Ads
            SetDefine(defines, "AD_ADMOB", s.adMob.enabled);
            SetDefine(defines, "AD_TAPSELL", s.tapsell.enabled);
            SetDefine(defines, "AD_IRONSOURCE", s.ironSource.enabled);
            // Stores
            SetDefine(defines, "STORE_GOOGLEPLAY", s.googlePlay.enabled);
            SetDefine(defines, "STORE_CAFEBAZAAR", s.cafeBazaar.enabled);
            SetDefine(defines, "STORE_MYKET", s.myket.enabled);
            SetDefine(defines, "PAY_ZARINPAL", s.zarinpal.enabled);

            var symbols = string.Join(";", defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);
            Debug.Log("Applied scripting define symbols: " + symbols);
        }

        static void SetDefine(HashSet<string> set, string symbol, bool enabled)
        {
            if (enabled) set.Add(symbol); else set.Remove(symbol);
        }
    }
}
#endif
