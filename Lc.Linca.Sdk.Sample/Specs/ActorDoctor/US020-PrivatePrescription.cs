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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US020_PrivatePrescription : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered client Klient 1. 
        He has received a LINCA order position requesting medication prescription for him.
        He decides to issue a prescription for the medication for Klient 1 intended by that order position. 
        He submits a private prescription for that position without eMedId and eRezeptId
          and his software will send that to the LINCA server,
          and the ordering caregiver organization Haus Vogelsang will be informed that the order position has been prescribed as ordered.";

    protected MedicationRequest prescription = new();

    public US020_PrivatePrescription(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        // LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposalsToPrescribe = BundleHelper.FilterProposalsToPrescribe(orders);

            MedicationRequest? orderProposalKlient1 = proposalsToPrescribe.Find(x => x.Id.Equals("cb19359ee44941cfbebb2f017a55d66c"));
            
            if (orderProposalKlient1 == null)
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Klient 1 not found, or it was already processed, prescription cannot be created");

                return false;
            }

            prescription.BasedOn.Add(new()
            {
                Reference = $"LINCAProposalMedicationRequest/{orderProposalKlient1.Id}"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = orderProposalKlient1!.Subject;
            prescription.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "5571902",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "ABOCA GRINTUSS HUSTENSAFT"
                        }
                    }
                }
            };

            prescription.DosageInstruction.Add(new Dosage()
            {
                Text = "1 - 0 - 0 - 0"
            });

            // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
            // prescription.Requester will be copied from resource in basedOn by the Fhir server
            // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                }
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
