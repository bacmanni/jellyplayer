using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Handlers;

public class HttpClientExceptionHandler : DelegatingHandler
{
    private readonly IConfigurationService _configurationService;

    public HttpClientExceptionHandler(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");

            return response;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("HTTP request failed", ex);
        }
    }
}