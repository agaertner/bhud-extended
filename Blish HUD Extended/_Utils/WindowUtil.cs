using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
namespace Blish_HUD.Extended
{
    public class WindowUtil
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static bool GetInnerBounds(IntPtr hWnd, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            if (!GetWindowRect(hWnd, out var wndBounds) || !GetClientRect(hWnd, out var clientBounds)) return false;
            // Border thickness
            var widthOffset = wndBounds.Right - wndBounds.Left - (clientBounds.Right - clientBounds.Left);
            // Titlebar height + Border thickness
            var heightOffset = wndBounds.Bottom - wndBounds.Top - (clientBounds.Bottom - clientBounds.Top);
            var width = Math.Abs(wndBounds.Left - wndBounds.Right) - widthOffset * 2;
            var height = Math.Abs(wndBounds.Top - wndBounds.Bottom) - heightOffset * 2;
            bounds = new Rectangle(wndBounds.Left + widthOffset, wndBounds.Top + heightOffset, width, height);
            return true;
        }
    }
}
