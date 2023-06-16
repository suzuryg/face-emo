using Suzuryg.FaceEmo.Domain;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public class SerializableGroup : SerializableMenuItemListBase
    {
        public string DisplayName;

        public void Save(IGroup group, bool isAsset)
        {
            base.Save(group, isAsset);
            DisplayName = group.DisplayName;
        }

        public override void Load(Menu menu, string destination)
        {
            menu.ModifyGroupProperties(destination, displayName: DisplayName);
            base.Load(menu, destination);
        }
    }
}
