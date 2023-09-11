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
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Lc.Linca.Sdk;

internal static class LicaConnector
{
    public static LicaConnection Connect(string licaUrl)
    {
        licaUrl = licaUrl.TrimEnd('/');

        /* 1.  initiate a connection to the Linked Care FHIR server 
         * 1a. select a client certificate for authentication
         *     (this is about authentication on organization level,
         *     where organization means care, practitioner or pharmacy). */
        var store = new X509Store(Constants.CertificateStoreKeyOwn, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        var collection = store.Certificates;
        X509Certificate2? certificate = null;

        if (OperatingSystem.IsWindows())
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
             * platform (for example, load from a PEM file) */
        }

        if (certificate == null)
        {
            return new();
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
        var smartRequest = new HttpRequestMessage(HttpMethod.Get, $"{licaUrl}/{Constants.FhirSmartPath}");
        smartRequest.Headers.Accept.Add(Constants.FhirJson);
        using var smartResponse = certHttp.Send(smartRequest);
        var smartRaw = new StreamReader(smartResponse.Content.ReadAsStream()).ReadToEnd();
        var tokenEndpoint = JToken.Parse(smartRaw)[Constants.OAuthKeyTokenEndpoint]?.Value<string>() ?? string.Empty;

        /* 3. obtain a JWT for the subsequent REST interactions */
        var tokenRequest = new HttpRequestMessage(HttpMethod.Get, tokenEndpoint);
        tokenRequest.Headers.Accept.Add(Constants.TextPlain);
        using var tokenResponse = certHttp.Send(tokenRequest);
        var tokenRaw = new StreamReader(tokenResponse.Content.ReadAsStream()).ReadToEnd();

        return new(tokenRaw, licaUrl);
    }

    public static bool NegotiateCapabilities(LicaConnection connection)
    {
        if(!connection.Succeeded)
        {
            Console.Error.WriteLine("No connection");

            return false;
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
            Console.Error.WriteLine("Unauthorized");

            return false;
        }

        var capabilityStatementRaw = new StreamReader(conformityResponse.Content.ReadAsStream()).ReadToEnd();
        if (new FhirJsonPocoDeserializer().TryDeserializeResource(capabilityStatementRaw, out Resource? resource, out var _) && resource is CapabilityStatement capabilityStatement)
        {
            Terminal.Info info = new();
            var name = capabilityStatement
                .Name
                .Replace(Constants.ServerProductLead, $"{Constants.AnsiColorFire}{Constants.ServerProductLead}{Constants.AnsiReset}")
                .Replace(Constants.ServerProductTail, $"{Constants.AnsiColorCaat}{Constants.ServerProductTail}{Constants.AnsiReset}");

            var desc = capabilityStatement
                .Description
                .Replace(Constants.ManufacturerName, $"{Constants.AnsiColorMaroon}{Constants.ManufacturerName}{Constants.AnsiReset}");

            info.WriteLine($@"{name}, FHIR version {capabilityStatement.FhirVersion}");
            info.WriteLine($@"Statement from {capabilityStatement.Date} supporting {capabilityStatement.Rest.First().Resource.Count} resources");
            info.HorizontalRule();
            info.WriteLine($@"Connected to {desc}, version {capabilityStatement.Version}");
            Console.Clear();
            info.Show();
        }
        else
        {
            Console.Error.WriteLine("Could not parse the FHIR Server's capability statement");

            return false;
        }

        return true;
    }
}
