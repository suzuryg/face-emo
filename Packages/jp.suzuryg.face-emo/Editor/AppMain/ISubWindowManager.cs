using Suzuryg.FaceEmo.Detail.View;
using System;

namespace Suzuryg.FaceEmo.AppMain
{
    public interface ISubWindowManager : ISubWindowProvider, IDisposable
    {
        void Initialize(FaceEmoInstaller installer);
        void CloseAllSubWinodows();
    }
}
