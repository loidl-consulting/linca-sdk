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

internal class US020_DispenseSIMVASTATINCancelResend : Spec
{
    protected MedicationDispense dispense1 = new();
    protected MedicationDispense dispense2 = new();
    protected string? dispenseId;

    public const string UserStory = @"
        Pharmacist Mag. Franziska Fröschl, owner of the pharmacy Apotheke 'Klappernder Storch' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When she is expected to fullfil medication orders for a customer, e.g., Gertrude Steinmaier, 
        and she has a LINCA order Id to go with a purchase her care giver just made for her, 
        then Mag. Fröschl submits a dispense record for the order position in question. She recognizes a mistake, 
        cancels the created medication dispense and submit another dispense with adjused quantity, 
          and her software will send that to the LINCA server.";

    public US020_DispenseSIMVASTATINCancelResend(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create MedicationDispense", CreateMedicationDispenseRecord),
            //new("Cancel MedicationDispense", CancelMedicationDispenseRecord),
            //new("Create replacement MedicationDispense", CreateReplacementMedicationDispenseRecord),
        };

    }

    private bool CreateMedicationDispenseRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "ZZZZXXXXYYYY");

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            MedicationRequest? prescriptionSimvastatin = prescriptionsToDispense.Find(x => x.Medication.Concept.Coding.First().Code.Equals("3517502"));

            if (prescriptionSimvastatin == null)
            { 
                Console.WriteLine("Linca PrescriptionMedicationRequest for Gertrude Steinmaier not found, LINCAMedicationDispense cannot be created");

                return (false);
            }

            dispense1.AuthorizingPrescription.Add(new()
            {
                Reference = $"LINCAPrescriptionMedicationRequest/{prescriptionSimvastatin.Id}"
            });

            dispense1.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
            dispense1.Subject = prescriptionSimvastatin.Subject;
            dispense1.Medication = prescriptionSimvastatin.Medication;
            /*
            dispense1.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "3517502",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "SIMVASTATIN ACT FTBL 80MG"
                        }
                    }
                }
            };
            */

            dispense1.Quantity = new() { Value = 3 };

            dispense1.DosageInstruction = prescriptionSimvastatin.DosageInstruction;

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
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC")
                }
            };

            (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense1);

            if (canCue)
            {
                Console.WriteLine($"Linca MedicationDispense transmitted, id {postedMD.Id}");
                dispenseId = postedMD.Id;
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

    private bool CancelMedicationDispenseRecord()
    {
        (var outcome, var deleted) = LincaDataExchange.DeleteMedicationDispense(Connection, dispenseId!);
        
        if (deleted)
        {
            Console.WriteLine($"LINCA Medication Dispense id '{dispenseId}' successfully cancelled");

            return true;
        }
        else 
        {
            Console.WriteLine("Failed to delete LINCA Medication Dispense");

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return false;
        }
    }

    private bool CreateReplacementMedicationDispenseRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "ZZZZXXXXYYYY");

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            MedicationRequest? prescriptionSimvastatin = prescriptionsToDispense.Find(x => x.Medication.Concept.Coding.First().Code.Equals("3517502"));

            if (prescriptionSimvastatin == null)
            {
                Console.WriteLine("Linca PrescriptionMedicationRequest for Gertrude Steinmaier not found, LINCAMedicationDispense cannot be created");

                return (false);
            }

            dispense2.AuthorizingPrescription.Add(new()
            {
                Reference = $"LINCAPrescriptionMedicationRequest/{prescriptionSimvastatin.Id}"
            });

            dispense2.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
            dispense2.Subject = prescriptionSimvastatin.Subject;
            dispense2.Medication = prescriptionSimvastatin.Medication;
            /*
            dispense2.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "3517502",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "SIMVASTATIN ACT FTBL 80MG"
                        }
                    }
                }
            };
            */

            dispense2.Quantity = new() { Value = 1 };

            dispense2.Performer.Add(new()
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

            dispense2.Type = new()
            {
                Coding = new()
                {
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC")
                }
            };

            (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense2);

            if (canCue)
            {
                Console.WriteLine($"Linca MedicationDispense transmitted, id {postedMD.Id}");
                dispenseId = postedMD.Id;
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
