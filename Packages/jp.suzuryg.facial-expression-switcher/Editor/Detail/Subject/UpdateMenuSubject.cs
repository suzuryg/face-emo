using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Subject
{
    public class UpdateMenuSubject
    {
        public event Action<IMenu> OnMenuUpdated;

        public void Notify(IMenu menu)
        {
            OnMenuUpdated(menu);
        }
    }
}
