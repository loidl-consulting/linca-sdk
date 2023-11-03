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

using Hl7.Fhir.Model;

namespace Lc.Linca.Sdk;

/// <summary>
/// Represents a connection to a Linked Care FHIR Server
/// and provides methods to establish and maintain such a connection
/// </summary>
public class LincaConnection : IDisposable
{
    private HttpClient httpClient;

    /// <summary>
    /// Creates a new Linked Care FHIR Server connection object
    /// Does not attempt to connect immediately
    /// </summary>
    public LincaConnection()
    {
        JavaWebToken = string.Empty;
        ServerBaseUrl = string.Empty;
        Succeeded = false;
        httpClient = new HttpClient();
    }

    /// <summary>
    /// Initializes a new instance of the Linked Care FHIR Server
    /// connection object with an existing access token and known server address
    /// </summary>
    /// <param name="jwt"></param>
    /// <param name="serverBaseUrl"></param>
    public LincaConnection(string jwt, string serverBaseUrl) : this()
    {
        JavaWebToken = jwt;
        ServerBaseUrl = serverBaseUrl;
        Succeeded = !string.IsNullOrWhiteSpace(jwt);
    }

    /// <summary>
    /// Indicates whether the last attempt to establish a connection
    /// to the Linked Care FHIR Server was successful
    /// </summary>
    public bool Succeeded { init; get; }

    /// <summary>
    /// The access token used to authenticate the client
    /// towards the Linked Care FHIR Server
    /// </summary>
    public string JavaWebToken { get; set; }

    /// <summary>
    /// The base Url of the Linked Care FHIR Server on the
    /// internet
    /// </summary>
    public string ServerBaseUrl { init; get; }

    /// <summary>
    /// The capability statement read from the Linked Care
    /// FHIR Server
    /// </summary>
    public CapabilityStatement Capabilities { get; internal set; } = new();

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

    /// <summary>
    /// Attempts to establish a new connection based on the
    /// same credentials that were previously used to establish
    /// a connection. Call when the token expires.
    /// </summary>
    public void Reauthenticate()
    {
        using var newConnection = LincaConnector.Connect(ServerBaseUrl);
        
        if (newConnection.Succeeded)
        {
            JavaWebToken = newConnection.JavaWebToken;
        }
    }

    /// <summary>
    /// Releases unmanaged resources that the connection
    /// has allocated
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release unmanaged resources that the connection
    /// has allocated
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClient.Dispose();
        }
    }
}
