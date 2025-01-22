using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended.Properties;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Blish_HUD.Extended
{
    internal sealed class ErrorPrompt : Container
    {
        [Flags]
        public enum DialogButtons : ushort
        {
            None = 0,
            OK = 1 << 0,
            Confirm = 1 << 1,
            Cancel = 1 << 2,
            Yes = 1 << 3,
            No = 1 << 4,
            Ignore = 1 << 5,
            Close = 1 << 6,
            Apply = 1 << 7
        }

        public enum DialogIcon
        {
            None,
            Exclamation,
            Question,
        }

        private static ErrorPrompt _singleton;

        private AsyncTexture2D _bgTexture;
        private AsyncTexture2D _icon;

        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size20, ContentService.FontStyle.Regular);

        private Rectangle _bgBounds;
        private Point _iconMargin = new(9, 8);

        private const int BUTTON_HEIGHT = 25;
        private const int BUTTON_WIDTH = 112;

        private readonly Dictionary<DialogButtons, StandardButton> _buttons;

        private readonly Action<DialogButtons> _callback;

        private readonly string _text;
        private readonly DialogButtons _enterButton;
        private readonly DialogButtons _escapeButton;


        private ErrorPrompt(string text, DialogButtons buttons, Action<DialogButtons> callback = null, DialogIcon icon = DialogIcon.None, AsyncTexture2D customIcon = null, DialogButtons enterButton = DialogButtons.None, DialogButtons escapeButton = DialogButtons.None) {
            _text = text;
            _buttons = new Dictionary<DialogButtons, StandardButton>();
            foreach (DialogButtons button in Enum.GetValues(typeof(DialogButtons))) {
                if (button != DialogButtons.None && buttons.HasFlag(button)) {
                    _buttons[button] = null;
                }
            }

            if (!IsValidDialog(out var errorMessage)) {
                throw new ArgumentException(errorMessage);
            }

            _enterButton = enterButton;
            _escapeButton = escapeButton;
            _callback = callback;

            this.ZIndex = 999;
            this.LoadIcon(icon, customIcon);
            this.LoadTextures();

            GameService.Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private void LoadTextures() {
            _bgTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); // Do not dispose this as it holds a shared cached texture.
        }

        private void LoadIcon(DialogIcon icon, AsyncTexture2D customIcon) {
            if (customIcon != null) {
                _icon = customIcon; // Possibly a DatAssetCache texture.
            } else if (icon > DialogIcon.None) {
                _icon = new AsyncTexture2D(); // Can be disposed. Will hold a non-cached texture that is just a region of the cached version.
                GameService.Content.DatAssetCache.GetTextureFromAssetId(154985).TextureSwapped += (_, e) => {
                    if (e.NewValue != null) {
                        GetIconRegion(icon, e.NewValue);
                    }
                };
            }
        }

        private void GetIconRegion(DialogIcon icon, Texture2D atlas) {
            if (icon == DialogIcon.Exclamation) {
                _icon.SwapTexture(atlas.GetRegion(0, 0, 64, 64));
            } else if (icon == DialogIcon.Question) {
                _icon.SwapTexture(atlas.GetRegion(64, 0, 64, 64));
            }
        }

        private bool IsValidDialog(out string errorMessage) {
            errorMessage = string.Empty;
            var noButtons = _buttons.Count < 1;
            if (noButtons)
            {
                errorMessage += "Prompt dialog must have at least one button. ";
            }
            var noText = string.IsNullOrWhiteSpace(_text);
            if (noText)
            {
                errorMessage += "Prompt dialog must have text content.";
            }
            return string.IsNullOrEmpty(errorMessage);
        }

        protected override void DisposeControl() {
            _singleton = null;
            _icon?.Dispose();
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            base.DisposeControl();
        }

        private void ButtonPress(DialogButtons button) {
            if (button == DialogButtons.None)
            {
                return;
            }
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            GameService.Content.PlaySoundEffectByName("button-click");
            _callback?.Invoke(button);
            _singleton = null;
            this.Dispose();
        }

        private void OnKeyPressed(object o, KeyboardEventArgs e) {
            switch (e.Key) {
                case Keys.Enter:
                    this.ButtonPress(_enterButton);
                    break;
                case Keys.Escape:
                    this.ButtonPress(_escapeButton);
                    break;
                default: return;
            }
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(string text, DialogButtons buttons, Action<DialogButtons> callback = null,
                                      DialogButtons enterButton = DialogButtons.None,
                                      DialogButtons escapeButton = DialogButtons.None) {
            Show(text, DialogIcon.None, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(string text, DialogIcon icon, DialogButtons buttons, Action<DialogButtons> callback = null,
                                      DialogButtons enterButton = DialogButtons.None,
                                      DialogButtons escapeButton = DialogButtons.None) {
            Show(text, icon, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Custom icon to use. Gets disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        /// <remarks>
        ///     If a texture from the DatAssetCache should be used as icon then call
        ///     <see cref="Texture2DExtension.Duplicate"/> on <see cref="AsyncTexture2D.Texture"/> prior
        ///     to construction; otherwise the cached texture gets disposed with the prompt.
        /// </remarks>
        public static void Show(string text, AsyncTexture2D icon, DialogButtons buttons, Action<DialogButtons> callback = null,
                                      DialogButtons enterButton = DialogButtons.None,
                                      DialogButtons escapeButton = DialogButtons.None) {
            Show(text, DialogIcon.None, icon, buttons, callback, enterButton, escapeButton);
        }

        private static void Show(string text, DialogIcon icon, AsyncTexture2D customIcon, DialogButtons buttons, Action<DialogButtons> callback,
                                       DialogButtons enterButton,
                                       DialogButtons escapeButton) {
            if (_singleton != null) {
                return;
            }
            _singleton = new ErrorPrompt(text, buttons, callback, icon, customIcon, enterButton, escapeButton)
            {
                Parent = Graphics.SpriteScreen,
                Location = Point.Zero,
                Size = Graphics.SpriteScreen.Size
            };
            _singleton.Show();
        }

        private void CreateButtons() {
            int minLeftOffset = 50;
            int buttonCount = _buttons.Count;
            int availableWidth = _bgBounds.Width - minLeftOffset;

            int buttonWidth = BUTTON_WIDTH;
            if (buttonCount * buttonWidth > availableWidth) {
                buttonWidth = availableWidth / buttonCount;
            }

            int xOffset = _bgBounds.Width - minLeftOffset - buttonWidth - Panel.RIGHT_PADDING;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING;
            var buttonKeys = _buttons.Keys.ToList();
            foreach (var buttonKey in buttonKeys) {
                StandardButton button = _buttons[buttonKey];
                if (button == null) {
                    button = new StandardButton {
                        Parent = this,
                        Text = GetButtonText(buttonKey),
                        Width = buttonWidth,
                        Height = BUTTON_HEIGHT,
                        Location = new Point(_bgBounds.Left + minLeftOffset + xOffset, yOffset),
                        Enabled = true
                    };
                    button.Click += (_, _) => this.ButtonPress(buttonKey);
                }
                xOffset -= buttonWidth + _iconMargin.X;
                _buttons[buttonKey] = button;
            }
        }

        private string GetButtonText(DialogButtons button) {
            return button switch {
                DialogButtons.OK => Resources.OK,
                DialogButtons.Confirm => Resources.Confirm,
                DialogButtons.Cancel => Resources.Cancel,
                DialogButtons.Yes => Resources.Yes,
                DialogButtons.No => Resources.No,
                DialogButtons.Ignore => Resources.Ignore,
                DialogButtons.Close => Resources.Close,
                DialogButtons.Apply => Resources.Apply,
                _ => string.Empty
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var textMarginRight = 25;
            var text = DrawUtil.WrapText(_font, _text, 412);
            var textSize = _font.MeasureString(text);
            var textWidth = (int)textSize.Width;
            var textHeight = (int)textSize.Height;

            var iconSize = _icon == null ? 0 : 64;
            var textMargin = new Point(iconSize + _iconMargin.X * 2, 17);

            var contentWidth = textWidth + iconSize + _iconMargin.X + textMargin.X + textMarginRight;
            var contentHeight = textHeight > 64 ? textHeight : textHeight + iconSize;

            contentHeight = contentHeight < 150 ? 150 : contentHeight;

            // Darken background outside container
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.5f);

            // Calculate background bounds
            var bgTextureSize = new Point(contentWidth, contentHeight + (BUTTON_HEIGHT + Panel.TOP_PADDING) * 2);
            var bgTexturePos = new Point((bounds.Width - bgTextureSize.X) / 2, (bounds.Height - bgTextureSize.Y) / 2);
            var bgBounds = new Rectangle(bgTexturePos, bgTextureSize);
            _bgBounds = bgBounds;

            var textBounds = new Rectangle(bgBounds.Left + textMargin.X, bgBounds.Y + textMargin.Y, bgBounds.Width - textMarginRight, contentHeight);

            // Draw Background
            spriteBatch.DrawOnCtrl(this, _bgTexture, bgBounds, new Rectangle(29, 23, 942, 942), Color.White);

            // Draw border
            spriteBatch.DrawRectangleOnCtrl(this, _bgBounds, 2, Color.Black * 0.8f);

            if (_icon != null && _icon.HasTexture) {
                var iconBounds = new Rectangle(bgBounds.Left + _iconMargin.X, bgBounds.Top + _iconMargin.Y, 64, 64);
                spriteBatch.DrawOnCtrl(this, _icon, iconBounds);
            }

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, text, _font, textBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
            this.CreateButtons();
        }
    }
}
