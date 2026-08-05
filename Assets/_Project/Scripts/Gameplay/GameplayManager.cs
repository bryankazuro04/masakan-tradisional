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
    /// IStationController implementations - this class never touches a slider,
    /// a heat value, or a timer directly. Adding a new station = write a controller
    /// + assign it in the inspector, no edits needed here beyond that assignment.
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
 
        [Header("Step Information UI")]
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepInstructionText;
        [SerializeField] private TMP_Text stepProgressText;
        [SerializeField] private Image stepIllustrationImage;
        [SerializeField] private TMP_Text targetToolText;
 
        [Header("Step Completion")]
        [Tooltip("Single persistent 'Finish Step' button, always available regardless of which station panel is currently displayed.")]
        [SerializeField] private Button finishStepButton;
 
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
 
        /// <summary>
        /// Registers every assigned station controller by its StationType.
        /// Leaving a slot unassigned (e.g. while incrementally building the scene)
        /// simply means that station is skipped, not a null-ref crash.
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
 
            if (GameStateMachine.Instance != null && IsHeatBasedAction(step.actionType))
            {
                GameStateMachine.Instance.GoToCooking();
            }
 
            if (stepTitleText != null) stepTitleText.text = step.stepTitle;
            if (stepInstructionText != null) stepInstructionText.text = step.instruction;
            if (stepProgressText != null) stepProgressText.text = $"Step {index + 1} / {activeRecipe.StepCount}";
            if (stepIllustrationImage != null) stepIllustrationImage.sprite = step.stepIllustration;
            if (targetToolText != null) targetToolText.text = $"Required Tool: {(string.IsNullOrEmpty(step.requiredTool.toolName) ? "None" : step.requiredTool.toolName)}";
 
            // Auto-navigate to the recommended station for this step type
            SwitchStation(RecommendStationForStep(step.actionType));
        }
 
        /// <summary>
        /// Activates the target station's controller and deactivates every other
        /// registered station. Nav buttons call this for free browsing; LoadStep
        /// calls it to auto-navigate to a step's home station.
        /// </summary>
        public void SwitchStation(KitchenStationType targetStation)
        {
            AudioManager.Instance?.PlayButtonSFX();
 
            CookingStep step = (activeRecipe != null && currentStepIndex < activeRecipe.StepCount)
                ? activeRecipe.GetStep(currentStepIndex)
                : null;
 
            foreach (var kvp in stationControllers)
            {
                if (kvp.Key == targetStation)
                {
                    kvp.Value.Activate(step);
                }
                else
                {
                    kvp.Value.Deactivate();
                }
            }
 
            currentStation = targetStation;
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
 
        private bool IsHeatBasedAction(StepActionType actionType)
        {
            return actionType == StepActionType.Saute ||
                   actionType == StepActionType.Boil ||
                   actionType == StepActionType.Simmer ||
                   actionType == StepActionType.Fry ||
                   actionType == StepActionType.Bake;
        }
 
        public void CompleteCurrentStep()
        {
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