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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US013_ModifyPrescribedDosage : Spec
{
    protected MedicationRequest prescription = new();

    public const string UserStory = @"
        Practitioner Dr. Silvia Spitzmaus is responsible for the LINCA registered care giver client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him and has already 
        submitted a prescription for that order position.
        She decides that the dosage instructions in the prior prescription need to be defined or modified. 
        Hence, she submits an update to that prescription with new dosage instructions,
          and her software will send that to the LINCA server,
          and the ordering care giver organization Haus Vogelsang will be informed that the prescription has been 
          updated with altered dosage";

    public US013_ModifyPrescribedDosage(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create PrescriptionMedicationRequest with modified dosage", CreatePrescriptionRecord)
        };
    }

    private bool CreatePrescriptionRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        if (!string.IsNullOrEmpty(LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter))
        {
            prescription.PriorPrescription = new()
            {
                Reference = $"LINCAPrescriptionMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter}"
            };

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = new ResourceReference()                             // REQUIRED
            {
                Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"     // relative path to Linca Fhir patient resource, copy from order
            };

            prescription.Medication = new() // the doctor changes the medication to a ready-to-use ointment
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0059714",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Ultralan - Salbe"
                        }
                    }
                }
            };

            prescription.DosageInstruction.Add(new Dosage()
            {
                Text = "täglich morgens und abends auf die betroffene Stelle auftragen"
            });

            // prescription.InformationSource           // will be copied from reference in basedOn
            // prescription.Requester                   // will be copied from reference in basedOn
            // prescription.DispenseRequest.Dispenser   // will be copied from reference in basedOn, if available

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            prescription.Identifier.Add(new Identifier()
            {
                Value = "CVF1 23ER USW1",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"     // OID: eMed-Id
            });

            prescription.GroupIdentifier = new()
            {
                Value = "ABCD 1234 EFGH",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"        // OID: Rezeptnummer
            };

            (var postedPMR, var canCue) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

            if (canCue)
            {
                Console.WriteLine($"Linca PrescriptionMedicationRequest transmitted, id {postedPMR.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
            }

            return canCue;
        }
        else 
        {
            Console.WriteLine($"Linca PrescriptionMedicationRequest for Guenter has not been created before");
            return false;
        }
    }
}
