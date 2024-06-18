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

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US021_DispenseTavipec : Spec
{
    protected MedicationDispense dispense1 = new();

    public const string UserStory = @"
        Pharmacist Mag. Franziska Fröschl, owner of the pharmacy Apotheke 'Klappernder Storch' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When she is expected to fullfil medication orders for a customer, e.g., Peter Kainrath, 
        and she has a LINCA order Id to go with a purchase his care giver just made for him, 
        then Mag. Fröschl submits a dispense record for the order position in question
          and her software will send that to the LINCA server.";

    public US021_DispenseTavipec(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create MedicationDispense", CreateMedicationDispenseRecord)
        };

    }

    private bool CreateMedicationDispenseRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "   ");  // ADD LC REZEPT ID HERE

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            MedicationRequest? prescriptionTavipec = prescriptionsToDispense.FirstOrDefault();

            if (prescriptionTavipec == null)
            {
                Console.WriteLine("Linca PrescriptionMedicationRequest for Peter Kainrath not found, LINCAMedicationDispense cannot be created");

                return (false);
            }

            dispense1.AuthorizingPrescription.Add(new()
            {
                Reference = $"LINCAPrescriptionMedicationRequest/{prescriptionTavipec.Id}"
            });

            dispense1.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
            dispense1.Subject = prescriptionTavipec.Subject;
            dispense1.Medication = prescriptionTavipec.Medication;

            /*
            dispense1.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "2453007",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "TAVIPEC KPS"
                        }
                    }
                }
            };
            */

            dispense1.Quantity = new() { Value = 1 };

            dispense1.DosageInstruction = prescriptionTavipec.DosageInstruction;

            dispense1.Performer.Add(new()
            {
                Actor = new()
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.5.1.1",  // OID of dispensing pharmacy
                        System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                    },
                    Display = "Apotheke 'Klappernder Storch'"
                }
            });

            dispense1.Type = new()
            {
                Coding = new()
                {
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC") // complete the dispense
                }
            };

            (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense1);

            if (canCue)
            {
                Console.WriteLine($"Linca MedicationDispense transmitted, id {postedMD.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca MedicationDispense");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return canCue;
        }
        else
        {
            Console.WriteLine($"Failed to receive Linca Prescription Medication Requests");

            return false;
        }
    }
}
