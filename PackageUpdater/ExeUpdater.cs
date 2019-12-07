using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PackageUpdater
{
    internal static class ExeUpdater
    {
        private static readonly string clientId = "8de192eff6ded1510bd0";
        private static readonly string clientSecret = Secret.GithubSecret;
        private static readonly string gitUser = "PawKanarek";
        private static readonly string gitRepo = "PackageUpdater";
        private static readonly string newVersion = "newVersion.zip";
        private static readonly GitHubClient client = new GitHubClient(new ProductHeaderValue(gitRepo));

        public static async Task<bool> UpdateIfAvailable()
        {
            var token = await OAuthAsync();
            var credentials = new Credentials(token);
            client.Credentials = credentials;

            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(gitUser, gitRepo);

            var versions = new List<(Version version, Release release)>();
            foreach (Release item in releases)
            {
                try
                {
                    versions.Add((new Version(item.TagName), item));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldnt parse version of release {item} because {ex.Message}");
                }
            }
            versions.Sort();

            (Version highestVersion, Release highestRelease) = versions.LastOrDefault();
            Version currentVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            if (highestVersion == null || currentVersion >= highestVersion)
            {
                Console.WriteLine($"Current version is up to date");
                return false;
            }

            var response = await client.Connection.Get<object>(new Uri(highestRelease.Assets[0].BrowserDownloadUrl), new Dictionary<string, string>(), "application/json");
            var currentDir = Directory.GetCurrentDirectory();

            using (var zipstream = new MemoryStream((byte[])response.Body))
            using (var archive = new ZipArchive(zipstream))
            {
                archive.ExtractToDirectory(currentDir, true);
            }

            PrintLimtis();
            return true;
        }

        private static void PrintLimtis()
        {
            ApiInfo apiInfo = client.GetLastApiInfo();
            RateLimit rateLimit = apiInfo?.RateLimit;
            var howManyRequestsCanIMakePerHour = rateLimit?.Limit;
            var howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
            DateTimeOffset? whenDoesTheLimitReset = rateLimit?.Reset; // UTC time
            Console.WriteLine($"How Many Requests Do I Have Left: {howManyRequestsDoIHaveLeft}\nWhen Does The Limit Reset: {whenDoesTheLimitReset}\nHow Many Requests Can I Make Per Hour: {howManyRequestsCanIMakePerHour}");
        }

        /// <summary>
        /// First go to url under oAuthLoginUrl and copy returned code from browser into last parameter OAuthToken reuqest
        /// then create new access token
        /// that api was created for web application not console so its bit hacky, but im using it to obtain 5000 requests instead of 50
        /// </summary>
        /// <returns>Token</returns>
        private static async Task<string> OAuthAsync()
        {
            //var request = new OauthLoginRequest(clientId)
            //{
            //    Scopes = { "user", "notifications", "repos" },
            //};
            //var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);

            //var requestToken = new OauthTokenRequest(clientId, clientSecret, code: "ae119a602f0fa787ab1a");
            //var token = await client.Oauth.CreateAccessToken(requestToken);
            //return token.AccessToken;
            return await Task.FromResult("53d8f5e89fc50dd89f05d517e50c65a4ef7a1372");// dunno how long this token will last 
        }
    }
}
