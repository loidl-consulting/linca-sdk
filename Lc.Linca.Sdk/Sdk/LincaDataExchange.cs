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

namespace Lc.Linca.Sdk;

/// <summary>
/// Methods to interact with the Linked Care FHIR Server
/// </summary>
public static class LincaDataExchange
{
    /// <summary>
    /// Create a new patient record on the FHIR server,
    /// and return the Id that has been assigned.
    /// If the Id is included in the submitted resource,
    /// and a patient with this Id does not yet exist in
    /// the patient store, then the FHIR server will create
    /// the patient resource using the specified Id (external assignment).
    /// If a patient record matching that Id is found, it
    /// it will be updated.
    /// </summary>
    public static (Patient created, bool canCue) CreatePatient(LincaConnection connection, Patient patient)
    {
        (var createdPatient, var canCue) = FhirDataExchange<Patient>.CreateResource(connection, patient, LincaEndpoints.HL7ATCorePatient);

        if(canCue)
        {
            return (createdPatient, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Submits a new order for prescription of medication.
    /// The orchestration request represents the order header,
    /// and it must contain one or more order positions
    /// (represented by contained order medication request resources)
    /// </summary>
    public static (RequestOrchestration created, bool canCue) PlaceOrder(LincaConnection connection, RequestOrchestration order)
    {

        return (new(), false);
    }

    /// <summary>
    /// Post a new Linked Care Medication Order
    /// </summary>
    public static (RequestOrchestration createdRO, bool canCue) CreateRequestOrchestration(LincaConnection connection, RequestOrchestration ro)
    {
        (var createdRO, var canCue) = FhirDataExchange<RequestOrchestration>.CreateResource(connection, ro, LincaEndpoints.LINCARequestOrchestration);

        if (canCue)
        {
            return (createdRO, true);
        }

        return (new(), false);
    }

    public static (MedicationRequest postedOMR, bool canCue) PostOrderMedicationRequest(LincaConnection connection, MedicationRequest omr)
    {
        (var postedOMR, var canCue) = FhirDataExchange<MedicationRequest>.CreateResource(connection, omr, LincaEndpoints.LINCAOrderMedicationRequest);

        if (canCue)
        {
            return (postedOMR, true);
        }

        return (new(), false);
    }

    public static (MedicationRequest postedPMR, bool canCue) CreatePrescriptionMedicationRequest(LincaConnection connection, MedicationRequest pmr)
    {
        (var postedPMR, var canCue) = FhirDataExchange<MedicationRequest>.CreateResource(connection, pmr, LincaEndpoints.LINCAPrescriptionMedicationRequest);

        if (canCue)
        {
            return (postedPMR, true);
        }

        return (new(), false);
    }

    public static (MedicationDispense postedMD, bool canCue) CreateMedicationDispense(LincaConnection connection, MedicationDispense md)
    {
        (var postedMD, var canCue) = FhirDataExchange<MedicationDispense>.CreateResource(connection, md, LincaEndpoints.LINCAMedicationDispense);

        if (canCue)
        {
            return (postedMD, true);
        }

        return (new(), false);
    }
}
