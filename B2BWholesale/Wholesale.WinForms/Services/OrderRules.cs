using System;
using System.Collections.Generic;

namespace Wholesale.WinForms.Services
{
    /// <summary>Pure business rules for the order life cycle (easy to unit test).</summary>
    public static class OrderRules
    {
        public static readonly string[] AllStatuses = { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

        public static IList<string> NextStatuses(string current)
        {
            switch (current)
            {
                case "Pending": return new List<string> { "Confirmed", "Cancelled" };
                case "Confirmed": return new List<string> { "Shipped", "Cancelled" };
                case "Shipped": return new List<string> { "Delivered" };
                default: return new List<string>();   // Delivered and Cancelled are final
            }
        }

        public static bool CanTransition(string from, string to)
        {
            return NextStatuses(from).Contains(to);
        }
    }
}
