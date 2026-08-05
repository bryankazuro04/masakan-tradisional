using System;
using UnityEngine;

namespace MasakanTradisional.Gameplay.Evaluation
{
    /// <summary>
    /// ScriptableObject container for recipe-specific Fuzzy Logic evaluation tolerances and weights.
    /// Allows creating reusable fuzzy configuration files (.asset) in Unity.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFuzzyCookingData", menuName = "Masakan Tradisional/Data/Fuzzy Cooking Data")]
    public class FuzzyCookingData : ScriptableObject
    {
        [Header("Heat Level Tolerances (Ratio relative to Target = 1.0)")]
        [Tooltip("Ratio at or below which heat is completely 'Low'")]
        [SerializeField] private float lowHeatLimit = 0.7f;
        
        [Tooltip("Target heat ratio (Ideal = 1.0)")]
        [SerializeField] private float idealHeatTarget = 1.0f;
        
        [Tooltip("Ratio at or above which heat is completely 'High'")]
        [SerializeField] private float highHeatLimit = 1.3f;

        [Header("Timing Tolerances (Ratio relative to Target = 1.0)")]
        [Tooltip("Ratio at or below which time is completely 'Short'")]
        [SerializeField] private float shortTimeLimit = 0.7f;
        
        [Tooltip("Target time ratio (Ideal = 1.0)")]
        [SerializeField] private float idealTimeTarget = 1.0f;
        
        [Tooltip("Ratio at or above which time is completely 'Long'")]
        [SerializeField] private float longTimeLimit = 1.3f;

        [Header("Sugeno Rule Output Scores (0 to 100)")]
        [Range(0f, 100f)] public float heatLow_timeShortScore = 20f;   // Undercooked & Cold
        [Range(0f, 100f)] public float heatLow_timeIdealScore = 50f;   // Raw / Warm
        [Range(0f, 100f)] public float heatLow_timeLongScore = 40f;    // Soft / Soggy
        [Range(0f, 100f)] public float heatIdeal_timeShortScore = 65f;  // Underdone
        [Range(0f, 100f)] public float heatIdeal_timeIdealScore = 100f; // Perfect
        [Range(0f, 100f)] public float heatIdeal_timeLongScore = 70f;   // Slightly Overcooked
        [Range(0f, 100f)] public float heatHigh_timeShortScore = 55f;   // Seared / Uneven
        [Range(0f, 100f)] public float heatHigh_timeIdealScore = 60f;   // Slightly Burnt
        [Range(0f, 100f)] public float heatHigh_timeLongScore = 0f;     // Burnt / Charred

        public float LowHeatLimit => lowHeatLimit;
        public float IdealHeatTarget => idealHeatTarget;
        public float HighHeatLimit => highHeatLimit;

        public float ShortTimeLimit => shortTimeLimit;
        public float IdealTimeTarget => idealTimeTarget;
        public float LongTimeLimit => longTimeLimit;
    }
}