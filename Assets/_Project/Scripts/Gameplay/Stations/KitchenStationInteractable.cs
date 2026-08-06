using UnityEngine;
using MasakanTradisional.Data;

namespace MasakanTradisional.Gameplay
{
    /// <summary>
    /// Attach this to any 3D kitchen appliance that has a Collider.
    /// Identifies which KitchenStationType this object represents so that
    /// KitchenInputController can route a raycast hit to the correct station.
    ///
    /// Usage:
    ///   1. Add this component to the root GameObject of a 3D appliance
    ///      (e.g. FreeRefrigerator, StoveWithExtractorHood, FreeShelf, etc.)
    ///   2. Make sure at least one Collider exists on the object or a child.
    ///   3. Set stationType to the matching KitchenStationType enum value.
    /// </summary>
    public class KitchenStationInteractable : MonoBehaviour
    {
        [Tooltip("Which gameplay station this 3D object represents.")]
        [SerializeField] private KitchenStationType stationType;

        [Tooltip("Optional. If set, renders a highlight effect when the pointer hovers over this object. " +
                 "Leave null to skip hover feedback.")]
        [SerializeField] private Renderer[] highlightRenderers;

        [Tooltip("Tint color applied to highlightRenderers on hover.")]
        [SerializeField] private Color hoverTint = new Color(1f, 1f, 0.6f, 1f);

        private Color[] _originalColors;
        private bool _isHovered;

        // -------------------------------------------------------------------------
        public KitchenStationType StationType => stationType;

        // -------------------------------------------------------------------------
        private void Awake()
        {
            // Cache original colors for highlight restore
            if (highlightRenderers != null && highlightRenderers.Length > 0)
            {
                _originalColors = new Color[highlightRenderers.Length];
                for (int i = 0; i < highlightRenderers.Length; i++)
                {
                    if (highlightRenderers[i] != null)
                        _originalColors[i] = highlightRenderers[i].material.color;
                }
            }
        }

        // -------------------------------------------------------------------------
        // Called by KitchenInputController when the player taps/clicks this object
        // -------------------------------------------------------------------------

        /// <summary>
        /// Routes the tap to the GameplayManager to navigate to this station.
        /// </summary>
        public void OnTapped(GameplayManager gameplayManager)
        {
            if (gameplayManager == null)
            {
                Debug.LogWarning($"[KitchenStationInteractable] {name}: gameplayManager reference is null.");
                return;
            }

            gameplayManager.NavigateToStation(stationType);
        }

        // -------------------------------------------------------------------------
        // Optional hover highlight (called by KitchenInputController on desktop)
        // -------------------------------------------------------------------------

        public void SetHover(bool hovered)
        {
            if (_isHovered == hovered) return;
            _isHovered = hovered;

            if (highlightRenderers == null || highlightRenderers.Length == 0) return;

            for (int i = 0; i < highlightRenderers.Length; i++)
            {
                if (highlightRenderers[i] == null) continue;
                highlightRenderers[i].material.color = hovered ? hoverTint : _originalColors[i];
            }
        }
    }
}
