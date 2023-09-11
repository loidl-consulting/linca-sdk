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

internal static class LicaDataExchange
{
    public static (Patient created, bool canCue) CreatePatient(LicaConnection connection, Patient patient)
    {
        (var createdPatient, var canCue) = FhirDataExchange<Patient>.CreateResource(connection, patient);

        if(canCue)
        {
            Console.WriteLine($"patient created, got id {createdPatient.Id}");

            return (createdPatient, true);
        }

        return (new(), false);
    }
}
