﻿/***********************************************************************************
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
using Hl7.Fhir.Support;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class Test010_FrueherVogel_CreateDispense : Spec
{
    protected MedicationDispense dispense = new();
    protected MedicationRequest? prescriptionGuenterUltralan;

    public const string UserStory = @"
        First, run Test007 and Test008 with the certificate of Dr. Spitzmaus. 
        Run this testcase with the certificate of Apotheke 'Zum frühen Vogel'. ";

    public Test010_FrueherVogel_CreateDispense(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create MedicationDispense, LCVAL49, cast failed", CreateMedicationDispenseLCVAL49),
            new("Create MedicationDispense, LCVAL50, authorizingPrescription is missing", CreateMedicationDispenseLCVAL50A),
            new("Create MedicationDispense, LCVAL50, authorizingPrescription is not unique", CreateMedicationDispenseLCVAL50B),
            new("Create MedicationDispense, LCVAL51, refstring in authorizingPrescription not valid", CreateMedicationDispenseLCVAL51),
            new("Create MedicationDispense, LCVAL52, reference in authorizingPrescription not found", CreateMedicationDispenseLCVAL52),
            new("Create MedicationDispense, LCVAL54, status invallid", CreateMedicationDispenseLCVAL54),
            new("Create MedicationDispense, LCVAL29, performer missing", CreateMedicationDispenseLCVAL29),
            new("Create MedicationDispense, LCVAL57, performer.actor.value missing", CreateMedicationDispenseLCVAL57),
            new("Create MedicationDispense, LCVAL66, performer OID wrong", CreateMedicationDispenseLCVAL66),
            new("Create MedicationDispense, LCVAL26, medication missing", CreateMedicationDispenseLCVAL26),
            new("Create MedicationDispense, LCVAL45, subject invalid", CreateMedicationDispenseLCVAL45),
            new("Create MedicationDispense successfully", CreateMedicationDispenseSuccess),
            new("Create MedicationDispense, LCVAL53, authorizingPrescription is not latest", CreateMedicationDispenseLCVAL53)
        };
    }

    private bool CreateMedicationDispenseLCVAL49()
    {
        var patient = new Patient();

        (var postedMD, var canCue, var outcome) = LincaDataExchange.PostPatientToDispenseEndpoint(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL50A()
    {
        (Bundle orders, bool received) = LincaDataExchange.GetPrescriptionsToDispense(Connection);

        if (received)
        {
            List<MedicationRequest> prescriptionsToDispense = BundleHelper.FilterPrescriptionsToDispense(orders);

            prescriptionGuenterUltralan = prescriptionsToDispense.Find(x => x.Medication.Concept.Coding.First().Display.Contains("Ultralan"));

            if (prescriptionGuenterUltralan != null)
            {
                dispense.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
                dispense.Subject = prescriptionGuenterUltralan!.Subject;
                dispense.Medication = new()
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

                dispense.Performer.Add(new()
                {
                    Actor = new()
                    {
                        Identifier = new()
                        {
                            Value = "2.999.40.0.34.5.1.2",  // OID of dispensing pharmacy
                            System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                        }
                    }
                });

                dispense.Type = new()
                {
                    Coding = new()
                {
                    new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC")
                }
                };
            }
            else
            {
                Console.WriteLine("Linca PrescriptionMedicationRequest for Günter Gürtelthier not found, LINCAMedicationDispense cannot be created");

                return (false);
            }

            (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
                Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
            }
            else
            {
                Console.WriteLine("Validation result:");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;
        }
        else
        {
            Console.WriteLine($"Failed to receive Linca Prescription Medication Requests");

            return false;
        }
    }

    private bool CreateMedicationDispenseLCVAL50B()
    {
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{prescriptionGuenterUltralan!.Id}"
        });
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{Guid.NewGuid().ToFhirId()}"
        });

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL51()
    {
        dispense.AuthorizingPrescription.Clear();
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest.{prescriptionGuenterUltralan!.Id}"
        });

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL52()
    {
        dispense.AuthorizingPrescription.Clear();
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{Guid.NewGuid().ToFhirId()}"
        });

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL54()
    {
        dispense.AuthorizingPrescription.Clear();
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{prescriptionGuenterUltralan!.Id}"
        });

        dispense.Status = MedicationDispense.MedicationDispenseStatusCodes.InProgress;

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL29()
    {
        dispense.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;

        dispense.Performer.First().Actor = null;

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL57()
    {
        dispense.Performer.Clear();
        dispense.Performer.Add(new()
        {
            Actor = new()
            {
                Identifier = new()
                {
                    // missing OID of dispensing  pharmacy in Value
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                }
            }
        });

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL66()
    {
        dispense.Performer.Clear();
        dispense.Performer.Add(new()
        {
            Actor = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.1",  // wrong OID of dispensing pharmacy
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                }
            }
        });

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL26()
    {
        dispense.Performer.Clear();
        dispense.Performer.Add(new()
        {
            Actor = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.2",  // wrong OID of dispensing pharmacy
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                }
            }
        });

        dispense.Medication = null;

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseLCVAL45()
    {
        dispense.Medication = new()
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

        dispense.Subject = new ResourceReference { Reference = $"{LincaEndpoints.HL7ATCorePatient}/{Guid.NewGuid().ToFhirId()}" };

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool CreateMedicationDispenseSuccess()
    {
        dispense.Subject = prescriptionGuenterUltralan!.Subject;

        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            // Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Failed to create MedicationDispense");
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

    private bool CreateMedicationDispenseLCVAL53()
    {
        (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca MedicationDispense with id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

}
