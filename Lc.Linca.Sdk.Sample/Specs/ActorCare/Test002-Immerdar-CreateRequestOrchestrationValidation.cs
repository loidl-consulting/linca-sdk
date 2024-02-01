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

internal class Test002_Immerdar_CreateRequestOrchestrationValidation : Spec
{
    protected Patient createdPatient = new();
    protected MedicationRequest medReq = new();
    protected RequestOrchestration ro = new();

    public const string UserStory = @"
        Validate RequestOrchestration, with correct LincaProposalMedicationRequest, if contained. 
        Run this testcase with the certificate of Pflegedienst Immerdar ";

    public Test002_Immerdar_CreateRequestOrchestrationValidation(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create patient record successfully", CreateClientRecord),
            new("Create RequestOrchestration: LCVAL07 intent missing", RequestOrchestrationErrorLCVAL07),
            new("Create RequestOrchestration: LCVAL08 status missing", RequestOrchestrationErrorLCVAL08),
            new("Create RequestOrchestration: LCVAL09 subject missing", RequestOrchestrationErrorLCVAL09),
            new("Create RequestOrchestration: LCVAL10 intent not allowed", RequestOrchestrationErrorLCVAL10),
            new("Create RequestOrchestration: LCVAL11 status not allowed", RequestOrchestrationErrorLCVAL11),
            new("Create RequestOrchestration: LCVAL12 OID in subject invalid", RequestOrchestrationErrorLCVAL12),
            new("Create RequestOrchestration: LCVAL13 contained resource missing", RequestOrchestrationErrorLCVAL13),
            new("Create RequestOrchestration: LCVAL14 cast failed", RequestOrchestrationErrorLCVAL14),
            new("Create RequestOrchestration: LCVAL64 contained resource not referenced", RequestOrchestrationErrorLCVAL64),
            new("Create RequestOrchstration successfully", CreateRequestOrchestrationSuccess)
        };
    }


    private bool CreateClientRecord()
    {
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

        (createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine($"Created patient record with Id {createdPatient.Id}");
            PrepareOrderMedicationRequest();
        }
        else
        {
            Console.WriteLine("Create patient failed");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool RequestOrchestrationErrorLCVAL07()
    {
        ro.Status = RequestStatus.Active;      // REQUIRED
        //ro.Intent = RequestIntent.Proposal;  // REQUIRED
        ro.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        };

        ro.Contained.Add(medReq);

        var action = new RequestOrchestration.ActionComponent()
        {
            Type = new(),
            Resource = new ResourceReference($"#{medReq.Id}")
        };

        action.Type.Coding.Add(new() { Code = "create" });

        ro.Action.Add(action);

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL07");
    }

    private bool RequestOrchestrationErrorLCVAL08()
    {
        ro.Status = null;      // REQUIRED
        ro.Intent = RequestIntent.Proposal;  // REQUIRED

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL08");
    }

    private bool RequestOrchestrationErrorLCVAL09()
    {
        ro.Status = RequestStatus.Active;      // REQUIRED
        //ro.Intent = RequestIntent.Proposal;  // REQUIRED

        ro.Subject = null;

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL09");
    }

    private bool RequestOrchestrationErrorLCVAL10()
    {
        ro.Intent = RequestIntent.Order;  // REQUIRED is Proposal

        ro.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        };

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL10");
    }

    private bool RequestOrchestrationErrorLCVAL11()
    {
        ro.Intent = RequestIntent.Proposal;  // REQUIRED
        ro.Status = RequestStatus.Draft; // REQUIRED is value Active

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL11");
    }

    private bool RequestOrchestrationErrorLCVAL12()
    {
        ro.Status = RequestStatus.Active; // REQUIRED

        ro.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.",  // OID misses the last digit
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = null   // optional
        };

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL12");
    }

    private bool RequestOrchestrationErrorLCVAL13()
    {
        ro.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        };

        ro.Contained.Clear(); 

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL13");
    }

    private bool RequestOrchestrationErrorLCVAL14()
    {
        var mr = new MedicationRequest();

        (var createdRO, var canCue, var outcome) = LincaDataExchange.PostProposalToOrchestrationEndpoint(Connection, mr);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL14");
    }

    private bool RequestOrchestrationErrorLCVAL64()
    {
        ro.Contained.Add(medReq);

        ro.Action.Clear();

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL64");
    }

    private bool CreateRequestOrchestrationSuccess()
    {
        var action = new RequestOrchestration.ActionComponent()
        {
            Type = new(),
            Resource = new ResourceReference($"#{medReq.Id}")
        };

        action.Type.Coding.Add(new() { Code = "create" });

        ro.Action.Add(action);

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine($"Linca Request Orchestration with id '{createdRO.Id}' successfully created");
        }
        else
        {
            Console.WriteLine("Validation failed, result:");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }
    private void PrepareOrderMedicationRequest()
    {
        medReq.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq.Subject = new ResourceReference()                                // REQUIRED
        {
            // relative path to Linca Fhir patient resource
            Reference = $"HL7ATCorePatient/{createdPatient.Id}"
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
            },
            Quantity = new()
            {
                Value = 2
            }
        };
    }
}
