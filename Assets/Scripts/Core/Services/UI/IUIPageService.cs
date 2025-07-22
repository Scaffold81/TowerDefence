using Game.UI;
using System;
using System.Collections.Generic;

namespace Game.Services
{
    public interface IUIPageService
    {
        Dictionary<Type, IPageBase> Pages { get;}

        void AddPage(Type key, IPageBase page);
        void ShowOn(Type key);
        void ShowOnTop(Type key);
    }
}
