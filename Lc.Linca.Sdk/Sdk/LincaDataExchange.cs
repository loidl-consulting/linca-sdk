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

    /// <summary>
    /// Post a new Linked Care order position
    /// </summary>
    public static (MedicationRequest postedOMR, bool canCue) PostProposalMedicationRequest(LincaConnection connection, MedicationRequest omr)
    {
        (var postedOMR, var canCue) = FhirDataExchange<MedicationRequest>.CreateResource(connection, omr, LincaEndpoints.LINCAProposalMedicationRequest);

        if (canCue)
        {
            return (postedOMR, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Create a new Linked Care prescription
    /// </summary>
    public static (MedicationRequest postedPMR, bool canCue) CreatePrescriptionMedicationRequest(LincaConnection connection, MedicationRequest pmr)
    {
        (var postedPMR, var canCue) = FhirDataExchange<MedicationRequest>.CreateResource(connection, pmr, LincaEndpoints.LINCAPrescriptionMedicationRequest);

        if (canCue)
        {
            return (postedPMR, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Create a new Linked Care medication dispense
    /// </summary>
    public static (MedicationDispense postedMD, bool canCue) CreateMedicationDispense(LincaConnection connection, MedicationDispense md)
    {
        (var postedMD, var canCue) = FhirDataExchange<MedicationDispense>.CreateResource(connection, md, LincaEndpoints.LINCAMedicationDispense);

        if (canCue)
        {
            return (postedMD, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Revoke a Linked Care request orchestration and cancel all contained order positions
    /// </summary>
    public static bool DeleteRequestOrchestration(LincaConnection connection, string id)
    {
        var deleted = FhirDataExchange<RequestOrchestration>.DeleteResource(connection, id, LincaEndpoints.LINCARequestOrchestration);

        if (deleted)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get a all order chain links for the given lc_id
    /// </summary>
    public static (Bundle results, bool canCue) GetProposalStatus(LincaConnection connection, string id)
    {
        string operationQuery = $"{LincaEndpoints.proposal_status}?lc_id={id}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get a all order chain links (order positions, prescriptions, dispenses) for the given lc_id
    /// </summary>
    public static (Bundle results, bool canCue) GetProposalsToPrescribe(LincaConnection connection)
    {
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, LincaEndpoints.proposals_to_prescribe);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get all prescriptions to dispense (the complete order chains)
    /// </summary>
    public static (Bundle results, bool canCue) GetPrescriptionsToDispense(LincaConnection connection)
    {
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, LincaEndpoints.prescriptions_to_dispense);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get all Linked Care prescriptions which are connected to the given id
    /// </summary>
    public static (Bundle results, bool canCue) GetPrescriptionToDispense(LincaConnection connection, string id)
    {
        string operationQuery = $"{LincaEndpoints.prescription_to_dispense}?id={id}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }

    /// <summary>
    /// Get initial prescriptions (and corresponding dispenses) from the last 90 days for the given svnr
    /// </summary>
    public static (Bundle results, bool canCue) GetInitialPrescription(LincaConnection connection, string svnr)
    {
        string operationQuery = $"{LincaEndpoints.patient_initial_prescriptions}?svnr={svnr}";
        (Bundle proposalChains, bool canCue) = FhirDataExchange<Bundle>.GetResource(connection, operationQuery);

        if (canCue)
        {
            return (proposalChains, true);
        }

        return (new(), false);
    }
}
