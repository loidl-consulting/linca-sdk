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
using System.Drawing.Text;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US010_CancelPrescriptionPosition : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered care giver client Renate Rüssel-Olifant. 
        He has received a LINCA order position requesting medication prescription for her.
        He decides that Renate Rüssel-Olifant shall no longer take the medication intended by that order position. 
        Hence, he submits an update on that order position with the status set to 'stopped' or 'ended',
          and his software will send that to the LINCA server,
          and the ordering care giver organization Pflegedienst Immerdar will be informed that this position will not be prescribed further on, 
          and their software system will inform Susanne Allzeit(DGKP)";

    protected MedicationRequest medReq = new();

    public US010_CancelPrescriptionPosition(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Stop the intake of an ordered medication", SetProposalStatusEnded )
        };


    }

    private bool SetProposalStatusEnded()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle results, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

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

            MedicationRequest? bisoprololForRenate = proposals.Find(x => x.Medication.Concept.Coding.First().Display.Contains("Bisoprolol"));

            if (bisoprololForRenate != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdRenateAtKreuzotter = bisoprololForRenate.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine("Order proposal for Renate Rüssel-Olifant (Med. Bisoprolol) not found");

                return false;
            }
            
            medReq.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdRenateAtKreuzotter}"
            });

            medReq.Status = MedicationRequest.MedicationrequestStatus.Stopped;    // REQUIRED
            medReq.Intent = MedicationRequest.MedicationRequestIntent.Order;      // REQUIRED
            medReq.Subject = bisoprololForRenate.Subject;                           

            medReq.Medication = new()
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

            medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Pflegedienst Immerdar"   // optional
            });

            medReq.Requester = new ResourceReference()  // REQUIRED
            {
                Identifier = new()
                {
                    Value = "ALLZEIT_BEREIT",               // e.g., org internal username or handsign of Susanne Allzeit
                    System = "urn:oid:2.999.40.0.34.1.1.3"  // Code-System: Care-Org Pflegedienst Immerdar
                },
                Display = "DGKP Susanne Allzeit"
            };

            medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Kunibert Kreuzotter"   // optional
            });

            medReq.DispenseRequest = new()
            {
                Dispenser = new()
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.5.1.1",  // OID of designated pharmacy
                        System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                    },
                    Display = "Apotheke 'Klappernder Storch'"
                }
            };

            (var postedPMR, var canCue) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, medReq);

            if (canCue)
            {
                Console.WriteLine($"Linca PrescriptionMedicationRequest (with status:ended) transmitted, id {postedPMR.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
            }

            return canCue;
        }
        else
        {
            Console.WriteLine($"Failed to retrieve id of ProposalMedicationRequest for Renate Rüssel-Olifant");

            return false;
        }
    }
}
