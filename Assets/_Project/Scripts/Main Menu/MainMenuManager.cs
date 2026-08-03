using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MasakanTradisional.Core.Audio;

namespace MasakanTradisional.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject recipeSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject exitConfirmationModal;

        [Header("Main Navigation Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;

        [Header("Modal Buttons")]
        [SerializeField] private Button confirmExitButton;
        [SerializeField] private Button cancelExitButton;

        [Header("Back Buttons")]
        [SerializeField] private Button[] backButtons;

        private GameObject currentActivePanel;

        private void Start()
        {
            // Bind Button Click Events
            if (playButton != null) playButton.onClick.AddListener(() => SwitchPanel(recipeSelectPanel));
            if (settingsButton != null) settingsButton.onClick.AddListener(() => SwitchPanel(settingsPanel));
            if (creditsButton != null) creditsButton.onClick.AddListener(() => SwitchPanel(creditsPanel));
            if (exitButton != null) exitButton.onClick.AddListener(ShowExitModal);

            if (confirmExitButton != null) confirmExitButton.onClick.AddListener(QuitApplication);
            if (cancelExitButton != null) cancelExitButton.onClick.AddListener(HideExitModal);

            foreach (var backBtn in backButtons)
            {
                if (backBtn != null)
                {
                    backBtn.onClick.AddListener(() => SwitchPanel(mainPanel));
                }
            }

            // Initialize Menu State: Disable all panels first to avoid overlap
            if (mainPanel != null) mainPanel.SetActive(false);
            if (recipeSelectPanel != null) recipeSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (exitConfirmationModal != null) exitConfirmationModal.SetActive(false);

            // Switch to Main Panel
            SwitchPanel(mainPanel);
        }

        public void SwitchPanel(GameObject targetPanel)
        {
            AudioManager.Instance?.PlayButtonSFX();

            if (currentActivePanel != null)
            {
                currentActivePanel.SetActive(false);
            }

            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                currentActivePanel = targetPanel;
            }
        }

        public void ShowExitModal()
        {
            AudioManager.Instance?.PlayButtonSFX();
            if (exitConfirmationModal != null) exitConfirmationModal.SetActive(true);
        }

        public void HideExitModal()
        {
            AudioManager.Instance?.PlayButtonSFX();
            if (exitConfirmationModal != null) exitConfirmationModal.SetActive(false);
        }

        public void LoadGameplayScene(string sceneName)
        {
            AudioManager.Instance?.PlayButtonSFX();
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }

        private void QuitApplication()
        {
            AudioManager.Instance?.PlayButtonSFX();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}