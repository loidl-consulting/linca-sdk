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

internal class US018_Dispense : Spec
{
    protected MedicationDispense dispense = new();

    public const string UserStory = @"
        Pharmacist Mag. Franziska Fröschl, owner of the pharmacy Apotheke 'Klappernder Storch' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When he is expected to fullfil medication orders for a customer, e.g., Renate Rüssel-Olifant, 
        and he has a LINCA order Id to go with a purchase her care giver Susanne Allzeit just made for her, 
        then Mag. Andreas Amsel submits a dispense record for the order position in question
          and his software will send that to the LINCA server,
          and notify the ordering organization, Pflegedienst Immerdar, about the thus completed order position.";

    public US018_Dispense(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create MedicationDispense", CreateMedicationDispenseRecord)
        };

    }

    private bool CreateMedicationDispenseRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "ASDFGHJ4KL34");

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            MedicationRequest? prescriptionRenateLasix = prescriptionsToDispense.Find(x => x.Medication.Concept.Coding.First().Display.Contains("Lasix"));

            if (prescriptionRenateLasix != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLasix = prescriptionRenateLasix.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine("Linca PrescriptionMedicationRequest for Renate Rüssel-Olifant not found, LINCAMedicationDispense cannot be created");

                return (false);
            }

            dispense.AuthorizingPrescription.Add(new()
            {
                Reference = $"LINCAPrescriptionMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLasix}"
            });

            dispense.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
            dispense.Subject = prescriptionRenateLasix!.Subject;
            dispense.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0031130",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Lasix 40 mg Tabletten"
                        }
                    }
                }
            };

            dispense.Quantity = new() { Value = 1 };

            dispense.DosageInstruction.Add(new Dosage()
            {
                Sequence = 1,
                Text = "1 Tablette täglich",
                Timing = new Timing()
                {
                    Repeat = new()
                    {
                        Bounds = new Duration
                        {
                            Value = 1,
                            Code = "d"
                        },
                        Frequency = 1,
                        Period = 1,
                        PeriodUnit = Timing.UnitsOfTime.D
                    }
                }
            });

            dispense.Performer.Add(new()
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

            dispense.Type = new()
            {
                Coding = new()
                {
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC")
                }
            };

            (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

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
