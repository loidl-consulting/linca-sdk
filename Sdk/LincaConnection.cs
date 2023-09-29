/***********************************************************************************
 * Project:   Linked Care AP5
 * Component: LINCA FHIR SDK and Demo Client
 * Copyright: 2023 LOIDL Consulting & IT Services GmbH
 * Authors:   Annemarie Goldmann, Daniel Latikaynen
 * Purpose:   Sample code to test LINCA and template for client prototypes
 * Licence:   BSD 3-Clause
 * ---------------------------------------------------------------------------------
 * The Linked Care project is co-funded by the Austrian FFG
 ***********************************************************************************/

namespace Lc.Linca.Sdk;

internal class LincaConnection : IDisposable
{
    private HttpClient httpClient;

    public LincaConnection()
    {
        JavaWebToken = string.Empty;
        ServerBaseUrl = string.Empty;
        Succeeded = false;
        httpClient = new HttpClient();
    }

    public LincaConnection(string jwt, string serverBaseUrl) : this()
    {
        JavaWebToken = jwt;
        ServerBaseUrl = serverBaseUrl;
        Succeeded = !string.IsNullOrWhiteSpace(jwt);
    }

    public bool Succeeded { init; get; }

    public string JavaWebToken { get; internal set; }

    public string ServerBaseUrl { init; get; }

    /// <summary>
    /// Caller must dispose or use using pattern
    /// </summary>
    public HttpClient GetAuthenticatedClient()
    {
        httpClient = new();
        if (httpClient.DefaultRequestHeaders.Authorization == null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new
            (
                Constants.AuthenticationScheme,
                JavaWebToken
            );
        }

        return httpClient;
    }

    public void Reauthenticate()
    {
        using var newConnection = LincaConnector.Connect(ServerBaseUrl);
        
        if (newConnection.Succeeded)
        {
            JavaWebToken = newConnection.JavaWebToken;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClient.Dispose();
        }
    }
}
