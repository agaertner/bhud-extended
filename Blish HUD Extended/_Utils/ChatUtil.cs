using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public static class ChatUtil
    {
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
        public static async Task Send(string text, KeyBinding messageKey)
        {
            var prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text) || !Focus(messageKey)) return;
            KeyboardUtil.Press(162, true); // LControl
            KeyboardUtil.Stroke(65, true); // A
            Thread.Sleep(2);
            KeyboardUtil.Release(162, true); // LControl
            KeyboardUtil.Stroke(46, true); // Del
            KeyboardUtil.Press(162, true); // LControl
            KeyboardUtil.Stroke(86, true); // V
            Thread.Sleep(2);
            KeyboardUtil.Release(162, true); // LControl
            Thread.Sleep(2);
            KeyboardUtil.Stroke(13); // Enter
            if (prevClipboardContent == null) return;
            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
        }

        /// <summary>
        /// Inserts or appends text without sending it.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        /// <param name="messageKey">The key which is used to open the message box.</param>
        public static async Task Insert(string text, KeyBinding messageKey)
        {
            var prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text) || !Focus(messageKey)) return;
            KeyboardUtil.Press(162, true); // LControl
            KeyboardUtil.Stroke(86, true); // V
            Thread.Sleep(2);
            KeyboardUtil.Release(162, true); // LControl
            if (prevClipboardContent == null) return;
            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
        }

        private static bool Focus(KeyBinding messageKey)
        {
            if (!GameService.Gw2Mumble.IsAvailable || GameService.Gw2Mumble.UI.IsTextInputFocused) return false;

            // Tell the game to release the shift keys so chat can be opened.
            KeyboardUtil.Release(160);
            KeyboardUtil.Release(161);

            var hasModifierKey = ModifierLookUp.TryGetValue(messageKey.ModifierKeys, out var modifierKey);
            if (hasModifierKey)
            {
                KeyboardUtil.Press(modifierKey, true);
            }
            if (messageKey.PrimaryKey != Keys.None)
            {
                KeyboardUtil.Stroke((int)messageKey.PrimaryKey, true);
            }
            if (hasModifierKey)
            {
                KeyboardUtil.Release(modifierKey, true);
            }
            return true;
        }
    }
}
