using HtmlAgilityPack;

namespace SeoLeadPlatform.Infrastructure.Seo;

public class ParsedPage
{
    public string Url { get; init; } = string.Empty;
    public string Html { get; init; } = string.Empty;
    public HtmlDocument Document { get; init; } = new();
    public Uri BaseUri { get; init; } = default!;
    public int? HttpStatus { get; init; }
    public long? ResponseTimeMs { get; init; }
    public long? PageSizeBytes { get; init; }

    public string? Title => Document.DocumentNode
        .SelectSingleNode("//title")?.InnerText?.Trim();

    public string? MetaDescription => Document.DocumentNode
        .SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", null)?.Trim();

    public string? Canonical => Document.DocumentNode
        .SelectSingleNode("//link[@rel='canonical']")?.GetAttributeValue("href", null)?.Trim();

    public bool HasViewport => Document.DocumentNode
        .SelectSingleNode("//meta[@name='viewport']") != null;

    public bool HasH1 => Document.DocumentNode.SelectNodes("//h1")?.Count > 0;

    public int WordCount
    {
        get
        {
            var text = Document.DocumentNode.SelectSingleNode("//body")?.InnerText ?? string.Empty;
            var normalized = System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(normalized)) return 0;
            return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    public IReadOnlyList<HtmlNode> Images => Document.DocumentNode.SelectNodes("//img")?.ToList() ?? new List<HtmlNode>();

    public IReadOnlyList<HtmlNode> InternalLinks
    {
        get
        {
            var links = Document.DocumentNode.SelectNodes("//a[@href]")?.ToList() ?? new List<HtmlNode>();
            var host = BaseUri.Host;
            return links.Where(a =>
            {
                var href = a.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrWhiteSpace(href)) return false;
                if (href.StartsWith("mailto:") || href.StartsWith("tel:") || href.StartsWith("#")) return false;
                if (Uri.TryCreate(BaseUri, href, out var abs))
                {
                    return abs.Host.Equals(host, StringComparison.OrdinalIgnoreCase);
                }
                return true;
            }).ToList();
        }
    }

    public bool HasStructuredData => Document.DocumentNode
        .SelectNodes("//script[@type='application/ld+json']")?.Count > 0;
}
