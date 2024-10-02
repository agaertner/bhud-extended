using Blish_HUD.Gw2Mumble;
using Microsoft.Xna.Framework;
using System;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Blish_HUD.Extended
{
    public static class Gw2MumbleServiceExtensions {
        public static Vector3 Position(this PlayerCharacter playerCharacter, bool swapYZ)
        {
            return swapYZ ? playerCharacter.Position.SwapYZ() : playerCharacter.Position;
        }

        public static Vector3 Position(this PlayerCamera playerCamera, bool swapYZ)
        {
            return swapYZ ? playerCamera.Position.SwapYZ() : playerCamera.Position;
        }

        public static Vector3 Forward(this PlayerCharacter playerCharacter, bool swapYZ)
        {
            return swapYZ ? playerCharacter.Forward.SwapYZ() : playerCharacter.Forward;
        }

        public static Vector3 Forward(this PlayerCamera playerCamera, bool swapYZ)
        {
            return swapYZ ? playerCamera.Forward.SwapYZ() : playerCamera.Forward;
        }

        private const int MAPWIDTH_MAX = 362;
        private const int MAPHEIGHT_MAX = 338;
        private const int MAPWIDTH_MIN = 170;
        private const int MAPHEIGHT_MIN = 170;
        private const int MAPOFFSET_MIN = 19;

        private static int GetOffset(float curr, float max, float min, float val)
        {
            return (int)Math.Round((curr - min) / (max - min) * (val - MAPOFFSET_MIN) + MAPOFFSET_MIN, 0);
        }

        public static Rectangle CompassBounds(this UI ui)
        {
            int offsetWidth = GetOffset(ui.CompassSize.Width, MAPWIDTH_MAX, MAPWIDTH_MIN, 40);
            int offsetHeight = GetOffset(ui.CompassSize.Height, MAPHEIGHT_MAX, MAPHEIGHT_MIN, 40);
            int width = ui.CompassSize.Width + offsetWidth;
            int height = ui.CompassSize.Height + offsetHeight;
            int x = GameService.Graphics.SpriteScreen.ContentRegion.Width - width;
            int y = 0;
            if (!ui.IsCompassTopRight) {
                y += GameService.Graphics.SpriteScreen.ContentRegion.Height - height - 40;
            }
            return new Rectangle(x, y, width, height);
        }

        private static Vector3 SwapYZ(this Vector3 vec)
        {
            return new Vector3(vec.X, vec.Z, vec.Y);
        }
    }
}
