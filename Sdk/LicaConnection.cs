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

internal class LicaConnection
{
    public LicaConnection()
    {
        JavaWebToken = string.Empty;
        ServerBaseUrl = string.Empty;
        Succeeded = false;
    }

    public LicaConnection(string jwt, string serverBaseUrl)
    {
        JavaWebToken = jwt;
        ServerBaseUrl = serverBaseUrl;
        Succeeded = !string.IsNullOrWhiteSpace(jwt);
    }

    public bool Succeeded { init; get; }

    public string JavaWebToken { init; get; }

    public string ServerBaseUrl { init; get; }

    /// <summary>
    /// Caller must dispose or use using pattern
    /// </summary>
    public HttpClient GetAuthenticatedClient()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new
        (
            Constants.AuthenticationScheme,
            JavaWebToken
        );

        return http;
    }
}
