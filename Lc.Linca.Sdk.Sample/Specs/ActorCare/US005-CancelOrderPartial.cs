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

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US005_CancelOrder : Spec
{
    protected MedicationRequest medReq2 = new MedicationRequest();

    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to cancel individual order positions for his client Patrizia Platypus.
        He submits updates on those positions, providing a reason for cancellation, such as 'ordered by mistake', 
        and sets their status to 'cancelled'. 
        The LINCA systems prevents Walter Specht from submitting such cancellations
        if Patrizia's practitioner, Dr. Kunibert Kreuzotter, has already issued a prescription for the original order position";

    public US005_CancelOrder(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Post ProposalMedicationRequest for cancellation", PostProposalMedicationRequestCancel)
        };
    }

    private bool PostProposalMedicationRequestCancel()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle results, bool received) = LincaDataExchange.GetProposalStatus(Connection, $"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang}");

        if (received)
        {
            List<MedicationRequest> proposals = new List<MedicationRequest>();

            foreach (var item in results.Entry)
            {
                if (item.FullUrl.Contains("LINCAProposal"))
                {
                    proposals.Add((item.Resource as MedicationRequest)!);
                }
            }

            LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdPatrizia = proposals.Find(x => x.Subject.Reference.Contains($"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdPatrizia}"))!.Id;
            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            // post order medication request for Patrizia Platypus based on an existing order medication request
            // set Status to cancelled 
            medReq2.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdPatrizia}"
            });

            medReq2.Status = MedicationRequest.MedicationrequestStatus.Cancelled;    // REQUIRED
            medReq2.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
            medReq2.Subject = new ResourceReference()                                // REQUIRED
            {
                Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdPatrizia}"     // relative path to Linca Fhir patient resource
            };

            medReq2.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0028903",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Isoptin 80 mg - Dragees"
                        }
                    }
                }
            };

            medReq2.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            });

            medReq2.Requester = new ResourceReference()  // REQUIRED
            {
                Identifier = new()
                {
                    Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                    System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
                },
                Display = "DGKP Walter Specht"
            };

            medReq2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                },
                Display = "Dr. Kunibert Kreuzotter"   // optional
            });

            medReq2.DispenseRequest = new()
            {
                Dispenser = new()
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.5.1.2",  // OID of designated pharmacy
                        System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                    },
                    Display = "Apotheke 'Zum frühen Vogel'"
                }
            };

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, medReq2);

            if (canCue)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.CancelledOrderProposalPatricia = postedOMR.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

                Console.WriteLine($"Linca ProposalMedicationRequest transmitted, id {postedOMR.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca ProposalMedicationRequest");
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
            Console.WriteLine($"Failed to retrieve id of ProposalMedicationRequest for update");

            return false;
        }
    }
}
