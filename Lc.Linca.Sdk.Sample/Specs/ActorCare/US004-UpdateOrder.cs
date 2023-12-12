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
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US004_UpdateOrder : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to modify details of that order, in particular he wants to update one 
        individual order position for his client Günter Gürtelthier.
        The LINCA systems prevents Walter Specht from updating such a position 
        if Günter's practitioner, Dr. Silvia Spitzmaus, has already issued a prescription for that order position";

    protected MedicationRequest medReq1 = new();

    public US004_UpdateOrder(LincaConnection conn) : base(conn) {

        Steps = new Step[]
        {
            new("Post ProposalMedicationRequest with update informations", PostProposalMedicationRequestUpdate)
        };

    }

    private bool PostProposalMedicationRequestUpdate()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle results, bool received) = LincaDataExchange.GetProposalStatus(Connection, $"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang}");

        if (received)
        {
            List<MedicationRequest> proposals = BundleHelper.FilterProposalsToPrescribe(results);

            MedicationRequest? proposalForUpdate = proposals.Find(x => x.Medication.Concept.Coding.First().Display.Contains("Effortil") && x.Subject.Reference.Contains($"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"));

            if (proposalForUpdate != null )
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter = proposalForUpdate.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for update not found, it might have been already processed");

                return false;
            }
           
            // post order medication request for Günter Gürtelthier based on an existing order medication request
            // Medication and Dispenser are updated
            medReq1.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
            });

            medReq1.Status = MedicationRequest.MedicationrequestStatus.Active;      // REQUIRED
            medReq1.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
            medReq1.Subject = new ResourceReference()                                // REQUIRED
            {
                Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"     // relative path to Linca Fhir patient resource
            };

            medReq1.Medication = new()
            {
                Concept = new()
                {
                    Coding = new()
                    {
                        new Coding()
                        {
                            Display = "Eine Salbe, die in der Apotheke angemischt wird"
                        }
                    }
                }
            };

            medReq1.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                    System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            });

            medReq1.Requester = new ResourceReference()  // REQUIRED
            {
                Identifier = new()
                {
                    Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                    System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
                },
                Display = "DGKP Walter Specht"
            };

            medReq1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            medReq1.DispenseRequest = new()
            {
                Dispenser = new()
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.5.1.3",  // OID of designated pharmacy
                        System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                    },
                    Display = "Apotheke 'Zum Linden Wurm'"
                }
            };

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, medReq1);

            if (canCue)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.UpdateOrderProposalGuenter = postedOMR.Id;
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
