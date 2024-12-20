using System.Net.Http;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System;

namespace GiacScraper.Features
{
    public class DropdownScraper
    {
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _allUrls = new();

        public DropdownScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CrawlDropdownsAsync(string baseUrl)
        {
            Console.WriteLine("Fetching initial page...");
            var rootHtml = await FetchHtmlAsync(baseUrl);
            var brandOptions = ExtractDropdownOptions(rootHtml, "//select[@id='Marche']");

            //foreach (var brand in brandOptions.Take(1))
            foreach (var brand in brandOptions)
            {
                var brandUrl = $"{baseUrl}?Marche={HttpUtility.UrlEncode(brand)}";
                Console.WriteLine($"-> Exploring brand: {brand}...");

                var brandHtml = await FetchHtmlAsync(brandUrl);
                var modelOptions = ExtractDropdownOptions(brandHtml, "//select[@id='Modello']");

                foreach (var model in modelOptions)
                {
                    var modelUrl = $"{brandUrl}&Modello={HttpUtility.UrlEncode(model.Replace("+", "_piu_"))}";
                    Console.WriteLine($"--> Exploring model: {model}...");

                    var modelHtml = await FetchHtmlAsync(modelUrl);
                    var yearOptions = ExtractDropdownOptions(modelHtml, "//select[@id='Anno']");

                    foreach (var year in yearOptions)
                    {
                        var finalUrl = $"{modelUrl}&Anno={HttpUtility.UrlEncode(year)}";
                        Console.WriteLine($"----> Exploring: {model} ({year})...");
                        _allUrls.Add(finalUrl);

                        // Fetch final page to discover additional links
                        var finalHtml = await FetchHtmlAsync(finalUrl);
                        var additionalLinks = ExtractPageLinks(finalHtml);
                        foreach (var link in additionalLinks)
                        {
                            _allUrls.Add(link);
                        }
                    }
                }
            }

            Console.WriteLine($"Scraping completed. Total URLs collected: {_allUrls.Count}");
        }

        private async Task<string> FetchHtmlAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR]: ");
                Console.ResetColor();
                Console.WriteLine(url);
                Console.WriteLine($" could not fetch {url}: {ex.Message}");
                return string.Empty;
            }
        }

        private List<string> ExtractDropdownOptions(string htmlContent, string xpath)
        {
            var options = new List<string>();
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var selectNode = htmlDoc.DocumentNode.SelectSingleNode(xpath);
                if (selectNode != null)
                {
                    foreach (var optionNode in selectNode.SelectNodes("option"))
                    {
                        var value = optionNode.InnerText.Trim();
                        if (!string.IsNullOrEmpty(value) && value != "Marca" && value != "Tutti" && value != "Anno")
                        {
                            options.Add(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR]: ");
                Console.ResetColor();
                Console.WriteLine($" could not extract dropdown options: {ex.Message}");
            }
            return options;
        }

        private List<string> ExtractPageLinks(string htmlContent)
        {
            var links = new List<string>();
            try
            {
                var regex = new Regex("href=\"(.*?)\"", RegexOptions.IgnoreCase);
                var matches = regex.Matches(htmlContent);
                foreach (Match match in matches)
                {
                    var link = match.Groups[1].Value;
                    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        links.Add(link);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR]: ");
                Console.ResetColor();
                Console.WriteLine($" could not extract dropdown options: {ex.Message}");
            }
            return links;
        }

        public IEnumerable<string> GetAllDropdownUrls()
        {
            return _allUrls;
        }
    }
}
