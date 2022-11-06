using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel
{
    public class HierarchyControlViewModel : ViewModelBase
    {
        public ReactiveProperty<string> Title { get; }
        //public ReactiveCommand<bool> AddGroupCommand { get; }

        public HierarchyControlViewModel()
        {
            Title = new ReactiveProperty<string>().AddTo(Disposable);
            //AddGroupCommand = new ReactiveCommand<bool>().AddTo(Disposable);

            Title.Value = "階層ビュー";
            //AddGroupCommand.Subscribe(AddGroup);
        }

        public void AddGroup()
        {
            Title.Value += "*";
        }
    }
}
