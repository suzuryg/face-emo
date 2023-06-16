using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public interface IMenuBackupper
    {
        void Export(SerializableMenu rootAsset);
        void Import(SerializableMenu rootAsset);
    }

    public class MenuRepository : IMenuRepository, IMenuBackupper
    {
        private MenuRepositoryComponent _component;

        public MenuRepository(MenuRepositoryComponent component)
        {
            _component = component;
        }

        public bool Exists(string source) => true;

        public Domain.Menu Load(string source)
        {
            if (_component.SerializableMenu == null)
            {
                _component.SerializableMenu =  ScriptableObject.CreateInstance<SerializableMenu>();
                _component.SerializableMenu.Save(new Domain.Menu(), isAsset: false);
            }
            return _component.SerializableMenu.Load();
        }

        public void Save(string destination, Domain.Menu menu, string comment)
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Changes in play mode are not saved.", "OK");
            }

            // Re-create an instance of SerializableMenu when saving so that it is saved independently even if the Component is copied.
            // Undo records the complete state of MenuRepository.
            Undo.RegisterCompleteObjectUndo(_component, comment);
            _component.SerializableMenu = ScriptableObject.CreateInstance<SerializableMenu>();
            _component.SerializableMenu.Save(menu, isAsset: false);

            // Need to resolve the following issues
            // - UI updates are delayed
            // - Exclusive control of data access
            //var autoSave = EditorPrefs.HasKey(DetailConstants.KeyAutoSave) ? EditorPrefs.GetBool(DetailConstants.KeyAutoSave) : DetailConstants.DefaultAutoSave;
            //if (autoSave) { EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()); }
        }

        public void Export(SerializableMenu rootAsset)
        {
            // Instances held in the MenuRepository should be separate from asset instances.
            var menu = Load(string.Empty);
            rootAsset.Save(menu, isAsset: true);
            AssetDatabase.SaveAssets();
        }

        public void Import(SerializableMenu rootAsset)
        {
            // Instances held in the MenuRepository should be separate from asset instances.
            var menu = rootAsset.Load();
            Save(string.Empty, menu, "Import Menu");
        }
    }
}
