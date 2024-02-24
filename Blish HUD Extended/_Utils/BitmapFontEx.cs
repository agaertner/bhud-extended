using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Extended
{
    public class BitmapFontEx : BitmapFont, IDisposable
    {
        private readonly Texture2D _texture;

        public BitmapFontEx(string name, IEnumerable<BitmapFontRegion> regions, int lineHeight, Texture2D texture) : base(name, regions, lineHeight)
        {
            _texture = texture;
        }

        public BitmapFontEx(string name, IReadOnlyList<BitmapFontRegion> regions, int lineHeight) : base(name, regions, lineHeight)
        {
            _texture = regions[0].TextureRegion.Texture;
        }

        public void Dispose()
        {
            _texture?.Dispose();
        }
    }
}
