using DG.Tweening;
using R3;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PageBase : MonoBehaviour, IPageBase
    {
        [SerializeField]
        protected CanvasGroup canvasGroup;
        protected float durationTime = 0.1f;
       
        protected ReactiveProperty<bool> isShowed = new ReactiveProperty<bool>();
        
        public virtual void ShowAsLastSibling(float showTime = 0.1f)
        {
            transform.SetAsLastSibling();
            
            canvasGroup.DOFade(1, showTime).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        public virtual void Show(float showTime = 0.1f)
        {
            isShowed.Value = true;
            canvasGroup.DOFade(1, showTime).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        public virtual void Hide(float hideTime = 0.1f)
        {
            isShowed.Value = false;
            canvasGroup.DOFade(0, hideTime).OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
        }
    }
}
