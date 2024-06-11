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

internal class US028_TwoPrivatePrescriptionsSpitzmaus : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Spitzmaus is responsible for the LINCA registered client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him.
        shee decides to issue a prescription for the medication for Günter intended by that order positions. 
        Shee submits a private prescription for two proposals without eMedId and eRezeptId
          and his software will send that to the LINCA server,
          and the ordering caregiver organization Haus Vogelsang will be informed that the order position has been prescribed as ordered.";

    protected MedicationRequest prescription1 = new();
    protected MedicationRequest prescription2 = new();

    public US028_TwoPrivatePrescriptionsSpitzmaus(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecords)
            };
    }

    private bool CreatePrescriptionRecords()
    {
        // LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalGünter1 = proposalsToPrescribe.Find(x => x.Id.Equals("e6aa9e4da0234d5d8218a1aac2714c62"));
            MedicationRequest? orderProposalGünter2 = proposalsToPrescribe.Find(x => x.Id.Equals("65911d16df7e4ee39cb0c663b8d48b77"));

            if (orderProposalGünter1 == null || orderProposalGünter2 == null)
            {
                Console.WriteLine($"Linca ProposalMedicationRequests for Günter not found, or it was already processed, prescription cannot be created");

                return false;
            }

            prescription1.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalGünter1.Id}"
            });

            prescription1.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription1.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription1.Subject = orderProposalGünter1!.Subject;
            prescription1.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0018589",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Effortil 7,5 mg/ml - Tropfen"
                        }
                    }
                }
            };

            prescription1.DosageInstruction.Add(new Dosage()
            {
                Text = "10 - 0 - 0 - 0"
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                }
            });

            prescription1.DispenseRequest = new() { Quantity = new() { Value = 1 } };

        /*********************************************************************************************/

            prescription2.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalGünter2.Id}"
            });

            prescription2.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription2.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription2.Subject = orderProposalGünter2!.Subject;
            prescription2.Medication = new()
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

            prescription2.DosageInstruction.Add(new Dosage()
            {
                Text = "1 - 0 - 1 - 0"
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                }
            });

            prescription2.DispenseRequest = new() { Quantity = new() { Value = 2 } };

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
            prescriptions.AddResourceEntry(prescription2, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

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
