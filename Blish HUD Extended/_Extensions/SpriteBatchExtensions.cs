using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Extended
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth, Color color)
        {
            // Top-Left to Top-Right
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, lineWidth), color);

            // Top-Left to Bottom-Left
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y, lineWidth, bounds.Height), color);

            // Bottom-Left to Bottom-Right
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y + bounds.Height - lineWidth, bounds.Width, lineWidth), color);

            // Top-Right to Bottom-Right
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X + bounds.Width - lineWidth, bounds.Y, lineWidth, bounds.Height), color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth)
        {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, lineWidth, Color.Black);
        }
    }
}
