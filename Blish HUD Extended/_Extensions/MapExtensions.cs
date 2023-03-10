using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Extended
{
    public static class MapExtensions
    {
        /// <summary>
        /// Gets a hash of the map's continent rectangle which can be used to identify copies of the same map.
        /// </summary>
        /// <param name="map">The map to get the hash of.</param>
        /// <returns>
        /// The first 8 characters of the SHA1 hash of the following string<br/>
        /// <c>SHA1(&lt;continent_id&gt;&lt;continent_rect[0][0]&gt;&lt;continent_rect[0][1]&gt;&lt;continent_rect[1][0]&gt;&lt;continent_rect[1][1]&gt;)</c>
        /// </returns>
        public static string GetSHA1(int continentId, Rectangle continentRect)
        {
            var rpcHash = $"{continentId}{continentRect.TopLeft.X}{continentRect.TopLeft.Y}{continentRect.BottomRight.X}{continentRect.BottomRight.Y}";
            return rpcHash.ToSHA1Hash().Substring(0, 8);
        }
    }
}
