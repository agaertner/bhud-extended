using System;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Blish_HUD.Extended
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth, Color color)
        {
          if (lineWidth <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
          {
            return;
          }

          // Clamp lineWidth so it doesn't exceed half the rect size
          lineWidth = Math.Min(lineWidth, Math.Min(bounds.Width / 2, bounds.Height / 2));

          // Top
          spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
            new Rectangle(bounds.X, bounds.Y, bounds.Width, lineWidth), color);

          // Bottom
          spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
            new Rectangle(bounds.X, bounds.Bottom - lineWidth, bounds.Width, lineWidth), color);

          // Left
          spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
            new Rectangle(bounds.X, bounds.Y + lineWidth, lineWidth, bounds.Height - (lineWidth * 2)), color);

          // Right
          spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
            new Rectangle(bounds.Right - lineWidth, bounds.Y + lineWidth, lineWidth, bounds.Height - (lineWidth * 2)), color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth)
        {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, lineWidth, Color.Black);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, Color color) {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, Math.Min(bounds.Width, bounds.Height) / 2, color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds) {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, Color.Black);
        }
    }
}
