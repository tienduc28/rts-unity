using UnityEngine;

namespace RTS.UI.Components
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform mask;
        [SerializeField] private Vector2 padding = new(9, 8);
        private RectTransform maskParentRectTransform;

        private void Awake()
        {
            if (mask == null)
            {
                Debug.LogError($"Progress bar {name} is missing mask");
                return;
            }
            maskParentRectTransform = mask.parent as RectTransform;
        }

        public void SetProgress(float progress)
        {
            Vector2 parentSize = maskParentRectTransform.sizeDelta - padding * 2;
            Vector2 targetSize = parentSize;

            targetSize.x *= Mathf.Clamp01(progress);

            mask.offsetMin = padding;
            mask.offsetMax = new Vector2(targetSize.x - parentSize.x - padding.x, -padding.y);
        }
    }
}