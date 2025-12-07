using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Extended
{
    public class TextDropdown<T> : BaseDropdown<T> {

        #region Load Static

        private static readonly Texture2D _textureInputBox = Content.GetTexture("input-box");

        private static readonly TextureRegion2D _textureArrow       = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow");
        private static readonly TextureRegion2D _textureArrowActive = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow-active");

        #endregion

        private readonly SortedList<T, string> _itemTexts;
        private readonly SortedList<T, Color>  _itemColors;

        private          string _selectedItemText;
        private          Color  _selectedItemColor;
        private readonly Color  _defaultColor;
        private readonly Color  _placeholderColor;

        private string _placeholderText;
        public string PlaceholderText {
            get => _placeholderText;
            set {
                if (SetProperty(ref _placeholderText, value)) {
                    OnItemsUpdated();
                    Invalidate();
                }
            }
        }

        private bool _autoSizeWidth;
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set {
                if (SetProperty(ref _autoSizeWidth, value)) {
                    OnItemsUpdated();
                    Invalidate();
                }
            }
        }

        private BitmapFont _font;
        public BitmapFont Font {
            get => _font;
            set {
                if (SetProperty(ref _font, value)) {
                    OnItemsUpdated();
                    Invalidate();
                }
            }
        }

        public TextDropdown() {
            _itemTexts         = new SortedList<T, string>();
            _itemColors        = new SortedList<T, Color>();
            _placeholderText   = string.Empty;
            _selectedItemText  = string.Empty;
            _defaultColor      = Color.FromNonPremultiplied(239, 240, 239, 255);
            _selectedItemColor = _defaultColor;
            _placeholderColor  = Color.FromNonPremultiplied(209, 210, 209, 255);
            _font              = Content.DefaultFont14;
        }

        /// <summary>
        /// Adds an item to the dropdown.
        /// </summary>
        /// <param name="value">Value of the item.</param>
        /// <param name="text">Displayed text.</param>
        /// <param name="tooltip">Basic tooltip text of the item.</param>
        /// <param name="color">Color of the displayed text.</param>
        public void AddItem(T value, string text, Func<string> tooltip = null, Color color = default) {
            if (base.AddItem(value, tooltip ?? (() => text)))
            {
                _itemTexts.Add(value, text);
                _itemColors.Add(value, color.Equals(default) ?
                                           _defaultColor :
                                           color);
                OnItemsUpdated();
            }
        }

        /// <summary>
        /// Adds an item with an associated text color to the dropdown.
        /// </summary>
        /// <param name="value">Value of the item.</param>
        /// <param name="text">Displayed text.</param>
        /// <param name="color">Color of the displayed text.</param>
        /// <returns></returns>
        public void AddItem(T value, string text, Color color) {
            AddItem(value, text, null, color);
        }

        protected override void OnDropdownMenuShown(DropdownMenu menu) {
            menu.FlowDirection = ControlFlowDirection.SingleTopToBottom;

            foreach (var items in _itemTexts) {
                _ = new Label {
                    Parent = menu,
                    Width = this.Width,
                    Height = this.Height,
                    Text = items.Value,
                    Font = this.Font,
                    TextColor = GetItemColor(items.Key)
                };
                //TODO: When FormattedLabelBuilder supports changing Font, use it.
                /*var label = new FormattedLabelBuilder()
                           .SetWidth(this.Width)
                           .SetHeight(this.Height)
                           .CreatePart(text, p => {
                }).Build();*/
            }
        }

        protected override void OnItemRemoved(T value) {
            _itemTexts.Remove(value);
            _itemColors.Remove(value);
        }

        protected override void OnItemsCleared() {
            _itemTexts.Clear();
            _itemColors.Clear();
        }

        protected override void OnItemsUpdated() 
        {
            if (this.HasSelected && _itemTexts != null && _itemColors != null) {
                // Update in case SelectedItem was set before the items were added (eg. object initializer syntax).
                _selectedItemText  = _itemTexts.TryGetValue(SelectedItem, out var displayText) ? displayText : string.Empty;
                _selectedItemColor = _itemColors.TryGetValue(SelectedItem, out var color) ? color : _defaultColor;
            } else {
                _selectedItemText  = string.Empty;
                _selectedItemColor = _defaultColor;
            }

            if (AutoSizeWidth) {
                int width = this.Width;
                if (!string.IsNullOrEmpty(_selectedItemText)) {
                    width = (int)Math.Round(_font.MeasureString(_selectedItemText).Width);
                } else if (!string.IsNullOrEmpty(_placeholderText)) {
                    width = (int)Math.Round(_font.MeasureString(_placeholderText).Width);
                }
                this.Width = width + 13 + _textureArrow.Width;
            }
        }

        private Color GetItemColor(T item) {
            if (_itemColors.TryGetValue(item, out var color)) {
                return color;
            }
            return _defaultColor;
        }

        private string GetItemText(T item) {
            if (_itemTexts.TryGetValue(item, out string text)) {
                return text;
            }
            return string.Empty;
        }

        protected override void OnSelectedItemChanged(T previous, T current) {
            _selectedItemText  = GetItemText(current);
            _selectedItemColor = GetItemColor(current);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw dropdown
            spriteBatch.DrawOnCtrl(this,
                                   _textureInputBox,
                                   new Rectangle(Point.Zero, _size).Subtract(new Rectangle(0, 0, 5, 0)),
                                   new Rectangle(0, 0,
                                                 Math.Min(_textureInputBox.Width - 5, this.Width - 5),
                                                 _textureInputBox.Height));

            // Draw right side of dropdown
            spriteBatch.DrawOnCtrl(this,
                                   _textureInputBox,
                                   new Rectangle(_size.X - 5, 0, 5, _size.Y),
                                   new Rectangle(_textureInputBox.Width - 5, 0,
                                                 5, _textureInputBox.Height));

            // Draw dropdown arrow
            spriteBatch.DrawOnCtrl(this,
                                   (this.Enabled && this.MouseOver) ? _textureArrowActive : _textureArrow,
                                   new Rectangle(_size.X - _textureArrow.Width - 5,
                                                 _size.Y / 2 - _textureArrow.Height / 2,
                                                 _textureArrow.Width,
                                                 _textureArrow.Height));

            // Draw text
            if (string.IsNullOrEmpty(_selectedItemText)) {
                spriteBatch.DrawStringOnCtrl(this,
                                             _placeholderText,
                                             _font,
                                             new Rectangle(5, 0,
                                                           _size.X - 10 - _textureArrow.Width,
                                                           _size.Y),
                                             (this.Enabled
                                                  ? _placeholderColor
                                                  : Control.StandardColors.DisabledText));
            } else {
                spriteBatch.DrawStringOnCtrl(this,
                                             _selectedItemText,
                                             _font,
                                             new Rectangle(5, 0,
                                                           _size.X - 10 - _textureArrow.Width,
                                                           _size.Y),
                                             (this.Enabled
                                                  ? _selectedItemColor
                                                  : Control.StandardColors.DisabledText));
            }
        }

        protected override void PaintDropdownItem(DropdownMenu menu, SpriteBatch spriteBatch, T item, int index, bool highlighted) {
            var itemBounds = new Rectangle(0, index * this.Height, menu.Width, this.Height);
            if (highlighted) {
                spriteBatch.DrawOnCtrl(menu, ContentService.Textures.Pixel,
                                       new Rectangle(2, 2 + itemBounds.Y, this.Width - 4, itemBounds.Height - 4),
                                       new Color(45, 37, 25, 255));
            }
        }

        protected override int GetHighlightedItemIndex(DropdownMenu menu) {
            int adjustedY = menu.RelativeMousePosition.Y + menu.VerticalScrollOffset;
            return adjustedY / this.Height;
        }

        protected override Point GetDropdownSize() {
            return new Point(this.Width, this.MenuHeight > this.Height ? this.MenuHeight : this.Height * _itemTexts.Count); // No scrollbar (draw all items) if max menu height smaller than one row.
        }

    }
}
