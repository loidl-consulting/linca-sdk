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

internal class Constants
{
    internal const string AuthenticationScheme = "Bearer";
    internal const string CertificateStoreKeyOwn = "MY";
    internal const string ManufacturerName = "LOIDL Consulting";
    internal const string FhirSmartPath = ".well-known/smart-configuration";
    internal const string OAuthKeyTokenEndpoint = "token_endpoint";
    internal const string AnsiColorMaroon = "\x1B[31;1m";
    internal const string AnsiColorFire = "\u001b[38;5;196m";
    internal const string AnsiColorCaat = "\u001b[38;5;202m";
    internal const string AnsiReset = "\x1b[0m";
    internal const string ServerProduct = "FHIRCAAT";
    internal const string AnsiSuccess = "\u001b[38;5;2m■\u001b[0m";
    internal const string AnsiFail = "\u001b[38;5;1mX\u001b[0m";
    internal const string DobFormat = "yyyyMMdd";
    internal const string WellknownOidSocialInsuranceNr = "urn:oid:1.2.40.0.10.1.4.3.1";

    internal static readonly string ServerProductLead = ServerProduct[..4];
    internal static readonly string ServerProductTail = ServerProduct[4..];
    internal static readonly MediaTypeWithQualityHeaderValue FhirJson = new("application/fhir+json");
    internal static readonly MediaTypeWithQualityHeaderValue TextPlain = new("text/plain");
}
