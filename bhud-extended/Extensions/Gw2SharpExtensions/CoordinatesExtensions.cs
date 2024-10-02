using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Rectangle = Gw2Sharp.WebApi.V2.Models.Rectangle;
// ReSharper disable InconsistentNaming

namespace Blish_HUD.Extended
{
    public static class CoordinatesExtensions
    {
        private const float INCH_TO_METER = 0.0254F;
        
        public static Coordinates3 SwapYZ(this Coordinates3 coords)
        {
            return new Coordinates3(coords.X, coords.Z, coords.Y);
        }

        public static Coordinates2 ToPlane(this Coordinates3 coords)
        {
            return new Coordinates2(coords.X, coords.Y);
        }

        public static Coordinates3 ToUnit(this Coordinates3 coords, CoordsUnit fromUnit, CoordsUnit toUnit)
        {
            if (fromUnit == CoordsUnit.Meters && toUnit == CoordsUnit.Inches)
                return new Coordinates3(coords.X / INCH_TO_METER, coords.Y / INCH_TO_METER, coords.Z / INCH_TO_METER);
            else if (fromUnit == CoordsUnit.Inches && toUnit == CoordsUnit.Meters)
                return new Coordinates3(coords.X * INCH_TO_METER, coords.Y * INCH_TO_METER, coords.Z * INCH_TO_METER);
            return coords;
        }

        public static Coordinates3 ToMapCoords(this Coordinates3 coords, CoordsUnit fromUnit)
        {
            coords = coords.ToUnit(fromUnit, CoordsUnit.GameWorld);
            return new Coordinates3(coords.X, coords.Y, coords.Z);
        }

        public static Coordinates3 ToContinentCoords(this Coordinates3 coords, CoordsUnit fromUnit, Rectangle mapRectangle, Rectangle continentRectangle)
        {
            var mapCoords = coords.ToMapCoords(fromUnit);
            double x = ((mapCoords.X - mapRectangle.TopLeft.X) / mapRectangle.Width * continentRectangle.Width) + continentRectangle.TopLeft.X;
            double z = ((1 - ((mapCoords.Z - mapRectangle.BottomRight.Y) / mapRectangle.Height)) * continentRectangle.Height) + continentRectangle.TopRight.Y;
            return new Coordinates3(x, mapCoords.Y, z);
        }

        public static Vector3 ToXnaVector3(this Coordinates3 coords)
        {
            return new Vector3((float)coords.X, (float)coords.Y, (float)coords.Z);
        }

        public static bool Inside(this Coordinates2 targetPoint, IReadOnlyList<Coordinates2> polygon)
        {
            if (polygon.Count < 3)
            {
                // Must have at least 3 vertices to be valid.
                return false;
            }
            double x = targetPoint.X;
            double y = targetPoint.Y;
            bool isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > y) != (polygon[j].Y > y) &&
                    x < (polygon[j].X - polygon[i].X) * (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }

    public enum CoordsUnit
    {
        Inches,
        GameWorld = Inches,

        Meters,
        Mumble = Meters
    }
}
