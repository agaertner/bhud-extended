using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
namespace Blish_HUD.Extended
{
    public static class EmbeddedResourceLoader {
        /// <summary>
        /// Loads a Texture2D from an embedded resource by file name.
        /// </summary>
        public static Texture2D LoadTexture(string fileName)
        {
            var assembly     = typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                throw new InvalidOperationException($"Embedded resource '{fileName}' not found.");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Failed to open stream for '{fileName}'.");

            using var gdx = GameService.Graphics.LendGraphicsDeviceContext();
            return Texture2D.FromStream(gdx.GraphicsDevice, stream);
        }

        /// <summary>
        /// Loads a SoundEffect from an embedded resource by file name.
        /// </summary>
        public static SoundEffect LoadSound(string fileName)
        {
            var assembly     = typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                throw new InvalidOperationException($"Embedded resource '{fileName}' not found.");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Failed to open stream for '{fileName}'.");

            return SoundEffect.FromStream(stream);
        }
    }
}