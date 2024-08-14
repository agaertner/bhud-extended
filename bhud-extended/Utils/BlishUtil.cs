using System.Linq;
using System.Reflection;

namespace Blish_HUD.Extended
{
    public class BlishUtil {
        /// <summary>
        /// Gets the Blish HUD version.
        /// </summary>
        /// <returns>Blish HUD's assembly version.</returns>
        public static string GetVersion() {
            var version = typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var release = string.IsNullOrEmpty(version) ? string.Empty : $"Blish HUD v{version.Split('+').First()}";
            return release;
        }
    }
}
