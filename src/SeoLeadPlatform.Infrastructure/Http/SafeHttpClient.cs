using System.Net;
using System.Net.Sockets;

namespace SeoLeadPlatform.Infrastructure.Http;

public class SafeHttpFetchResult
{
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public string? Body { get; set; }
    public long ResponseTimeMs { get; set; }
    public long BodyBytes { get; set; }
    public string? FinalUrl { get; set; }
}

public class SafeHttpClient
{
    private const int MaxBodyBytes = 3 * 1024 * 1024;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private readonly IHttpClientFactory _httpClientFactory;

    public SafeHttpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SafeHttpFetchResult> FetchAsync(string url, CancellationToken ct)
    {
        if (!IsPublicHttpUrl(url, out var uri, out var reason))
        {
            return new SafeHttpFetchResult { Success = false, FailureReason = reason };
        }

        if (!await IsPublicHostAsync(uri.Host, ct))
        {
            return new SafeHttpFetchResult { Success = false, FailureReason = "URL points to a private/internal network — refused." };
        }

        var client = _httpClientFactory.CreateClient("seo-fetch");
        client.Timeout = Timeout;
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SeoLeadPlatform/1.0 (+https://seoleadplatform.example)");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");

        var start = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            var finalUri = response.RequestMessage?.RequestUri ?? uri;
            if (!await IsPublicHostAsync(finalUri.Host, ct))
            {
                return new SafeHttpFetchResult { Success = false, FailureReason = "Redirect resolved to a private/internal host — refused." };
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var buffer = new byte[8192];
            using var ms = new MemoryStream();
            int read;
            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                ms.Write(buffer, 0, read);
                if (ms.Length > MaxBodyBytes) break;
            }
            start.Stop();

            var bytes = ms.ToArray();
            var body = System.Text.Encoding.UTF8.GetString(bytes);

            return new SafeHttpFetchResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Body = body,
                ResponseTimeMs = start.ElapsedMilliseconds,
                BodyBytes = bytes.LongLength,
                FinalUrl = finalUri.ToString(),
                FailureReason = response.IsSuccessStatusCode ? null : $"Server returned {(int)response.StatusCode}."
            };
        }
        catch (HttpRequestException ex)
        {
            return new SafeHttpFetchResult { Success = false, FailureReason = $"HTTP error: {ex.Message}" };
        }
        catch (TaskCanceledException)
        {
            return new SafeHttpFetchResult { Success = false, FailureReason = "Request timed out." };
        }
        catch (Exception ex)
        {
            return new SafeHttpFetchResult { Success = false, FailureReason = $"Unexpected error: {ex.Message}" };
        }
    }

    public async Task<bool> HeadOrGetSucceedsAsync(string url, CancellationToken ct)
    {
        if (!IsPublicHttpUrl(url, out var uri, out _)) return false;
        if (!await IsPublicHostAsync(uri.Host, ct)) return false;

        var client = _httpClientFactory.CreateClient("seo-fetch");
        client.Timeout = TimeSpan.FromSeconds(8);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, uri);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.IsSuccessStatusCode) return true;
            if ((int)response.StatusCode == 405)
            {
                using var g = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
                return g.IsSuccessStatusCode;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsPublicHttpUrl(string url, out Uri uri, out string reason)
    {
        reason = string.Empty;
        uri = null!;
        if (string.IsNullOrWhiteSpace(url)) { reason = "URL is empty."; return false; }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var candidate)) { reason = "URL is not absolute."; return false; }
        if (candidate.Scheme != Uri.UriSchemeHttp && candidate.Scheme != Uri.UriSchemeHttps)
        {
            reason = "Only http/https URLs are supported."; return false;
        }
        if (candidate.HostNameType == UriHostNameType.IPv4 || candidate.HostNameType == UriHostNameType.IPv6)
        {
            if (IPAddress.TryParse(candidate.Host, out var ip) && IsPrivateIp(ip))
            {
                reason = "URL points to a private IP address."; return false;
            }
        }
        uri = candidate;
        return true;
    }

    public static async Task<bool> IsPublicHostAsync(string host, CancellationToken ct)
    {
        try
        {
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return false;
            var addresses = await Dns.GetHostAddressesAsync(host, ct);
            if (addresses.Length == 0) return false;
            return addresses.All(a => !IsPrivateIp(a));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPrivateIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip)) return true;
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast) return true;
            if (ip.Equals(IPAddress.IPv6Loopback)) return true;
            var mapped = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : null;
            if (mapped is not null) return IsPrivateIp(mapped);
            return false;
        }
        var bytes = ip.GetAddressBytes();
        if (bytes[0] == 10) return true;
        if (bytes[0] == 127) return true;
        if (bytes[0] == 169 && bytes[1] == 254) return true;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
        if (bytes[0] == 192 && bytes[1] == 168) return true;
        if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) return true;
        if (bytes[0] == 0) return true;
        if (bytes[0] >= 224) return true;
        return false;
    }
}
