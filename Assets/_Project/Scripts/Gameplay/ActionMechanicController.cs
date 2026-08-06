using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MasakanTradisional.Data;
using MasakanTradisional.Core.Audio;

namespace MasakanTradisional.Gameplay
{
    /// <summary>
    /// Mini-game controller for player actions like Chopping, Mixing, Stirring, and Grinding.
    /// Handles tool display (Knife, Mixer, Mortar & Pestle, Spatula), player gestures (tap, hold, drag),
    /// visual animations (punch scale, rotation), audio feedback, and score calculation.
    /// </summary>
    public class ActionMechanicController : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject actionPanelRoot;
        [SerializeField] private Image toolIconImage;
        [SerializeField] private TMP_Text toolNameText;
        [SerializeField] private TMP_Text actionInstructionText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text progressPercentText;
        [SerializeField] private RectTransform actionTargetArea;
        [SerializeField] private Image toolGraphicImage;
        [SerializeField] private Image ingredientGraphicImage;

        [Header("Fallback Tool Icons")]
        [SerializeField] private Sprite defaultKnifeIcon;
        [SerializeField] private Sprite defaultMixerIcon;
        [SerializeField] private Sprite defaultSpatulaIcon;
        [SerializeField] private Sprite defaultMortarIcon;

        [Header("Action Parameters")]
        [Tooltip("Number of taps required for chop/grind actions to reach 100%.")]
        [SerializeField] private int tapsToComplete = 10;
        [Tooltip("Seconds of continuous hold/drag required for mix/stir actions to reach 100%.")]
        [SerializeField] private float holdDurationToComplete = 4.0f;
        [Tooltip("Animation speed for tool rotation during mixing.")]
        [SerializeField] private float rotationSpeed = 360.0f;

        // Runtime Action State
        private CookingStep currentStep;
        private StepActionType actionType;
        private bool isActionRequired = false;
        private bool isActionCompleted = false;
        private float currentProgress = 0f; // 0.0 to 1.0
        private float qualityScore = 1.0f;  // 0.0 to 1.0
        private int currentTapCount = 0;
        private bool isHoldingPointer = false;
        private Vector3 originalToolScale = Vector3.one;

        // Events
        public event Action OnActionProgressUpdated;
        public event Action OnActionCompleted;

        // Public Accessors
        public bool IsActionRequired => isActionRequired;
        public bool IsActionCompleted => isActionCompleted;
        public float ActionProgress => currentProgress;
        public float ActionQualityScore => qualityScore;

        private void Awake()
        {
            EnsureRuntimeUI();
            if (toolGraphicImage != null)
            {
                originalToolScale = toolGraphicImage.transform.localScale;
            }
            Hide();
        }

        public void EnsureRuntimeUI()
        {
            if (actionPanelRoot != null && progressSlider != null && actionInstructionText != null) return;

            // Ensure actionPanelRoot container exists
            if (actionPanelRoot == null)
            {
                actionPanelRoot = new GameObject("ActionPanelOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                actionPanelRoot.transform.SetParent(transform, false);

                RectTransform panelRect = actionPanelRoot.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.15f, 0.15f);
                panelRect.anchorMax = new Vector2(0.85f, 0.85f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                Image panelBg = actionPanelRoot.GetComponent<Image>();
                panelBg.color = new Color(0.12f, 0.12f, 0.18f, 0.92f); // Dark translucent modal panel
            }

            if (actionTargetArea == null)
            {
                actionTargetArea = actionPanelRoot.GetComponent<RectTransform>();
            }

            // Instruction Text
            if (actionInstructionText == null)
            {
                GameObject instrObj = new GameObject("InstructionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                instrObj.transform.SetParent(actionPanelRoot.transform, false);
                actionInstructionText = instrObj.GetComponent<TMP_Text>();
                actionInstructionText.fontSize = 22;
                actionInstructionText.alignment = TextAlignmentOptions.Center;
                actionInstructionText.color = Color.white;

                RectTransform rt = instrObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.78f);
                rt.anchorMax = new Vector2(0.95f, 0.95f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Tool Name Text
            if (toolNameText == null)
            {
                GameObject toolNameObj = new GameObject("ToolNameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                toolNameObj.transform.SetParent(actionPanelRoot.transform, false);
                toolNameText = toolNameObj.GetComponent<TMP_Text>();
                toolNameText.fontSize = 20;
                toolNameText.alignment = TextAlignmentOptions.Center;
                toolNameText.color = new Color(1.0f, 0.85f, 0.3f); // Gold accent

                RectTransform rt = toolNameObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.65f);
                rt.anchorMax = new Vector2(0.95f, 0.77f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Tool Graphic Image
            if (toolGraphicImage == null)
            {
                GameObject toolImgObj = new GameObject("ToolGraphicImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                toolImgObj.transform.SetParent(actionPanelRoot.transform, false);
                toolGraphicImage = toolImgObj.GetComponent<Image>();

                RectTransform rt = toolImgObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.35f, 0.3f);
                rt.anchorMax = new Vector2(0.65f, 0.62f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                originalToolScale = toolGraphicImage.transform.localScale;
            }

            // Progress Slider
            if (progressSlider == null)
            {
                GameObject sliderObj = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(actionPanelRoot.transform, false);
                progressSlider = sliderObj.GetComponent<Slider>();

                RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0.1f, 0.12f);
                sliderRect.anchorMax = new Vector2(0.9f, 0.24f);
                sliderRect.offsetMin = Vector2.zero;
                sliderRect.offsetMax = Vector2.zero;

                // Background
                GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgObj.transform.SetParent(sliderObj.transform, false);
                Image bgImg = bgObj.GetComponent<Image>();
                bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1.0f);
                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;

                // Fill Area & Fill
                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(sliderObj.transform, false);
                RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;

                GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillObj.transform.SetParent(fillArea.transform, false);
                Image fillImg = fillObj.GetComponent<Image>();
                fillImg.color = new Color(0.2f, 0.85f, 0.45f, 1.0f); // Bright green fill
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;

                progressSlider.targetGraphic = bgImg;
                progressSlider.fillRect = fillRect;
            }

            // Progress Percent Text
            if (progressPercentText == null)
            {
                GameObject pctObj = new GameObject("ProgressPercentText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                pctObj.transform.SetParent(actionPanelRoot.transform, false);
                progressPercentText = pctObj.GetComponent<TMP_Text>();
                progressPercentText.fontSize = 18;
                progressPercentText.alignment = TextAlignmentOptions.Center;
                progressPercentText.color = Color.white;

                RectTransform rt = pctObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.1f, 0.02f);
                rt.anchorMax = new Vector2(0.9f, 0.12f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        private void Update()
        {
            if (!isActionRequired || isActionCompleted) return;

            // Handle continuous actions (Mixing / Stirring / Sauteing) while holding pointer
            if (isHoldingPointer)
            {
                if (actionType == StepActionType.Mix || actionType == StepActionType.Saute ||
                    actionType == StepActionType.Boil || actionType == StepActionType.Simmer)
                {
                    AddContinuousProgress(Time.deltaTime / Mathf.Max(0.5f, holdDurationToComplete));

                    // Tool rotation effect while mixing/stirring
                    if (toolGraphicImage != null)
                    {
                        toolGraphicImage.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the action controller for a given cooking step.
        /// </summary>
        public void SetStep(CookingStep step)
        {
            currentStep = step;
            currentProgress = 0f;
            currentTapCount = 0;
            isActionCompleted = false;
            isHoldingPointer = false;
            qualityScore = 1.0f;

            if (step == null)
            {
                isActionRequired = false;
                Hide();
                return;
            }

            actionType = step.actionType;
            isActionRequired = DetermineIfActionRequired(actionType);

            if (!isActionRequired)
            {
                // Non-action step auto-completes
                isActionCompleted = true;
                currentProgress = 1.0f;
                Hide();
                return;
            }

            SetupUIForStep();
        }

        private bool DetermineIfActionRequired(StepActionType type)
        {
            switch (type)
            {
                case StepActionType.Prepare:
                case StepActionType.Chop:
                case StepActionType.Grind:
                case StepActionType.Mix:
                case StepActionType.Saute:
                case StepActionType.PlateAndGarnish:
                    return true;
                default:
                    return false;
            }
        }

        private void SetupUIForStep()
        {
            // Set Tool Icon & Name
            Sprite toolSprite = null;
            string toolName = "Tool";

            if (currentStep.requiredTool.icon != null)
            {
                toolSprite = currentStep.requiredTool.icon;
                toolName = currentStep.requiredTool.toolName;
            }
            else
            {
                toolSprite = GetDefaultToolSprite(actionType);
                toolName = GetDefaultToolName(actionType);
            }

            if (toolIconImage != null) toolIconImage.sprite = toolSprite;
            if (toolGraphicImage != null)
            {
                toolGraphicImage.sprite = toolSprite;
                toolGraphicImage.color = toolSprite != null ? Color.white : new Color(1.0f, 0.7f, 0.2f, 1.0f);
            }
            if (toolNameText != null) toolNameText.text = toolName;

            // Set Instructions
            if (actionInstructionText != null)
            {
                actionInstructionText.text = GetActionPromptText(actionType, toolName);
            }

            UpdateProgressUI();
        }

        private Sprite GetDefaultToolSprite(StepActionType type)
        {
            switch (type)
            {
                case StepActionType.Prepare:
                case StepActionType.Chop: return defaultKnifeIcon;
                case StepActionType.Mix: return defaultMixerIcon;
                case StepActionType.Saute: return defaultSpatulaIcon;
                case StepActionType.Grind: return defaultMortarIcon;
                default: return defaultKnifeIcon;
            }
        }

        private string GetDefaultToolName(StepActionType type)
        {
            switch (type)
            {
                case StepActionType.Prepare: return "Pisau & Talenan (Knife & Board)";
                case StepActionType.Chop: return "Pisau Dapur (Knife)";
                case StepActionType.Mix: return "Mixer / Whisk";
                case StepActionType.Saute: return "Spatula";
                case StepActionType.Grind: return "Cobek & Ulekan";
                case StepActionType.PlateAndGarnish: return "Sendok Saji";
                default: return "Alat Dapur";
            }
        }

        private string GetActionPromptText(StepActionType type, string toolName)
        {
            switch (type)
            {
                case StepActionType.Prepare:
                    return $"Ketuk (Tap) area untuk mempersiapkan & memotong bahan menggunakan {toolName}!";
                case StepActionType.Chop:
                    return $"Ketuk (Tap) area untuk memotong bahan dengan {toolName}!";
                case StepActionType.Mix:
                    return $"Tahan & gerakkan jari/mouse untuk mengocok/mengaduk adonan dengan {toolName}!";
                case StepActionType.Saute:
                    return $"Aduk tumisan secara merata menggunakan {toolName}!";
                case StepActionType.Grind:
                    return $"Ketuk & tekan untuk melumatkan bumbu dengan {toolName}!";
                case StepActionType.PlateAndGarnish:
                    return $"Ketuk untuk menata sajian dengan {toolName}!";
                default:
                    return $"Lakukan aksi menggunakan {toolName}!";
            }
        }

        // -------------------------------------------------------------------------
        // Input Event Handlers (Tap / Drag / Hold)
        // -------------------------------------------------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isActionRequired || isActionCompleted) return;

            // Discrete tap actions (Prepare, Chopping, Grinding, Plating)
            if (actionType == StepActionType.Prepare || actionType == StepActionType.Chop ||
                actionType == StepActionType.Grind || actionType == StepActionType.PlateAndGarnish)
            {
                currentTapCount++;
                float progressIncrement = 1.0f / Mathf.Max(1, tapsToComplete);
                AddProgress(progressIncrement);

                AnimateToolTap();
                AudioManager.Instance?.PlayButtonSFX();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActionRequired || isActionCompleted) return;
            isHoldingPointer = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isHoldingPointer = false;

            // Reset tool rotation transform on release
            if (toolGraphicImage != null)
            {
                toolGraphicImage.transform.localScale = originalToolScale;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActionRequired || isActionCompleted) return;

            // Dragging over target also boosts continuous progress (Mixing / Stirring)
            if (actionType == StepActionType.Mix || actionType == StepActionType.Saute)
            {
                float dragDelta = eventData.delta.magnitude * 0.002f;
                AddProgress(dragDelta);
            }
        }

        // -------------------------------------------------------------------------
        // Progress Logic & Visual Animation
        // -------------------------------------------------------------------------

        private void AddProgress(float amount)
        {
            currentProgress = Mathf.Clamp01(currentProgress + amount);
            UpdateProgressUI();

            OnActionProgressUpdated?.Invoke();

            if (currentProgress >= 1.0f && !isActionCompleted)
            {
                CompleteAction();
            }
        }

        private void AddContinuousProgress(float amount)
        {
            AddProgress(amount);
        }

        private void CompleteAction()
        {
            isActionCompleted = true;
            isHoldingPointer = false;
            qualityScore = 1.0f; // Perfect completion score

            if (actionInstructionText != null)
            {
                actionInstructionText.text = "Selesai! Action Completed!";
            }

            AnimateCompletion();
            AudioManager.Instance?.PlayButtonSFX();

            OnActionCompleted?.Invoke();
        }

        private void UpdateProgressUI()
        {
            if (progressSlider != null) progressSlider.value = currentProgress;
            if (progressPercentText != null) progressPercentText.text = $"{Mathf.RoundToInt(currentProgress * 100f)}%";
        }

        private void AnimateToolTap()
        {
            if (toolGraphicImage == null) return;

            // Quick punch scale animation
            toolGraphicImage.transform.localScale = originalToolScale * 1.25f;
            CancelInvoke(nameof(ResetToolScale));
            Invoke(nameof(ResetToolScale), 0.1f);
        }

        private void ResetToolScale()
        {
            if (toolGraphicImage != null)
            {
                toolGraphicImage.transform.localScale = originalToolScale;
            }
        }

        private void AnimateCompletion()
        {
            if (toolGraphicImage != null)
            {
                toolGraphicImage.transform.localScale = originalToolScale * 1.4f;
                CancelInvoke(nameof(ResetToolScale));
                Invoke(nameof(ResetToolScale), 0.3f);
            }
        }

        // -------------------------------------------------------------------------
        // Panel Control
        // -------------------------------------------------------------------------

        public void Show()
        {
            if (actionPanelRoot != null) actionPanelRoot.SetActive(true);
            else gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (actionPanelRoot != null) actionPanelRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
