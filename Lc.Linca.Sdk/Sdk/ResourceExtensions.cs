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

namespace Hl7.Fhir.Model.Extensions;

/// <summary>
/// The Linked Care profile uses a copy of the
/// Austrian Patient, because in the current FHIR
/// tooling of HL7 Austria, importing it is not
/// possible.
/// </summary>
public static class ResourceExtensions
{
    /// <summary>
    /// The name of the resource as defined in the IG profile
    /// </summary>
    public static string GetProfiledResourceName(this Resource resource)
    {
        if(resource is Patient)
        {
            return "HL7ATCorePatient";
        }

        if(resource is RequestOrchestration) 
        {
            return "LINCARequestOrchestration";
        }

        return typeof(Resource).Name;
    }
}