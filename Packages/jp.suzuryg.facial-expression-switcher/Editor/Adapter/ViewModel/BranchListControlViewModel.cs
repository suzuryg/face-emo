using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel
{
    public class BranchListControlViewModel
    {
        private BranchControlViewModel _branchControlViewModel;
        private GestureTableWindowViewModel _gestureTableWindowView;

        public BranchListControlViewModel(
            BranchControlViewModel branchControlViewModel,
            GestureTableWindowViewModel gestureTableWindowView)
        {
            _branchControlViewModel = branchControlViewModel;
            _gestureTableWindowView = gestureTableWindowView;
        }
    }
}
