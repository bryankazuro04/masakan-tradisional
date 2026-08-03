using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace MasakanTradisional.UI.MainMenu
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio Setup")]
        [SerializeField] private AudioMixer mainAudioMixer;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Graphics & Performance")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown frameRateDropdown;

        private const string BGM_PARAM = "BGMVolume";
        private const string SFX_PARAM = "SFXVolume";

        private void Start()
        {
            LoadSettings();

            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
            if (frameRateDropdown != null) frameRateDropdown.onValueChanged.AddListener(SetTargetFrameRate);
        }

        public void SetBGMVolume(float value)
        {
            float db = ConvertToDecibels(value);
            if (mainAudioMixer != null) mainAudioMixer.SetFloat(BGM_PARAM, db);
            PlayerPrefs.SetFloat("BGM_Volume", value);
        }

        public void SetSFXVolume(float value)
        {
            float db = ConvertToDecibels(value);
            if (mainAudioMixer != null) mainAudioMixer.SetFloat(SFX_PARAM, db);
            PlayerPrefs.SetFloat("SFX_Volume", value);
        }

        public void SetQualityLevel(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt("GraphicsQuality", index);
        }

        public void SetTargetFrameRate(int index)
        {
            // 0 = 30 FPS (Battery Saver), 1 = 60 FPS (Smooth)
            int targetFps = (index == 0) ? 30 : 60;
            Application.targetFrameRate = targetFps;
            PlayerPrefs.SetInt("TargetFrameRate", targetFps);
        }

        private float ConvertToDecibels(float linear)
        {
            // Clamp minimum to avoid log10(0) evaluation
            linear = Mathf.Clamp(linear, 0.0001f, 1f);
            return Mathf.Log10(linear) * 20f;
        }

        private void LoadSettings()
        {
            float savedBgm = PlayerPrefs.GetFloat("BGM_Volume", 0.8f);
            float savedSfx = PlayerPrefs.GetFloat("SFX_Volume", 0.8f);
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
            int savedFps = PlayerPrefs.GetInt("TargetFrameRate", 60);

            if (bgmSlider != null) bgmSlider.value = savedBgm;
            if (sfxSlider != null) sfxSlider.value = savedSfx;
            if (qualityDropdown != null) qualityDropdown.value = savedQuality;
            if (frameRateDropdown != null) frameRateDropdown.value = (savedFps == 30) ? 0 : 1;

            SetBGMVolume(savedBgm);
            SetSFXVolume(savedSfx);
            SetQualityLevel(savedQuality);
            Application.targetFrameRate = savedFps;
        }
    }
}