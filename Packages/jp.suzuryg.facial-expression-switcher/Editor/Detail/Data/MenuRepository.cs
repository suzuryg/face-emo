using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class MenuRepository : IMenuRepository
    {
        private SerializableMenu _serializableMenu;

        public MenuRepository(SerializableMenu serializableMenu)
        {
            _serializableMenu = serializableMenu;
        }

        public bool Exists(string source)
        {
            return _serializableMenu is SerializableMenu;
        }

        public Menu Load(string source) => _serializableMenu.Load();

        public void Save(string destination, Menu menu, string comment)
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(_serializableMenu, comment);
            _serializableMenu.Save(menu);
        }
    }
}
