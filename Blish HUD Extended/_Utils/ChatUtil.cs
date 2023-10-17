using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public static class ChatUtil
    {
        public const int MAX_MESSAGE_LENGTH = 199;

        private static Logger _logger = Logger.GetLogger(typeof(ChatUtil));
        
        private static readonly IReadOnlyDictionary<ModifierKeys, int> ModifierLookUp = new Dictionary<ModifierKeys, int>
        {
            {ModifierKeys.Alt, 18},
            {ModifierKeys.Ctrl, 17},
            {ModifierKeys.Shift, 16}
        };

        /// <summary>
        /// Clears the input box and then sends the given text. 
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <param name="messageKey">The key which is used to open the message box.</param>
        public static void Send(string text, KeyBinding messageKey)
        {
            if (!IsTextValid(text) || !Focus(messageKey)) {
                return;
            }
            try {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;

                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(text).Result) {
                    
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                Thread.Sleep(10);
                KeyboardUtil.Press(162, true); // LControl
                Thread.Sleep(10);
                KeyboardUtil.Stroke(65, true); // A
                Thread.Sleep(10);
                KeyboardUtil.Release(162, true); // LControl
                Thread.Sleep(10);
                KeyboardUtil.Stroke(46, true);   // Del
                Thread.Sleep(10);
                KeyboardUtil.Press(162, true);   // LControl
                Thread.Sleep(50);
                KeyboardUtil.Stroke(86, true);   // V
                Thread.Sleep(50);
                KeyboardUtil.Release(162, true); // LControl
                Thread.Sleep(10);
                KeyboardUtil.Stroke(13); // Enter
                SetUnicodeBytesAsync(prevClipboardContent);
            } catch (Exception e) {
                _logger.Info(e, e.Message);
            }
        }

        public static void SendWhisper(string recipient, string cmdAndMessage, KeyBinding messageKey) {
            if (!IsTextValid(cmdAndMessage) || !Focus(messageKey)) {
                return;
            }
            try
            {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;

                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(cmdAndMessage).Result)
                {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                Thread.Sleep(10);
                KeyboardUtil.Press(162, true);   // LControl
                Thread.Sleep(50);
                KeyboardUtil.Stroke(86, true);   // V
                Thread.Sleep(50);
                KeyboardUtil.Release(162, true); // LControl
                Thread.Sleep(10);

                // We are now in the recipient field
                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(recipient.Trim()).Result)
                {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                // Paste recipient
                Thread.Sleep(10);
                KeyboardUtil.Press(162, true);   // LControl
                Thread.Sleep(10);
                KeyboardUtil.Stroke(86, true);   // V
                Thread.Sleep(50);
                KeyboardUtil.Release(162, true); // LControl

                Thread.Sleep(10);

                // Switch to text message field to be able to send the message
                KeyboardUtil.Stroke(9); // Tab

                Thread.Sleep(10);

                // Send message
                KeyboardUtil.Stroke(13); // Enter

                // Restore clipboard
                SetUnicodeBytesAsync(prevClipboardContent);
            }
            catch (Exception e)
            {
                _logger.Info(e, e.Message);
            }
        }

        /// <summary>
        /// Inserts or appends text without sending it.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        /// <param name="messageKey">The key which is used to open the message box.</param>
        public static void Insert(string text, KeyBinding messageKey)
        {
            if (!IsTextValid(text) || !Focus(messageKey))
            {
                return;
            }
            try {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;
                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(text).Result)
                {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }
                Thread.Sleep(1);
                KeyboardUtil.Press(162, true); // LControl
                KeyboardUtil.Stroke(86, true); // V
                Thread.Sleep(1);
                KeyboardUtil.Release(162, true); // LControl
                SetUnicodeBytesAsync(prevClipboardContent);
            } catch (Exception e) {
                _logger.Info(e, e.Message);
            }
        }

        private static bool Focus(KeyBinding messageKey)
        {
            if (messageKey == null || (messageKey.PrimaryKey == Keys.None && 
                                       messageKey.ModifierKeys == ModifierKeys.None)) {
                return GameService.Gw2Mumble.IsAvailable && GameService.Gw2Mumble.UI.IsTextInputFocused;
            }

            while (GameService.Gw2Mumble.IsAvailable && !GameService.Gw2Mumble.UI.IsTextInputFocused)
            {
                // Tell the game to release the shift keys so chat can be opened.
                KeyboardUtil.Release(160);
                KeyboardUtil.Release(161);
                Thread.Sleep(5);

                var hasModifierKey = ModifierLookUp.TryGetValue(messageKey.ModifierKeys, out var modifierKey);
                if (hasModifierKey)
                {
                    KeyboardUtil.Press(modifierKey);
                    Thread.Sleep(5);
                }
                if (messageKey.PrimaryKey != Keys.None)
                {
                    KeyboardUtil.Stroke((int)messageKey.PrimaryKey);
                }
                if (hasModifierKey)
                {
                    Thread.Sleep(5);
                    KeyboardUtil.Release(modifierKey);
                }
                Thread.Sleep(250);
                if (IsBusy()) {
                    break;
                }
            }
            return true;
        }

        private static async Task SetUnicodeBytesAsync(byte[] clipboardContent)
        {
            if (clipboardContent == null) {
                return;
            }
            try {
                await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(clipboardContent);
            } catch (Exception e) {
                _logger.Info(e, e.Message);
            }
        }

        private static bool IsBusy()
        {
            return !GameService.Gw2Mumble.IsAvailable 
                   || !GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                   || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus
                   || !GameService.GameIntegration.Gw2Instance.IsInGame
                   || GameService.Gw2Mumble.UI.IsTextInputFocused;
        }

        private static bool IsTextValid(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger.Info($"Invalid chat message. Argument '{nameof(text)}' was null or empty.");
                return false;
            }
            if (text.Length > MAX_MESSAGE_LENGTH)
            {
                _logger.Info($"Invalid chat message. Argument '{nameof(text)}' exceeds limit of {MAX_MESSAGE_LENGTH} characters. Value: \"{text.Substring(0, 25)}[..+{MAX_MESSAGE_LENGTH-25}]\"");
                return false;
            }
            return true;
        }

        public static bool IsLengthValid(string message)
        {
            return string.IsNullOrEmpty(message) || message.Length <= MAX_MESSAGE_LENGTH;
        }
    }
}
