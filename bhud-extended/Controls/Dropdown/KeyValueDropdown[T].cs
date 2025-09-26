using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Blish_HUD.Extended
{
    public abstract class KeyValueDropdown<T> : Control
    {
        protected sealed class DropdownPanel : Control
        {

            private const int TOOLTIP_HOVER_DELAY = 800;
            private const int SCROLL_CLOSE_THRESHOLD = 20;

            private KeyValueDropdown<T> _dropdown;

            private int _highlightedItemIndex = -1;

            private int HighlightedItemIndex
            {
                get => _highlightedItemIndex;
                set
                {
                    if (SetProperty(ref _highlightedItemIndex, value))
                    {
                        _hoverTime = 0;
                    }
                }
            }

            private double _hoverTime;

            private int _startTop;


            private DropdownPanel(KeyValueDropdown<T> assocDropdown)
            {
                _dropdown = assocDropdown;
                _size     = _dropdown.GetDropdownSize();
                _location = GetPanelLocation();
                _zIndex   = Screen.TOOLTIP_BASEZINDEX;

                _startTop = _location.Y;

                this.Parent = Graphics.SpriteScreen;

                Input.Mouse.LeftMouseButtonPressed += InputOnMousedOffDropdownPanel;
                Input.Mouse.RightMouseButtonPressed += InputOnMousedOffDropdownPanel;
            }

            private Point GetPanelLocation()
            {
                var dropdownLocation = _dropdown.AbsoluteBounds.Location;

                int yUnderDef = Graphics.SpriteScreen.Bottom - (dropdownLocation.Y + _dropdown.Height + _size.Y);
                int yAboveDef = Graphics.SpriteScreen.Top + (dropdownLocation.Y - _size.Y);

                return yUnderDef > 0 || yUnderDef > yAboveDef
                // flip down
                ? dropdownLocation + new Point(0, _dropdown.Height - 1)
                // flip up
                : dropdownLocation - new Point(0, _size.Y + 1);
            }

            public static DropdownPanel ShowPanel(KeyValueDropdown<T> assocDropdown)
            {
                return new DropdownPanel(assocDropdown);
            }

            private void InputOnMousedOffDropdownPanel(object sender, MouseEventArgs e)
            {
                if (!this.MouseOver)
                {
                    if (e.EventType == MouseEventType.RightMouseButtonPressed)
                    {
                        // Required to prevent right-click exiting the menu from eating the next left click
                        _dropdown.HideDropdownPanelWithoutDebounce();
                    }
                    else
                    {
                        _dropdown.HideDropdownPanel();
                    }
                }
            }

            protected override void OnMouseMoved(MouseEventArgs e)
            {
                this.HighlightedItemIndex = _dropdown.GetHighlightedItemIndex(this.RelativeMousePosition);
                base.OnMouseMoved(e);
            }

            private KeyValuePair<T, string> GetActiveItem()
            {
                return _highlightedItemIndex > 0 && _highlightedItemIndex < _dropdown._items.Count 
                           ? _dropdown._items.ElementAt(_highlightedItemIndex) 
                           : default;
            }

            private void UpdateHoverTimer(double elapsedMilliseconds)
            {
                if (_mouseOver)
                {
                    _hoverTime += elapsedMilliseconds;
                }
                else
                {
                    _hoverTime = 0;
                }
                this.BasicTooltipText = _hoverTime > TOOLTIP_HOVER_DELAY 
                                            ? GetActiveItem().Value 
                                            : string.Empty;
            }

            private void UpdateDropdownLocation()
            {
                _location = GetPanelLocation();

                if (Math.Abs(_location.Y - _startTop) > SCROLL_CLOSE_THRESHOLD)
                {
                    Dispose();
                }
            }

            public override void DoUpdate(GameTime gameTime)
            {
                UpdateHoverTimer(gameTime.ElapsedGameTime.TotalMilliseconds);
                UpdateDropdownLocation();
            }

            protected override void OnClick(MouseEventArgs e)
            { 
                if (this.HighlightedItemIndex >= 0 && this.HighlightedItemIndex < _dropdown._items.Count) { 
                    _dropdown.SelectedItem = _dropdown._items.ElementAt(this.HighlightedItemIndex).Key;
                }
                base.OnClick(e);
                Dispose();
            }

            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                _dropdown.PaintDropdown(this, spriteBatch);
                int index = 0;
                foreach (var item in _dropdown._items.Keys) {
                    _dropdown.PaintDropdownItem(this, spriteBatch, item, index, index == this.HighlightedItemIndex);
                    index++;
                }
            }

            protected override void DisposeControl()
            {
                if (_dropdown != null)
                {
                    _dropdown._panel = null;
                    _dropdown = null;
                }

                Input.Mouse.LeftMouseButtonPressed -= InputOnMousedOffDropdownPanel;
                Input.Mouse.RightMouseButtonPressed -= InputOnMousedOffDropdownPanel;

                base.DisposeControl();
            }
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedItem"/> property has changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<T>> SelectedItemChanged;
        protected virtual void OnSelectedItemChanged(T previous, T current) {
            /* NOOP */
        }

        private readonly SortedList<T, string> _items;

        private T _selectedItem;
        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                T prev = _selectedItem;
                if (SetProperty(ref _selectedItem, value)) {
                    this.HasSelected = true;
                    SelectedItemChanged?.Invoke(this, new ValueChangedEventArgs<T>(prev, _selectedItem));
                    OnSelectedItemChanged(prev, _selectedItem);
                }
            }
        }

        protected bool HasSelected { get; private set; }

        private DropdownPanel _panel;
        private bool _hadPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dropdown"/> class.
        /// </summary>
        protected KeyValueDropdown()
        {
            _items = new SortedList<T, string>();
            this.Size = Dropdown.Standard.Size;
        }

        protected bool AddItem(T item, string tooltip)
        {
            if (_items.ContainsKey(item))
            {
                return false;
            }
            _items.Add(item, tooltip);
            OnItemAdded(item);
            OnItemsUpdated();
            Invalidate();
            return true;
        }

        public bool RemoveItem(T key)
        {
            if (!_items.Remove(key))
            {
                return false;
            }
            OnItemRemoved(key);
            OnItemsUpdated();
            Invalidate();
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            OnItemsCleared();
            OnItemsUpdated();
            Invalidate();
        }

        /// <summary>
        /// If the Dropdown box items are currently being shown, they are hidden.
        /// </summary>
        public void HideDropdownPanel()
        {
            _hadPanel = _mouseOver;
            _panel?.Dispose();
        }

        private void HideDropdownPanelWithoutDebounce()
        {
            HideDropdownPanel();
            _hadPanel = false;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (_panel == null && !_hadPanel)
            {
                _panel = DropdownPanel.ShowPanel(this);
            }
            else
            {
                _hadPanel = false;
            }
        }

        /// <summary>
        /// Called whenever an item is added to the <see cref="KeyValueDropdown{T}"/>.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnItemAdded(T item) {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever an item is removed from the <see cref="KeyValueDropdown{T}"/>.
        /// </summary>
        /// <param name="item">Item that was removed.</param>
        protected virtual void OnItemRemoved(T item) {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever all items are cleared from the <see cref="KeyValueDropdown{T}"/>.
        /// </summary>
        protected virtual void OnItemsCleared() {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever the amount of items in the <see cref="KeyValueDropdown{T}"/> has changed.
        /// </summary>
        protected virtual void OnItemsUpdated() {
            /* NOOP */
        }

        /// <summary>
        /// Draws the background of the expanded <see cref="DropdownPanel"/>.
        /// </summary>
        /// <param name="panel">The expanded <seealso cref="DropdownPanel"/> to draw on.</param>
        /// <param name="spriteBatch">The <seealso cref="SpriteBatch"/> to use for drawing.</param>
        protected virtual void PaintDropdown(DropdownPanel panel, SpriteBatch spriteBatch) {
            spriteBatch.DrawRectangleOnCtrl(panel, new Rectangle(Point.Zero, panel.Size), Color.Black);

            // Border (1px thick around dropdown)
            spriteBatch.DrawRectangleOnCtrl(panel, new Rectangle(Point.Zero, panel.Size), 1, Color.White * 0.5f);
        }

        /// <summary>
        /// Draws an individual item on the expanded <see cref="DropdownPanel"/>.
        /// </summary>
        /// <param name="panel">The expanded <seealso cref="DropdownPanel"/> to draw on.</param>
        /// <param name="spriteBatch">The <seealso cref="SpriteBatch"/> to use for drawing.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">The index of the item.</param>
        /// <param name="highlighted">If the mouse is currently hovering the item.</param>
        protected abstract void PaintDropdownItem(DropdownPanel panel, SpriteBatch spriteBatch, T item, int index, bool highlighted);

        /// <summary>
        /// Returns the index of the item being hovered over given the mouse position relative to the expanded <seealso cref="DropdownPanel"/>, 
        /// </summary>
        /// <param name="relativeMousePosition">Mouse position relative to the expanded <seealso cref="DropdownPanel"/>.</param>
        /// <returns>Index of the hovered item.</returns>
        protected abstract int GetHighlightedItemIndex(Point relativeMousePosition);

        /// <summary>
        /// Returns the size of the expanded <seealso cref="DropdownPanel"/>.
        /// </summary>
        /// <returns>Size of the expanded <seealso cref="DropdownPanel"/>.</returns>
        protected abstract Point GetDropdownSize();
    }
}
