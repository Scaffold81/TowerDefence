using UnityEngine;

namespace Game.Services
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Game/Configs/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Header("Animation Settings")]
        public float defaultFadeDuration = 0.3f;
        public float defaultScaleDuration = 0.25f;
        public float defaultSlideDuration = 0.4f;
        
        [Header("Transition Settings")]
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Button Settings")]
        public float buttonClickScale = 0.95f;
        public float buttonHoverScale = 1.05f;
        public float buttonAnimationDuration = 0.1f;
        
        [Header("Popup Settings")]
        public float popupBackgroundAlpha = 0.8f;
        public Vector3 popupStartScale = Vector3.zero;
        public Vector3 popupEndScale = Vector3.one;
        
        [Header("Loading Settings")]
        public float loadingSpinSpeed = 360f;
        public string loadingText = "Loading...";
        public float loadingTextBlinkInterval = 0.5f;
        
        [Header("Tooltip Settings")]
        public float tooltipDelay = 0.5f;
        public Vector2 tooltipOffset = new Vector2(10, 10);
        public float tooltipMaxWidth = 300f;
        
        [Header("Notification Settings")]
        public float notificationDuration = 3f;
        public float notificationSlideDistance = 50f;
        public int maxNotifications = 5;
        
        [Header("Health Bar Settings")]
        public float healthBarUpdateSpeed = 2f;
        public Color healthBarGoodColor = Color.green;
        public Color healthBarMediumColor = Color.yellow;
        public Color healthBarLowColor = Color.red;
        [Range(0f, 1f)] public float healthBarMediumThreshold = 0.5f;
        [Range(0f, 1f)] public float healthBarLowThreshold = 0.25f;
        
        public Color GetHealthBarColor(float healthPercentage)
        {
            if (healthPercentage > healthBarMediumThreshold)
                return healthBarGoodColor;
            else if (healthPercentage > healthBarLowThreshold)
                return healthBarMediumColor;
            else
                return healthBarLowColor;
        }
        
        private void OnValidate()
        {
            defaultFadeDuration = Mathf.Clamp(defaultFadeDuration, 0.1f, 2f);
            defaultScaleDuration = Mathf.Clamp(defaultScaleDuration, 0.1f, 2f);
            defaultSlideDuration = Mathf.Clamp(defaultSlideDuration, 0.1f, 2f);
            buttonAnimationDuration = Mathf.Clamp(buttonAnimationDuration, 0.05f, 0.5f);
            tooltipDelay = Mathf.Clamp(tooltipDelay, 0.1f, 2f);
            notificationDuration = Mathf.Clamp(notificationDuration, 1f, 10f);
            maxNotifications = Mathf.Clamp(maxNotifications, 1, 20);
            healthBarUpdateSpeed = Mathf.Clamp(healthBarUpdateSpeed, 0.5f, 10f);
        }
    }
}