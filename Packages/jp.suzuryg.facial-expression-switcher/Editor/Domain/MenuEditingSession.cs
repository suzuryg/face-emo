using System;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class MenuEditingSession
    {
        public Menu Menu { get; private set; }
        public bool IsOpened { get; private set; } = false;
        public bool IsModified { get; private set; } = false;

        private IMenuRepository _menuRepository;
        private string _destination;

        public MenuEditingSession(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public void SetAsModified()
        {
            IsModified = true;
        }

        public bool Create()
        {
            if (IsModified)
            {
                return false;
            }
            Menu = new Menu();
            IsOpened = true;
            IsModified = true;
            return true;
        }

        public bool Save()
        {
            if (!(_destination is string))
            {
                return false;
            }
            SaveAs(_destination);
            return true;
        }

        public void SaveAs(string destination)
        {
            _menuRepository.Save(destination, Menu);
            _destination = destination;
            IsModified = false;
        }

        public bool Load(string source)
        {
            if (IsModified)
            {
                return false;
            }
            Menu = _menuRepository.Load(source);
            IsOpened = true;
            IsModified = false;
            return true;
        }
    }
}
