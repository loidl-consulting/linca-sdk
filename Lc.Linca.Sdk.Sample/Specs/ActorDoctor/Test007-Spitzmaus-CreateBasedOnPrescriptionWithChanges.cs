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
using Hl7.Fhir.Support;
using Hl7.Fhir.Model.Extensions;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class Test007_Spitzmaus_CreateBasedOnPrescriptionWithChanges : Spec
{
    protected MedicationRequest prescription = new();

    public const string UserStory = @"
        It might be necessary to run Test000 with the certificate of Haus Vogelsang first. 
        Run this testcase with the certificate of Dr. Silvia Spitzmaus.";

    public Test007_Spitzmaus_CreateBasedOnPrescriptionWithChanges(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL37, basedOn not unique", PrescriptionRecordLCVAL37),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL31, basedOn refstring invalid", PrescriptionRecordLCVAL31),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL32, basedOn reference not found", PrescriptionRecordLCVAL32),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL09, subject required", PrescriptionRecordLCVAL09),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL45, subject inconsistent", PrescriptionRecordLCVAL45),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL46, performer inconsistent", PrescriptionRecordLCVAL46),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL42, informationSource inconsistent", PrescriptionRecordLCVAL42),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL34, informationSource not unique", PrescriptionRecordLCVAL34),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL43, requester inconsistent", PrescriptionRecordLCVAL43),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL44, dispenser inconsistent", PrescriptionRecordLCVAL44),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL07, intent required", PrescriptionRecordLCVAL07),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL47, intent invalid", PrescriptionRecordLCVAL47),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL08, status required", PrescriptionRecordLCVAL08),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL48, status invalid", PrescriptionRecordLCVAL48),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL36, supportingInformation inconsistent", PrescriptionRecordLCVAL36),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL75, dispenseRequest quantity is missing", PrescriptionRecordLCVAL75),
            new("Create PrescriptionMedicationRequest based on proposal successfully", CreatePrescriptionRecordSuccess),
            new("Create PrescriptionMedicationRequest based on proposal, LCVAL33, reference in basedOn not latest", PrescriptionRecordLCVAL33)
        };
    }

    private bool PrescriptionRecordLCVAL37()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposals = BundleHelper.FilterProposalsToPrescribe(orders);


            MedicationRequest? proposalsGuenterNotGranpidam = proposals.Find(x => x.Subject.Reference.Contains($"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}") 
                                                                                        && ! x.Medication.Concept.Coding.First().Display.Contains("Granpidam"));

            if (proposalsGuenterNotGranpidam != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter = proposalsGuenterNotGranpidam.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Guenter not found, prescription cannot be created");

                return false;
            }

            // Validation Error: add the basedOn Reference twice
            prescription.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
            });
            prescription.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
            });

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
                Text = "1x täglich auf die betroffene Stelle auftragen"
            });

            // prescription.InformationSource.Add(new ResourceReference()  // will be copied from reference in basedOn
            // prescription.Requester = new ResourceReference()  //will be copied from reference in basedOn
            // prescription.DispenseRequest.Dispenser // will be copied from reference in basedOn, if available

            prescription.DispenseRequest = new();
            prescription.DispenseRequest.DispenserInstruction.Add(new Annotation() { Text = "Ersatzweise Generikum abgeben" });
            prescription.DispenseRequest.ValidityPeriod = new()
            {
                Start = DateTime.Today.ToFhirDate(),
                End = DateTime.Today.AddMonths(1).ToFhirDate()
            };
            prescription.DispenseRequest.Quantity = new() { Value = 1 };

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

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

                BundleHelper.ShowOrderChains(results);
            }
            else
            {
                Console.WriteLine("Validation result:");
            }

            return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL37");
        }
        else
        {
            Console.WriteLine($"Failed to receive Linca ProposalMedicationRequest for Guenter, run Test000 first");

            return false;
        }
    }

    private bool PrescriptionRecordLCVAL31()
    {
        // Validation Error: refstring is invalid
        prescription.BasedOn.Clear();
        prescription.BasedOn.Add(new ResourceReference()
        {
            Reference = $"LINCAProposalMedicationRequest{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
        });

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL31");
    }

    private bool PrescriptionRecordLCVAL32()
    {
        // Validation Error: refstring is invalid
        prescription.BasedOn.Clear();
        prescription.BasedOn.Add(new ResourceReference()
        {
            Reference = $"LINCAProposalMedicationRequest/{Guid.NewGuid().ToFhirId()}"
        }); ;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL32");
    }

    private bool PrescriptionRecordLCVAL09()
    {
        // Validation Error: refstring is invalid
        prescription.BasedOn.Clear();
        prescription.BasedOn.Add(new ResourceReference()
        {
            Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
        });

        prescription.Subject = null;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL09");
    }

    private bool PrescriptionRecordLCVAL45()
    {

        prescription.Subject = new ResourceReference()  // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{Guid.NewGuid().ToFhirId()}"
            // Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"     // relative path to Linca Fhir patient resource, copy from order
        };

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL45");
    }

    private bool PrescriptionRecordLCVAL46()
    {

        prescription.Subject = new ResourceReference()  // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"     // relative path to Linca Fhir patient resource, copy from order
        };

        prescription.Performer.Clear();
        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Validation Error
            },
            Display = "Vertretung von Dr. Silvia Spitzmaus"   
        });

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL46");
    }

    private bool PrescriptionRecordLCVAL42()
    {
        prescription.Performer.Clear();
        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Silvia Spitzmaus"   // 
        });

        prescription.InformationSource.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Silvia Spitzmaus"   // Validation Error
        });

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL42");
    }

    private bool PrescriptionRecordLCVAL34()
    {
        // Validation error: add a second entry to InformationSource
        prescription.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL34");
    }

    private bool PrescriptionRecordLCVAL43()
    { 
        prescription.InformationSource.Clear();  // it will be copied from basedOn

        prescription.Requester = new ResourceReference()  // validation error
        {
            Identifier = new()
            {
                Value = "ECHT_JETZT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "Walter"
        };


        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL43");
    }

    private bool PrescriptionRecordLCVAL44()
    {
        prescription.Requester = null; // it will be copied from basedOn

        prescription.DispenseRequest = new()
        {
            Dispenser = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.3",  // OID of designated pharmacy
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Die andere Apotheke"
            },
            Quantity = new() { Value = 1}
        };


        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL44");
    }

    private bool PrescriptionRecordLCVAL07()
    {
        prescription.DispenseRequest = null; // will be copied from basedOn if available
        prescription.DispenseRequest = new() { Quantity = new() { Value = 1} };

        prescription.Intent = null;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL07");
    }

    private bool PrescriptionRecordLCVAL47()
    {
        prescription.Intent = MedicationRequest.MedicationRequestIntent.ReflexOrder;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL47");
    }

    private bool PrescriptionRecordLCVAL08()
    {
        prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;
        prescription.Status = null;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL08");
    }

    private bool PrescriptionRecordLCVAL48()
    {
        prescription.Status = MedicationRequest.MedicationrequestStatus.OnHold;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL48");
    }

    private bool PrescriptionRecordLCVAL36()
    {
        prescription.Status = MedicationRequest.MedicationrequestStatus.Active;

        prescription.SupportingInformation.Add( new() { Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}" });

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL36");
    }

    private bool PrescriptionRecordLCVAL75()
    {
        prescription.SupportingInformation.Clear();
        prescription.Performer.First().Display = "";

        prescription.DispenseRequest.Quantity.Value = null;

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL75");
    }

    private bool CreatePrescriptionRecordSuccess()
    {
            prescription.DispenseRequest.Quantity = new() { Value = 2 };

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

            if (canCue)
            {
                Console.WriteLine($"Linca PrescriptionMedicationRequestBundle transmitted, created Linca PrescriptionMedicationRequest");
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter = results.Entry.First().Resource.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

                BundleHelper.ShowOrderChains(results);
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
            }

            OutcomeHelper.PrintOutcome(outcome);

            return canCue;
    }

    private bool PrescriptionRecordLCVAL33()
    {
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL33");
    }
}
