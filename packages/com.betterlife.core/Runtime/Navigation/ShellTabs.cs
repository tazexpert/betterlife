using System.Collections.Generic;

namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// The six bottom navigation tabs, left to right.
    /// </summary>
    public static class ShellTabs
    {
        /// <summary>Tabs, left to right; the first is home.</summary>
        public static readonly IReadOnlyList<TabDefinition> All = new[]
        {
            new TabDefinition(ScreenIds.Home, "nav.home", "icon-home", ScreenIds.Home),
            new TabDefinition(ScreenIds.Life, "nav.life", "icon-life", ScreenIds.Life),
            new TabDefinition(ScreenIds.Money, "nav.money", "icon-money", ScreenIds.Money),
            new TabDefinition(ScreenIds.People, "nav.people", "icon-people", ScreenIds.People),
            new TabDefinition(ScreenIds.Work, "nav.work", "icon-work", ScreenIds.Work),
            new TabDefinition(ScreenIds.More, "nav.more", "icon-more", ScreenIds.More),
        };
    }
}
