using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel
{
    public class MainWindowViewModel
    {
        // UseCaseをDIコンテナ内でSingleにして、各ViewModelのコンストラクタに渡す
        // 子ViewModelをpublicにしてViewからプロパティを参照 ← Viewにはインターフェースで公開
        // ViewModelのインターフェースについてテスト
        // Viewのスタブでコマンドを作ってViewModelが保持しているUseCaseを呼び出す

        private HierarchyControlViewModel _hierarchyControlViewModel;
        private MenuItemListControlViewModel _menuItemListControlView;
        private BranchListControlViewModel _branchListControlViewModel;

        public MainWindowViewModel(
            HierarchyControlViewModel hierarchyControlViewModel,
            MenuItemListControlViewModel menuItemListControlView,
            BranchListControlViewModel branchListControlViewModel)
        {
            _hierarchyControlViewModel = hierarchyControlViewModel;
            _menuItemListControlView = menuItemListControlView;
            _branchListControlViewModel = branchListControlViewModel;
        }
    }
}
