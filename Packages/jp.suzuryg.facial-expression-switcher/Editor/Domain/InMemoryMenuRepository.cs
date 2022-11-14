using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class InMemoryMenuRepository : IMenuRepository
    {
        private readonly Dictionary<string, Menu> _data = new Dictionary<string, Menu>();

        public void Save(string destination, Menu menu) => _data[destination] = menu;

        public bool Exists(string source) => _data.ContainsKey(source);

        public Menu Load(string source) => _data[source];
    }
}
