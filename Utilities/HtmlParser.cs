namespace GiacScraper
{
    public static class HtmlParser
    {
        private static readonly string[] _validExtensions = { ".html", ".htm", ".aspx", ".php", ".asp" };

        public static List<string> ExtractLinks(string htmlContent)
        {
            var links = new List<string>();
            int index = 0;

            while ((index = htmlContent.IndexOf("href=\"", index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                index += 6; // Move past 'href="'
                var endIndex = htmlContent.IndexOf('"', index);
                if (endIndex == -1) break;

                var link = htmlContent[index..endIndex];
                if (!string.IsNullOrWhiteSpace(link) && IsValidLink(link))
                {
                    links.Add(link);
                }
            }

            return links;
        }

        private static bool IsValidLink(string link)
        {
            try
            {
                var uri = new Uri(link, UriKind.RelativeOrAbsolute);

                // Extract the path from the URL (ignoring query parameters and fragments)
                var path = uri.IsAbsoluteUri ? uri.AbsolutePath : link.Split('?')[0].Split('#')[0];

                // Check if any valid extension exists in the path
                return _validExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                // If the link is malformed or cannot be parsed as a URI, treat it as invalid
                return false;
            }
        }
    }
}
