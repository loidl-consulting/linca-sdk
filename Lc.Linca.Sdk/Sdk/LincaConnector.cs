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
using Hl7.Fhir.Serialization;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Lc.Linca.Sdk;

/// <summary>
/// Utility functions to manage connections
/// to the Linked Care FHIR Server
/// </summary>
public static class LincaConnector
{
    private static X509Certificate2? certificate = null;

    /// <summary>
    /// Establish a connection to the Linked Care FHIR Server
    /// cluster located at the specified internet address
    /// </summary>
    /// <param name="lincaUrl">The internet address of the Linked Care Cluster</param>
    /// <param name="clientCertificate">The X509 certificate which is presented to the FHIR server for authentication<br/>
    /// (if null, and on Windows, and interactive, will prompt to select one from the store)</param>
    public static LincaConnection Connect(string lincaUrl, X509Certificate2? clientCertificate = null)
    {
        lincaUrl = lincaUrl.TrimEnd('/');

        /* 1.  initiate a connection to the Linked Care FHIR server 
         * 1a. select a client certificate for authentication
         *     (this is about authentication on organization level,
         *     where organization means care, practitioner or pharmacy). */
        if (clientCertificate == null)
        {
            var store = new X509Store(Constants.CertificateStoreKeyOwn, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            var collection = store.Certificates;

            if (certificate == null)
            {
                if (OperatingSystem.IsWindows() && Environment.UserInteractive)
                {
                    certificate = X509Certificate2UI.SelectFromCollection
                    (
                        collection,
                        Localization.CertificatePromptTitleDe,
                        Localization.CertificatePromptDescDe,
                        X509SelectionFlag.SingleSelection
                    ).FirstOrDefault();
                }
                else
                {
                    /* use a method to load the certificate, that works on your
                     * platform (for example, load from a PEM file). the following
                     * line is just an example assuming that there is a PEM file
                     * present in the current working directory. */
                    certificate = X509Certificate2.CreateFromPemFile("linca-pflegeeinrichtung-001-dev.pem");
                }
            }

            if (certificate == null)
            {
                return new();
            }
        }
        else
        {
            certificate = clientCertificate;
        }

        HttpClientHandler handler = new()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
#if DEBUG
            /* some local development environments might not yet
             * have an operating system which supports TLS 1.3
             * as for example Microsoft Windows 10 */
            SslProtocols = /* SslProtocols.Tls13 | */ SslProtocols.Tls12
#else
            /* in production and as per specification, we do not
             * support anything less than TLS 1.3 for REST communications */
            SslProtocols = SslProtocols.Tls13
#endif
        };

        handler.ClientCertificates.Add(certificate);
        using var certHttp = new HttpClient(handler);

        /* 2. use FHIR SMART to present our client certificate and
         *    ask for the token endpoint that we should contact */
        var smartRaw = string.Empty;
        var smartRequest = new HttpRequestMessage(HttpMethod.Get, $"{lincaUrl}/{Constants.FhirSmartPath}");
        smartRequest.Headers.Accept.Add(Constants.FhirJson);
        try
        {
            using var smartResponse = certHttp.Send(smartRequest);
            smartRaw = new StreamReader(smartResponse.Content.ReadAsStream()).ReadToEnd();
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException?.InnerException is Win32Exception wex)
            {
                Console.Error.WriteLine("No permission to read the client certificate: {0}", wex.Message);
            }
            else
            {
                Console.Error.WriteLine("SMART Authentication failed: {0}", ex.Message);
            }

            return new();
        }

        var tokenEndpoint = JToken.Parse(smartRaw)[Constants.OAuthKeyTokenEndpoint]?.Value<string>() ?? string.Empty;

        /* 3. obtain a JWT for the subsequent REST interactions */
        var tokenRequest = new HttpRequestMessage(HttpMethod.Get, tokenEndpoint);
        tokenRequest.Headers.Accept.Add(Constants.TextPlain);
        using var tokenResponse = certHttp.Send(tokenRequest);
        var tokenRaw = new StreamReader(tokenResponse.Content.ReadAsStream()).ReadToEnd();

        return new(tokenRaw, lincaUrl);
    }

    /// <summary>
    /// Read the Linked Care FHIR Server's capability statement
    /// and check whether a connection is possible by comparing
    /// the major version and supported resources and operations
    /// </summary>
    public static (CapabilityNegotiationOutcome outcome, CapabilityStatement? statement) NegotiateCapabilities(LincaConnection connection)
    {
        if(!connection.Succeeded)
        {
            return (CapabilityNegotiationOutcome.NotConnected, null);
        }
        
        /* 2a. from now on, the http client does not need to send
         *     the client certificate, because there is already the
         *     JWT which is passed via the authorization header */
        using var http = connection.GetAuthenticatedClient();

        /* 3. negotiate capabilities with the Linked Care FHIR server */
        var conformityRequest = new HttpRequestMessage(HttpMethod.Options, $"{connection.ServerBaseUrl}/");
        conformityRequest.Headers.Accept.Add(Constants.FhirJson);
        using var conformityResponse = http.Send(conformityRequest);

        if (!conformityResponse.IsSuccessStatusCode)
        {
            return (CapabilityNegotiationOutcome.Unauthorized, null);
        }

        var capabilityStatementRaw = new StreamReader(conformityResponse.Content.ReadAsStream()).ReadToEnd();
        if (new FhirJsonPocoDeserializer().TryDeserializeResource(capabilityStatementRaw, out Resource? resource, out var _) && resource is CapabilityStatement capabilityStatement)
        {
            connection.Capabilities = capabilityStatement;
        }
        else
        {
            return (CapabilityNegotiationOutcome.CouldNotParse, null);
        }

        return (CapabilityNegotiationOutcome.Succeeded, capabilityStatement);
    }
}
