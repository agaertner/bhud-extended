using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Extended
{
    /// <summary>
    /// A dropdown control that displays icons for each item instead of text.
    /// </summary>
    /// <typeparam name="T">The value an item holds and which is returned when selected.</typeparam>
    public class IconDropdown<T> : BaseDropdown<T> {

        private static Rectangle GetInner(Rectangle bounds) {
            // Returns the "visual" inner frame of the empty slot texture.
            var shrink = bounds;
            shrink.Inflate(-7, -7);
            return shrink;
        }

        private readonly SortedList<T, AsyncTexture2D> _itemIcons;

        private AsyncTexture2D _selectedItemIcon;

        private int _itemsPerRow = 5;
        /// <summary>
        /// Gets or sets the number of items to display per row.
        /// </summary>
        public int ItemsPerRow {
            get => _itemsPerRow;
            set {
                if (SetProperty(ref _itemsPerRow, value)) {
                    Invalidate();
                }
            }
        }

        private readonly Texture2D _textureEmptySlot;

        public IconDropdown() { 
            this.Spacing      = 2;
            _itemIcons        = new SortedList<T, AsyncTexture2D>();
            _textureEmptySlot = EmbeddedResourceLoader.LoadTexture("156900.png");
        }

        protected override void DisposeControl() {
            _textureEmptySlot?.Dispose();
            DisposeIcons();
            base.DisposeControl();
        }

        private void DisposeIcons() {
            foreach (var icon in _itemIcons.Values) {
                icon?.Dispose();
            }
            _itemIcons.Clear();
        }

        /// <summary>
        /// Adds an item with an associated icon to the dropdown.
        /// </summary>
        /// <param name="value">Value of the item.</param>
        /// <param name="tooltip">Tooltip of the item.</param>
        /// <param name="icon">Associated icon that is displayed.</param>
        /// <returns></returns>
        public void AddItem(T value, Func<string> tooltip, AsyncTexture2D icon) {
            if (base.AddItem(value, tooltip)) {
                _itemIcons.Add(value, icon);
                OnItemsUpdated();
            }
        }

        /// <summary>
        /// Adds an item with an associated icon to the dropdown.
        /// </summary>
        /// <param name="value">Value of the item.</param>
        /// <param name="icon">Associated icon that is displayed.</param>
        public void AddItem(T value, AsyncTexture2D icon) {
            AddItem(value, null, icon);
        }

        protected override void OnDropdownMenuShown(DropdownMenu menu) {
            menu.FlowDirection = ControlFlowDirection.LeftToRight;
        }

        protected override void OnItemRemoved(T item) {
            if (_itemIcons.TryGetValue(item, out var icon)) { 
                icon?.Dispose();
                _itemIcons.Remove(item);
            }
        }

        protected override void OnItemsCleared() {
            DisposeIcons();
        }

        protected override void OnItemsUpdated() {
            if (this.HasSelected && _itemIcons != null) {
                // Update in case SelectedItem was set before the items were added (eg. object initializer syntax).
                _selectedItemIcon = _itemIcons.TryGetValue(SelectedItem, out var displayIcon) ? displayIcon : null;
            } else {
                _selectedItemIcon = null;
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
        }

        private Point GetMaxItemSize() {
            if (_itemIcons.Count == 0 || _itemsPerRow <= 0) return Point.Zero;
            int maxIconWidth  = 0;
            int maxIconHeight = 0;
            foreach (var icon in _itemIcons.Values) {
                if (icon == null || !icon.HasTexture) continue;
                if (icon.Width  > maxIconWidth) maxIconWidth   = icon.Width;
                if (icon.Height > maxIconHeight) maxIconHeight = icon.Height;
            }
            return new Point(maxIconWidth, maxIconHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureEmptySlot, bounds, Color.White);
            if (_selectedItemIcon != null && _selectedItemIcon.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _selectedItemIcon, GetInner(bounds).GetCenteredFit(_selectedItemIcon.Bounds.Size));
            }
        }

        protected override void PaintDropdownItem(DropdownMenu.DropdownItem ctrl, SpriteBatch spriteBatch, Rectangle bounds) {
            var icon = GetItemIcon(ctrl.Item);
            if (icon == null || !icon.HasTexture) return;

            spriteBatch.DrawOnCtrl(ctrl, _textureEmptySlot, bounds, Color.White); // Background slot texture.
            var centered = GetInner(bounds).GetCenteredFit(icon.Bounds.Size);
            spriteBatch.DrawOnCtrl(ctrl, icon, centered); // Icon fitted to slot texture.

            if (ctrl.MouseOver) { // Should be highlighted.
                spriteBatch.DrawBorderOnCtrl(ctrl, bounds, Color.White * 0.7f, BORDER_WIDTH); // Border.
            } else if (!this.HasSelected || !Equals(ctrl.Item, SelectedItem)) {
                spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, bounds, Color.Black * 0.4f);
            }
        }

        protected override Point GetDropdownSize() {
            if (_itemIcons.Count == 0 || _itemsPerRow <= 0) return Point.Zero;

            var itemSize = GetDropdownItemSize();

            int columns  = Math.Min(_itemsPerRow, _itemIcons.Count);
            int rows = (_itemIcons.Count + _itemsPerRow - 1) / _itemsPerRow;

            int totalWidth  = columns * itemSize.X + (columns - 1) * this.Spacing + 2 * this.Margin;
            int totalHeight = rows    * itemSize.Y + (rows    - 1) * this.Spacing + 2 * this.Margin;

            // Scale to fit all items at once (no scrollbar) if custom MenuHeight is smaller than one row.
            return new Point(totalWidth, this.MenuHeight > itemSize.Y + 2 * this.Margin ? this.MenuHeight : totalHeight); 
        }

        protected override Point GetDropdownItemSize() {
            return GetMaxItemSize();
        }
    }
}
