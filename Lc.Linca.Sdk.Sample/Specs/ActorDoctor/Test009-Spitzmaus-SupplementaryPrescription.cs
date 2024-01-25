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

internal class Test009_Spitzmaus_SupplementaryPrescription : Spec
{
    public const string UserStory = @"
        Run this test with the certificate of Dr. Silvia Spitzmaus";

    MedicationRequest? proposal;
    MedicationRequest prescription = new();
    MedicationRequest adhoc = new();

    public Test009_Spitzmaus_SupplementaryPrescription(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Create a prescription plus supplementary prescription Bundle, LCVAL60, supportingInformation missing", CreateAdhocPrescriptionLCVAL60),
            new ("Create a prescription plus supplementary prescription Bundle, LCVAL61, supportingInformation wrong", CreateAdhocPrescriptionLCVAL61)
        };
    }

    private bool CreateAdhocPrescriptionLCVAL60()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposals = BundleHelper.FilterProposalsToPrescribe(orders);

            proposal = proposals.Find(
                x =>
                x.Subject.Reference.Contains($"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}")
                && x.Medication.Concept.Coding.First().Display.Contains("Granpidam")
            );

            if (proposal == null)
            {
                Console.WriteLine($"Failed to receive Linca ProposalMedicationRequest for Guenter");

                return false;
            }

            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            /******* Create prescription for proposal ******/
            prescription.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{proposal.Id}"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = proposal.Subject;

            prescription.SupportingInformation = proposal.SupportingInformation;

            prescription.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "4460951",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Granpidam 20 mg Filmtabletten"
                        }
                    }
                }
            };

            prescription.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-1-0"
            });

            // prescription.InformationSource.Add(new ResourceReference()  // will be copied from reference in basedOn
            // prescription.Requester = new ResourceReference()  //will be copied from reference in basedOn
            // prescription.DispenseRequest.Dispenser // will be copied from reference in basedOn, if available

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            prescription.DispenseRequest = new()
            {
                Dispenser = new()
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.5.1.2",  // OID of designated pharmacy
                        System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                    },
                    Display = "Apotheke 'Zum frühen Vogel'"
                }
            };

            prescription.Identifier.Add(new Identifier()
            {
                Value = "CVF1 23ER 12VV",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"     // OID: eMed-Id
            });

            prescription.GroupIdentifier = new()
            {
                Value = "1A2B 3C4D 5E6F",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"        // OID: Rezeptnummer
            };

            /****** Create Adhoc prescription ******/

            adhoc.Status = MedicationRequest.MedicationrequestStatus.Active;             // REQUIRED
            adhoc.Intent = MedicationRequest.MedicationRequestIntent.OriginalOrder;      // REQUIRED
            adhoc.Subject = proposal.Subject;

            adhoc.SupportingInformation = null; //LCVAL60
            //adhoc.SupportingInformation.Add(new() { Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}" }); // LCVAL61
            //adhoc.SupportingInformation = proposal.SupportingInformation;

            adhoc.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "1292648",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Pantoloc 40 mg Filmtabletten"
                        }
                    }
                }
            };

            adhoc.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-1-0"
            });

            adhoc.InformationSource.Add(new ResourceReference()   // in adhoc prescriptions the informationSource is the prescribing practitioner
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            adhoc.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            adhoc.Identifier.Add(new Identifier()
            {
                Value = "CVF1 23ER 12VV",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"     // OID: eMed-Id
            });

            adhoc.GroupIdentifier = new()
            {
                Value = "1A2B 3C4D 5E6F",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"        // OID: Rezeptnummer
            };

            /***** Add both to one Bundle ******/

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
            prescriptions.AddResourceEntry(adhoc, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected, but a Bundle was returned:");

                BundleHelper.ShowOrderChains(results);
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
            Console.WriteLine($"Failed to receive Linca ProposalMedicationRequest for Guenter");

            return false;
        }
    }

    private bool CreateAdhocPrescriptionLCVAL61()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        /****** Create Adhoc prescription ******/

        adhoc.SupportingInformation.Add(new() { Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}" }); // LCVAL61
                                            //adhoc.SupportingInformation = proposal.SupportingInformation;
        
        /***** Add both to one Bundle ******/

        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
        prescriptions.AddResourceEntry(adhoc, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected, but a Bundle was returned:");

            BundleHelper.ShowOrderChains(results);
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
