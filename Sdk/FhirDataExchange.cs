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
using Hl7.Fhir.Model.Extensions;
using Hl7.Fhir.Serialization;
using System.Net;

namespace Lc.Linca.Sdk;

internal static class FhirDataExchange<T> where T : Resource, new()
{
    /// <summary>
    /// Posts an R5 resource to the FHIR server, reads back the 
    /// REST entity location from the response, and returns the
    /// new resource obtained from there
    /// </summary>
    public static (T created, bool canCue) CreateResource(LincaConnection connection, T resource)
    {
        using var response = Send(connection, HttpMethod.Post, resource);
        if (response?.StatusCode == HttpStatusCode.Created)
        {
            using var getResponse = Receive(connection, response.Headers.Location);
            if (getResponse != null)
            {
                var createdResourceRaw = new StreamReader
                (
                    getResponse.Content.ReadAsStream()
                ).ReadToEnd();

                if (new FhirJsonPocoDeserializer().TryDeserializeResource
                (
                    createdResourceRaw,
                    out Resource? parsedResource,
                    out var _
                ) && parsedResource is T createdResource)
                {
                    return (createdResource, true);
                }
            }
        }

        return (new(), false);
    }

    private static HttpResponseMessage? Receive(LincaConnection connection, Uri? fromLocation)
    {
        for (; ; )
        {
            using var http = connection.GetAuthenticatedClient();
            var getRequest = new HttpRequestMessage
            (
                HttpMethod.Get,
                fromLocation
            );

            getRequest.Headers.Accept.Add(Constants.FhirJson);
            var getResponse = http.Send(getRequest);

            if (getResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                /* token may be expired or may have been revoked.
                 * retry once with certificate reauthentication */
                connection.Reauthenticate();

                continue;
            }

            return getResponse;
        }
    }

    private static HttpResponseMessage? Send(LincaConnection connection, HttpMethod method, T resource)
    {
        for (; ; )
        {
            using var http = connection.GetAuthenticatedClient();
            var request = new HttpRequestMessage
            (
                method,
                $"{connection.ServerBaseUrl}/{resource.GetProfiledResourceName()}"
            );

            var fhirJson = resource.ToJson();

            request.Content = new StringContent(fhirJson);
            request.Content.Headers.ContentType = Constants.FhirJson;
            request.Headers.Accept.Add(Constants.FhirJson);
            var response = http.Send(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                /* token may be expired or may have been revoked.
                 * retry once with certificate reauthentication */
                connection.Reauthenticate();

                continue;
            }

            return response;
        }
    }
}