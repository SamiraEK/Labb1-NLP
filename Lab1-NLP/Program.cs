using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace translate_text
{
    class Program
    {
        private static string translatorEndpoint = "https://api.cognitive.microsofttranslator.com";
        private static string cogSvcKey;
        private static string cogSvcRegion;

        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                cogSvcKey = configuration["CognitiveServiceKey"];
                cogSvcRegion = configuration["CognitiveServiceRegion"];
                var run = "yes";

                while (run == "yes")
                {
                    Console.WriteLine("***Welcome to my text translator!***\n" +
            
                                          "\n" +
                                          "sv = Swedish\n" +
                                          "fr = French\n" +
                                          "es = Spanish\n" +
                                          "\n" +
                                          "Choose a target language!");
                    var targetLanguage = Console.ReadLine();

                    Console.WriteLine("Enter text you want to translate:\n" +
                                       "\n" +
                                      "******************************** \n");

                    var textToTranslate = Console.ReadLine();

                    // Set console encoding to unicode
                    Console.InputEncoding = Encoding.Unicode;
                    Console.OutputEncoding = Encoding.Unicode;

                    string language = await GetLanguage(textToTranslate);
                    Console.WriteLine("Language: " + language);

                    string translatedText = await Translate(textToTranslate, language, targetLanguage);
                    Console.WriteLine($"\nTranslation:\n" +
                                      $"{translatedText}\n" +
                                      $"Language: {targetLanguage}");

                    Console.WriteLine($"\n" +
                                      $"\n" +
                                      $"Enter:\n" +
                                      $"yes = translate another text\n" +
                                      $"no = quit");
                    run = Console.ReadLine();
                    Console.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<string> GetLanguage(string text)
        {
            // Default language is English
            string language = "en";

            // Use the Translator detect function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = "/detect?api-version=3.0";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get language
                    JArray jsonResponse = JArray.Parse(responseContent);
                    language = (string)jsonResponse[0]["language"];
                }
            }


            // return the language
            return language;
        }

        static async Task<string> Translate(string text, string sourceLanguage, string targetLanguage)
        {
            string translation = "";

            // Use the Translator translate function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = $"/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get translation
                    JArray jsonResponse = JArray.Parse(responseContent);
                    translation = (string)jsonResponse[0]["translations"][0]["text"];
                }
            }


            // Return the translation
            return translation;

        }
    }
}

