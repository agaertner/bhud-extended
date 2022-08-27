using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;

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
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) return;
            Focus(messageKey);
            KeyboardUtil.Press(162, true);
            KeyboardUtil.Stroke(65, true);
            Thread.Sleep(1);
            KeyboardUtil.Release(162, true);
            KeyboardUtil.Stroke(46, true);
            KeyboardUtil.Press(162, true);
            KeyboardUtil.Stroke(86, true);
            Thread.Sleep(1);
            KeyboardUtil.Release(162, true);
            KeyboardUtil.Stroke(13);
            UnFocus();
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
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) return;
            Focus(messageKey);
            KeyboardUtil.Press(162, true);
            KeyboardUtil.Stroke(86, true);
            Thread.Sleep(1);
            KeyboardUtil.Release(162, true);
            if (prevClipboardContent == null) return;
            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
        }

        private static void Focus(KeyBinding messageKey)
        {
            UnFocus();

            // Tell the game to release the shift keys so chat can be opened.
            KeyboardUtil.Release(160);
            KeyboardUtil.Release(161);

            if (messageKey.ModifierKeys != ModifierKeys.None)
            {
                KeyboardUtil.Press(ModifierLookUp[messageKey.ModifierKeys]);
            }
            if (messageKey.PrimaryKey != Keys.None)
            {
                KeyboardUtil.Press((int)messageKey.PrimaryKey);
                KeyboardUtil.Release((int)messageKey.PrimaryKey);
            }
            if (messageKey.ModifierKeys != ModifierKeys.None)
            {
                KeyboardUtil.Release(ModifierLookUp[messageKey.ModifierKeys]);
            }
        }

        private static void UnFocus()
        {
            MouseUtil.Click(MouseUtil.MouseButton.LEFT, MouseUtil.GetPosition().X - 1);
        }
    }
}
