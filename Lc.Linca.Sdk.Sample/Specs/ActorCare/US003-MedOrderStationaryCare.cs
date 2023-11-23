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
using System;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US003_MedOrderStationaryCare : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He needs to collectively order prescription medication for several clients, amongst others 
        for Günter Gürtelthier and Patrizia Platypus. Patrizia's practitioner is 
        Dr. Kunibert Kreuzotter, Günter's practitioner is Dr. Silvia Spitzmaus. 
        Walter Specht places an order for all needed client prescription medication on LINCA 
        and specifies in advance the pharmacy Apotheke 'Zum frühen Vogel' that ought 
        to prepare the order";

    protected Patient createdGuenter = new Patient();
    protected Patient createdPatrizia = new Patient();
    protected MedicationRequest medReq1 = new();
    protected MedicationRequest medReq2 = new();


    public US003_MedOrderStationaryCare(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create client record Günter Gürtelthier", CreateClientRecord1),
            new("Create client record Patrizia Platypus", CreateClientRecord2),
            new("Place orders for two patients with pharmacy specified", CreateRequestOrchestrationRecord)
        };
    }

    private bool CreateClientRecord1()
    {
        var patient = new Patient
        {
            BirthDate = DateTime.ParseExact(
                "20011024",
                Constants.DobFormat,
                CultureInfo.InvariantCulture
            ).ToFhirDate()
        };

        patient.Name.Add(new()
        {
            Text = "Gürtelthier Günter"
        });

        patient.Gender = AdministrativeGender.Male;

        (createdGuenter, var canCue) = LincaDataExchange.CreatePatient(Connection, patient);

        if (canCue)
        {
            LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter = createdGuenter.Id;
            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Client information transmitted, id {createdGuenter.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        return canCue;
    }

    private bool CreateClientRecord2()
    {

        var patient = new Patient();

        patient.Name.Add(new()
        {
            Family = "Platypus",
            Text = "Patrizia Platypus"
        });

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: "1148070771"
        ));
        patient.Gender = AdministrativeGender.Other;

        (createdPatrizia, var canCue) = LincaDataExchange.CreatePatient(Connection, patient);

        if (canCue)
        {
            LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdPatrizia = createdPatrizia.Id;
            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Client information transmitted, id {createdPatrizia.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        return canCue;
    }

    private bool CreateRequestOrchestrationRecord() 
    {  
        PrepareMedicationRequests();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,       // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization from certificate
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Haus Vogelsang"   // optional
            }
        };

        ro.Contained.Add(medReq1);
        ro.Contained.Add(medReq2);

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (var createdRO, var canCue) = LincaDataExchange.CreateRequestOrchestration(Connection, ro);

        if (canCue)
        {
            LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang = createdRO.Id;
            LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
        }

        return canCue;
    }

    private void PrepareMedicationRequests()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        
        // medication request for Günter Gürtelthier
        medReq1.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq1.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
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
                            Code = "0018589",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Effortil 7,5 mg/ml - Tropfen"
                        }
                    }
            }
        };

        medReq1.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
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
                    Value = "2.999.40.0.34.5.1.2",  // OID of designated pharmacy
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Apotheke 'Zum frühen Vogel'"
            }
        };

        /***********************************************************************************/

        // medication request for Patricia Platypus
        medReq2.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq2.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
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
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
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
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
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
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Apotheke 'Zum frühen Vogel'"
            }
        };
    }
}
