using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Extended {
    public static class XnaExtensions {

        /// <summary>
        /// Scales the given size so it fits inside the target size, preserving aspect ratio.
        /// Can scale up or down.
        /// </summary>
        public static Point ScaleTo(this Point size, Point target, bool keepAspect = true, bool enlarge = false) {
            if (size.X <= 0 || size.Y <= 0 || target.X <= 0 || target.Y <= 0)
                return Point.Zero;
            if (keepAspect) {
                float scale         = Math.Min((float)target.X / size.X, (float)target.Y / size.Y);
                if (!enlarge) scale = Math.Min(1f, scale);
                int width           = Math.Min((int)Math.Round(size.X * scale), target.X);
                int height          = Math.Min((int)Math.Round(size.Y * scale), target.Y);
                return new Point(width, height);
            } else {
                int width  = enlarge ? target.X : Math.Min(size.X, target.X);
                int height = enlarge ? target.Y : Math.Min(size.Y, target.Y);
                return new Point(width, height);
            }
        }

        /// <summary>
        /// Returns a rectangle centered within the target, using the given size.
        /// </summary>
        public static Rectangle CenterWithin(this Point size, Rectangle target) {
            if (size.X <= 0 || size.Y <= 0) return Rectangle.Empty;

            int x = target.X + (target.Width  - size.X) / 2;
            int y = target.Y + (target.Height - size.Y) / 2;

            return new Rectangle(x, y, size.X, size.Y);
        }

        /// <summary>
        /// Combines ScaleToFit and CenterWithin: scales the given size to fit inside the bounds,
        /// then centers it within the bounds.
        /// </summary>
        public static Rectangle GetCenteredFit(this Rectangle bounds, Point size, bool keepAspect = true, bool enlarge = false) {
            var scaled = size.ScaleTo(bounds.Size, keepAspect, enlarge);
            return scaled.CenterWithin(bounds);
        }
    }
}