using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MasakanTradisional.Data;
using MasakanTradisional.Core.FSM;
using MasakanTradisional.Gameplay.Evaluation;
using MasakanTradisional.Gameplay.Stations;
using MasakanTradisional.Core.Audio;
using UnityEngine.SceneManagement;
 
namespace MasakanTradisional.Gameplay
{
    /// <summary>
    /// Coordinator for Gameplay.unity. Owns recipe/step progression, fuzzy scoring,
    /// and result presentation. Delegates all per-station UI/interaction to
    /// IStationController implementations. Also gates step completion on
    /// ICollectibleStation.AllRequiredItemsCollected, which is what actually gives
    /// KitchenPrepState a gameplay purpose instead of just being a logged state.
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        [Header("Data & Fallback Settings")]
        [SerializeField] private RecipeData defaultFallbackRecipe;
        [SerializeField] private FuzzyCookingEvaluator fuzzyEvaluator;
 
        [Header("Station Controllers")]
        [SerializeField] private ShelvesController shelvesController;
        [SerializeField] private FridgeController fridgeController;
        [SerializeField] private CounterController counterController;
        [SerializeField] private StoveController stoveController;
        [SerializeField] private OvenController ovenController;
 
        [Header("Station Switcher Buttons")]
        [SerializeField] private Button navShelvesButton;
        [SerializeField] private Button navFridgeButton;
        [SerializeField] private Button navCounterButton;
        [SerializeField] private Button navStoveButton;
        [SerializeField] private Button navOvenButton;
 
        [Header("Camera Rig")]
        [Tooltip("Manages Cinemachine virtual camera switching between overview and station views.")]
        [SerializeField] private StationCameraRig cameraRig;
        [Tooltip("Button shown in station view to return to the main kitchen overview.")]
        [SerializeField] private Button backToOverviewButton;
 
        [Header("Step Information UI")]
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepInstructionText;
        [SerializeField] private TMP_Text stepProgressText;
        [SerializeField] private Image stepIllustrationImage;
        [SerializeField] private TMP_Text targetToolText;
 
        [Header("Step Completion")]
        [Tooltip("Single persistent 'Finish Step' button, always available regardless of which station panel is currently displayed. Becomes interactable only once all required items for the step are collected.")]
        [SerializeField] private Button finishStepButton;
        [Tooltip("Optional. Shown while items are still missing, hidden once the player can finish the step.")]
        [SerializeField] private TMP_Text collectionHintText;
 
        [Header("Result / Evaluation Modal")]
        [SerializeField] private GameObject resultModal;
        [SerializeField] private TMP_Text resultScoreText;
        [SerializeField] private TMP_Text resultGradeText;
        [SerializeField] private TMP_Text resultStatusLabelText;
        [SerializeField] private Image[] resultStars;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
 
        // Internal Gameplay Tracking
        private RecipeData activeRecipe;
        private int currentStepIndex = 0;
        private KitchenStationType currentStation = KitchenStationType.Counter;
        private Dictionary<KitchenStationType, IStationController> stationControllers;
 
        private List<FuzzyCookingEvaluator.EvaluationResult> stepResults = new List<FuzzyCookingEvaluator.EvaluationResult>();
 
        private void Awake()
        {
            BuildStationRegistry();
        }
 
        private void Start()
        {
            InitializeRecipeData();
            SetupButtonListeners();
            StartRecipeSession();
        }
 
        private void OnDestroy()
        {
            foreach (var kvp in stationControllers)
            {
                if (kvp.Value is ICollectibleStation collectible)
                {
                    collectible.OnCollectionChanged -= RefreshFinishButtonAvailability;
                }
            }
        }
 
        /// <summary>
        /// Registers every assigned station controller by its StationType, and
        /// subscribes to collection updates from any that implement ICollectibleStation.
        /// Leaving a slot unassigned simply means that station is skipped, not a crash.
        /// </summary>
        private void BuildStationRegistry()
        {
            stationControllers = new Dictionary<KitchenStationType, IStationController>();
 
            RegisterStation(shelvesController);
            RegisterStation(fridgeController);
            RegisterStation(counterController);
            RegisterStation(stoveController);
            RegisterStation(ovenController);
        }
 
        private void RegisterStation(IStationController controller)
        {
            if (controller == null) return;
            stationControllers[controller.StationType] = controller;
 
            if (controller is ICollectibleStation collectible)
            {
                collectible.OnCollectionChanged += RefreshFinishButtonAvailability;
            }
        }
 
        private void InitializeRecipeData()
        {
            if (GameStateMachine.Instance != null && GameStateMachine.Instance.CurrentSelectedRecipe != null)
            {
                activeRecipe = GameStateMachine.Instance.CurrentSelectedRecipe;
            }
            else
            {
                activeRecipe = defaultFallbackRecipe;
                Debug.LogWarning("[GameplayManager] No recipe selected from Main Menu. Using defaultFallbackRecipe for scene testing.");
            }
 
            if (fuzzyEvaluator == null)
            {
                fuzzyEvaluator = GetComponent<FuzzyCookingEvaluator>();
                if (fuzzyEvaluator == null) fuzzyEvaluator = gameObject.AddComponent<FuzzyCookingEvaluator>();
            }
        }
 
        private void SetupButtonListeners()
        {
            if (navShelvesButton != null) navShelvesButton.onClick.AddListener(() => SwitchStation(KitchenStationType.Shelves));
            if (navFridgeButton != null) navFridgeButton.onClick.AddListener(() => SwitchStation(KitchenStationType.Fridge));
            if (navCounterButton != null) navCounterButton.onClick.AddListener(() => SwitchStation(KitchenStationType.Counter));
            if (navStoveButton != null) navStoveButton.onClick.AddListener(() => SwitchStation(KitchenStationType.Stove));
            if (navOvenButton != null) navOvenButton.onClick.AddListener(() => SwitchStation(KitchenStationType.Oven));
 
            if (finishStepButton != null) finishStepButton.onClick.AddListener(CompleteCurrentStep);
 
            if (retryButton != null) retryButton.onClick.AddListener(RestartCookingSession);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (backToOverviewButton != null) backToOverviewButton.onClick.AddListener(ReturnToOverview);
        }
 
        public void StartRecipeSession()
        {
            if (activeRecipe == null)
            {
                Debug.LogError("[GameplayManager] Cannot start session! Active Recipe is null.");
                return;
            }
 
            currentStepIndex = 0;
            stepResults.Clear();
 
            if (resultModal != null) resultModal.SetActive(false);
            if (recipeNameText != null) recipeNameText.text = activeRecipe.recipeName;
 
            LoadStep(currentStepIndex);
        }
 
        private void LoadStep(int index)
        {
            if (activeRecipe == null || index < 0 || index >= activeRecipe.StepCount) return;
 
            CookingStep step = activeRecipe.GetStep(index);
            KitchenStationType homeStation = RecommendStationForStep(step.actionType);
 
            // Ask the station itself whether it's heat-based, instead of maintaining
            // a second switch statement that has to be kept in sync with the first.
            if (GameStateMachine.Instance != null &&
                stationControllers.TryGetValue(homeStation, out IStationController homeController) &&
                homeController is IHeatStationController)
            {
                GameStateMachine.Instance.GoToCooking();
            }
 
            if (stepTitleText != null) stepTitleText.text = step.stepTitle;
            if (stepInstructionText != null) stepInstructionText.text = step.instruction;
            if (stepProgressText != null) stepProgressText.text = $"Step {index + 1} / {activeRecipe.StepCount}";
            if (stepIllustrationImage != null) stepIllustrationImage.sprite = step.stepIllustration;
            if (targetToolText != null) targetToolText.text = $"Required Tool: {(string.IsNullOrEmpty(step.requiredTool.toolName) ? "None" : step.requiredTool.toolName)}";
 
            // Sync EVERY station to the new step, even hidden ones, so collection
            // state and heat/timer resets are correct no matter which panel the
            // player looks at first.
            foreach (var kvp in stationControllers)
            {
                kvp.Value.SetStep(step);
            }
 
            SwitchStation(homeStation);
            RefreshFinishButtonAvailability();
        }
 
        /// <summary>
        /// Pure visibility toggle - shows the target station's panel and hides every
        /// other registered station. Does not touch step data (see SetStep in LoadStep).
        /// Called internally by LoadStep (no camera change) and by NavigateToStation (with camera change).
        /// </summary>
        public void SwitchStation(KitchenStationType targetStation)
        {
            AudioManager.Instance?.PlayButtonSFX();
 
            foreach (var kvp in stationControllers)
            {
                if (kvp.Key == targetStation) kvp.Value.Show();
                else kvp.Value.Hide();
            }
 
            currentStation = targetStation;
        }
 
        /// <summary>
        /// Called by KitchenStationInteractable (world-space tap) when the player taps
        /// an appliance in the overview. Zooms the Cinemachine camera to the station
        /// and then opens its panel. Use this instead of SwitchStation() for player-
        /// initiated navigation — SwitchStation() alone does not change the camera.
        /// </summary>
        public void NavigateToStation(KitchenStationType targetStation)
        {
            cameraRig?.ZoomToStation(targetStation);
            SwitchStation(targetStation);
        }
 
        /// <summary>
        /// Returns the Cinemachine camera to the overview shot.
        /// Wired to the Back button in the station HUD.
        /// Does not close the current station panel — the overview is just for navigation.
        /// </summary>
        public void ReturnToOverview()
        {
            AudioManager.Instance?.PlayButtonSFX();
            cameraRig?.ReturnToOverview();
        }
 
        private KitchenStationType RecommendStationForStep(StepActionType actionType)
        {
            switch (actionType)
            {
                case StepActionType.Prepare:
                    return KitchenStationType.Fridge;
                case StepActionType.Chop:
                case StepActionType.Grind:
                case StepActionType.Mix:
                case StepActionType.PlateAndGarnish:
                    return KitchenStationType.Counter;
                case StepActionType.Saute:
                case StepActionType.Boil:
                case StepActionType.Simmer:
                case StepActionType.Fry:
                    return KitchenStationType.Stove;
                case StepActionType.Bake:
                    return KitchenStationType.Oven;
                default:
                    return KitchenStationType.Counter;
            }
        }
 
        /// <summary>
        /// True only when every registered ICollectibleStation reports its items
        /// collected for the current step (stations with no items for this step are
        /// vacuously satisfied, so a pure-Stove step never gets blocked by an
        /// empty Shelves panel).
        /// </summary>
        private bool AreAllRequiredItemsCollected()
        {
            foreach (var kvp in stationControllers)
            {
                if (kvp.Value is ICollectibleStation collectible && !collectible.AllRequiredItemsCollected)
                {
                    return false;
                }
            }
            return true;
        }
 
        private void RefreshFinishButtonAvailability()
        {
            bool ready = AreAllRequiredItemsCollected();
 
            if (finishStepButton != null) finishStepButton.interactable = ready;
            if (collectionHintText != null)
            {
                if (!ready)
                {
                    collectionHintText.text = "Collect all ingredients first!";
                }
                collectionHintText.gameObject.SetActive(!ready);
            }
        }
 
        public void CompleteCurrentStep()
        {
            if (!AreAllRequiredItemsCollected())
            {
                Debug.LogWarning("[GameplayManager] Blocked: not all required items are collected yet.");
                return;
            }
 
            AudioManager.Instance?.PlayButtonSFX();
 
            CookingStep step = activeRecipe.GetStep(currentStepIndex);
            KitchenStationType homeStation = RecommendStationForStep(step.actionType);
            FuzzyCookingEvaluator.EvaluationResult evalResult;
 
            FuzzyCookingData activeFuzzyData = step.fuzzyData != null ? step.fuzzyData : activeRecipe.DefaultFuzzyData;
 
            if (stationControllers.TryGetValue(homeStation, out IStationController controller) &&
                controller is IHeatStationController heatController &&
                step.targetTemperature > 0f && step.timeLimitSeconds > 0f)
            {
                evalResult = fuzzyEvaluator.EvaluateCooking(
                    heatController.CurrentHeat,
                    step.targetTemperature,
                    heatController.ElapsedCookingTime,
                    step.timeLimitSeconds,
                    activeFuzzyData
                );
            }
            else
            {
                // Non-heat steps (Prepare/Chop/Mix/PlateAndGarnish) auto-resolve to a
                // perfect ratio - there's no slider/timer to score them against.
                evalResult = fuzzyEvaluator.EvaluateRatios(1.0f, 1.0f, activeFuzzyData);
            }
 
            stepResults.Add(evalResult);
            Debug.Log($"[GameplayManager] Step {currentStepIndex + 1} Score: {evalResult.FinalScore:F1} | Status: {evalResult.StatusLabel} | Grade: {evalResult.LetterGrade}");
 
            currentStepIndex++;
 
            if (currentStepIndex < activeRecipe.StepCount)
            {
                LoadStep(currentStepIndex);
            }
            else
            {
                ShowFinalResults();
            }
        }
 
        private void ShowFinalResults()
        {
            if (stepResults.Count == 0) return;
 
            float totalScore = 0f;
            foreach (var res in stepResults) totalScore += res.FinalScore;
            float averageScore = totalScore / stepResults.Count;
 
            int earnedStars = 0;
            if (averageScore >= 90f) earnedStars = 3;
            else if (averageScore >= 70f) earnedStars = 2;
            else if (averageScore >= 50f) earnedStars = 1;
 
            if (activeRecipe != null && !string.IsNullOrEmpty(activeRecipe.recipeId))
            {
                string starKey = $"Recipe_Stars_{activeRecipe.recipeId}";
                int existingStars = PlayerPrefs.GetInt(starKey, 0);
                if (earnedStars > existingStars)
                {
                    PlayerPrefs.SetInt(starKey, earnedStars);
                    PlayerPrefs.Save();
                }
            }
 
            if (resultScoreText != null) resultScoreText.text = $"Final Score: {averageScore:F1} / 100";
            if (resultGradeText != null) resultGradeText.text = DetermineLetterGrade(averageScore);
            if (resultStatusLabelText != null) resultStatusLabelText.text = GetOverallStatusSummary(averageScore);
 
            for (int i = 0; i < resultStars.Length; i++)
            {
                if (resultStars[i] != null)
                {
                    resultStars[i].sprite = (i < earnedStars) ? filledStarSprite : emptyStarSprite;
                }
            }
 
            if (resultModal != null) resultModal.SetActive(true);
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
 
        private string GetOverallStatusSummary(float score)
        {
            if (score >= 90f) return "Sempurna! Quality Masterpiece!";
            if (score >= 75f) return "Lezat & Sangat Baik!";
            if (score >= 60f) return "Cukup Baik, Perlu Sedikit Latihan.";
            return "Masakan Kurang Pas / Gosong. Coba Lagi!";
        }
 
        public void RestartCookingSession()
        {
            AudioManager.Instance?.PlayButtonSFX();
            StartRecipeSession();
        }
 
        public void ReturnToMainMenu()
        {
            AudioManager.Instance?.PlayButtonSFX();
            if (GameStateMachine.Instance != null) GameStateMachine.Instance.GoToRecipeSelection();
            SceneManager.LoadScene("MainMenu");
        }
    }
}