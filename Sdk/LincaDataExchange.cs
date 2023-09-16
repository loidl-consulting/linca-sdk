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

internal static class LincaDataExchange
{
    public static (Patient created, bool canCue) CreatePatient(LincaConnection connection, Patient patient)
    {
        (var createdPatient, var canCue) = FhirDataExchange<Patient>.CreateResource(connection, patient);

        if(canCue)
        {
            return (createdPatient, true);
        }

        return (new(), false);
    }
}
