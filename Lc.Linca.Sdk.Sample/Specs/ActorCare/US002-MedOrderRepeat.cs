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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US002_MedOrderRepeat : Spec
{
    protected Patient createdPatient = new();
    protected MedicationRequest medReq = new();

    public const string UserStory = @"
        User Susanne Allzeit (DGKP) is an employee at the mobile caregiver 
        organization Pflegedienst Immerdar, whose client, Renate Rüssel-Olifant, is 
        already registered as patient in the LINCA system. 
        Susanne Allzeit needs to re-stock prescription medication for Renate Rüssel-Olifant. 
        Hence, she places an order on LINCA referring to the existing patient 
        record of Renate Rüssel-Olifant. 
        Additionally, she specifies her preferred pharmacy, Apotheke 'Klappernder Storch', in advance 
        to collect the order there. ";

    public US002_MedOrderRepeat(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Place order with pharmacy specified", CreateRequestOrchestrationRecord)
        };
    }

    private bool CreateRequestOrchestrationRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        var patientId = LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdRenate;

        if (!string.IsNullOrEmpty(patientId)) // in UserStory002 we assume that this has already been done
        {
            // first prepare a LincaOrderMedicationRequest to be contained in the LincaRequestOrchestration
            PrepareOrderMedicationRequest(patientId);

            RequestOrchestration ro = new()
            {
                Status = RequestStatus.Active,      // REQUIRED
                Intent = RequestIntent.Proposal,       // REQUIRED
                Subject = new ResourceReference()   // REQUIRED
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                        System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                    },
                    Display = "Pflegedienst Immerdar"   // optional
                }
            };

            ro.Contained.Add(medReq);

            var action = new RequestOrchestration.ActionComponent()
            {
                Type = new(),
                Resource = new ResourceReference($"#{medReq.Id}")
            };

            action.Type.Coding.Add(new() { Code = "create" });

            ro.Action.Add(action);

            (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestration(Connection, ro);

            if (canCue)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdImmerdar002 = createdRO.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

                Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca Request Orchestration");
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
            Console.WriteLine($"No patient information stored in care information system");

            return false;
        }
    }

    private void PrepareOrderMedicationRequest(string patientId)
    {
        medReq.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq.Subject = new ResourceReference()                                // REQUIRED
        {
            // relative path to Linca Fhir patient resource
            Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdRenate}"
        };

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
    }
}
