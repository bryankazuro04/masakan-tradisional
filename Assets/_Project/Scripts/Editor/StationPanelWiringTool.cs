using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MasakanTradisional.Gameplay.Stations;

namespace MasakanTradisional.Editor
{
    public static class StationPanelWiringTool
    {
        [MenuItem("Tools/Wire Station Panels")]
        public static void WirePanels()
        {
            string prefabPath = "Assets/_Project/Prefabs/CollectibleItemView.prefab";
            CollectibleItemView itemPrefab = AssetDatabase.LoadAssetAtPath<CollectibleItemView>(prefabPath);

            if (itemPrefab == null)
            {
                Debug.LogError($"[StationPanelWiringTool] Could not find CollectibleItemView prefab at {prefabPath}");
                return;
            }

            Scene currentScene = SceneManager.GetActiveScene();

            string[] panelNames = new string[] { "ShelvesPanel", "FridgePanel", "CounterPanel" };

            foreach (string panelName in panelNames)
            {
                GameObject panelGo = FindGameObjectInScene(currentScene, panelName);
                if (panelGo == null)
                {
                    Debug.LogError($"[StationPanelWiringTool] GameObject '{panelName}' not found in scene '{currentScene.name}'!");
                    continue;
                }

                // 1. Find or create ItemContainer child
                Transform containerTrans = panelGo.transform.Find("ItemContainer");
                GameObject containerGo;

                if (containerTrans == null)
                {
                    containerGo = new GameObject("ItemContainer", typeof(RectTransform));
                    containerGo.transform.SetParent(panelGo.transform, false);
                    Undo.RegisterCreatedObjectUndo(containerGo, $"Create ItemContainer in {panelName}");
                }
                else
                {
                    containerGo = containerTrans.gameObject;
                }

                RectTransform containerRect = containerGo.GetComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.05f, 0.05f);
                containerRect.anchorMax = new Vector2(0.95f, 0.75f);
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;

                // 2. Add or ensure GridLayoutGroup
                GridLayoutGroup gridLayout = containerGo.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                {
                    gridLayout = containerGo.AddComponent<GridLayoutGroup>();
                }
                gridLayout.cellSize = new Vector2(100, 100);
                gridLayout.spacing = new Vector2(15, 15);
                gridLayout.padding = new RectOffset(10, 10, 10, 10);
                gridLayout.childAlignment = TextAnchor.UpperLeft;

                // 3. Assign properties on panel controller component
                IngredientDisplayStationBase stationController = panelGo.GetComponent<IngredientDisplayStationBase>();
                if (stationController != null)
                {
                    SerializedObject so = new SerializedObject(stationController);
                    SerializedProperty containerProp = so.FindProperty("itemContainer");
                    SerializedProperty prefabProp = so.FindProperty("itemViewPrefab");

                    if (containerProp != null) containerProp.objectReferenceValue = containerGo.transform;
                    if (prefabProp != null) prefabProp.objectReferenceValue = itemPrefab;

                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(stationController);
                    Debug.Log($"[StationPanelWiringTool] Successfully wired {panelName}: itemContainer assigned, itemViewPrefab assigned.");
                }
                else
                {
                    Debug.LogError($"[StationPanelWiringTool] No IngredientDisplayStationBase component found on {panelName}!");
                }

                EditorUtility.SetDirty(containerGo);
                EditorUtility.SetDirty(panelGo);
            }

            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[StationPanelWiringTool] Station panel wiring completed successfully!");
        }

        private static GameObject FindGameObjectInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transforms)
                {
                    if (t.gameObject.name == name)
                    {
                        return t.gameObject;
                    }
                }
            }
            return null;
        }
    }
}
