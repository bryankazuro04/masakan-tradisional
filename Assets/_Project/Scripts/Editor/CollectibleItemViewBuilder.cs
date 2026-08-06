using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MasakanTradisional.Gameplay.Stations;

namespace MasakanTradisional.Editor
{
    public static class CollectibleItemViewBuilder
    {
        [MenuItem("Tools/Build CollectibleItemView Prefab")]
        public static void BuildPrefab()
        {
            // 1. Create root GameObject
            GameObject root = new GameObject("CollectibleItemView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CollectibleItemView));
            
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(100, 100);

            Image rootImage = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            button.targetGraphic = rootImage;

            CollectibleItemView view = root.GetComponent<CollectibleItemView>();

            // 2. Child 1: Icon (Image)
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(root.transform, false);
            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconGo.GetComponent<Image>();

            // 3. Child 2: Name (TMP_Text)
            GameObject nameGo = new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(root.transform, false);
            RectTransform nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.3f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = nameGo.GetComponent<TextMeshProUGUI>();
            nameText.text = "Item Name";
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = 14;

            // 4. Child 3: Checkmark (GameObject, starts inactive)
            GameObject checkmarkGo = new GameObject("CollectedCheckmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmarkGo.transform.SetParent(root.transform, false);
            RectTransform checkmarkRect = checkmarkGo.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.7f, 0.7f);
            checkmarkRect.anchorMax = new Vector2(1f, 1f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            checkmarkGo.SetActive(false); // starts inactive

            // 5. Wire references on CollectibleItemView
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("iconImage").objectReferenceValue = iconImage;
            serializedView.FindProperty("nameText").objectReferenceValue = nameText;
            serializedView.FindProperty("clickButton").objectReferenceValue = button;
            serializedView.FindProperty("collectedCheckmark").objectReferenceValue = checkmarkGo;
            serializedView.ApplyModifiedProperties();

            // 6. Save as Prefab
            string dir = "Assets/_Project/Prefabs";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
            string prefabPath = $"{dir}/CollectibleItemView.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Destroy temp hierarchy in scene
            Object.DestroyImmediate(root);

            Debug.Log($"Successfully created CollectibleItemView prefab at {prefabPath}");
            AssetDatabase.Refresh();
        }
    }
}
