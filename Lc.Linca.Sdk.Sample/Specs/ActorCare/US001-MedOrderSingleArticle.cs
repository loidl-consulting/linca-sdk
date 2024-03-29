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
using Hl7.Fhir.Support;
using Lc.Linca.Sdk.Client;
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US001_MedOrderSingleArticle : Spec
{
    public const string UserStory = @"
        User Susanne Allzeit (DGKP) is an employee at the mobile caregiver organization Pflegedienst Immerdar, 
        whose client, Renate Rüssel-Olifant, is not in the LINCA system yet. 
        Hence, Susanne Allzeit creates a client record in the system.
        Now, it is possible to order prescriptions for Renate Rüssel-Olifant. 
        As Susanne Allzeit will pick up the medication on the go, she places the order 
        without specifying a pharmacy.";

    protected MedicationRequest medReq = new();

    public US001_MedOrderSingleArticle(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create client record", CreateClientRecord),
            new("Place order with no pharmacy specified", CreateRequestOrchestrationRecord)
        };
    }

    /// <summary>
    /// As an actor who is an order placer,
    /// it is necessary to ensure that all patient records
    /// which later occur in the order position(s), are present
    /// as FHIR resources on the linked care server.
    /// 
    /// This is where an actual care information system
    /// would fetch the client data from its database, 
    /// and convert it into a FHIR R5 resource
    /// </summary>
    private bool CreateClientRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        var client = new CareInformationSystem.Client();
        var patient = new Patient
        {
            BirthDate = DateTime.ParseExact(
                client.DoB,
                Constants.DobFormat,
                CultureInfo.InvariantCulture
            ).ToFhirDate()
        };

        patient.Name.Add(new()
        {
            Family = client.Lastname,
            Given = new[] { client.Firstname },
            Text = client.Firstname + " " + client.Lastname
        });

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: client.SocInsNumber
        ));

        patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);
       
        if(canCue)
        {
            LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdRenate = createdPatient.Id;
            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Client information transmitted, id {createdPatient.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
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

    private void PrepareOrderMedicationRequest()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

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
                        Code = "0031130",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "Lasix 40 mg Tabletten"
                    }
                }
            }
        };

        medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
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
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
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
                    Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                    System = "urn:ietf:rfc:3986"  // Code-System: eHVD
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
