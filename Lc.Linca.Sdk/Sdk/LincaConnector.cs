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
    //private static X509Certificate2? certificate = null;

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


        return new("", lincaUrl);
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
        var conformityRequest = new HttpRequestMessage(HttpMethod.Get, $"{connection.ServerBaseUrl}/metadata");
        conformityRequest.Headers.Accept.Add(Constants.FhirJson);
        using var conformityResponse = http.Send(conformityRequest);

        if (!conformityResponse.IsSuccessStatusCode)
        {
            return (CapabilityNegotiationOutcome.Unauthorized, null);
        }

        var capabilityStatementRaw = new StreamReader(conformityResponse.Content.ReadAsStream()).ReadToEnd();
        if (new FhirJsonPocoDeserializer().TryDeserializeResource(capabilityStatementRaw, out Resource? resource, out var issues) && resource is CapabilityStatement capabilityStatement)
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
