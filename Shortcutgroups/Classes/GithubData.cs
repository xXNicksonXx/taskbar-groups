using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;

namespace Shortcutgroups.Classes {
    class GithubData {
        private static string GetDataFromKey(string key) {
            string json = "";
            using (WebClient webClient = new WebClient()) {
                webClient.Headers.Add("User-Agent", "taskbar-groups");
                json = webClient.DownloadString("https://api.github.com/repos/tjackenpacken/taskbar-groups/releases");
            }
            JArray jArray = JArray.Parse(json);
            JObject jObject = jArray[0].ToObject<JObject>();

            return jObject[key].ToString();
        }

        public static string GetCurrentVersion() {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public static string GetLatestVersion() {
            string value = GetDataFromKey("tag_name");
            return value.Substring(1, value.Length - 1);
        }

        public static string GetLatestReleaseUrl() {
            return GetDataFromKey("html_url");
        }
    }
}
