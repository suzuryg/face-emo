using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel
{
    public abstract class ViewModelBase : IDisposable
    {
        public CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}
