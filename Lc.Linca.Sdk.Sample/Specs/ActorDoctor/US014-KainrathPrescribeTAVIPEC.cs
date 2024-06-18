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

internal class US014_KainrathPrescribeTavipec : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered mobile caregiver client Peter Kainrath. 
        He has received a LINCA order position requesting medication prescription for him.
        He decides to issue a prescription for the medication for Peter Kainrath intended by that order position. 
        Hence, he submits a private prescription for that position
          and her software will send that to the LINCA server.";

    protected MedicationRequest prescription = new();

    public US014_KainrathPrescribeTavipec(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalTavipec = proposalsToPrescribe.Find(x => x.Subject.Display.Contains("Kainrath"));
            //MedicationRequest? orderProposalTavipec = proposalsToPrescribe.Find(x => x.Id.Equals("   "));  // ENTER ID STRING HERE
            
            if (orderProposalTavipec == null)
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Gertrude Steinmaier not found, or it was already processed, prescription cannot be created");

                return false;
            }

            prescription.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalTavipec.Id}"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = orderProposalTavipec!.Subject;
            prescription.Medication = orderProposalTavipec!.Medication;
            /*
            prescription.Medication = new()
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

            //prescription.DosageInstruction = orderProposalSimvastatin.DosageInstruction;
            prescription.DosageInstruction.Add(new Dosage()
            {
                Sequence = 1,
                Text = "1-1-1-0",
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

            prescription.DispenseRequest = new() { Quantity = new() { Value = 1 } };

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
