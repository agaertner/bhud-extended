using Gw2Sharp.WebApi;
using System.Linq;

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

        /// <summary>
        /// Checks if the given locale is supported and returns the default supported locale if not.
        /// </summary>
        /// <param name="locale">Locale to verify.</param>
        /// <param name="supported">Custom list of supported languages. The first entry is the default fallback.</param>
        /// <returns>Given locale or default fallback locale.</returns>
        /// <remarks>English, Spanish, German, French are the default supported locales.</remarks>
        public static Locale SupportedOrDefault(this Locale locale, params Locale[] supported) {
            if (supported?.Any() ?? true) {
                return locale switch
                {
                    Locale.Spanish => locale,
                    Locale.German => locale,
                    Locale.French => locale,
                    _ => Locale.English
                };
            }
            return supported.Contains(locale) ? locale : supported.FirstOrDefault();
        }
    }
}
