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

using System.Net.Http.Headers;

namespace Lc.Linca.Sdk;

/// <summary>
/// Useful constants to use with client implementations
/// </summary>
public class Constants
{
    /// <summary>
    /// The preferred FHIR resource content type
    /// </summary>
    public static readonly MediaTypeWithQualityHeaderValue FhirJson = new("application/fhir+json");

    /// <summary>
    /// Xml as alternative FHIR resource content type
    /// </summary>
    public static readonly MediaTypeWithQualityHeaderValue FhirXml = new("application/fhir+xml");


    /// <summary>
    /// First part of the colored Linked Care FHIR Server product name
    /// </summary>
    public static readonly string ServerProductLead = ServerProduct[..4];
    
    /// <summary>
    /// Second part of the colored Linked Care FHIR Server product name
    /// </summary>
    public static readonly string ServerProductTail = ServerProduct[4..];

    /// <summary>
    /// The name of the Linked Care FHIR Server and SDK manufacturer
    /// </summary>
    public const string ManufacturerName = "LOIDL Consulting";

    /// <summary>
    /// Command to set the console color to the manufacturer's
    /// primary logo accent color
    /// </summary>
    public const string AnsiColorMaroon = "\x1B[31;1m";
    
    /// <summary>
    /// Command to set the console color to the Linked Care
    /// FHIR Server product name logo color (first part)
    /// </summary>
    public const string AnsiColorFire = "\u001b[38;5;196m";

    /// <summary>
    /// Command to set the console color to the Linked Care
    /// FHIR Server product name logo color (second part)
    /// </summary>
    public const string AnsiColorCaat = "\u001b[38;5;202m";

    /// <summary>
    /// Command to reset the console color to its default
    /// </summary>
    public const string AnsiReset = "\x1b[0m";

    /// <summary>
    /// Command to set the console color to green
    /// to indicate success
    /// </summary>
    public const string AnsiSuccess = "\u001b[38;5;2m■\u001b[0m";

    /// <summary>
    /// Command to set the console color to red
    /// to indicate failure
    /// </summary>
    public const string AnsiFail = "\u001b[38;5;1mX\u001b[0m";

    /// <summary>
    /// Date format string (sortable, ISO-style date)
    /// </summary>
    public const string DobFormat = "yyyyMMdd";

    /// <summary>
    /// The OID of the Austrian Patient's social insurance number
    /// code system
    /// </summary>
    public const string WellknownOidSocialInsuranceNr = "urn:oid:1.2.40.0.10.1.4.3.1";

    internal const string AuthenticationScheme = "Bearer";
    internal const string CertificateStoreKeyOwn = "MY";
    internal const string FhirSmartPath = ".well-known/smart-configuration";
    internal const string OAuthKeyTokenEndpoint = "token_endpoint";
    internal const string ServerProduct = "FHIRCAAT";

    internal static readonly MediaTypeWithQualityHeaderValue TextPlain = new("text/plain");
}
