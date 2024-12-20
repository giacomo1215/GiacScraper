using GiacScraper.Services;
using GiacScraper.Features;
using GiacScraper.Utilities;
using System;

class Program
{ 
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to GiacScraper!");

        Console.Write("Enter the root URL to start scraping: ");
        string rootUrl = Console.ReadLine();

        Console.Write("Enter keywords to ignore (comma-separated): ");
        string ignoreInput = Console.ReadLine();
        var keywordsToIgnore = ignoreInput?.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();

        if (string.IsNullOrWhiteSpace(rootUrl) || keywordsToIgnore == null)
        {
            Console.WriteLine("Invalid input. Exiting.");
            return;
        }

        var scraper = new WebScraper(new HttpClient(), keywordsToIgnore, rootUrl);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[INFO]: ");
        Console.ResetColor();
        Console.WriteLine("Starting scrape...");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[INFO]: ");
        Console.ResetColor();
        Console.WriteLine("Scraping normal links first...");
        await scraper.ScrapeAsync(rootUrl);

        if(scraper.GetValidUrls().Contains("https://www.ganciotraino.it/ganci.asp"))
        {
            string towbarUrl = "https://www.ganciotraino.it/ganci.asp";
            var dropdownScraper = new DropdownScraper(new HttpClient());
            Console.WriteLine("Found towbars page, scraping dropdowns...");
            Console.WriteLine("Looking for cars...");
            await dropdownScraper.CrawlDropdownsAsync(towbarUrl);
            var towbarUrls = dropdownScraper.GetAllDropdownUrls();
            foreach (var url in towbarUrls)
            {
                string completeUrl = url + "&Carrozzeria=Tutti";
                await scraper.ScrapeAsync(completeUrl);
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[FINISH]: ");
        Console.ResetColor();
        Console.WriteLine("Scraping completed.");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[INFO]: ");
        Console.ResetColor();
        Console.WriteLine("Writing XML file...");

        try
        {
            string destination = rootUrl
            .Replace("https://www.", "") // Remove the "https://www." part
            .Replace("/", "")            // Remove slashes
            .Replace(".", "_");          // Replace dots with underscores

            // Append today's date and time
            destination += $"_{DateTime.Now:yyyyMMddHHmmss}";
            destination += ".xml";
            WriteToXML.WriteUrlsToXml(scraper.GetValidUrls(), destination);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[FINISH]: ");
            Console.ResetColor();
            Console.WriteLine("XML file written successfully.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[ERROR]: ");
            Console.ResetColor();
            Console.WriteLine(ex.Message);
        }

    }
}