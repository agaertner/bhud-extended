using Blish_HUD.Controls;
using Blish_HUD.Extended.Properties;
using Blish_HUD.Modules.Managers;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Blish_HUD.Extended
{
    public static class Gw2ApiManagerExtensions
    {
        /// <summary>
        /// Checks if the GW2 API is available. Eg. no outage and not disabled.
        /// </summary>
        /// <param name="gw2ApiManager">The <see cref="Gw2ApiManager"/> associated with the module.</param>
        /// <param name="showMessage">Whether to show a localized screen notification to the user.</param>
        /// <returns><see langword="True"/> if the GW2 API is available; otherwise <see langword="false"/>.</returns>
        public static bool IsApiAvailable(this Gw2ApiManager gw2ApiManager, bool showMessage = false) {
            var l_err = string.Empty;
            try {
                // Get a response from the base api url and infer a rough global status.
                using var response = "https://api.guildwars2.com/".AllowHttpStatus(HttpStatusCode.ServiceUnavailable)
                    .AllowHttpStatus(HttpStatusCode.InternalServerError)
                    .GetAsync(default, HttpCompletionOption.ResponseHeadersRead).Result;

                // API is broken.
                if (response.StatusCode == HttpStatusCode.InternalServerError) {
                    l_err = $"{Resources.API_is_down_} {Resources.Please__try_again_later_}";
                } else if (response.StatusCode == HttpStatusCode.ServiceUnavailable) { // API is down for maintenance. Chances are high body contains a message.
                    var body = response.Content.ReadAsStringAsync().Result;
                    var header = body.GetTextBetweenTags("h1").Trim(); // Eg. "<h1>API Temporarily disabled</h1><p>Scheduled reactivation: 23 August.</p>"
                    var paragraph = (body.GetTextBetweenTags("p").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Reverse().FirstOrDefault() ?? string.Empty).Trim();
                    l_err = $"{header}. {paragraph}.";
                }

            } catch (Exception e) {
                l_err = "Failed to check API status.";
                Logger.GetLogger<Gw2ApiManager>().Info(e, l_err);
                showMessage = false;
            }

            bool isDown = !string.IsNullOrEmpty(l_err);
            if (showMessage && isDown) {
                ScreenNotification.ShowNotification(l_err, ScreenNotification.NotificationType.Error);
            }
            // If we don't immediately get a 503 then the API is probably available.
            return !isDown;
        }

        /// <summary>
        /// Calls <see cref="IsApiAvailable"/> and checks if <see cref="Gw2ApiManager"/> is equipped with an authorized subkey.
        /// </summary>
        /// <param name="gw2ApiManager">The <see cref="Gw2ApiManager"/> associated with the module.</param>
        /// <param name="showMessage">Whether to show a localized screen notification to the user.</param>
        /// <param name="requiredPermissions">Required permissions to check against available permissions. Leave empty if none required.</param>
        /// <returns><see langword="True"/> if <see cref="IsApiAvailable"/> returns <see langword="true"/> <b>AND</b> a subkey covering the required permissions is available; otherwise <see langword="false"/>.</returns>
        public static bool IsAuthorized(this Gw2ApiManager gw2ApiManager, bool showMessage = false, params TokenPermission[] requiredPermissions)
        {
            if (IsApiAvailable(gw2ApiManager, showMessage)) {
                return false;
            }

            if (string.IsNullOrWhiteSpace(GameService.Gw2Mumble.PlayerCharacter.Name))
            {
                if (showMessage) {
                    ScreenNotification.ShowNotification($"{Resources.API_unavailable_} {Resources.Please__login_to_a_character_}", ScreenNotification.NotificationType.Error);
                }
                Logger.GetLogger<Gw2ApiManager>().Info("API unavailable: No character logged in. - No key can be selected because a character has to be logged in once.");
                return false;
            }

            // Check if the scope Account exists; otherwise a subtoken has not been generated yet since BlishHUD enforces the Account scope.
            if (!gw2ApiManager.HasPermission(TokenPermission.Account)) // Subtokens created without the essential Account scope are unusable.
            {
                if (showMessage) {
                    ScreenNotification.ShowNotification($"{Resources.Missing_API_key_} {string.Format(Resources.Please__add_an_API_key_to__0__, "Blish HUD")}", ScreenNotification.NotificationType.Error);
                }
                Logger.GetLogger<Gw2ApiManager>().Info("Missing API key: Foreign account. - No key associated with the logged in account was found.");
                return false;
            }

            if (!gw2ApiManager.HasPermissions(requiredPermissions)) {
                var missing = string.Join(", ", requiredPermissions.Except(Enum.GetValues(typeof(TokenPermission)).Cast<TokenPermission>().Where(gw2ApiManager.HasPermission)));
                if (showMessage) {
                    ScreenNotification.ShowNotification($"{Resources.Insufficient_API_permissions_}\n{string.Format(Resources.Required___0_, missing)}", ScreenNotification.NotificationType.Error);
                }
                Logger.GetLogger<Gw2ApiManager>().Info($"Insufficient API permissions. Required: {missing}");
                return false;
            }
            return true;
        }
    }
}
