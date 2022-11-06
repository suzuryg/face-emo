using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Zenject;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class EditorLauncher : EditorWindow
    {
        [MenuItem("Window/Suzuryg/FacialExpressionSwitcher")]
        public static void ShowWindow() => GetWindow<EditorLauncher>();

        private MainWindow _mainWindow;

        private void OnEnable()
        {
            var editorInstaller = new EditorInstaller();
            editorInstaller.Install();

            _mainWindow?.Dispose();

            // Resolveの処理が重ければAwakeで実行することを検討する
            // Awakeで実行する場合はUXMLとUSSのリロードができない？
            _mainWindow = editorInstaller.Container.Resolve<MainWindow>();
            _mainWindow.Apply(rootVisualElement);
            titleContent.text = CommonSetting.SystemName;
        }

        private void OnDestroy()
        {
            _mainWindow?.Dispose();
        }
    }
}
