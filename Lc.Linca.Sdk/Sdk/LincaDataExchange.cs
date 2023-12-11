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
        (var createdPatient, var canCue, var outcome) = FhirDataExchange<Patient>.CreateResourceWithOutcome(connection, patient);

        if(canCue)
        {
            return (createdPatient, true);
        }
        else if (outcome != null)
        {
            foreach(var item in outcome.Issue) 
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Post a new Linked Care Medication Order
    /// </summary>
    public static (RequestOrchestration createdRO, bool canCue) CreateRequestOrchestration(LincaConnection connection, RequestOrchestration ro)
    {
        (var createdRO, var canCue, var outcome) = FhirDataExchange<RequestOrchestration>.CreateResourceWithOutcome(connection, ro);

        if (canCue)
        {
            return (createdRO, true);
        }
        else if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Post a new Linked Care proposal order position
    /// </summary>
    public static (MedicationRequest postedOMR, bool canCue) PostProposalMedicationRequest(LincaConnection connection, MedicationRequest omr)
    {
        (var postedOMR, var canCue, var outcome) = FhirDataExchange<MedicationRequest>.CreateResourceWithOutcome(connection, omr, LincaEndpoints.LINCAProposalMedicationRequest);

        if (canCue)
        {
            return (postedOMR, true);
        }
        else if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Create a LINCAPrescriptionsMedicationRequest: used to stop or end single order positions
    /// </summary>
    public static (MedicationRequest postedPMR, bool canCue) CreatePrescriptionMedicationRequest(LincaConnection connection, MedicationRequest pmr)
    {
        (var postedPMR, var canCue, var outcome) = FhirDataExchange<MedicationRequest>.CreateResourceWithOutcome(connection, pmr, LincaEndpoints.LINCAPrescriptionMedicationRequest);

        if (canCue)
        {
            return (postedPMR, true);
        }
        else if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Create a new LINCA prescription 
    /// </summary>
    public static (Bundle results, bool canCue) CreatePrescriptionBundle(LincaConnection connection, Bundle prescriptions)
    {
        (Bundle createdPrescriptions, bool canCue, var outcome) = FhirDataExchange<Bundle>.CreateResourceBundle(connection, prescriptions, LincaEndpoints.prescription);

        if (canCue)
        {
            return (createdPrescriptions, true);
        }
        else if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Create a new Linked Care medication dispense
    /// </summary>
    public static (MedicationDispense postedMD, bool canCue) CreateMedicationDispense(LincaConnection connection, MedicationDispense md)
    {
        (var postedMD, var canCue, var outcome) = FhirDataExchange<MedicationDispense>.CreateResourceWithOutcome(connection, md);

        if (canCue)
        {
            return (postedMD, true);
        }
        else if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return (new(), false);
    }

    /// <summary>
    /// Revoke a Linked Care request orchestration and cancel all contained order positions
    /// </summary>
    public static (OperationOutcome oo, bool deleted) DeleteRequestOrchestration(LincaConnection connection, string id)
    {
        return FhirDataExchange<RequestOrchestration>.DeleteResource(connection, id, LincaEndpoints.LINCARequestOrchestration);
    }

    /// <summary>
    /// Get a all order chain links (proposal order positions, prescriptions, dispenses) for the given lc_id
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
    /// Get a all order chain links starting within the last 90 days for the requesting doctor (OID in certificate)
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
    /// Get all order chain links starting within the last 90 days for the requesting pharmacy (OID in certificate)
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
    /// Get all Linked Care prescriptions which are connected to the given id (eRezept-Id or LinkedCare-prescriptionId
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
