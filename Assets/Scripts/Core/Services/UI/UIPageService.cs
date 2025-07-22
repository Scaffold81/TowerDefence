using Game.UI;
using System;
using System.Collections.Generic;
using Zenject;

namespace Game.Services
{
    public class UIPageService : IUIPageService
    {
        public Dictionary<Type, IPageBase> Pages { get;private set; }

        [Inject]
        void Construct()
        {
            Pages = new Dictionary<Type, IPageBase>();
        }

        public void AddPage(Type key, IPageBase page)
        {
            Pages.Add(key,page);
        }

        public void ShowOnTop(Type key)
        {
            if (Pages.ContainsKey(key))
            {
                Pages[key].ShowAsLastSibling();
            }
            else
            {
                UnityEngine.Debug.Log("Page not found in dictionary.");
            }
        }

        public void ShowOn(Type key)
        {

            if (Pages.ContainsKey(key))
            {
                Pages[key].Show();
            }
            else
            {
                UnityEngine.Debug.Log("Page not found in dictionary.");
            }
        }
    }
}
