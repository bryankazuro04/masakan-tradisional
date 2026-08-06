using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MasakanTradisional.Core.Audio;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Shared behaviour for any station that exposes an adjustable heat value and a
    /// start/pause cooking timer (Stove, Oven). Heat/timer state resets in SetStep
    /// (once per step) rather than Show (every time the panel is viewed) - so
    /// browsing to another station and back no longer wipes cooking progress.
    /// </summary>
    public abstract class HeatStationControllerBase : StationControllerBase, IHeatStationController
    {
        [Header("Heat Controls")]
        [SerializeField] protected Slider heatSlider;
        [SerializeField] protected TMP_Text currentHeatText;
        [SerializeField] protected TMP_Text targetHeatText;
 
        [Header("Timer Controls")]
        [SerializeField] protected TMP_Text cookingTimerText;
        [SerializeField] protected TMP_Text targetTimerText;
        [SerializeField] protected Button startCookingButton;
        [SerializeField] protected TMP_Text startButtonText;
 
        protected bool isCookingActive = false;
        protected float currentCookingTime = 0f;
        protected float currentHeatValue = 0f;
 
        public float CurrentHeat => currentHeatValue;
        public float ElapsedCookingTime => currentCookingTime;
 
        protected virtual void Awake()
        {
            if (heatSlider != null) heatSlider.onValueChanged.AddListener(OnHeatSliderChanged);
            if (startCookingButton != null) startCookingButton.onClick.AddListener(ToggleCookingTimer);
        }
 
        protected virtual void Update()
        {
            if (!isCookingActive) return;
            currentCookingTime += Time.deltaTime;
            UpdateTimerText();
        }
 
        public override void SetStep(CookingStep step)
        {
            base.SetStep(step); // caches currentStep, calls RefreshUI()
 
            if (heatSlider != null)
            {
                heatSlider.minValue = 0f;
                heatSlider.maxValue = Mathf.Max(200f, step.targetTemperature * 1.5f);
                heatSlider.value = step.targetTemperature > 0f ? step.targetTemperature * 0.8f : 0f;
                currentHeatValue = heatSlider.value;
            }
 
            isCookingActive = false;
            currentCookingTime = 0f;
            UpdateTimerText();
 
            if (targetHeatText != null) targetHeatText.text = $"Target Heat: {step.targetTemperature:F0}\u00B0C";
            if (targetTimerText != null) targetTimerText.text = $"Target Time: {step.timeLimitSeconds:F0}s";
            if (startButtonText != null) startButtonText.text = "Start Cooking";
        }
 
        protected virtual void OnHeatSliderChanged(float value)
        {
            currentHeatValue = value;
            if (currentHeatText != null) currentHeatText.text = $"Heat: {currentHeatValue:F0}\u00B0C";
        }
 
        protected virtual void ToggleCookingTimer()
        {
            AudioManager.Instance?.PlayButtonSFX();
            isCookingActive = !isCookingActive;
            if (startButtonText != null)
            {
                startButtonText.text = isCookingActive ? "Pause Cooking" : "Resume Cooking";
            }
        }
 
        protected virtual void UpdateTimerText()
        {
            if (cookingTimerText != null) cookingTimerText.text = $"Time: {currentCookingTime:F1}s";
        }
    }
}