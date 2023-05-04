using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class ModeNameProvider : IDisposable
    {
        private IReadOnlyLocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ModeNameProvider(IReadOnlyLocalizationSetting localizationSetting)
        {
            // Dependencies
            _localizationSetting = localizationSetting;

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        public string Provide(IMode mode)
        {
            if (mode.UseAnimationNameAsDisplayName)
            {
                var clip = AV3Utility.GetAnimationClipWithName(mode.Animation);
                if (clip.clip != null)
                {
                    return clip.name;
                }
                else
                {
                    return _localizationTable.ModeNameProvider_NoExpression;
                }
            }
            else
            {
                return mode.DisplayName;
            }
        }
    }
}
