using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Extended.Core.Views
{
    public class SocialsSettingsPresenter : Presenter<SocialsSettingsView, SocialsSettingsModel>
    {
        public SocialsSettingsPresenter(SocialsSettingsView view, SocialsSettingsModel model) : base(view, model) {}

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.BrowserButtonClick += View_BrowserButtonClicked;
            return base.Load(progress);
        }

        protected override void Unload()
        {
            this.View.BrowserButtonClick -= View_BrowserButtonClicked;
        }

        private async void View_BrowserButtonClicked(object o, EventArgs e)
        {
            GameService.Overlay.BlishHudWindow.Hide();
            await BrowserUtil.Open(((Control)o).BasicTooltipText);
        }
    }
}
