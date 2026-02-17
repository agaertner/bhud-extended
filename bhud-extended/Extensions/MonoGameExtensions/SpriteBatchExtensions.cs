using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
namespace Blish_HUD.Extended
{
    public static class SpriteBatchExtensions
    {
        /// <summary>
        /// Draws a rectangular border on the specified control.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to render the rectangle border.</param>
        /// <param name="ctrl">The control on which the border will be drawn.</param>
        /// <param name="bounds">The bounds to draw the border on relative to the control's bounds.</param>
        /// <param name="lineWidth">The line width, in pixels, of the border.</param>
        /// <param name="color">The color used to draw the border.</param>
        public static void DrawBorderOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth, Color color)
        {
            if (lineWidth <= 0 || bounds.Width <= 0 || bounds.Height <= 0) return;

            lineWidth = Math.Min(lineWidth, Math.Min(bounds.Width / 2, bounds.Height / 2));

            var localBounds = bounds.ToBounds(ctrl.AbsoluteBounds);
            var col = color * ctrl.AbsoluteOpacity();

            // Top
            spriteBatch.Draw(ContentService.Textures.Pixel,
                new Rectangle(localBounds.X, localBounds.Y, localBounds.Width, lineWidth), col);

            // Bottom
            spriteBatch.Draw(ContentService.Textures.Pixel,
                new Rectangle(localBounds.X, localBounds.Bottom - lineWidth, localBounds.Width, lineWidth), col);

            int innerHeight = localBounds.Height - (lineWidth * 2);
            if (innerHeight > 0) {
                // Left
                spriteBatch.Draw(ContentService.Textures.Pixel,
                    new Rectangle(localBounds.X, localBounds.Y + lineWidth, lineWidth, innerHeight), col);

                // Right
                spriteBatch.Draw(ContentService.Textures.Pixel,
                    new Rectangle(localBounds.Right - lineWidth, localBounds.Y + lineWidth, lineWidth, innerHeight), col);
            }
        }

        /// <inheritdoc cref="DrawBorderOnCtrl(SpriteBatch, Control, Rectangle, int, Color)"/>
        public static void DrawBorderOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth = 2)
        {
            DrawBorderOnCtrl(spriteBatch, ctrl, bounds, lineWidth, Color.Black);
        }
    }
}
