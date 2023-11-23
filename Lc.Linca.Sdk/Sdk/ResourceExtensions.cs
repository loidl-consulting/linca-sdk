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
public static class LincaEndpoints
{
    /// <summary>
    /// The name of the patient resource as defined in the IG profile
    /// </summary>
    public static string HL7ATCorePatient = "HL7ATCorePatient";

    /// <summary>
    /// The name of the request orchestration resource as defined in the IG profile
    /// </summary>
    public static string LINCARequestOrchestration = "LINCARequestOrchestration";

    /// <summary>
    /// The name of the order medication request resource as defined in the IG profile
    /// </summary>
    public static string LINCAProposalMedicationRequest = "LINCAProposalMedicationRequest";

    /// <summary>
    /// The name of the prescription medication request resource as defined in the IG profile
    /// </summary>
    public static string LINCAPrescriptionMedicationRequest = "LINCAPrescriptionMedicationRequest";

    /// <summary>
    /// The name of the prescription medication request resource as defined in the IG profile
    /// </summary>
    public static string LINCAMedicationDispense = "LINCAMedicationDispense";

    /// <summary>
    /// The name of the operation to GET the order status for all 
    /// order positions associated with a specific LC_ID (the Id of a posted LINCARequestOrchestration
    /// can only be queried by the ordering entity
    /// </summary>
    public static string proposal_status = "$proposal-status";

    /// <summary>
    /// The name of the operation to GET the orders that have been adressed to 
    /// the querying doctor within the last XX days 
    /// (includes all requests where the doctor's OID is mentioned as designated performer 
    /// and their associated resource instances)
    /// can only be queried by doctors (ELGA-role in eHVD)
    /// </summary>
    public static string proposals_to_prescribe = "$proposals-to-prescribe";

    /// <summary>
    /// The name of the operation to POST a Bundle of LINCAPrescriptionMedicationRequest
    /// that share the same eRezeptId in groupIdentifier
    /// </summary>
    public static string prescription = "$prescription";

    /// <summary>
    /// The name of the operation to GET the prescriptions that have been adressed to 
    /// the querying pharmacy within the last XX days 
    /// (includes all requests where the pharmacy's OID is mentioned as designated dispenser 
    /// and their associated resource instances)
    /// can only be queried by pharmacies (ELGA-role in eHVD)
    /// can be combined with a specific Id presented to the pharmacist by a customer 
    /// </summary>
    public static string prescriptions_to_dispense = "$prescriptions-to-dispense";

    /// <summary>
    /// The name of the operation to GET the prescription(s) associated with
    /// a specific Id presented to the pharmacist by a customer/caregiver 
    /// can only be queried by pharmacies (ELGA-role in eHVD)
    /// </summary>
    public static string prescription_to_dispense = "$prescription-to-dispense";

    /// <summary>
    /// The name of the operation to GET the prescription(s) associated with
    /// a specific Id presented to the pharmacist by a customer/caregiver 
    /// can only be queried by pharmacies (ELGA-role in eHVD)
    /// </summary>
    public static string patient_initial_prescriptions = "$patient-initial-prescriptions";

    /// <summary>
    /// Get the profiled name of a standard Fhir resource
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