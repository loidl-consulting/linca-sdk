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
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US021_MedOrderSingleArticleWithPharmacy : Spec
{
    public const string UserStory = @"
        Employee 1276 at the mobile caregiver organization Haus Vogelsang, 
        whose client, Klient 2, is already in the LINCA system, orders medication for him. 
        The employee also specifies his preferred pharmacy for pick-up.";

    protected MedicationRequest medReq = new();

    public US021_MedOrderSingleArticleWithPharmacy(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Place order with pharmacy specified", CreateRequestOrchestrationRecord)
        };
    }


    private void PrepareOrderMedicationRequest()
    {
        //LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        medReq.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED    
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq.Subject = new ResourceReference()                                // REQUIRED
        {
            // relative path to Linca Fhir patient resource
            Reference = "HL7ATCorePatient/47effc7d2c784930a5c18791aec561ad"
        };

        medReq.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "0507928",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "CATAPRESAN TBL 0,15MG"
                    }
                }
            }
        };

        medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                //System = "urn:oid:1.2.40.0.34.5.2"
            }
        });

        medReq.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "1276",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Haus Vogelsang
            }
        };

        medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                //System = "urn:oid:1.2.40.0.34.5.2"
            }
        });

        medReq.DispenseRequest = new()
        {
            Dispenser = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.2",  // OID of designated pharmacy
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                    //System = "urn:oid:1.2.40.0.34.5.2"
                },
                Display = "Apotheke 'Zum frühen Vogel'"
            }
        };


    }

    private bool CreateRequestOrchestrationRecord ()
    {
        // first prepare a LincaOrderMedicationRequest to be contained in the LincaRequestOrchestration
        PrepareOrderMedicationRequest();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                  //System = "urn:oid:1.2.40.0.34.5.2"
                }
            }
        };

        ro.Contained.Add(medReq);

        var action = new RequestOrchestration.ActionComponent()
        {
            Type = new(),
            Resource = new ResourceReference($"#{medReq.Id}")
        };

        action.Type.Coding.Add(new() { Code = "create" });
        ro.Action.Add( action );

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdImmerdar001 = createdRO.Id;
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
}
