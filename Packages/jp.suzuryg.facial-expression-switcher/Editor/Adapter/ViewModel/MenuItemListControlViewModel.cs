using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel
{
    public class MenuItemListControlViewModel
    {
        private ModeControlViewModel _modeControlViewModel;
        private GroupControlViewModel _groupControlViewModel;

        public MenuItemListControlViewModel(
            ModeControlViewModel modeControlViewModel,
            GroupControlViewModel groupControlViewModel)
        {
            _modeControlViewModel = modeControlViewModel;
            _groupControlViewModel = groupControlViewModel;
        }
    }
}
