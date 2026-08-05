using System;
using UnityEngine;

namespace MasakanTradisional.Gameplay.Evaluation
{
    /// <summary>
    /// Standalone Fuzzy Logic evaluator for calculating cooking scores (0 to 100).
    /// Uses Mamdani/Sugeno hybrid fuzzy inference based on heat level and timing accuracy.
    /// Supports custom recipe-specific FuzzyCookingData ScriptableObjects.
    /// </summary>
    public class FuzzyCookingEvaluator : MonoBehaviour
    {
        [Header("Default Recipe Data (Fallback)")]
        [Tooltip("Optional default configuration asset. Can be overridden per evaluation call.")]
        [SerializeField] private FuzzyCookingData defaultFuzzyData;

        [Header("Heat Tolerances (Fallback Ratio relative to Target = 1.0)")]
        [Tooltip("Ratio at or below which heat is completely 'Low' (e.g. 0.7 = 70% of target heat)")]
        [SerializeField] private float lowHeatLimit = 0.7f;
        [Tooltip("Target heat ratio (Ideal = 1.0)")]
        [SerializeField] private float idealHeatTarget = 1.0f;
        [Tooltip("Ratio at or above which heat is completely 'High' (e.g. 1.3 = 130% of target heat)")]
        [SerializeField] private float highHeatLimit = 1.3f;

        [Header("Timing Tolerances (Fallback Ratio relative to Target = 1.0)")]
        [Tooltip("Ratio at or below which time is completely 'Short' (e.g. 0.7 = 70% of target duration)")]
        [SerializeField] private float shortTimeLimit = 0.7f;
        [Tooltip("Target time ratio (Ideal = 1.0)")]
        [SerializeField] private float idealTimeTarget = 1.0f;
        [Tooltip("Ratio at or above which time is completely 'Long' (e.g. 1.3 = 130% of target duration)")]
        [SerializeField] private float longTimeLimit = 1.3f;

        /// <summary>
        /// Struct containing crisp score, quality assessment, and fuzzified degree values.
        /// </summary>
        [Serializable]
        public struct EvaluationResult
        {
            public float FinalScore;           // Crisp calculated score (0.0 - 100.0)
            public string StatusLabel;         // Descriptive cooking status (e.g., "Masterpiece", "Burnt")
            public string LetterGrade;         // Grade rank ("A+", "A", "B", "C", "D", "F")
            
            // Fuzzified Membership Values
            public float HeatLowDegree;
            public float HeatIdealDegree;
            public float HeatHighDegree;

            public float TimeShortDegree;
            public float TimeIdealDegree;
            public float TimeLongDegree;
        }

        /// <summary>
        /// Evaluates cooking performance using raw target and actual parameters.
        /// Optionally accepts a custom FuzzyCookingData asset.
        /// </summary>
        public EvaluationResult EvaluateCooking(float actualHeat, float targetHeat, float actualTime, float targetTime, FuzzyCookingData configData = null)
        {
            if (targetHeat <= 0f || targetTime <= 0f)
            {
                Debug.LogWarning("[FuzzyCookingEvaluator] Target parameters must be greater than zero!");
                return new EvaluationResult { FinalScore = 0f, StatusLabel = "Invalid Parameters", LetterGrade = "F" };
            }

            float heatRatio = actualHeat / targetHeat;
            float timeRatio = actualTime / targetTime;

            return EvaluateRatios(heatRatio, timeRatio, configData);
        }

        /// <summary>
        /// Evaluates cooking performance directly using normalized ratios (where 1.0 = ideal).
        /// Reads custom rule output scores from configData if provided.
        /// </summary>
        public EvaluationResult EvaluateRatios(float heatRatio, float timeRatio, FuzzyCookingData configData = null)
        {
            // Use active config or fallback to script defaults
            FuzzyCookingData activeConfig = configData != null ? configData : defaultFuzzyData;

            // Resolve limits
            float lowHeat = activeConfig != null ? activeConfig.LowHeatLimit : lowHeatLimit;
            float idealHeat = activeConfig != null ? activeConfig.IdealHeatTarget : idealHeatTarget;
            float highHeat = activeConfig != null ? activeConfig.HighHeatLimit : highHeatLimit;

            float shortTime = activeConfig != null ? activeConfig.ShortTimeLimit : shortTimeLimit;
            float idealTime = activeConfig != null ? activeConfig.IdealTimeTarget : idealTimeTarget;
            float longTime = activeConfig != null ? activeConfig.LongTimeLimit : longTimeLimit;

            // 1. Fuzzification - Heat Level
            float uHeatLow = CalculateLowMembership(heatRatio, lowHeat, idealHeat);
            float uHeatIdeal = CalculateIdealMembership(heatRatio, lowHeat, idealHeat, highHeat);
            float uHeatHigh = CalculateHighMembership(heatRatio, idealHeat, highHeat);

            // 1. Fuzzification - Timing
            float uTimeShort = CalculateLowMembership(timeRatio, shortTime, idealTime);
            float uTimeIdeal = CalculateIdealMembership(timeRatio, shortTime, idealTime, longTime);
            float uTimeLong = CalculateHighMembership(timeRatio, idealTime, longTime);

            // 2. Fuzzy Rule Inference Matrix (9 Rules)
            float r1Weight = Mathf.Min(uHeatLow, uTimeShort);   // Heat Low  & Time Short
            float r2Weight = Mathf.Min(uHeatLow, uTimeIdeal);   // Heat Low  & Time Ideal
            float r3Weight = Mathf.Min(uHeatLow, uTimeLong);    // Heat Low  & Time Long

            float r4Weight = Mathf.Min(uHeatIdeal, uTimeShort); // Heat Ideal & Time Short
            float r5Weight = Mathf.Min(uHeatIdeal, uTimeIdeal); // Heat Ideal & Time Ideal
            float r6Weight = Mathf.Min(uHeatIdeal, uTimeLong);  // Heat Ideal & Time Long

            float r7Weight = Mathf.Min(uHeatHigh, uTimeShort);  // Heat High & Time Short
            float r8Weight = Mathf.Min(uHeatHigh, uTimeIdeal);  // Heat High & Time Ideal
            float r9Weight = Mathf.Min(uHeatHigh, uTimeLong);   // Heat High & Time Long

            // Resolve Sugeno Rule Scores (0 - 100) from ScriptableObject or hardcoded defaults
            float s1 = activeConfig != null ? activeConfig.heatLow_timeShortScore : 20f;
            float s2 = activeConfig != null ? activeConfig.heatLow_timeIdealScore : 50f;
            float s3 = activeConfig != null ? activeConfig.heatLow_timeLongScore : 40f;
            float s4 = activeConfig != null ? activeConfig.heatIdeal_timeShortScore : 65f;
            float s5 = activeConfig != null ? activeConfig.heatIdeal_timeIdealScore : 100f;
            float s6 = activeConfig != null ? activeConfig.heatIdeal_timeLongScore : 70f;
            float s7 = activeConfig != null ? activeConfig.heatHigh_timeShortScore : 55f;
            float s8 = activeConfig != null ? activeConfig.heatHigh_timeIdealScore : 60f;
            float s9 = activeConfig != null ? activeConfig.heatHigh_timeLongScore : 0f;

            // 3. Defuzzification via Weighted Average Method (Sugeno Centroid)
            float weightedSum = (r1Weight * s1) + (r2Weight * s2) + (r3Weight * s3) +
                                (r4Weight * s4) + (r5Weight * s5) + (r6Weight * s6) +
                                (r7Weight * s7) + (r8Weight * s8) + (r9Weight * s9);

            float totalWeight = r1Weight + r2Weight + r3Weight +
                                r4Weight + r5Weight + r6Weight +
                                r7Weight + r8Weight + r9Weight;

            float crispScore = (totalWeight > 0.0001f) ? (weightedSum / totalWeight) : 0f;
            crispScore = Mathf.Clamp(crispScore, 0f, 100f);

            // 4. Construct Result Payload
            return new EvaluationResult
            {
                FinalScore = crispScore,
                StatusLabel = DetermineStatusLabel(r1Weight, r2Weight, r3Weight, r4Weight, r5Weight, r6Weight, r7Weight, r8Weight, r9Weight),
                LetterGrade = DetermineLetterGrade(crispScore),
                HeatLowDegree = uHeatLow,
                HeatIdealDegree = uHeatIdeal,
                HeatHighDegree = uHeatHigh,
                TimeShortDegree = uTimeShort,
                TimeIdealDegree = uTimeIdeal,
                TimeLongDegree = uTimeLong
            };
        }

        /// <summary>
        /// Left-shoulder membership function (Low / Under).
        /// </summary>
        private float CalculateLowMembership(float val, float minLimit, float idealTarget)
        {
            if (val <= minLimit) return 1.0f;
            if (val >= idealTarget) return 0.0f;
            return (idealTarget - val) / (idealTarget - minLimit);
        }

        /// <summary>
        /// Triangular membership function (Ideal).
        /// </summary>
        private float CalculateIdealMembership(float val, float minLimit, float idealTarget, float maxLimit)
        {
            if (val <= minLimit || val >= maxLimit) return 0.0f;
            if (Mathf.Approximately(val, idealTarget)) return 1.0f;

            if (val < idealTarget)
            {
                return (val - minLimit) / (idealTarget - minLimit);
            }
            else
            {
                return (maxLimit - val) / (maxLimit - idealTarget);
            }
        }

        /// <summary>
        /// Right-shoulder membership function (High / Over).
        /// </summary>
        private float CalculateHighMembership(float val, float idealTarget, float maxLimit)
        {
            if (val <= idealTarget) return 0.0f;
            if (val >= maxLimit) return 1.0f;
            return (val - idealTarget) / (maxLimit - idealTarget);
        }

        private string DetermineStatusLabel(params float[] ruleWeights)
        {
            int maxRuleIndex = 0;
            float maxWeight = -1f;

            for (int i = 0; i < ruleWeights.Length; i++)
            {
                if (ruleWeights[i] > maxWeight)
                {
                    maxWeight = ruleWeights[i];
                    maxRuleIndex = i;
                }
            }

            switch (maxRuleIndex)
            {
                case 0: return "Raw & Cold";
                case 1: return "Undercooked";
                case 2: return "Soggy & Lukewarm";
                case 3: return "Underdone";
                case 4: return "Sempurna (Perfect)";
                case 5: return "Slightly Overcooked";
                case 6: return "Seared & Raw Inside";
                case 7: return "Slightly Burnt";
                case 8: return "Gosong (Burnt & Charred)";
                default: return "Satisfactory";
            }
        }

        private string DetermineLetterGrade(float score)
        {
            if (score >= 90f) return "A+";
            if (score >= 80f) return "A";
            if (score >= 70f) return "B";
            if (score >= 60f) return "C";
            if (score >= 40f) return "D";
            return "F";
        }
    }
}