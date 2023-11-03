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
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;

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
        if (CreateClientRecord()) // in UserStory002 we assume that this has already been done
        {
            // first prepare a LincaOrderMedicationRequest to be contained in the LincaRequestOrchestration
            PrepareOrderMedicationRequest();

            RequestOrchestration ro = new()
            {
                Status = RequestStatus.Active,      // REQUIRED
                Intent = RequestIntent.Order,       // REQUIRED
                Subject = new ResourceReference()   // REQUIRED
                {
                    Identifier = new()
                    {
                        Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                        System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                    },
                    Display = "Pflegedienst Immerdar"   // optional
                }
            };

            ro.Contained.Add(medReq);

            (var createdRO, var canCue) = LincaDataExchange.CreateRequestOrchestration(Connection, ro);

            if (canCue)
            {
                Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca Request Orchestration");
            }

            return canCue;
        }
        else
        {
            return false;
        }

    }

    private bool CreateClientRecord()
    {
        var client = CareInformationSystem.GetClient();
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

        (createdPatient, var canCue) = LincaDataExchange.CreatePatient(Connection, patient);

        if (canCue)
        {
            Console.WriteLine($"Client information transmitted, id {createdPatient.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        return canCue;
    }

    private void PrepareOrderMedicationRequest()
    {
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdPatient.Id}"     // relative path to Linca Fhir patient resource
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
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
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
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
        medReq.DispenseRequest = new()
        {
            Dispenser = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.1",  // OID of designated pharmacy
                    System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                },
                Display = "Apotheke 'Klappernder Storch'"
            }
        };
    }
}