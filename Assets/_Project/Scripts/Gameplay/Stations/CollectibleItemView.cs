using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// One clickable ingredient/tool entry inside a collectible station panel.
    /// Spawned at runtime by IngredientDisplayStationBase for each item this
    /// station is responsible for on the current step. Build the prefab as:
    /// Button root -> Icon (Image) + NameLabel (TMP_Text) + CollectedCheckmark (Image/GameObject, starts inactive).
    /// </summary>
    public class CollectibleItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button clickButton;
        [SerializeField] private GameObject collectedCheckmark;
 
        private bool isCollected;
        private Action onCollectedCallback;
 
        public void Setup(Sprite icon, string displayName, Action onCollected)
        {
            if (iconImage != null) iconImage.sprite = icon;
            if (nameText != null) nameText.text = displayName;
 
            isCollected = false;
            if (collectedCheckmark != null) collectedCheckmark.SetActive(false);
 
            onCollectedCallback = onCollected;
 
            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(HandleClick);
                clickButton.interactable = true;
            }
        }
 
        private void HandleClick()
        {
            if (isCollected) return;
            isCollected = true;
 
            if (collectedCheckmark != null) collectedCheckmark.SetActive(true);
            if (clickButton != null) clickButton.interactable = false;
 
            onCollectedCallback?.Invoke();
        }
    }
}