using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Extended
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth, Color color)
        {
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y - lineWidth, bounds.Width + lineWidth, lineWidth), color);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y, lineWidth, bounds.Height + lineWidth), color);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width + lineWidth, lineWidth), color);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X + bounds.Width, bounds.Y - lineWidth, lineWidth, bounds.Height + lineWidth), color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth)
        {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, lineWidth, Color.Black);
        }
    }
}
