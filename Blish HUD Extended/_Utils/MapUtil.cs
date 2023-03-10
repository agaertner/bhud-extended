namespace Blish_HUD.Extended
{
    public class MapUtil
    {
        /// <summary>
        /// Gets a hash of the map's continent rectangle which can be used to identify copies of the same map.
        /// </summary>
        /// <param name="continentId">The continent id of the map.</param
        /// <param name="topLeftX">The x-coordinate of the top-left corner of the map's bounding rectangle.</param>
        /// <param name="topLeftY">The y-coordinate of the top-left corner of the map's bounding rectangle.</param>
        /// <param name="bottomRightX">The x-coordinate of the bottom-right corner of the map's bounding rectangle.</param>
        /// <param name="bottomRightY">The y-coordinate of the bottom-right corner of the map's bounding rectangle.</param>
        /// <returns>
        /// The first 8 characters of the SHA1 hash of the following string<br/>
        /// <c>SHA1(&lt;continent_id&gt;&lt;continent_rect[0][0]&gt;&lt;continent_rect[0][1]&gt;&lt;continent_rect[1][0]&gt;&lt;continent_rect[1][1]&gt;)</c>
        /// </returns>
        public static string GetSHA1(int continentId, int topLeftX, int topLeftY, int bottomRightX, int bottomRightY)
        {
            var rpcHash = $"{continentId}{topLeftX}{topLeftY}{bottomRightX}{bottomRightY}";
            return rpcHash.ToSHA1Hash().Substring(0, 8);
        }
    }
}
