﻿using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Extended
{
    public class IconDropdown<T> : KeyValueDropdown<T> {

        private readonly SortedList<T, AsyncTexture2D> _itemIcons;

        private AsyncTexture2D _selectedItemIcon;

        private const int BORDER_WIDTH = 2;

        private int _itemsPerRow = 5;
        public int ItemsPerRow {
            get => _itemsPerRow;
            set {
                if (SetProperty(ref _itemsPerRow, value)) {
                    Invalidate();
                }
            }
        }
        private int _iconPadding = 2;
        public int IconPadding {
            get => _iconPadding;
            set {
                if (SetProperty(ref _iconPadding, value)) {
                    Invalidate();
                }
            }
        }

        private int _edgePadding = 8;
        public  int EdgePadding { get => _edgePadding;
            set {
                if (SetProperty(ref _edgePadding, value)) {
                    Invalidate();
                }
            }
        }

        private readonly Texture2D _textureEmptySlot;

        public IconDropdown() {
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

        private Rectangle GetItemBounds(int index) {
            int col = index % _itemsPerRow;
            int row = index / _itemsPerRow;

            var maxIconSize  = GetMaxItemSize();

            int paddedWidth  = maxIconSize.X + _iconPadding;
            int paddedHeight = maxIconSize.Y + _iconPadding;

            int x = _edgePadding + col * paddedWidth;
            int y = _edgePadding + row * paddedHeight;

            return new Rectangle(x, y, maxIconSize.X, maxIconSize.Y);
        }

        private Rectangle GetInner(Rectangle bounds) {
            // Returns the "visual" inner frame of the empty slot texture.
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

            spriteBatch.DrawOnCtrl(panel, _textureEmptySlot, bounds, Color.White);
            var centered = GetInner(bounds).GetCenteredFit(icon.Bounds.Size);
            spriteBatch.DrawOnCtrl(panel, icon, centered);

            if (highlighted) {
                spriteBatch.DrawRectangleOnCtrl(panel, bounds, BORDER_WIDTH, Color.White * 0.7f);
            } else if (!this.HasSelected || !Equals(item, SelectedItem)) {
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
            if (_itemIcons.Count == 0 || _itemsPerRow <= 0) return Point.Zero;

            var maxIconSize = GetMaxItemSize();

            int columns  = Math.Min(_itemsPerRow, _itemIcons.Count);
            int rows = (_itemIcons.Count + _itemsPerRow - 1) / _itemsPerRow;

            int totalWidth  = columns * maxIconSize.X + (columns - 1) * _iconPadding + 2 * _edgePadding;
            int totalHeight = rows    * maxIconSize.Y + (rows    - 1) * _iconPadding + 2 * _edgePadding;

            return new Point(totalWidth, totalHeight);
        }
    }
}
