using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Blish_HUD.Extended
{
    public abstract class BaseDropdown<T> : Control
    {
        protected sealed class DropdownMenu : FlowPanel
        {
            private const int TOOLTIP_HOVER_DELAY = 800;
            private const int SCROLL_CLOSE_THRESHOLD = 20;

            private BaseDropdown<T> _dropdown;

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

            private readonly int _startTop;

            private DropdownMenu(BaseDropdown<T> assocDropdown)
            {
                _dropdown  = assocDropdown;
                _size      = _dropdown.GetDropdownSize();
                _location  = GetPanelLocation();
                _zIndex    = Screen.TOOLTIP_BASEZINDEX;
                _canScroll = true;
                _startTop  = _location.Y;

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

            public static DropdownMenu ShowPanel(BaseDropdown<T> assocDropdown)
            {
                return new DropdownMenu(assocDropdown);
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

            private KeyValuePair<T, Func<string>> GetActiveItem()
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
                                            ? GetActiveItem().Value?.Invoke() 
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

            public override void UpdateContainer(GameTime gameTime)
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

            public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
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
                    _dropdown._menu = null;
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

        private readonly SortedList<T, Func<string>> _items;

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

        private Point _menuSize = Point.Zero;
        public Point MenuSize {
            get => _menuSize;
            set {
                if (_menuSize == value || value.X < 0 || value.Y < 0) return;
                var menuSize = _menuSize;
                _menuSize = value; 
                OnPropertyChanged();
                if (menuSize.Y != _menuSize.Y) OnPropertyChanged(nameof(MenuHeight), false);
                if (menuSize.X != _menuSize.X) OnPropertyChanged(nameof(MenuWidth), false);
            }
        }

        public int MenuHeight {
            get => _menuSize.Y;
            set {
                if (_menuSize.Y == value) return;
                this.MenuSize = new Point(_menuSize.X, value);
            }
        }

        public int MenuWidth {
            get => _menuSize.X;
            set {
                if (_menuSize.X == value) return;
                this.MenuSize = new Point(value, _menuSize.Y);
            }
        }

        protected bool HasSelected { get; private set; }

        private DropdownMenu _menu;
        private bool _hadPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dropdown"/> class.
        /// </summary>
        protected BaseDropdown()
        {
            _items = new SortedList<T, Func<string>>();
            this.Size = Dropdown.Standard.Size;
        }

        protected bool AddItem(T item, Func<string> tooltip)
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
            Deselect();
            _items.Clear();
            OnItemsCleared();
            OnItemsUpdated();
            Invalidate();
        }

        public void Deselect() {
            this.HasSelected = false;
        }

        /// <summary>
        /// If the Dropdown box items are currently being shown, they are hidden.
        /// </summary>
        public void HideDropdownPanel()
        {
            _hadPanel = _mouseOver;
            OnDropdownMenuClosed(_menu);
            _menu?.Dispose();
        }

        private void HideDropdownPanelWithoutDebounce()
        {
            HideDropdownPanel();
            _hadPanel = false;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (_menu == null && !_hadPanel)
            {
                _menu = DropdownMenu.ShowPanel(this);
                OnDropdownMenuShown(_menu);
            }
            else
            {
                _hadPanel = false;
            }
        }

        /// <summary>
        /// Called whenever an item is added to the <see cref="BaseDropdown{T}"/>.
        /// </summary>
        /// <param name="item">Item that was added</param>
        protected virtual void OnItemAdded(T item) {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever an item is removed from the <see cref="BaseDropdown{T}"/>.
        /// </summary>
        /// <param name="item">Item that was removed.</param>
        protected virtual void OnItemRemoved(T item) {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever all items are cleared from the <see cref="BaseDropdown{T}"/>.
        /// </summary>
        protected virtual void OnItemsCleared() {
            /* NOOP */
        }

        /// <summary>
        /// Called whenever the amount of items in the <see cref="BaseDropdown{T}"/> has changed.
        /// </summary>
        protected virtual void OnItemsUpdated() {
            /* NOOP */
        }

        /// <summary>
        /// Draws the background of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        /// <param name="menu">The expanded <see cref="DropdownMenu"/> to draw on.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
        protected virtual void PaintDropdown(DropdownMenu menu, SpriteBatch spriteBatch) {
            spriteBatch.DrawRectangleOnCtrl(menu, new Rectangle(Point.Zero, menu.Size), Color.Black);

            // Border (1px thick around dropdown)
            spriteBatch.DrawRectangleOnCtrl(menu, new Rectangle(Point.Zero, menu.Size), 1, Color.White * 0.5f);
        }

        /// <summary>
        /// Draws an individual item on the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        /// <param name="menu">The expanded <see cref="DropdownMenu"/> to draw on.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">The index of the item.</param>
        /// <param name="highlighted">If the mouse is currently hovering the item.</param>
        protected abstract void PaintDropdownItem(DropdownMenu menu, SpriteBatch spriteBatch, T item, int index, bool highlighted);

        /// <summary>
        /// Returns the index of the item being hovered over given the mouse position relative to the expanded <see cref="DropdownMenu"/>, 
        /// </summary>
        /// <param name="menu">The expanded <see cref="DropdownMenu"/>.</param>
        /// <returns>Index of the hovered item.</returns>
        protected virtual int GetHighlightedItemIndex(DropdownMenu menu) {
            return -1;
        }

        /// <summary>
        /// Returns the size of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        /// <returns>Size of the expanded <see cref="DropdownMenu"/>.</returns>
        protected virtual Point GetDropdownSize() {
            return this.MenuSize;
        }

        /// <summary>
        /// Called when the expanded <see cref="DropdownMenu"/> is shown.
        /// </summary>
        /// <param name="menu">The expanded <see cref="DropdownMenu"/>.</param>
        protected virtual void OnDropdownMenuShown(DropdownMenu menu) {
            /* NOOP */
        }

        /// <summary>
        /// Called when the expanded <see cref="DropdownMenu"/> is closed.
        /// </summary>
        /// <param name="menu">The expanded <see cref="DropdownMenu"/>.</param>
        /// <remarks><see cref="DropdownMenu.Dispose"/> is called after this function.</remarks>
        protected virtual void OnDropdownMenuClosed(DropdownMenu menu) {
            /* NOOP */
        }
    }
}
