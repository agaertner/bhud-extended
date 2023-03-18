using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Extended
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Copies the pixel data from the source <see cref="Bitmap"/> onto the target <see cref="Texture2D"/>.
        /// </summary>
        /// <remarks>
        /// Existing pixel data of the target <see cref="Texture2D"/> will be overwritten with the copied pixel data.
        /// </remarks>
        /// <param name="source"><see cref="Bitmap"/> to copy the pixel data from.</param>
        /// <param name="destination"><see cref="Texture2D"/> to set the pixel data of.</param>
        public static void Copy(this Bitmap source, Texture2D destination)
        {
            // Lock the bitmap data
            BitmapData bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);

            // Calculate the size of the bitmap data in bytes
            int dataSize = bitmapData.Stride * bitmapData.Height;

            // Allocate a byte array to hold the bitmap data
            byte[] data = new byte[dataSize];

            // Copy the bitmap data into the byte array
            Marshal.Copy(bitmapData.Scan0, data, 0, dataSize);

            // Unlock the bitmap data
            source.UnlockBits(bitmapData);

            // Copy the byte array onto the texture
            destination.SetData(data);
        }
    }
}
