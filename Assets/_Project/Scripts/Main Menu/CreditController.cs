using UnityEngine;
using UnityEngine.UI;

namespace MasakanTradisional.UI.MainMenu
{
    public class CreditsController : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float scrollSpeed = 0.05f;

        private bool isAutoScrolling = true;

        private void OnEnable()
        {
            ResetScroll();
        }

        private void Update()
        {
            if (!isAutoScrolling || scrollRect == null) return;

            if (scrollRect.verticalNormalizedPosition > 0f)
            {
                scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
            }
            else
            {
                ResetScroll();
            }
        }

        public void OnUserDragStart()
        {
            isAutoScrolling = false;
        }

        public void ResetScroll()
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            isAutoScrolling = true;
        }
    }
}