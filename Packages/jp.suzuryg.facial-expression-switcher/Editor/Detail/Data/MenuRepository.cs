using Suzuryg.FacialExpressionSwitcher.Domain;

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
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Changes in play mode are not saved.", "OK");
            }

            UnityEditor.Undo.RegisterCompleteObjectUndo(_serializableMenu, comment);
            _serializableMenu.Save(menu);
        }
    }
}
