using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public interface ISubWindowManager : ISubWindowProvider, IDisposable
    {
        void Initialize(string windowTitle, FESInstaller installer);
        void CloseAllSubWinodows();
    }
}
