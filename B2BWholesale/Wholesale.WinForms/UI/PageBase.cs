using System.Windows.Forms;

namespace Wholesale.WinForms.UI
{
    /// <summary>Base class of every screen shown inside MainForm.</summary>
    public abstract class PageBase : UserControl
    {
        protected PageBase()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;
        }

        public abstract string PageTitle { get; }

        /// <summary>Called every time the page is shown so it can reload fresh data.</summary>
        public virtual void OnShown() { }

        /// <summary>Called when the page is hidden (stop timers here).</summary>
        public virtual void OnHidden() { }
    }
}
