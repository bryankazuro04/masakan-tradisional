#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MasakanTradisional.Gameplay;
using MasakanTradisional.Gameplay.Stations;

namespace MasakanTradisional.Editor
{
    public static class ActionMechanicSetupTool
    {
        [MenuItem("Tools/Setup Action Mechanics UI", false, 30)]
        public static void SetupActionMechanicsInScene()
        {
            CounterController counter = Object.FindFirstObjectByType<CounterController>();
            if (counter != null)
            {
                SetupController(counter.gameObject);
                Debug.Log("[ActionMechanicSetupTool] Successfully wired ActionMechanicController on CounterPanel.");
            }

            StoveController stove = Object.FindFirstObjectByType<StoveController>();
            if (stove != null)
            {
                SetupController(stove.gameObject);
                Debug.Log("[ActionMechanicSetupTool] Successfully wired ActionMechanicController on StovePanel.");
            }

            EditorUtility.SetDirty(counter != null ? counter.gameObject : stove?.gameObject);
        }

        private static void SetupController(GameObject parentObj)
        {
            ActionMechanicController controller = parentObj.GetComponentInChildren<ActionMechanicController>();
            if (controller == null)
            {
                GameObject actionObj = new GameObject("ActionMechanicOverlay", typeof(RectTransform));
                actionObj.transform.SetParent(parentObj.transform, false);
                controller = actionObj.AddComponent<ActionMechanicController>();
            }

            controller.EnsureRuntimeUI();
            Undo.RegisterCreatedObjectUndo(controller.gameObject, "Setup Action Mechanics UI");
        }
    }
}
#endif
