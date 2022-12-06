using Gw2Sharp.WebApi;

namespace Blish_HUD.Extended
{
    public static class LocaleExtensions
    {
        /// <summary>
        /// Returns the ISO 639-1 Language Code for the given locale.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://wikipedia.org/wiki/List_of_ISO_639-1_codes"/>
        /// </remarks>
        /// <param name="locale">Locale to return the language code of.</param>
        /// <returns>A new string representing the language code.</returns>
        public static string Code(this Locale locale)
        {
            return locale switch
            {
                Locale.English => "en",
                Locale.Spanish => "es",
                Locale.German => "de",
                Locale.French => "fr",
                Locale.Korean => "kr",
                Locale.Chinese => "zh",
                _ => "en"
            };
        }
    }
}
