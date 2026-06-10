using System.Collections.Generic;
using System.Linq;

namespace AgroRegionApp.Navigation
{
    internal static class RoleNavigation
    {
        private static readonly NavItem[] AllItems =
        {
            new NavItem
            {
                Section = NavSection.Sales,
                Label = "Продажи",
                Icon = "📋",
                Roles = new[] { Roles.SalesManager, Roles.CommercialDirector, Roles.Ceo }
            },
            new NavItem
            {
                Section = NavSection.Warehouse,
                Label = "Складской учёт",
                Icon = "🏭",
                Roles = new[] { Roles.WarehouseKeeper, Roles.CommercialDirector, Roles.Ceo }
            },
            new NavItem
            {
                Section = NavSection.Purchases,
                Label = "Закупки",
                Icon = "🛒",
                Roles = new[] { Roles.Ceo, Roles.CommercialDirector }
            },
            new NavItem
            {
                Section = NavSection.References,
                Label = "Справочники",
                Icon = "📖",
                Roles = new[] { Roles.SalesManager, Roles.WarehouseKeeper, Roles.CommercialDirector, Roles.Ceo }
            },
            new NavItem
            {
                Section = NavSection.Analytics,
                Label = "Аналитика",
                Icon = "📊",
                Roles = new[] { Roles.CommercialDirector, Roles.Ceo }
            }
        };

        public static IReadOnlyList<NavItem> GetItemsForRole(string roleName)
        {
            return AllItems.Where(i => i.Roles.Contains(roleName)).ToList();
        }

        public static NavItem GetItem(NavSection section)
        {
            return AllItems.First(i => i.Section == section);
        }
    }
}
