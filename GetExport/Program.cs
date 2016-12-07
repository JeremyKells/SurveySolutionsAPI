using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace DataExport
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApiDemo();
        }

        private static void ApiDemo()
        {
            //---------------------------------------------------------------------
            var server = "https://demo.mysurvey.solutions/api/v1/export/";   // substitute your server address here
            var apiLogin = "Expapi";                                         // substitute your API login here
            var apiPassword = "ExpApi2000";                                  // substitute your API password here
            var fmt = "stata"; // tabular spss stata binary paradata         // substitute your desirable file format here
            var template = "74e2d30854914e24af6beae9be64130c";               // substitute your questionnaire id here
            var version = "1";                                               // substitute your questionnaire version here
            //---------------------------------------------------------------------
            var qid = String.Format("{0}${1}", template, version);
            var baseUrl = SurveySolutionsApi.GetBaseUrl(server, fmt, qid);
            
            var result = SurveySolutionsApi.GetDetails(baseUrl, apiLogin, apiPassword);
            Console.WriteLine(result);

            SurveySolutionsApi.RefreshExport(baseUrl, apiLogin, apiPassword);
            Thread.Sleep(2000); // or wait for the status to be ready

            result = SurveySolutionsApi.GetDetails(baseUrl, apiLogin, apiPassword);
            Console.WriteLine(result);

            SurveySolutionsApi.DownloadFile(@"C:\temp\mydownload.zip", baseUrl, apiLogin, apiPassword);
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

        public static void DownloadFile(string filename, string url, string apiLogin, string apiPassword)
        {
            using (var httpClient = GetClient(apiLogin, apiPassword))
            {
                using (var response = httpClient.GetByteArrayAsync(url))
                {
                    response.Wait();
                    File.WriteAllBytes(filename, response.Result);
                }
            }
        }

    }
}
