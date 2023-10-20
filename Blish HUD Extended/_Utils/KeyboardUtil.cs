using Blish_HUD.Extended.WinApi;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Blish_HUD.Extended {
    public static class KeyboardUtil {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;

        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        private const uint KEY_PRESSED = 0x8000;

        private const uint VK_LSHIFT = 0xA0;
        private const uint VK_RSHIFT = 0xA1;

        private const uint VK_LCONTROL = 0xA2;
        private const uint VK_RCONTROL = 0xA3;

        private const uint VK_CONTROL = 0x11;
        private const uint VK_SHIFT = 0x10;

        [Flags]
        internal enum KeyEventF : uint {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeybdInput {
            internal short wVk;
            internal short wScan;
            internal KeyEventF dwFlags;
            internal int time;
            internal UIntPtr dwExtraInfo;
        }

        private static List<int> ExtendedKeys = new List<int> {
            0x2D, 0x24, 0x22,
            0x2E, 0x23, 0x21,
            0xA5, 0xA1, 0xA3,
            0x26, 0x28, 0x25, 
            0x27, 0x90, 0x2A
        };

        [DllImport("USER32.dll")]
        private static extern short GetKeyState(uint vk);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] WinApi.Input[] pInputs, int cbSize);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SendMessage(IntPtr hWnd, uint msg, uint wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, int lParam); // sends a message asynchronously.

        /// <summary>
        /// Presses a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to press.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Press(int keyCode, bool sendToSystem = false) {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem) {
                WinApi.Input[] nInputs;
                if (ExtendedKeys.Contains(keyCode)) {
                    nInputs = new[]
                    {
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = 224,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.EXTENDEDKEY
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk   = (short)keyCode
                                }
                            }
                        }
                    };
                }
                SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size);
            } else {
                uint vkCode = (uint)keyCode;
                ExtraKeyInfo lParam = new ExtraKeyInfo {
                    scanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC)
                };

                if (ExtendedKeys.Contains(keyCode))
                    lParam.extendedKey = 1;
                SendMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_KEYDOWN, vkCode, lParam.GetInt());
            }
            Thread.Sleep(1); // Just to give time to process the press.
        }

        /// <summary>
        /// Releases a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to release.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Release(int keyCode, bool sendToSystem = false) {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem) {
                WinApi.Input[] nInputs;
                if (ExtendedKeys.Contains(keyCode)) {
                    nInputs = new[]
                    {
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan = 224,
                                    wVk = 0,
                                    dwFlags = 0
                                }
                            }
                        },
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.EXTENDEDKEY | KeyEventF.KEYUP
                                }
                            }
                        }
                    };
                } else {
                    nInputs = new[]
                    {
                        new WinApi.Input
                        {
                            type = InputType.KEYBOARD,
                            U = new InputUnion
                            {
                                ki = new KeybdInput
                                {
                                    wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    wVk     = (short)keyCode,
                                    dwFlags = KeyEventF.KEYUP
                                }
                            }
                        }
                    };
                }
                SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size);
            } else {
                uint vkCode = (uint)keyCode;
                ExtraKeyInfo lParam = new ExtraKeyInfo {
                    scanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC),
                    repeatCount = 1,
                    prevKeyState = 1,
                    transitionState = 1
                };

                if (ExtendedKeys.Contains(keyCode))
                    lParam.extendedKey = 1;
                SendMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_KEYUP, vkCode, lParam.GetInt());
            }
        }

        /// <summary>
        /// Performs a keystroke in which a key is pressed and immediately released once.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to stroke.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Stroke(int keyCode, bool sendToSystem = false) {
            Press(keyCode, sendToSystem);
            Release(keyCode, sendToSystem);
        }

        /// <summary>
        /// Checks if the specified key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsPressed(uint keyCode)
        {
            return Convert.ToBoolean(GetKeyState(keyCode) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the left Control key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if LCtrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsLCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_LCONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the right Control key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if RCtrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsRCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_RCONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the left Shift key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if LShift is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsLShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_LSHIFT) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the right Shift key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if RShift is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsRShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_RSHIFT) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if any Control key (left or right) is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if ctrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_CONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if any Shift key (left or right) is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if shift is pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_SHIFT) & KEY_PRESSED);
        }

        private class ExtraKeyInfo {
            public ushort repeatCount;
            public char scanCode;
            public ushort extendedKey, prevKeyState, transitionState;

            public int GetInt() {
                return repeatCount | (scanCode << 16) | (extendedKey << 24) |
                       (prevKeyState << 30) | (transitionState << 31);
            }
        }
    }
}