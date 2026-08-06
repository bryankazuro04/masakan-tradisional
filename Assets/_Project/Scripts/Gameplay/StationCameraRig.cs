using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MasakanTradisional.Data;

namespace MasakanTradisional.Gameplay
{
    /// <summary>
    /// Serializable pairing of a KitchenStationType to a CinemachineVirtualCamera,
    /// so the mapping can be configured in the Inspector.
    /// </summary>
    [Serializable]
    public struct StationCameraEntry
    {
        public KitchenStationType stationType;
        public CinemachineVirtualCamera virtualCamera;
    }

    /// <summary>
    /// Manages Cinemachine virtual camera switching between the kitchen overview
    /// and individual station close-up cameras.
    ///
    /// Switching is done via priority:
    ///   - Active camera  → activePriority  (default 11)
    ///   - Inactive cameras → inactivePriority (default  9)
    /// CinemachineBrain on the Main Camera will blend automatically.
    ///
    /// Wire via Inspector:
    ///   overviewCamera  → the "whole kitchen" fixed shot vcam
    ///   stationCameras  → one entry per KitchenStationType
    /// </summary>
    public class StationCameraRig : MonoBehaviour
    {
        [Header("Overview Camera")]
        [Tooltip("The Cinemachine Virtual Camera showing the whole kitchen.")]
        [SerializeField] private CinemachineVirtualCamera overviewCamera;

        [Header("Station Cameras")]
        [Tooltip("One entry per station. Leave virtualCamera null if that station has no dedicated shot.")]
        [SerializeField] private List<StationCameraEntry> stationCameras = new List<StationCameraEntry>();

        [Header("Priority Settings")]
        [SerializeField] private int activePriority = 11;
        [SerializeField] private int inactivePriority = 9;

        // --- State ---
        private Dictionary<KitchenStationType, CinemachineVirtualCamera> _cameraMap;

        /// <summary>True while the overview shot is live (no station is zoomed in).</summary>
        public bool IsOverview { get; private set; } = true;

        // --- Events ---
        /// <summary>Fired when the camera returns to the overview shot.</summary>
        public event Action OnReturnedToOverview;

        /// <summary>Fired when the camera zooms into a station.</summary>
        public event Action<KitchenStationType> OnZoomedToStation;

        // -------------------------------------------------------------------------
        private void Awake()
        {
            // Build the runtime lookup
            _cameraMap = new Dictionary<KitchenStationType, CinemachineVirtualCamera>();
            foreach (StationCameraEntry entry in stationCameras)
            {
                if (entry.virtualCamera != null)
                    _cameraMap[entry.stationType] = entry.virtualCamera;
            }

            // Start in overview
            SetAllToInactivePriority();
            if (overviewCamera != null)
                overviewCamera.Priority = activePriority;

            IsOverview = true;
        }

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Raises the given station's virtual camera to active priority.
        /// CinemachineBrain will blend to it automatically.
        /// If no vcam is registered for this station the overview remains active.
        /// </summary>
        public void ZoomToStation(KitchenStationType station)
        {
            SetAllToInactivePriority();

            if (_cameraMap.TryGetValue(station, out CinemachineVirtualCamera vcam) && vcam != null)
            {
                vcam.Priority = activePriority;
                IsOverview = false;
                OnZoomedToStation?.Invoke(station);
            }
            else
            {
                // No dedicated camera for this station — fall back to overview
                if (overviewCamera != null)
                    overviewCamera.Priority = activePriority;

                IsOverview = true;
                Debug.LogWarning($"[StationCameraRig] No virtual camera registered for {station}. Staying in overview.");
            }
        }

        /// <summary>
        /// Returns the Cinemachine brain to the overview shot.
        /// </summary>
        public void ReturnToOverview()
        {
            SetAllToInactivePriority();

            if (overviewCamera != null)
                overviewCamera.Priority = activePriority;

            IsOverview = true;
            OnReturnedToOverview?.Invoke();
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private void SetAllToInactivePriority()
        {
            if (overviewCamera != null)
                overviewCamera.Priority = inactivePriority;

            foreach (var kvp in _cameraMap)
            {
                if (kvp.Value != null)
                    kvp.Value.Priority = inactivePriority;
            }
        }
    }
}
