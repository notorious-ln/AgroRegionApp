namespace AgroRegionApp.Navigation
{
    internal enum NavSection
    {
        Sales,
        Warehouse,
        Purchases,
        References,
        Analytics
    }

    internal sealed class NavItem
    {
        public NavSection Section { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public string[] Roles { get; set; }
    }
}
