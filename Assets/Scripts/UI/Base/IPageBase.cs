namespace Game.UI
{
    public interface IPageBase
    {
        void Hide(float hideTime = 0.1F);
        void Show(float showTime = 0.1F);
        void ShowAsLastSibling(float showTime = 0.1F);
    }
}
