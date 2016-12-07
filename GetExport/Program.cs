
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace DataExport
{
    public class Config
    {
        public string server { get; set; }
        public string apiLogin { get; set; }
        public string apiPassword { get; set; }
        public string fmt { get; set; }
        public string template { get; set; }
        public string version { get; set; }
        public string downloadTo { get; set; }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                System.Console.WriteLine("GetExport - Utility for downloading data from SurveySolutions");
                System.Console.WriteLine("Requires a SurveySolutions config file (json) in the current directory");
                System.Console.WriteLine("And a single command line parameter - the survey name - that must be in the config file under 'surveys'");
                System.Console.WriteLine("Output is zipfile piped to stdout - use redirection to save as file.");
                System.Console.WriteLine("e.g.      GetExport TA > TeachersAssessments.zip");
                return;
            }
            string survey = args[0];
            string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SurveySolutions.config"), Encoding.UTF8);

            dynamic jsonConfig = JObject.Parse(json);
            Config config = new Config();
            try
            {

                config.template = jsonConfig.surveys[survey].template;
                config.version = jsonConfig.surveys[survey].version;

                config.server = jsonConfig.server;
                config.apiLogin = jsonConfig.apiLogin;
                config.apiPassword = jsonConfig.apiPassword;
                config.fmt = jsonConfig.fmt;
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Survey Name or Config File");
                return;
            }

            ApiDemo(config);
        }

        private static void ApiDemo(Config config)
        {

            var qid = String.Format("{0}${1}", config.template, config.version);
            var baseUrl = SurveySolutionsApi.GetBaseUrl(config.server, config.fmt, qid);

            var result = SurveySolutionsApi.GetDetails(baseUrl, config.apiLogin, config.apiPassword);
            //Console.WriteLine(result);

            SurveySolutionsApi.RefreshExport(baseUrl, config.apiLogin, config.apiPassword);
            Thread.Sleep(2000); // or wait for the status to be ready

            result = SurveySolutionsApi.GetDetails(baseUrl, config.apiLogin, config.apiPassword);
            //Console.WriteLine(result);

            SurveySolutionsApi.DownloadFile(baseUrl, config.apiLogin, config.apiPassword);
        }
    }

    // API SYNTAX:
    // https://demo.mysurvey.solutions/api/v1/export/format/guid$version/details    (GET)
    // https://demo.mysurvey.solutions/api/v1/export/format/guid$version/start      (POST)
    // https://demo.mysurvey.solutions/api/v1/export/format/guid$version/           (GET)

    public class SurveySolutionsApi
    {
        public static string GetBaseUrl(string server, string fmt, string qid)
        {
            var baseUrl = String.Format("{0}{1}/{2}/", server, fmt, qid);
            return baseUrl;
        }

        private static string GetCredentials64(string username, string password)
        {
            var creds = String.Format("{0}:{1}", username, password);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(creds));
        }

        public static HttpClient GetClient(string username, string password)
        {
            var authValue = new AuthenticationHeaderValue("Basic", GetCredentials64(username, password));
            var client = new HttpClient()
            {
                DefaultRequestHeaders = { Authorization = authValue }
            };
            return client;
        }

        public static void RefreshExport(string url, string apiLogin, string apiPassword)
        {
            var cmd = "start/";
            using (var httpClient = GetClient(apiLogin, apiPassword))
            {
                httpClient.BaseAddress = new Uri(String.Format("{0}{1}", url, cmd));

                using (var content = new StringContent(String.Empty, Encoding.Default, "application/json"))
                {
                    using (var response = httpClient.PostAsync(String.Empty, content).Result)
                    {
                        var responseData = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
        }

        public static string GetDetails(string url, string apiLogin, string apiPassword)
        {
            var cmd = "details/";
            using (var httpClient = GetClient(apiLogin, apiPassword))
            {
                using (var response = httpClient.GetStringAsync(String.Format("{0}{1}", url, cmd)))
                {
                    response.Wait();
                    return response.Result;
                }
            }
        }

        public static void DownloadFile(string url, string apiLogin, string apiPassword)
        {
            using (var httpClient = GetClient(apiLogin, apiPassword))
            {
                using (var response = httpClient.GetByteArrayAsync(url))
                {
                    response.Wait();
                    using (Stream myOutStream = Console.OpenStandardOutput())
                    {
                        myOutStream.Write(response.Result, 0, response.Result.Length);
                    }
                }
            }
        }
    }
}
