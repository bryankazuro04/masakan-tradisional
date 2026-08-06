using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MasakanTradisional.Data;
using MasakanTradisional.Gameplay.Evaluation;
using MasakanTradisional.Gameplay.Stations;
using MasakanTradisional.Gameplay;

namespace MasakanTradisional.Editor
{
    public static class GameplayManagerWiringTool
    {
        [MenuItem("Tools/Wire Gameplay Manager")]
        [InitializeOnLoadMethod]
        public static void WireGameplayManager()
        {
            Gameplay.GameplayManager manager = Object.FindFirstObjectByType<Gameplay.GameplayManager>();
            if (manager == null)
            {
                Debug.LogError("[GameplayManagerWiringTool] GameplayManager component not found in scene!");
                return;
            }
            GameObject mgrGo = manager.gameObject;
            Scene currentScene = mgrGo.scene;

            SerializedObject so = new SerializedObject(manager);

            // 1. Data & Fallback Settings
            string recipePath = "Assets/_Project/Scripts/Data/Recipes/RendangPadang.asset";
            RecipeData recipe = AssetDatabase.LoadAssetAtPath<RecipeData>(recipePath);
            if (recipe == null)
            {
                recipePath = "Assets/_Project/Scripts/Data/Recipes/NewRecipeData.asset";
                recipe = AssetDatabase.LoadAssetAtPath<RecipeData>(recipePath);
            }
            SetObjectProp(so, "defaultFallbackRecipe", recipe);

            FuzzyCookingEvaluator fuzzy = mgrGo.GetComponent<FuzzyCookingEvaluator>();
            if (fuzzy == null) fuzzy = mgrGo.AddComponent<FuzzyCookingEvaluator>();
            SetObjectProp(so, "fuzzyEvaluator", fuzzy);

            // 2. Station Controllers
            SetStationControllerProp<ShelvesController>(so, currentScene, "ShelvesPanel", "shelvesController");
            SetStationControllerProp<FridgeController>(so, currentScene, "FridgePanel", "fridgeController");
            SetStationControllerProp<CounterController>(so, currentScene, "CounterPanel", "counterController");
            SetStationControllerProp<StoveController>(so, currentScene, "StovePanel", "stoveController");
            SetStationControllerProp<OvenController>(so, currentScene, "OvenPanel", "ovenController");

            // 3. Station Switcher Buttons
            SetComponentProp<Button>(so, currentScene, "NavShelvesButton", "navShelvesButton");
            SetComponentProp<Button>(so, currentScene, "NavFridgeButton", "navFridgeButton");
            SetComponentProp<Button>(so, currentScene, "NavCounterButton", "navCounterButton");
            SetComponentProp<Button>(so, currentScene, "NavStoveButton", "navStoveButton");
            SetComponentProp<Button>(so, currentScene, "NavOvenButton", "navOvenButton");

            // 3b. Create BackToOverviewButton if missing & wire it
            GameObject backBtnGo = FindGameObjectInScene(currentScene, "BackToOverviewButton");
            if (backBtnGo == null)
            {
                GameObject hudGo = FindGameObjectInScene(currentScene, "HUD");
                backBtnGo = new GameObject("BackToOverviewButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                if (hudGo != null) backBtnGo.transform.SetParent(hudGo.transform, false);
                
                RectTransform rt = backBtnGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -100);
                rt.sizeDelta = new Vector2(200, 60);

                Undo.RegisterCreatedObjectUndo(backBtnGo, "Create BackToOverviewButton");
            }

            if (backBtnGo != null)
            {
                TextMeshProUGUI label = backBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label == null)
                {
                    GameObject labelGo = new GameObject("Text (TMP)", typeof(RectTransform));
                    labelGo.transform.SetParent(backBtnGo.transform, false);
                    label = labelGo.AddComponent<TextMeshProUGUI>();
                    label.text = "← Back";
                    label.fontSize = 24;
                    label.alignment = TextAlignmentOptions.Center;
                    label.color = new Color(0.2f, 0.2f, 0.2f, 1f);

                    RectTransform labelRt = labelGo.GetComponent<RectTransform>();
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.sizeDelta = Vector2.zero;
                    labelRt.anchoredPosition = Vector2.zero;
                }

                Button backBtn = backBtnGo.GetComponent<Button>();
                SetObjectProp(so, "backToOverviewButton", backBtn);
            }

            // 3c. Create StationCameraRig if missing & wire it
            GameObject cameraRigGo = FindGameObjectInScene(currentScene, "StationCameraRig");
            if (cameraRigGo == null)
            {
                cameraRigGo = new GameObject("StationCameraRig", typeof(StationCameraRig));
                Undo.RegisterCreatedObjectUndo(cameraRigGo, "Create StationCameraRig");
            }
            if (cameraRigGo != null)
            {
                StationCameraRig rigComp = cameraRigGo.GetComponent<StationCameraRig>();
                SetObjectProp(so, "cameraRig", rigComp);
            }

            // 4. Step Information UI
            SetComponentProp<TMP_Text>(so, currentScene, "RecipeNameText", "recipeNameText");
            SetComponentProp<TMP_Text>(so, currentScene, "StepTitleText", "stepTitleText");
            SetComponentProp<TMP_Text>(so, currentScene, "StepInstructionText", "stepInstructionText");
            SetComponentProp<TMP_Text>(so, currentScene, "StepProgressText", "stepProgressText");
            SetComponentProp<Image>(so, currentScene, "StepIllustrationImage", "stepIllustrationImage");
            SetComponentProp<TMP_Text>(so, currentScene, "TargetToolText", "targetToolText");

            // 5. Step Completion
            SetComponentProp<Button>(so, currentScene, "FinishStepButton", "finishStepButton");

            GameObject hintGo = FindGameObjectInScene(currentScene, "CollectionHintText");
            if (hintGo == null)
            {
                GameObject finishBtnGo = FindGameObjectInScene(currentScene, "FinishStepButton");
                GameObject hudGo = FindGameObjectInScene(currentScene, "HUD");
                Transform parentTrans = finishBtnGo != null ? finishBtnGo.transform.parent : (hudGo != null ? hudGo.transform : null);

                if (parentTrans != null)
                {
                    hintGo = new GameObject("CollectionHintText", typeof(RectTransform));
                    hintGo.transform.SetParent(parentTrans, false);
                    TextMeshProUGUI tmp = hintGo.AddComponent<TextMeshProUGUI>();
                    tmp.text = "Collect all ingredients first!";
                    tmp.fontSize = 20;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(1f, 0.8f, 0.2f, 1f);

                    RectTransform rt = hintGo.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(350, 40);

                    if (finishBtnGo != null)
                    {
                        RectTransform finishRt = finishBtnGo.GetComponent<RectTransform>();
                        rt.anchoredPosition = finishRt.anchoredPosition + new Vector2(0, 45f);
                    }

                    Undo.RegisterCreatedObjectUndo(hintGo, "Create CollectionHintText");
                }
            }

            if (hintGo != null)
            {
                TMP_Text hintTmp = hintGo.GetComponent<TMP_Text>();
                SetObjectProp(so, "collectionHintText", hintTmp);
            }

            // 6. Result / Evaluation Modal
            GameObject resultModalGo = FindGameObjectInScene(currentScene, "ResultModal");
            SetObjectProp(so, "resultModal", resultModalGo);

            SetComponentProp<TMP_Text>(so, currentScene, "ResultScoreText", "resultScoreText");
            SetComponentProp<TMP_Text>(so, currentScene, "ResultGradeText", "resultGradeText");
            SetComponentProp<TMP_Text>(so, currentScene, "ResultStatusLabelText", "resultStatusLabelText");

            GameObject starsRowGo = FindGameObjectInScene(currentScene, "StarsRow");
            if (starsRowGo != null)
            {
                Image[] stars = starsRowGo.GetComponentsInChildren<Image>(true);
                SerializedProperty starsProp = so.FindProperty("resultStars");
                if (starsProp != null)
                {
                    starsProp.arraySize = stars.Length;
                    for (int i = 0; i < stars.Length; i++)
                    {
                        starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
                    }
                }
            }

            Sprite filledStar = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite emptyStar = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            SetObjectProp(so, "filledStarSprite", filledStar);
            SetObjectProp(so, "emptyStarSprite", emptyStar);

            SetComponentProp<Button>(so, currentScene, "RetryButton", "retryButton");
            SetComponentProp<Button>(so, currentScene, "MainMenuButton", "mainMenuButton");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(mgrGo);

            // Wire StationCameraRig Virtual Cameras
            if (cameraRigGo != null)
            {
                StationCameraRig rig = cameraRigGo.GetComponent<StationCameraRig>();
                if (rig != null)
                {
                    SerializedObject rigSo = new SerializedObject(rig);
                    Cinemachine.CinemachineVirtualCamera overviewCam = FindComponentInScene<Cinemachine.CinemachineVirtualCamera>(currentScene, "MainVirtualCamera");
                    if (overviewCam == null)
                    {
                        overviewCam = FindComponentInScene<Cinemachine.CinemachineVirtualCamera>(currentScene, "Virtual Camera (1)");
                    }
                    SetObjectProp(rigSo, "overviewCamera", overviewCam);

                    SerializedProperty listProp = rigSo.FindProperty("stationCameras");
                    if (listProp != null)
                    {
                        listProp.arraySize = 5;
                        string[] camNames = new string[] { "ShelvesCamera", "FridgeCamera", "CounterCamera", "StoveCamera", "OvenCamera" };
                        for (int i = 0; i < 5; i++)
                        {
                            Cinemachine.CinemachineVirtualCamera vcam = FindComponentInScene<Cinemachine.CinemachineVirtualCamera>(currentScene, camNames[i]);
                            if (vcam == null)
                            {
                                GameObject camGo = new GameObject(camNames[i], typeof(Cinemachine.CinemachineVirtualCamera));
                                vcam = camGo.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                                vcam.m_Priority = 9;
                                Undo.RegisterCreatedObjectUndo(camGo, $"Create {camNames[i]}");
                            }

                            SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                            SerializedProperty typeProp = elem.FindPropertyRelative("stationType");
                            SerializedProperty vcamProp = elem.FindPropertyRelative("virtualCamera");

                            if (typeProp != null) typeProp.enumValueIndex = i;
                            if (vcamProp != null) vcamProp.objectReferenceValue = vcam;
                        }
                    }
                    rigSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(rig);
                    EditorUtility.SetDirty(cameraRigGo);
                }
            }

            // Create & Wire KitchenInputController
            GameObject inputGo = FindGameObjectInScene(currentScene, "KitchenInputController");
            if (inputGo == null)
            {
                inputGo = new GameObject("KitchenInputController", typeof(KitchenInputController));
                Undo.RegisterCreatedObjectUndo(inputGo, "Create KitchenInputController");
            }
            if (inputGo != null)
            {
                KitchenInputController inputCtrl = inputGo.GetComponent<KitchenInputController>();
                if (inputCtrl != null)
                {
                    SerializedObject inputSo = new SerializedObject(inputCtrl);
                    SetObjectProp(inputSo, "mainCamera", Camera.main);
                    SetObjectProp(inputSo, "gameplayManager", manager);
                    if (cameraRigGo != null) SetObjectProp(inputSo, "cameraRig", cameraRigGo.GetComponent<StationCameraRig>());
                    inputSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(inputCtrl);
                    EditorUtility.SetDirty(inputGo);
                }
            }

            // Wire KitchenStationInteractable stationTypes on 3D objects
            WireInteractable(currentScene, "ShelfWithEquipment01", KitchenStationType.Shelves);
            WireInteractable(currentScene, "FreeRefrigerator", KitchenStationType.Fridge);
            WireInteractable(currentScene, "FreeCabinet03", KitchenStationType.Counter);
            WireInteractable(currentScene, "StoveWithExtractorHood", KitchenStationType.Stove);
            WireInteractable(currentScene, "FreeMirkowave", KitchenStationType.Oven);

            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[GameplayManagerWiringTool] GameplayManager & Camera Rig wiring completed successfully!");
        }

        private static void WireInteractable(Scene scene, string gameObjectName, KitchenStationType type)
        {
            GameObject go = FindGameObjectInScene(scene, gameObjectName);
            if (go != null)
            {
                KitchenStationInteractable interactable = go.GetComponent<KitchenStationInteractable>();
                if (interactable == null) interactable = go.AddComponent<KitchenStationInteractable>();
                
                SerializedObject interactableSo = new SerializedObject(interactable);
                SerializedProperty typeProp = interactableSo.FindProperty("stationType");
                if (typeProp != null)
                {
                    typeProp.enumValueIndex = (int)type;
                }
                interactableSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(interactable);
                EditorUtility.SetDirty(go);

                if (go.GetComponentInChildren<Collider>(true) == null)
                {
                    go.AddComponent<BoxCollider>();
                }
            }
        }

        private static T FindComponentInScene<T>(Scene scene, string name) where T : Component
        {
            GameObject go = FindGameObjectInScene(scene, name);
            return go != null ? go.GetComponent<T>() : null;
        }

        private static void SetObjectProp(SerializedObject so, string propName, UnityEngine.Object value)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
            else
            {
                Debug.LogWarning($"[GameplayManagerWiringTool] Property '{propName}' not found!");
            }
        }

        private static void SetComponentProp<T>(SerializedObject so, Scene scene, string gameObjectName, string propName) where T : Component
        {
            GameObject go = FindGameObjectInScene(scene, gameObjectName);
            if (go != null)
            {
                T comp = go.GetComponent<T>();
                if (comp != null)
                {
                    SetObjectProp(so, propName, comp);
                }
                else
                {
                    Debug.LogWarning($"[GameplayManagerWiringTool] Component {typeof(T).Name} not found on {gameObjectName}!");
                }
            }
            else
            {
                Debug.LogWarning($"[GameplayManagerWiringTool] GameObject '{gameObjectName}' not found in scene!");
            }
        }

        private static void SetStationControllerProp<T>(SerializedObject so, Scene scene, string gameObjectName, string propName) where T : Component
        {
            SetComponentProp<T>(so, scene, gameObjectName, propName);
        }

        private static GameObject FindGameObjectInScene(Scene scene, string name)
        {
            if (scene.isLoaded)
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
            }

            GameObject found = GameObject.Find(name);
            if (found != null) return found;

            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.hideFlags == HideFlags.None && t.gameObject.name == name)
                {
                    return t.gameObject;
                }
            }

            return null;
        }
    }
}
