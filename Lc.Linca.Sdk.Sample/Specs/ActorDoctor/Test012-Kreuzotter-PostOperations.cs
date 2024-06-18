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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class Test012_Kreuzotter_PostOperations : Spec
{
    public const string UserStory = @"
        Run this test with the certificate of Dr. Kreuzotter.";

    protected MedicationRequest prescription = new();

    public Test012_Kreuzotter_PostOperations(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Post prescriptions Bundle to empty string", PostPrescriptionBundleToEmptyString),
            new("Post prescriptions Bundle to dollar sign", PostPrescriptionBundleToDollarSign),
            new("Post prescriptions Bundle to $test-operation", PostPrescriptionBundleToTestOperation),
            new("Post prescriptions Bundle to AuditEvent", PostPrescriptionBundleToAuditEvent),
            new("Post prescriptions Bundle to LINCAAuditEvent", PostPrescriptionBundleToLINCAAuditEvent),
            new("Post prescriptions Bundle successfully", PostPrescriptionBundleSuccess),
            };
    }

    private bool PostPrescriptionBundleToEmptyString()
    {
        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalRenate = proposalsToPrescribe.Find(x => x.Subject.Display.Contains("Renate") && x.Medication.Concept.Coding.First().Display.Contains("Bisoprolol"));
            
            if (orderProposalRenate == null)
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Renate Rüssel-Olifant not found, or it was already processed, prescription cannot be created");

                return false;
                
            }

            prescription.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalRenate.Id}"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = orderProposalRenate!.Subject;
            prescription.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                {
                    new Coding()
                    {
                        Code = "2420396",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "Bisoprolol Arcana 5 mg Filmtabletten"
                    }
                }
                }
            };

            prescription.DosageInstruction.Add(new Dosage()
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
                            Code = "d",

                        },
                        Frequency = 1,
                        Period = 1,
                        PeriodUnit = Timing.UnitsOfTime.D
                    }
                },
                DoseAndRate = new()
                {
                    new Dosage.DoseAndRateComponent()
                    {
                        Dose = new Quantity(value: 1, unit: "St")
                    }
                }
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Dr. Kunibert Kreuzotter"   // optional
            });

            prescription.Identifier.Add(new Identifier()
            {
                Value = "XYZ1 ABC2 UVW3",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"    // OID: eMed-Id
            });

            prescription.GroupIdentifier = new()
            {
                Value = "ASDF GHJ4 KL34",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
            };

            prescription.DispenseRequest = new()
            {
                Quantity = new() { Value = 1 }
            };

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, "");

            if (canCue)
            {
                Console.WriteLine("Error: POST '' succeded, resulting Bundle:");

                BundleHelper.ShowOrderChains(results);  
            }
            else
            {
                Console.WriteLine($"POST '' failed, this is the expected outcome");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return ! canCue;
        }
        else
        {
            Console.WriteLine($"Failed to receive ProposalMedicationRequests");

            return false;
        }
    }

    private bool PostPrescriptionBundleToDollarSign()
    {
            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, "$");

            if (canCue)
            {
                Console.WriteLine("Error: POST '$' succeded, resulting Bundle:");

                BundleHelper.ShowOrderChains(results);
            }
            else
            {
                Console.WriteLine($"POST '$' failed, this is the expected outcome");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return ! canCue;
    }

    private bool PostPrescriptionBundleToTestOperation()
    {
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, "$test-operation");

        if (canCue)
        {
            Console.WriteLine("Error: POST '$test-operation' succeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("POST '$test-operation' failed, this is the expected outcome");
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

    private bool PostPrescriptionBundleToAuditEvent()
    {
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, "AuditEvent");

        if (canCue)
        {
            Console.WriteLine("Error: POST 'AuditEvent' succeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("POST 'AuditEvent' failed, this is the expected outcome");
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

    private bool PostPrescriptionBundleToLINCAAuditEvent()
    {
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, "LINCAAuditEvent");

        if (canCue)
        {
            Console.WriteLine("Error: POST 'LINCAAuditEvent' succeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("POST 'LINCAAuditEvent' failed, this is the expected outcome");
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

    private bool PostPrescriptionBundleSuccess()
    {
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.PostToAnyOperationOrResourceName(Connection, prescriptions, LincaEndpoints.prescription);

        if (canCue)
        {
            Console.WriteLine("POST $prescription Bundle succeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("POST $prescription Bundle failed");
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
}
