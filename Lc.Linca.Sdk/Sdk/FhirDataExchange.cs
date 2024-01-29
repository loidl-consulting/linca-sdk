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
    private const int maxReconnectRetries = 5;

    /// <summary>
    /// Deprecated, for backward compatibility
    /// </summary>
    public static (T created, bool canCue) CreateResource(LincaConnection connection, T resource, string overwriteResourceName)
    {
        (var created, var cancue, _) = CreateResourceWithOutcome(connection, resource, overwriteResourceName);

        return (created, cancue);
    }

    /// <summary>
    /// Posts an R5 resource to the FHIR server, reads back the 
    /// REST entity location from the response, and returns the
    /// new resource obtained from there
    /// </summary>
    public static (T created, bool canCue, OperationOutcome? outcome) CreateResourceWithOutcome(LincaConnection connection, T resource, string? overwriteResourceName = null)
    {
        HttpResponseMessage? response;

        if (overwriteResourceName == null)
        {
            if (resource is MedicationRequest)
            {
                throw new ArgumentNullException(
                    nameof(overwriteResourceName),
                    "Create resource MedicationRequest requires additional parameter overwriteResourceName"
                );
            }
            else
            {
                overwriteResourceName = LincaEndpoints.GetProfiledResourceName(resource);
            }
        }

        if (string.IsNullOrEmpty(resource.Id))
        {
            response = Send(connection, HttpMethod.Post, resource, overwriteResourceName);
        }
        else
        {
            response = Send(connection, HttpMethod.Put, resource, overwriteResourceName);
        }
        
        if (response?.StatusCode == HttpStatusCode.Created)
        {
            using var getResponse = Receive(connection, response.Headers.Location);
            response.Dispose();

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
                    return (createdResource, true, null);
                }
            }
        }
        else if (response != null)
        {
            var receivedResourceRaw = new StreamReader
            (
                response.Content.ReadAsStream()
            ).ReadToEnd();

            if (new FhirJsonPocoDeserializer().TryDeserializeResource
            (
                receivedResourceRaw,
                out Resource? parsedResource,
                out var issues
            ) && parsedResource is OperationOutcome outcome)
            {
                return (new(), false, outcome);
            }
        }

        return (new(), false, null);

    }

    /// <summary>
    /// Posts an R5 resource to the FHIR server, reads back the 
    /// REST entity location from the response, and returns the
    /// new resource obtained from there
    /// </summary>
    public static (T created, bool canCue, OperationOutcome? outcome) CreateResourceWithOutcomeNotTypeSafe(LincaConnection connection, T resource, string lincaResourceName)
    {
        HttpResponseMessage? response;

        if (string.IsNullOrEmpty(resource.Id))
        {
            response = Send(connection, HttpMethod.Post, resource, lincaResourceName);
        }
        else
        {
            response = Send(connection, HttpMethod.Put, resource, lincaResourceName);
        }

        if (response?.StatusCode == HttpStatusCode.Created)
        {
            using var getResponse = Receive(connection, response.Headers.Location);
            response.Dispose();

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
                    return (createdResource, true, null);
                }
            }
        }
        else if (response != null)
        {
            var receivedResourceRaw = new StreamReader
            (
                response.Content.ReadAsStream()
            ).ReadToEnd();

            if (new FhirJsonPocoDeserializer().TryDeserializeResource
            (
                receivedResourceRaw,
                out Resource? parsedResource,
                out var issues
            ) && parsedResource is OperationOutcome outcome)
            {
                return (new(), false, outcome);
            }
        }

        return (new(), false, null);

    }

    /// <summary>
    /// Posts a Bundle of R5 resources to the FHIR server, and returns the
    /// new resource obtained as answer
    /// </summary>
    public static (T created, bool canCue, OperationOutcome? outcome) CreateResourceBundle(LincaConnection connection, T resource, string operation)
    {
        using var response = Send(connection, HttpMethod.Post, resource, operation);
        if (response?.StatusCode == HttpStatusCode.Created || response?.StatusCode == HttpStatusCode.BadRequest)
        {
            if (response != null)
            {
                var createdResourceRaw = new StreamReader
                (
                    response.Content.ReadAsStream()
                ).ReadToEnd();

                if (new FhirJsonPocoDeserializer().TryDeserializeResource
                (
                    createdResourceRaw,
                    out Resource? parsedResource,
                    out var _
                ))
                {
                    if (parsedResource is T createdResource)
                    {
                        return (createdResource, true, null);
                    }
                    if (parsedResource is  OperationOutcome outcome)
                    {
                        return (new(), false, outcome);
                    }
                }
            }
        }

        return (new(), false, null);
    }

    /// <summary>
    /// Retrieves a bundle as the result of a Fhir operation call
    /// </summary>
    public static (Bundle received, bool canCue) GetResource(LincaConnection connection, string operationQuery)
    {
        using var getResponse = Receive(connection, operationQuery);

        if (getResponse != null && getResponse.StatusCode == HttpStatusCode.OK)
        {
            var receivedResourceRaw = new StreamReader
            (
                getResponse.Content.ReadAsStream()
            ).ReadToEnd();

            if (new FhirJsonPocoDeserializer().TryDeserializeResource
            (
                receivedResourceRaw,
                out Resource? parsedResource,
                out var _
            ) && parsedResource is Bundle receivedResource)
            {
                return (receivedResource, true);
            }
        }
        
        return (new(), false);
    }

    /// <summary>
    /// Attempts to delete a resource
    /// </summary>
    public static (OperationOutcome received, bool canCue) DeleteResource(LincaConnection connection, string id, string overwriteResourceName)
    {
        using var response = Send(connection, HttpMethod.Delete, id, overwriteResourceName);

        if (response != null)
        {
            var receivedResourceRaw = new StreamReader
            (
                response.Content.ReadAsStream()
            ).ReadToEnd();

            if (new FhirJsonPocoDeserializer().TryDeserializeResource
            (
                receivedResourceRaw,
                out Resource? parsedResource,
                out var _
            ) && parsedResource is OperationOutcome receivedResource)
            {
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    return (receivedResource, true);
                }
                else
                {
                    return (receivedResource, false);
                }
            }
        }

        return (new(), false);
    }

    private static HttpResponseMessage? Receive(LincaConnection connection, Uri? fromLocation)
    {
        for (var attempt = 0; attempt < maxReconnectRetries; ++attempt)
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

        throw new TimeoutException($"Failed to reconnect after {maxReconnectRetries} attempts");
    }

    private static HttpResponseMessage? Receive(LincaConnection connection, string operationQuery)
    {
        for (var attempt = 0; attempt < maxReconnectRetries; ++attempt)
        {
            using var http = connection.GetAuthenticatedClient();
            var getRequest = new HttpRequestMessage
            (
                HttpMethod.Get,
                $"{connection.ServerBaseUrl}/{operationQuery}"
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

        throw new TimeoutException($"Failed to reconnect after {maxReconnectRetries} attempts");
    }

    private static HttpResponseMessage? Send(LincaConnection connection, HttpMethod method, T resource, string overwriteResourceName)
    {
        for (var attempt = 0; attempt < maxReconnectRetries; ++attempt)
        {
            using var http = connection.GetAuthenticatedClient();
            HttpRequestMessage request;

            if (string.IsNullOrEmpty(resource.Id))
            {
                request = new HttpRequestMessage
                (
                    method,
                    $"{connection.ServerBaseUrl}/{overwriteResourceName}"
                );
            }
            else
            {
                request = new HttpRequestMessage
                (
                    method,
                    $"{connection.ServerBaseUrl}/{overwriteResourceName}/{resource.Id}"
                );
            }

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

        throw new TimeoutException($"Failed to reconnect after {maxReconnectRetries} attempts");
    }

    private static HttpResponseMessage? Send(LincaConnection connection, HttpMethod method, string id, string overwriteResourceName)
    {
        for (var attempt = 0; attempt < maxReconnectRetries; ++attempt)
        {
            using var http = connection.GetAuthenticatedClient();
            var request = new HttpRequestMessage
            (
                method,
                $"{connection.ServerBaseUrl}/{overwriteResourceName}/{id}"
            );

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

        throw new TimeoutException($"Failed to reconnect after {maxReconnectRetries} attempts");
    }
}