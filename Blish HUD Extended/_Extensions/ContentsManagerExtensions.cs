using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using SpriteFontPlus;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public static class ContentsManagerExtensions
    {
        private static Logger Logger = Logger.GetLogger<ContentsManager>();

        /// <summary>
        /// Extracts a file from the module archive.
        /// </summary>
        /// <param name="contentsManager">The module's assigned <see cref="ContentsManager"/> object.</param>
        /// <param name="refFilePath">A file path relative to the ref folder inside the module archive.</param>
        /// <param name="outFilePath">Destination of the file.</param>
        /// <param name="overwrite">If any existing file at the destination should be overwritten.</param>
        public static async Task Extract(this ContentsManager contentsManager, string refFilePath, string outFilePath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(refFilePath))
            {
                throw new ArgumentException($"{nameof(refFilePath)} cannot be empty.", nameof(refFilePath));
            }

            if (string.IsNullOrEmpty(outFilePath))
            {
                throw new ArgumentException($"{nameof(outFilePath)} cannot be empty.", nameof(outFilePath));
            }

            if (!overwrite && File.Exists(outFilePath)) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outFilePath)!);

                using var stream = contentsManager.GetFileStream(refFilePath);
                if (stream == null)
                {
                    throw new FileNotFoundException($"File not found: '{refFilePath}'");
                }
                stream.Position = 0;
                using var file = File.Create(outFilePath);
                file.Position = 0;
                await stream.CopyToAsync(file);
            }
            catch (IOException e)
            {
                Logger.Warn(e, e.Message);
            }
        }

        /// <summary>
        /// Loads a <see cref="SpriteFont"/> from a TrueTypeFont (*.ttf) file.
        /// </summary>
        /// <param name="manager">Module's <see cref="ContentsManager"/>.</param>
        /// <param name="fontPath">The path to the TTF font file.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="textureSize">Size of the <see cref="SpriteFont.Texture"/>.<br/>A greater <c>fontSize</c> results in bigger glyphs which may require more texture space.</param>
        public static SpriteFont GetSpriteFont(this ContentsManager manager, string fontPath, int fontSize, int textureSize = 1392)
        {
            if (fontSize <= 0)
            {
                throw new ArgumentException("Font size must be greater than 0.", nameof(fontSize));
            }

            using var fontStream = manager.GetFileStream(fontPath);
            var fontData = new byte[fontStream.Length];
            var fontDataLength = fontStream.Read(fontData, 0, fontData.Length);

            if (fontDataLength > 0)
            {
                using var ctx = GameService.Graphics.LendGraphicsDeviceContext();
                var bakeResult = TtfFontBaker.Bake(fontData, fontSize, textureSize, textureSize, new[] {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA
                });
                return bakeResult.CreateSpriteFont(ctx.GraphicsDevice);
            }

            return null;
        }

        /// <summary>
        /// Loads a <see cref="BitmapFont"/> from a TrueTypeFont (*.ttf) file.
        /// </summary>
        /// <param name="manager">Module's <see cref="ContentsManager"/>.</param>
        /// <param name="fontPath">The path to the TTF font file.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="lineHeight">Sets the line height. By default, <see cref="SpriteFont.LineSpacing"/> will be used.</param>
        public static BitmapFontEx GetBitmapFont(this ContentsManager manager, string fontPath, int fontSize, int lineHeight = 0)
        {
            return manager.GetSpriteFont(fontPath, fontSize)?.ToBitmapFont(lineHeight);
        }
    }
}
