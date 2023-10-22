using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blish_HUD.Extended {
    public static class ChatUtil
    {
        public const int MAX_MESSAGE_LENGTH  = 199;

        private const int WAIT_MS = 250;

        private static readonly IReadOnlyDictionary<ModifierKeys, int> _modifierLookUp = new Dictionary<ModifierKeys, int>
        {
            {ModifierKeys.Alt, 18},
            {ModifierKeys.Ctrl, 17},
            {ModifierKeys.Shift, 16}
        };

        /// <summary>
        /// Gives focus to the chat edit box and deletes any existing text.
        /// </summary>
        /// <param name="messageKey">The key which is used to open the chat edit box.</param>
        public static async Task<bool> Clear(KeyBinding messageKey) {
            return await Focus(messageKey) && KeyboardUtil.Clear();
        }

        /// <summary>
        /// Clears the input box and then sends the given text. 
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <param name="messageKey">The key which is used to open the chat edit box.</param>
        /// <param name="logger">Logger to use for logging.</param>
        public static async Task Send(string text, KeyBinding messageKey, Logger logger = null)
        {
            logger ??= Logger.GetLogger(typeof(ChatUtil));

            byte[] prevClipboardContent = null;

            try {
               prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;
            } catch (Exception e) {
                logger.Debug(e, e.Message);
            }

            if (!await SetTextAsync(text, logger)) {
                await SetUnicodeBytesAsync(prevClipboardContent, logger);
                return;
            }

            if (!IsTextValid(text, logger) || !await Focus(messageKey)) {
                return;
            }

            try {
                if (!KeyboardUtil.Paste() || !KeyboardUtil.Stroke(13)) {
                    logger.Info($"Failed to send text to chat: {text}");
                    await Unfocus();
                }
            } finally {
                await SetUnicodeBytesAsync(prevClipboardContent, logger);
            }
        }

        public static async Task SendWhisper(string recipient, string cmdAndMessage, KeyBinding messageKey, Logger logger = null) {
            logger ??= Logger.GetLogger(typeof(ChatUtil));

            byte[] prevClipboardContent = null;

            try {
                prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;
            } catch (Exception e) {
                logger.Debug(e, e.Message);
            }

            if (!await SetTextAsync(cmdAndMessage, logger)) {
                await SetUnicodeBytesAsync(prevClipboardContent, logger);
                return;
            }

            if (!IsTextValid(cmdAndMessage, logger) || !await Focus(messageKey)) {
                return;
            }

            try { 
                if (!KeyboardUtil.Paste()) {
                    logger.Info($"Failed to send text to chat: {cmdAndMessage}");
                    await Unfocus();
                    return;
                }

                // We are now in the recipient field
                if (!await SetTextAsync(recipient.Trim(), logger)) {
                    await Unfocus();
                    return;
                }

                // Paste recipient
                if (!KeyboardUtil.Paste()) {
                    logger.Info($"Failed to paste recipient: {recipient}");
                    await Unfocus();
                    return;
                }

                // Switch to text message field to be able to send the message
                await Task.Delay(1);
                KeyboardUtil.Stroke(9);   // Tab
                await Task.Delay(1);
                KeyboardUtil.Stroke(13); // Enter
                
                // Fix game keeping focus in the Whisper chat edit box.
                await Task.Delay(50);
                await Unfocus();

            } finally {
                await SetUnicodeBytesAsync(prevClipboardContent, logger);
            }
        }

        /// <summary>
        /// Inserts or appends text without sending it.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        /// <param name="messageKey">The key which is used to open the message box.</param>
        /// <param name="logger">Logger to use for logging.</param>
        public static async Task Insert(string text, KeyBinding messageKey, Logger logger = null)
        {
            logger ??= Logger.GetLogger(typeof(ChatUtil));

            byte[] prevClipboardContent = null;

            try {
                prevClipboardContent = ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync().Result;
            } catch (Exception e) {
                logger.Debug(e, e.Message);
            }

            if (!await SetTextAsync(text, logger)) {
                await SetUnicodeBytesAsync(prevClipboardContent, logger);
                return;
            }

            if (!IsTextValid(text, logger) || !await Focus(messageKey)) {
                return;
            }

            KeyboardUtil.Paste();

            await SetUnicodeBytesAsync(prevClipboardContent, logger);
        }

        private static async Task<bool> Focus(KeyBinding messageKey) {
            return await Task.Run(() => {
                if (GameService.Gw2Mumble.UI.IsTextInputFocused) {
                    return true;
                }

                if (messageKey == null ||
                    messageKey.PrimaryKey == Keys.None && messageKey.ModifierKeys == ModifierKeys.None) {
                    return GameService.Gw2Mumble.UI.IsTextInputFocused;
                }

                // Tell the game to release the shift keys so chat can be opened.
                KeyboardUtil.Release(160);
                KeyboardUtil.Release(161);

                var hasModifierKey = _modifierLookUp.TryGetValue(messageKey.ModifierKeys, out var modifierKey);

                if (hasModifierKey) {
                    KeyboardUtil.Press(modifierKey);
                }

                if (messageKey.PrimaryKey != Keys.None) {
                    KeyboardUtil.Stroke((int)messageKey.PrimaryKey);
                }

                if (hasModifierKey) {
                    KeyboardUtil.Release(modifierKey);
                }

                var waitTil = DateTime.UtcNow.AddMilliseconds(WAIT_MS);
                while (DateTime.UtcNow < waitTil) {
                    if (GameService.Gw2Mumble.UI.IsTextInputFocused) {
                        return true;
                    }
                }
                return GameService.Gw2Mumble.UI.IsTextInputFocused;
            });
        }

        private static async Task<bool> Unfocus() {
            return await Task.Run(() => {
                if (!GameService.Gw2Mumble.UI.IsTextInputFocused) {
                    return true;
                }

                KeyboardUtil.Stroke(27); // ESC

                var waitTil = DateTime.UtcNow.AddMilliseconds(WAIT_MS);
                while (DateTime.UtcNow < waitTil) {
                    if (!GameService.Gw2Mumble.UI.IsTextInputFocused) {
                        return true;
                    }
                }
                return !GameService.Gw2Mumble.UI.IsTextInputFocused;
            });
        }

        private static async Task<bool> SetUnicodeBytesAsync(byte[] clipboardContent, Logger logger)
        {
            if (clipboardContent == null) {
                return true;
            }
            try {
                return await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(clipboardContent);
            } catch (Exception e) {
                logger.Debug(e, e.Message);
            }
            return false;
        }

        private static async Task<bool> SetTextAsync(string text, Logger logger, int retries = 5) {
            try {
                do {
                    if (await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) {
                        return true;
                    }
                    retries--;
                    await Task.Delay(WAIT_MS);
                } while (retries > 0);
            } catch (Exception e) {

                if (retries > 0) {
                    return await SetTextAsync(text, logger, retries - 1);
                }

                logger.Debug(e, e.Message);
            }
            return false;
        }

        private static bool IsTextValid(string text, Logger logger)
        {
            if (string.IsNullOrEmpty(text))
            {
                logger.Info($"Invalid chat message. Argument '{nameof(text)}' was null or empty.");
                return false;
            }
            if (text.Length > MAX_MESSAGE_LENGTH)
            {
                logger.Info($"Invalid chat message. Argument '{nameof(text)}' exceeds limit of {MAX_MESSAGE_LENGTH} characters. Value: \"{text.Substring(0, 25)}[..+{MAX_MESSAGE_LENGTH-25}]\"");
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
