using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Extended.Core.Views
{
    public class SocialsSettingsView : View<SocialsSettingsPresenter>
    {
        internal event EventHandler<EventArgs> BrowserButtonClick;

        #region SettingsView Defaults

        private FlowPanel _settingFlowPanel;

        private bool _lockBounds = true;
        public bool LockBounds
        {
            get => _lockBounds;
            set
            {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        private void UpdateBoundsLocking(bool locked)
        {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder = !locked;
            _settingFlowPanel.CanCollapse = !locked;
        }

        #endregion

        private FlowPanel _socialFlowPanel;

        public SocialsSettingsView(SocialsSettingsModel model)
        {
            this.WithPresenter(new SocialsSettingsPresenter(this, model));
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            return await this.Presenter.Model.LoadSocials();
        }

        protected override void Build(Container buildPanel)
        {
            BuildSocialsPanel(buildPanel);

            _settingFlowPanel = new FlowPanel
            {
                Size = new Point(buildPanel.Width, buildPanel.Height),
                Location = new Point(0, _socialFlowPanel?.Height ?? 0),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = buildPanel
            };

            foreach (var setting in Presenter.Model.Settings.Where(s => s.SessionDefined))
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null)
                {
                    _lastSettingContainer = new ViewContainer
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                    {
                        subSettingsView.LockBounds = false;
                    }
                }
            }
        }

        private void BuildSocialsPanel(Container buildPanel)
        {
            var socials = this.Presenter.Model.GetSocials();

            if (socials == null || !socials.Any()) return;

            _socialFlowPanel = new FlowPanel
            {
                Size = new Point(buildPanel.Width, 78),
                Location = new Point(0, 0),
                FlowDirection = ControlFlowDirection.SingleRightToLeft,
                ControlPadding = new Vector2(27, 2),
                OuterControlPadding = new Vector2(27, 2),
                WidthSizingMode = SizingMode.Fill,
                ShowBorder = true,
                Parent = buildPanel
            };

            foreach (var social in this.Presenter.Model.GetSocials())
            {
                var text = this.Presenter.Model.GetSocialText(social);
                var socialBtn = new StandardButton
                {
                    Parent = _socialFlowPanel,
                    Size = new Point((int)GameService.Content.DefaultFont14.MeasureString(text).Width + 48, 46),
                    Text = text,
                    Icon = this.Presenter.Model.GetSocialLogo(social),
                    ResizeIcon = true,
                    BasicTooltipText = this.Presenter.Model.GetSocialUrl(social)
                };
                socialBtn.Click += OnBrowserButtonClick;
            }
        }

        private void OnBrowserButtonClick(object sender, MouseEventArgs e)
        {
            this.BrowserButtonClick?.Invoke(sender, e);
        }

        protected override void Unload()
        {
        }
    }
}
