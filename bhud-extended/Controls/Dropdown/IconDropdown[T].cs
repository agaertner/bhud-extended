using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Extended
{
    public class IconDropdown<T> : KeyValueDropdown<T> {

        private SortedList<T, AsyncTexture2D> _itemIcons;

        private AsyncTexture2D _selectedItemIcon;

        private const int BORDER_WIDTH = 2;

        public int ItemsPerRow { get; set; } = 5;
        public int IconPadding { get; set; } = 2;
        public int EdgePadding { get; set; } = 8;

        private Texture2D _textureEmptySlot;

        public IconDropdown() {
            _itemIcons        = new SortedList<T, AsyncTexture2D>();
            _textureEmptySlot = EmbeddedResourceLoader.LoadTexture("156900.png");
        }

        public bool AddItem(T value, string tooltip, AsyncTexture2D icon) {
            if (base.AddItem(value, tooltip)) {
                _itemIcons.Add(value, icon);
                OnItemsUpdated();
                return true;
            }
            return false;
        }

        public override bool RemoveItem(T value) {
            if (base.RemoveItem(value)) {
                _itemIcons.Remove(value);
                OnItemsUpdated();
                return true;
            }
            return false;
        }

        public override void Clear() {
            _itemIcons.Clear();
            base.Clear();
            OnItemsUpdated();
        }

        private void OnItemsUpdated() {
            if (this.HasSelected && _itemIcons != null) {
                // Update in case SelectedItem was set before the items were added (eg. object initializer syntax).
                _selectedItemIcon = _itemIcons.TryGetValue(SelectedItem, out var displayIcon) ? displayIcon : null;
            }
        }

        private AsyncTexture2D GetItemIcon(T value) {
            if (_itemIcons.TryGetValue(value, out var icon)) {
                return icon;
            }
            return null;
        }

        protected override void OnSelectedItemChanged(T previous, T current) {
            _selectedItemIcon = GetItemIcon(current);
            base.OnSelectedItemChanged(previous, current);
        }

        private Point GetMaxItemSize() {
            if (_itemIcons.Count == 0 || ItemsPerRow <= 0) return Point.Zero;
            int maxIconWidth  = 0;
            int maxIconHeight = 0;
            foreach (var icon in _itemIcons.Values) {
                if (icon == null || !icon.HasTexture) continue;
                if (icon.Width  > maxIconWidth) maxIconWidth   = icon.Width;
                if (icon.Height > maxIconHeight) maxIconHeight = icon.Height;
            }
            return new Point(maxIconWidth, maxIconHeight);
        }

        private Rectangle GetItemBounds(int index) {
            int col = index % ItemsPerRow;
            int row = index / ItemsPerRow;

            var maxIconSize  = GetMaxItemSize();

            int paddedWidth  = maxIconSize.X + IconPadding;
            int paddedHeight = maxIconSize.Y + IconPadding;

            int x = EdgePadding + col * paddedWidth;
            int y = EdgePadding + row * paddedHeight;

            return new Rectangle(x, y, maxIconSize.X, maxIconSize.Y);
        }

        private Rectangle GetInner(Rectangle bounds) {
            var shrink = bounds;
            shrink.Inflate(-7, -7);
            return shrink;
        }

        protected override void PaintDropdownItem(DropdownPanel panel, SpriteBatch spriteBatch, T item, int index, bool highlighted) {
            var bounds = GetItemBounds(index);
            if (bounds == Rectangle.Empty) return;

            var icon = GetItemIcon(item);
            if (icon == null || !icon.HasTexture) {
                return;
            }

            spriteBatch.DrawOnCtrl(panel, _textureEmptySlot,  bounds, Color.White);
            var centered = GetInner(bounds).GetCenteredFit(icon.Bounds.Size);
            spriteBatch.DrawOnCtrl(panel, icon, centered);

            if (highlighted) {
                spriteBatch.DrawRectangleOnCtrl(panel, bounds, BORDER_WIDTH, Color.White * 0.7f);
            } else if (!Equals(item, SelectedItem)) {
                spriteBatch.DrawRectangleOnCtrl(panel, bounds, Color.Black * 0.4f);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureEmptySlot, bounds, Color.White);
            if (_selectedItemIcon != null && _selectedItemIcon.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _selectedItemIcon, GetInner(bounds).GetCenteredFit(_selectedItemIcon.Bounds.Size));
            }
        }

        protected override int GetHighlightedItemIndex(Point relativeMousePosition) {
            for (int i = 0; i < _itemIcons.Count; i++) {
                var bounds = GetItemBounds(i);
                if (bounds.Contains(relativeMousePosition)) {
                    return i;
                }
            }
            return -1;
        }

        protected override Point GetDropdownSize() {
            if (_itemIcons.Count == 0 || ItemsPerRow <= 0) return Point.Zero;

            var maxIconSize = GetMaxItemSize();

            int columns  = Math.Min(ItemsPerRow, _itemIcons.Count);
            int rows = (_itemIcons.Count + ItemsPerRow - 1) / ItemsPerRow;

            int totalWidth  = columns * maxIconSize.X + (columns - 1) * IconPadding + 2 * EdgePadding;
            int totalHeight = rows    * maxIconSize.Y + (rows    - 1) * IconPadding + 2 * EdgePadding;

            return new Point(totalWidth, totalHeight);
        }
    }
}
