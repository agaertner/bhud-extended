using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.Extended {
    public static class ChatUtil
    {
        public const int MAX_MESSAGE_LENGTH  = 199;

        private const int WAIT_INPUT_FOCUS_MS = 250;

        private static Logger _logger             = Logger.GetLogger(typeof(ChatUtil));
        
        private static readonly IReadOnlyDictionary<ModifierKeys, int> _modifierLookUp = new Dictionary<ModifierKeys, int>
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
        public static async Task Send(string text, KeyBinding messageKey)
        {
            try {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;

                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(text).Result) {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                if (!IsTextValid(text) || !await Focus(messageKey)) {
                    return;
                }

                KeyboardUtil.Press(162, true);   // LControl
                KeyboardUtil.Stroke(65);         // A
                KeyboardUtil.Release(162, true); // LControl
                KeyboardUtil.Stroke(46);         // Del
                KeyboardUtil.Press(162, true);   // LControl
                KeyboardUtil.Stroke(86);         // V
                KeyboardUtil.Release(162, true); // LControl
                KeyboardUtil.Stroke(13);         // Enter
                SetUnicodeBytesAsync(prevClipboardContent);
            } catch (Exception e) {
                _logger.Debug(e, e.Message);
            }
        }

        public static async Task SendWhisper(string recipient, string cmdAndMessage, KeyBinding messageKey) {
            try
            {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;

                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(cmdAndMessage).Result) {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                if (!IsTextValid(cmdAndMessage) || !await Focus(messageKey)) {
                    return;
                }

                KeyboardUtil.Press(162, true);   // LControl
                KeyboardUtil.Stroke(86);   // V
                KeyboardUtil.Release(162, true); // LControl

                // We are now in the recipient field
                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(recipient.Trim()).Result)
                {
                    await Unfocus();
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                // Paste recipient
                KeyboardUtil.Press(162, true);   // LControl
                KeyboardUtil.Stroke(86);   // V
                KeyboardUtil.Release(162, true); // LControl

                // Switch to text message field to be able to send the message
                KeyboardUtil.Stroke(9); // Tab

                // Send message
                KeyboardUtil.Stroke(13); // Enter

                // Fix game keeping focus in the Whisper chat edit box.
                Thread.Sleep(1);
                KeyboardUtil.Stroke(13); // Enter

                // Restore clipboard
                SetUnicodeBytesAsync(prevClipboardContent);
            }
            catch (Exception e)
            {
                _logger.Debug(e, e.Message);
            }
        }

        /// <summary>
        /// Inserts or appends text without sending it.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        /// <param name="messageKey">The key which is used to open the message box.</param>
        public static async Task Insert(string text, KeyBinding messageKey)
        {
            try {
                byte[] prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;
                if (!ClipboardUtil.WindowsClipboardService.SetTextAsync(text).Result) {
                    SetUnicodeBytesAsync(prevClipboardContent);
                    return;
                }

                if (!IsTextValid(text) || !await Focus(messageKey)) {
                    return;
                }

                KeyboardUtil.Press(162, true);   // LControl
                KeyboardUtil.Stroke(86);   // V
                KeyboardUtil.Release(162, true); // LControl
                SetUnicodeBytesAsync(prevClipboardContent);
            } catch (Exception e) {
                _logger.Debug(e, e.Message);
            }
        }

        private static async Task<bool> Focus(KeyBinding messageKey) {
            return await Task.Run(() => {
                if (messageKey == null ||
                    messageKey.PrimaryKey == Keys.None && messageKey.ModifierKeys == ModifierKeys.None) {
                    return GameService.Gw2Mumble.IsAvailable && GameService.Gw2Mumble.UI.IsTextInputFocused;
                }

                // Tell the game to release the shift keys so chat can be opened.
                KeyboardUtil.Release(160);
                KeyboardUtil.Release(161);

                var hasModifierKey = _modifierLookUp.TryGetValue(messageKey.ModifierKeys, out var modifierKey);

                if (hasModifierKey) {
                    KeyboardUtil.Press(modifierKey, true);
                }

                if (messageKey.PrimaryKey != Keys.None) {
                    KeyboardUtil.Stroke((int) messageKey.PrimaryKey);
                }

                if (hasModifierKey) {
                    KeyboardUtil.Release(modifierKey, true);
                }

                var waitTil = DateTime.UtcNow.AddMilliseconds(WAIT_INPUT_FOCUS_MS);
                while (DateTime.UtcNow.Subtract(waitTil).TotalMilliseconds < WAIT_INPUT_FOCUS_MS
                    && GameService.Gw2Mumble.IsAvailable
                    && GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                    && GameService.GameIntegration.Gw2Instance.Gw2HasFocus
                    && GameService.GameIntegration.Gw2Instance.IsInGame) {
                    if (GameService.Gw2Mumble.UI.IsTextInputFocused) {
                        return true;
                    }
                }
                return GameService.Gw2Mumble.UI.IsTextInputFocused;
            });
        }

        private static async Task<bool> Unfocus() {
            return await Task.Run(() => {
                if (GameService.Gw2Mumble.IsAvailable && GameService.Gw2Mumble.UI.IsTextInputFocused) {
                    KeyboardUtil.Stroke(27); // ESC
                }

                var waitTil = DateTime.UtcNow.AddMilliseconds(WAIT_INPUT_FOCUS_MS);
                while (DateTime.UtcNow.Subtract(waitTil).TotalMilliseconds < WAIT_INPUT_FOCUS_MS
                    && GameService.Gw2Mumble.IsAvailable
                    && GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                    && GameService.GameIntegration.Gw2Instance.Gw2HasFocus
                    && GameService.GameIntegration.Gw2Instance.IsInGame) {
                    if (!GameService.Gw2Mumble.UI.IsTextInputFocused) {
                        return true;
                    }
                }
                return !GameService.Gw2Mumble.UI.IsTextInputFocused;
            });
        }

        private static void SetUnicodeBytesAsync(byte[] clipboardContent)
        {
            if (clipboardContent == null) {
                return;
            }
            try {
                ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(clipboardContent);
            } catch (Exception e) {
                _logger.Debug(e, e.Message);
            }
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
