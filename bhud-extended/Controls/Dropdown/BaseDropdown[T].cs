using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
namespace Blish_HUD.Extended
{
    /// <summary>
    /// Implements base functionality of a dropdown control (i.e. items, click events).
    /// Can be used to create a dropdown with custom appearance and behaviour by overriding functions.
    /// </summary>
    /// <typeparam name="T">The value an item holds and which is returned when selected.</typeparam>
    public abstract class BaseDropdown<T> : Control
    {
        protected const int BORDER_WIDTH = 2;

        protected sealed class DropdownMenu : FlowPanel
        {
            private const int TOOLTIP_HOVER_DELAY    = 800;
            private const int SCROLL_CLOSE_THRESHOLD = 20;

            public sealed class DropdownItem : Control {
                public T Item => _item.Key;
                private readonly KeyValuePair<T, Func<string>> _item;
                public readonly DropdownMenu _menu;
                public DropdownItem(DropdownMenu assocMenu, KeyValuePair<T, Func<string>> item) {
                    _menu  = assocMenu;
                    _item = item;
                }

                protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
                    _menu._dropdown.PaintDropdownItem(this, spriteBatch, bounds);
                }

                protected override CaptureType CapturesInput() {
                    return CaptureType.Filter; // Allow clicks to go through to the DropdownMenu.
                }

                public override void DoUpdate(GameTime gameTime) {
                    if (this.MouseOver) {
                        _menu.HoveredItem = _item.Key;
                    }
                    // BasicTooltip (_item.Value) could be applied here instead of in DropdownMenu.
                }
            }

            private BaseDropdown<T> _dropdown;

            private T _hoveredItem;

            private T HoveredItem
            {
                get => _hoveredItem;
                set
                {
                    if (SetProperty(ref _hoveredItem, value))
                    {
                        _hoverTime = 0;
                    }
                }
            }

            private double _hoverTime;

            private readonly int _startTop;

            private DropdownMenu(BaseDropdown<T> assocDropdown)
            {
                _dropdown = assocDropdown;
                _size     = _dropdown.GetDropdownSize();
                _location = GetPanelLocation();
                _zIndex   = Screen.TOOLTIP_BASEZINDEX;
                _startTop = _location.Y;

                _canScroll           = true;
                _outerControlPadding = new Vector2(_dropdown.Margin, _dropdown.Margin);
                _controlPadding      = new Vector2(_dropdown.Spacing,_dropdown.Spacing);

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
                var menu = new DropdownMenu(assocDropdown);
                foreach (var item in assocDropdown._items) {
                    _ = new DropdownItem(menu, item) {
                        Parent = menu,
                        Size = assocDropdown.GetDropdownItemSize()
                    };
                }
                return menu;
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

                if (_hoverTime > TOOLTIP_HOVER_DELAY && _dropdown._items.TryGetValue(_hoveredItem, out var func)) {
                    this.BasicTooltipText = func?.Invoke();
                } else {
                    this.BasicTooltipText = string.Empty;
                }
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
                _dropdown.SelectedItem = _hoveredItem;
                base.OnClick(e);
                Dispose();
            }

            public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
                _dropdown.PaintDropdown(this, spriteBatch);
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
        /// <summary>
        /// Controls the size of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        public Point MenuSize {
            get => _menuSize;
            set {
                if (_menuSize == value || value.X < 0 || value.Y < 0) return;
                var prevSize = _menuSize;
                _menuSize = value; 
                OnPropertyChanged();
                if (prevSize.Y != _menuSize.Y) OnPropertyChanged(nameof(MenuHeight), false);
                if (prevSize.X != _menuSize.X) OnPropertyChanged(nameof(MenuWidth), false);
            }
        }

        /// <summary>
        /// Controls the height of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        public int MenuHeight {
            get => _menuSize.Y;
            set {
                if (_menuSize.Y == value) return;
                this.MenuSize = new Point(_menuSize.X, value);
            }
        }

        /// <summary>
        /// Controls the width of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        public int MenuWidth {
            get => _menuSize.X;
            set {
                if (_menuSize.X == value) return;
                this.MenuSize = new Point(value, _menuSize.Y);
            }
        }

        private int _spacing;
        /// <summary>
        /// Controls the spacing between items in the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        public int Spacing {
            get => _spacing;
            set {
                if (SetProperty(ref _spacing, value)) {
                    Invalidate();
                }
            }
        }

        private int _margin = 6;
        /// <summary>
        /// Controls the space around the items in the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        public int Margin {
            get => _margin;
            set {
                if (SetProperty(ref _margin, value)) {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Control variable.
        /// </summary>
        protected bool HasSelected { get; private set; }

        private DropdownMenu _menu;
        private bool         _hadPanel;

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
            // Background
            spriteBatch.DrawRectangleOnCtrl(menu, new Rectangle(Point.Zero, menu.Size), Color.Black);
            // Border
            spriteBatch.DrawRectangleOnCtrl(menu, new Rectangle(Point.Zero, menu.Size), BORDER_WIDTH, Color.White * 0.5f);
        }

        /// <summary>
        /// Draws an individual item on the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        /// <param name="ctrl">The expanded <see cref="DropdownMenu.DropdownItem"/> to draw on.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
        /// <param name="bounds">The bounds of the item.</param>
        protected abstract void PaintDropdownItem(DropdownMenu.DropdownItem ctrl, SpriteBatch spriteBatch, Rectangle bounds);

        /// <summary>
        /// Returns the size of the expanded <see cref="DropdownMenu"/>.
        /// </summary>
        /// <returns>Size of the expanded <see cref="DropdownMenu"/>.</returns>
        protected virtual Point GetDropdownSize() {
            return this.MenuSize;
        }

        /// <summary>
        /// Returns the size of each <see cref="DropdownMenu.DropdownItem"/>.
        /// </summary>
        /// <returns>Size of a <see cref="DropdownMenu.DropdownItem"/>.</returns>
        protected virtual Point GetDropdownItemSize() {
            return new Point(this.Width - BORDER_WIDTH, this.Height);
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
