using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;

namespace Blish_HUD.Extended
{
    internal static class VectorExtensions
    {
        public static Vector3 ToScreenSpace(this Vector3 position, Matrix view, Matrix projection)
        {
            int screenWidth = GameService.Graphics.SpriteScreen.Width;
            int screenHeight = GameService.Graphics.SpriteScreen.Height;

            position = Vector3.Transform(position, view);
            position = Vector3.Transform(position, projection);

            float x = position.X / position.Z;
            float y = position.Y / -position.Z;

            x = (x + 1) * screenWidth / 2;
            y = (y + 1) * screenHeight / 2;

            return new Vector3(x, y, position.Z);
        }

        public static Vector2 Flatten(this Vector3 v)
        {
            return new Vector2((v.X / v.Z + 1f) / 2f * (float)GameService.Graphics.SpriteScreen.Width, (1f - v.Y / v.Z) / 2f * (float)GameService.Graphics.SpriteScreen.Height);
        }

        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).Length();
        }

        public static double Angle(this Vector3 v, Vector3 u)
        {
            return Math.Acos(Vector3.Dot(v, u) / (v.Length() * u.Length()));
        }

        public static Vector2 XY(this Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }

        #region Conversion
        /// <summary>
        /// Converts a <see cref="System.Numerics.Vector2"/> to a <see cref="Microsoft.Xna.Framework.Vector2"/>.
        /// </summary>
        public static Vector2 ToXna(this System.Numerics.Vector2 coords)
        {
            return new Vector2(coords.X, coords.Y);
        }
        /// <summary>
        /// Converts a <see cref="Microsoft.Xna.Framework.Vector2"/> to a <see cref="System.Numerics.Vector2"/>.
        /// </summary>
        public static System.Numerics.Vector2 ToSystem(this Vector2 coords)
        {
            return new System.Numerics.Vector2(coords.X, coords.Y);
        }
        /// <summary>
        /// Converts a <see cref="System.Numerics.Vector3"/> to a <see cref="Microsoft.Xna.Framework.Vector3"/>.
        /// </summary>
        public static Vector3 ToXna(this System.Numerics.Vector3 coords)
        {
            return new Vector3(coords.X, coords.Y, coords.Z);
        }
        /// <summary>
        /// Converts a <see cref="Microsoft.Xna.Framework.Vector3"/> to a <see cref="System.Numerics.Vector3"/>.
        /// </summary>
        public static System.Numerics.Vector3 ToSystem(this Vector3 coords)
        {
            return new System.Numerics.Vector3(coords.X, coords.Y, coords.Z);
        }
        #endregion
    }

    public static class CoordinatesExtensions
    {
        public static Vector2 ToVector(this Coordinates2 coords)
        {
            return new Vector2((float)coords.X, (float)coords.Y);
        }
    }
}
