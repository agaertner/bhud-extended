using Gw2Sharp.WebApi.V2.Models;
using Color = Microsoft.Xna.Framework.Color;

namespace Blish_HUD.Extended
{
    public static class ItemExtensions
    {
        /// <summary>
        /// Returns a color representing the item rarity.
        /// </summary>
        /// <param name="rarity">The item rarity to get the color of.</param>
        /// <returns>A RGBA color (XNA) representing the item rarity.</returns>
        public static Color AsColor(this ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Unknown => Color.White,
                ItemRarity.Junk => new Color(170, 170, 170),
                ItemRarity.Basic => Color.White,
                ItemRarity.Fine => new Color(98, 164, 218),
                ItemRarity.Masterwork => new Color(26, 147, 6),
                ItemRarity.Rare => new Color(252, 208, 11),
                ItemRarity.Exotic => new Color(255, 164, 5),
                ItemRarity.Ascended => new Color(251, 62, 141),
                ItemRarity.Legendary => new Color(86, 29, 167),
                _ => Color.White
            };
        }
    }
}
