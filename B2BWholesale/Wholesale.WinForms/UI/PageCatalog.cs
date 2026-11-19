using System;
using System.Collections.Generic;
using Wholesale.WinForms.Pages;

namespace Wholesale.WinForms.UI
{
    /// <summary>
    /// The ONE place that lists the screens of the application (navigation order).
    /// Each feature branch adds its own line here, so merges stay small.
    /// </summary>
    public static class PageCatalog
    {
        public class Entry
        {
            public string Text;
            public Func<PageBase> Create;
        }

        public static List<Entry> All()
        {
            return new List<Entry>
            {
                new Entry { Text = "Items & Stock",     Create = () => new ItemsPage() },       // Task 1.2 (items)
                new Entry { Text = "Agents",            Create = () => new AgentsPage() }       // Task 1.2 (agents)
            };
        }
    }
}
