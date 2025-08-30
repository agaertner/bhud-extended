using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Blish_HUD.Extended
{
    internal static class WindowUtil
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        internal static bool GetCursorPosition(IntPtr hWnd, out Point cursorPos)
        {
            if (GetCursorPos(out var pos)
                && ScreenToClient(hWnd, ref pos)
                && GetWindowRect(hWnd, out var wndBounds)
                && GetClientRect(hWnd, out var clientBounds)) {
                // Border thickness
                var widthOffset = wndBounds.Right - wndBounds.Left - (clientBounds.Right - clientBounds.Left);
                // Titlebar height + Border thickness
                var heightOffset = wndBounds.Bottom - wndBounds.Top - (clientBounds.Bottom - clientBounds.Top);
                pos.X -= wndBounds.Left + widthOffset;
                pos.Y -= wndBounds.Top + heightOffset;
                // Replaces the client coordinates with screen coordinates.
                if (ClientToScreen(hWnd, ref pos))
                {
                    cursorPos = new Point(pos.X, pos.Y);
                    return true;
                }
            }
            cursorPos = Point.Empty;
            return false;
        }

        public static bool GetInnerBounds(IntPtr hWnd, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            if (GetWindowRect(hWnd, out var wndBounds) && GetClientRect(hWnd, out var clientBounds))
            {
                // Border thickness
                var widthOffset = wndBounds.Right - wndBounds.Left - (clientBounds.Right - clientBounds.Left);
                // Titlebar height + Border thickness
                var heightOffset = wndBounds.Bottom - wndBounds.Top - (clientBounds.Bottom - clientBounds.Top);
                var width = Math.Abs(wndBounds.Left - wndBounds.Right) - widthOffset * 2;
                var height = Math.Abs(wndBounds.Top - wndBounds.Bottom) - heightOffset * 2;
                bounds = new Rectangle(wndBounds.Left + widthOffset, wndBounds.Top + heightOffset, width, height);
                return true;
            }
            return false;
        }
    }
}