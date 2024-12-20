namespace GiacScraper.Services
{
    public class WebScraper
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly List<string> _keywordsToIgnore;
        private readonly HashSet<string> _visitedUrls = new();
        private readonly HashSet<string> _validUrlsToStore = new();

        public WebScraper(HttpClient httpClient, List<string> keywordsToIgnore, String baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
            _keywordsToIgnore = keywordsToIgnore;
        }

        public async Task ScrapeAsync(string url)
        {
            if (!url.Contains(_baseUrl))
            { 
                url = _baseUrl + url;
            }

            if (_visitedUrls.Contains(url)) return;

            if (!_visitedUrls.Contains(url))
            {
                _visitedUrls.Add(url);
            }

            _validUrlsToStore.Add(url);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[VISITING]: ");
            Console.ResetColor();
            Console.WriteLine(url);

            string pageContent;
            try
            {
                pageContent = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR]: ");
                Console.ResetColor();
                Console.WriteLine(url);
                Console.WriteLine($" could not fetch {url}: {ex.Message}");
                return;
            }

            var links = HtmlParser.ExtractLinks(pageContent);

            foreach (var link in links)
            {
                if (_keywordsToIgnore.Any(keyword => link.Contains(keyword)))
                {
                    continue;
                }

                await ScrapeAsync(link);
            }
        }

        public IEnumerable<string> GetValidUrls()
        {
            return _validUrlsToStore;
        }
    }
}