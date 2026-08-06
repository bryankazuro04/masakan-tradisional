using UnityEngine;
using UnityEngine.EventSystems;

namespace MasakanTradisional.Gameplay
{
    /// <summary>
    /// Handles touch/mouse input for the kitchen overview.
    /// Casts a ray from the main camera on every tap/click and routes the hit
    /// to the tapped KitchenStationInteractable (if any).
    ///
    /// Behaviour rules:
    ///   - Input is only processed when StationCameraRig.IsOverview is true.
    ///   - Taps that land on a UI element (Canvas) are ignored via EventSystem check.
    ///   - On desktop, moving the mouse over an interactable shows a hover highlight.
    ///
    /// Wire via Inspector:
    ///   mainCamera      → Main Camera (auto-found if null)
    ///   gameplayManager → GameplayManager in the scene
    ///   cameraRig       → StationCameraRig in the scene
    /// </summary>
    public class KitchenInputController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Leave null to auto-find Camera.main.")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameplayManager gameplayManager;
        [SerializeField] private StationCameraRig cameraRig;

        [Header("Raycast Settings")]
        [SerializeField] private float raycastDistance = 200f;
        [SerializeField] private LayerMask interactableLayerMask = ~0; // Everything by default

        // Hover tracking (desktop only)
        private KitchenStationInteractable _hoveredInteractable;

        // -------------------------------------------------------------------------
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null)
                Debug.LogError("[KitchenInputController] No camera found. Assign mainCamera in the Inspector.");
        }

        // -------------------------------------------------------------------------
        private void Update()
        {
            // Only act in overview mode
            if (cameraRig != null && !cameraRig.IsOverview) return;

            HandleHover();
            HandleTapOrClick();
        }

        // -------------------------------------------------------------------------
        // Hover (desktop only — skipped on mobile to save raycasts)
        // -------------------------------------------------------------------------

        private void HandleHover()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            KitchenStationInteractable hitInteractable = null;

            if (!IsPointerOverUI(Input.mousePosition))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayerMask))
                {
                    hitInteractable = hit.collider.GetComponentInParent<KitchenStationInteractable>();
                }
            }

            if (hitInteractable != _hoveredInteractable)
            {
                _hoveredInteractable?.SetHover(false);
                _hoveredInteractable = hitInteractable;
                _hoveredInteractable?.SetHover(true);
            }
#endif
        }

        // -------------------------------------------------------------------------
        // Tap / Click
        // -------------------------------------------------------------------------

        private void HandleTapOrClick()
        {
            if (!IsTapOrClickDown(out Vector2 screenPos)) return;
            if (IsPointerOverUI(screenPos)) return;

            TryInteract(screenPos);
        }

        private bool IsTapOrClickDown(out Vector2 screenPos)
        {
            // Touch input (mobile / Unity Remote)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPos = touch.position;
                    return true;
                }
            }
            // Mouse input (editor / standalone)
            else if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                return true;
            }

            screenPos = Vector2.zero;
            return false;
        }

        private void TryInteract(Vector2 screenPos)
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayerMask))
            {
                KitchenStationInteractable interactable = hit.collider.GetComponentInParent<KitchenStationInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"[KitchenInputController] Tapped: {interactable.gameObject.name} → {interactable.StationType}");
                    interactable.OnTapped(gameplayManager);
                }
            }
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the given screen position is over a UI canvas element.
        /// Handles both mouse and touch event systems.
        /// </summary>
        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            // Touch: check by finger ID
            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

            // Mouse
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
