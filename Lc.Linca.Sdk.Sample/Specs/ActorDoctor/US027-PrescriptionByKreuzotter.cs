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

internal class US027_PrescriptionByKreuzotter : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter.";

    protected MedicationRequest prescription = new();

    public US027_PrescriptionByKreuzotter(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            /*
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalRenate = proposalsToPrescribe.Find(x => x.Subject.Display.Contains("Klient 5") && x.Medication.Concept.Coding.First().Display.Contains("THOMAPYRIN"));
            
            if (orderProposalRenate != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdRenateLasix = orderProposalRenate.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Renate Rüssel-Olifant not found, or it was already processed, prescription cannot be created");

                return false;
            }
            */

            prescription.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/7646f5bb482446f9975dfb9e051d1761"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED

            //prescription.Subject = orderProposalRenate!.Subject;
            prescription.Subject = new() { Reference = "HL7ATCorePatient/073a032a25934f88bccfb16a5c5af709" };


            prescription.Medication = new()
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

            /*
            prescription.Reason = new()
            {
                new()
                {
                    Concept = new() { Text = "Dosis beibehalten, mehr Flüssigkeit verabreichen"}
                }
            };
            */

            prescription.DosageInstruction.Add(new Dosage()
            {
                Text = "1-0-0-1",
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
            });

            /*
            prescription.Identifier.Add(new Identifier()
            {
                Value = "XYZ1ABC2UVW3",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"    // OID: eMed-Id
            });

            prescription.GroupIdentifier = new()
            {
                Value = "ASDFGHJ4KL98",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
            };
            */

            prescription.DispenseRequest = new()
            {
                Quantity = new() { Value = 3 },
                //DispenserInstruction = new() { new() { Text = "Information für den Apotheker"} }
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
                Console.WriteLine($"Linca PrescriptionMedicationRequestBundle transmitted, created Linca PrescriptionMedicationRequests");

                BundleHelper.ShowOrderChains(results);  
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequestBundle");
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
            Console.WriteLine($"Failed to receive ProposalMedicationRequests");

            return false;
        }
    }
}
