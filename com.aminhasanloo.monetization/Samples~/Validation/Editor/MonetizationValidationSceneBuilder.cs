// SPDX-License-Identifier: MIT
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AminHasanloo.Monetization.Samples.Validation.Editor
{
    public static class MonetizationValidationSceneBuilder
    {
        [MenuItem("Tools/Monetization/Create Validation Scene")]
        public static void CreateValidationScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("Monetization Validation Dashboard");
            go.AddComponent<MonetizationValidationDashboard>();

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                "[Monetization/Validation] Validation scene created. " +
                "Save the scene, configure Assets/Resources/MonetizationSettings.asset, then enter Play Mode.");
        }
    }
}
#endif
