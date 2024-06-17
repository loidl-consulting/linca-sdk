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
using Hl7.Fhir.Support;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class Test008_Spitzmaus_CreatePrescriptionToPriorPrescription : Spec
{
    protected MedicationRequest prescription = new();

    public const string UserStory = @"
        It might be necessary to run Test007 with the certificate of Dr. Spitzmaus first. 
        Run this testcase with the certificate of Dr. Silvia Spitzmaus.";

    public Test008_Spitzmaus_CreatePrescriptionToPriorPrescription(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create prescription to prior prescription, LCVAL36, supportingInformation inconsistent", CreatePriorPrescriptionLCVAL36),
            new("Create prescription to prior prescription, LCVAL38, priorPrescription missing", CreatePriorPrescriptionLCVAL38),
            new("Create prescription to prior prescription, LCVAL39, refstring in priorPrescription not valid", CreatePriorPrescriptionLCVAL39),
            new("Create prescription to prior prescription, LCVAL40, refstring in priorPrescription not found", CreatePriorPrescriptionLCVAL40),
            new("Create prescription to prior prescription, LCVAL42, informationSource inconsistent", CreatePriorPrescriptionLCVAL42),
            new("Create prescription to prior prescription, LCVAL43, requester inconsistent", CreatePriorPrescriptionLCVAL43),
            new("Create prescription to prior prescription, LCVAL44, dispenser inconsistent", CreatePriorPrescriptionLCVAL44),
            new("Create prescription to prior prescription, LCVAL45, subject inconsistent", CreatePriorPrescriptionLCVAL45),
            new("Create prescription to prior prescription, LCVAL46, performer inconsistent", CreatePriorPrescriptionLCVAL46),
            new("Create prescription to prior prescription, LCVAL58, intent invalid", CreatePriorPrescriptionLCVAL58),
            new("Create prescription to prior prescription, LCVAL48, status invalid", CreatePriorPrescriptionLCVAL48),
            new("Create prescription to prior prescription successfully", CreatePriorPrescriptionSuccess),
            new("Create prescription to prior prescription, LCVAL41, priorPrescription not latest", CreatePriorPrescriptionLCVAL41),
        };
    }

    private bool CreatePriorPrescriptionLCVAL36()
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

            prescription.Medication = new() 
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

            prescription.SupportingInformation.Add(new() // Validation Error
            { 
                Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}" 
            }); 

            // prescription.Requester                   // will be copied from reference in priorPrescription
            // prescription.DispenseRequest.Dispenser   // will be copied from reference in priorPrescription, if available

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
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

            prescription.DispenseRequest = new() { Quantity = new() { Value = 1 } };

            (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
                Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
            }
            else
            {
                Console.WriteLine("Validation result:");
            }

            return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL36");
        }
        else 
        {
            Console.WriteLine($"Linca PrescriptionMedicationRequest for Guenter not found, run Test006 first");

            return false;
        }
    }

    private bool CreatePriorPrescriptionLCVAL38()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.PriorPrescription = null;
        prescription.SupportingInformation.Clear();

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL38");

    }

    private bool CreatePriorPrescriptionLCVAL39()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.PriorPrescription = new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest!!{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter}"
        };

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL39");

    }

    private bool CreatePriorPrescriptionLCVAL40()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.PriorPrescription = new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{Guid.NewGuid().ToFhirId()}"
        };

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL40");

    }

    private bool CreatePriorPrescriptionLCVAL42()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.PriorPrescription = new()
        {
            Reference = $"LINCAPrescriptionMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter}"
        };

        prescription.InformationSource.Add( new() { Identifier = new() { Value = "unknown"} });

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL42");

    }

    private bool CreatePriorPrescriptionLCVAL43()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.InformationSource.Clear();
        prescription.Requester = new() { Identifier = new() { Value = "Dr. House"} };

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL43");

    }

    private bool CreatePriorPrescriptionLCVAL44()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.Requester = null;
        prescription.DispenseRequest = new() 
        { 
            Dispenser = new() { Identifier = new() { Value = "Meine Hausapotheke" } },
            Quantity = new() { Value = 1}
        };

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL44");

    }

    private bool CreatePriorPrescriptionLCVAL45()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.DispenseRequest.Dispenser = null;
        prescription.DispenseRequest = new() { Quantity = new() { Value = 1 } };

        prescription.Subject = new ResourceReference()                             // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{Guid.NewGuid().ToFhirId()}"     
        };

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL45");

    }

    private bool CreatePriorPrescriptionLCVAL46()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.Subject = new ResourceReference()                             // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"
        };

        prescription.Performer.Clear();
        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = " "   // optional
        });

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL46");

    }

    private bool CreatePriorPrescriptionLCVAL58()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.Performer.Clear();
        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Silvia Spitzmaus"   // optional
        });

        prescription.Intent = MedicationRequest.MedicationRequestIntent.FillerOrder;

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL58");

    }

    private bool CreatePriorPrescriptionLCVAL48()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;
        prescription.Status = MedicationRequest.MedicationrequestStatus.OnHold;

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL48");

    }

    private bool CreatePriorPrescriptionSuccess()
    {
        prescription.Status = MedicationRequest.MedicationrequestStatus.Active;

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine($"Linca PrescriptionMedicationRequest transmitted, id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool CreatePriorPrescriptionLCVAL41()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created Linca PrescriptionMedicationRequest with id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL41");

    }
}
