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
using Hl7.Fhir.Model.Extensions;
using Hl7.Fhir.Support;
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class Test003_Immerdar_ContainedProposalMedicationRequestValidation : Spec
{
    protected Patient createdPatient = new();
    protected MedicationRequest medReq = new();
    protected RequestOrchestration ro = new();

    public const string UserStory = @"
        Create RequestOrchestration, validate the contained LincaProposalMedicationRequest. 
        Run this testcase with the certificate of Pflegedienst Immerdar ";

    public Test003_Immerdar_ContainedProposalMedicationRequestValidation(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create patient record successfully", CreateClientRecord),
            new("Create RO with Contained Proposal: LCVAL07 intent missing", ContainedProposalErrorLCVAL07),
            new("Create RO with Contained Proposal: LCVAL08 status missing", ContainedProposalErrorLCVAL08),
            new("Create RO with Contained Proposal: LCVAL09 subject missing", ContainedProposalErrorLCVAL09),
            new("Create RO with Contained Proposal: LCVAL15 id in contained OMR missing", ContainedProposalErrorLCVAL15),
            new("Create RO with Contained Proposal: LCVAL16 basedOn not empty", ContainedProposalErrorLCVAL16),
            new("Create RO with Contained Proposal: LCVAL17 status in contained proposal not valid", ContainedProposalErrorLCVAL17),
            new("Create RO with Contained Proposal: LCVAL19 intent in contained proposal not valid", ContainedProposalErrorLCVAL19),
            new("Create RO with Contained Proposal: LCVAL21 priorPrescription not empty", ContainedProposalErrorLCVAL21),
            new("Create RO with Contained Proposal: LCVAL22 groupIdentifier not empty", ContainedProposalErrorLCVAL22),
            new("Create RO with Contained Proposal: LCVAL23 informationSource required", ContainedProposalErrorLCVAL23),
            new("Create RO with Contained Proposal: LCVAL24 informationSource OID invalid", ContainedProposalErrorLCVAL24),
            new("Create RO with Contained Proposal: LCVAL25 requester required", ContainedProposalErrorLCVAL25),
            new("Create RO with Contained Proposal: LCVAL26 medication required", ContainedProposalErrorLCVAL26),
            new("Create RO with Contained Proposal: LCVAL27 refString in subject invalid", ContainedProposalErrorLCVAL27),
            new("Create RO with Contained Proposal: LCVAL28 patient in subject not found", ContainedProposalErrorLCVAL28),
            new("Create RO with Contained Proposal: LCVAL29 performer empty", ContainedProposalErrorLCVAL29),
            new("Create RO with Contained Proposal: LCVAL30 performer OID not valid", ContainedProposalErrorLCVAL30),
            new("Create RO with Contained Proposal: LCVAL56 dispenser.value missing", ContainedProposalErrorLCVAL56),
            new("Create RO with Contained Proposal: LCVAL65 dispenser OID not valid", ContainedProposalErrorLCVAL65),
            new("Create RequestOrchstration with contained proposal successfully", CreateRequestOrchestrationSuccess)
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
            
        }
        else
        {
            Console.WriteLine("Create patient failed");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private void RequestOrchestrationBase()
    {
        ro.Status = RequestStatus.Active;      // REQUIRED
        ro.Intent = RequestIntent.Proposal;  // REQUIRED
        ro.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        };
    }

    private void RequestOrchestrationUpdateContained()
    {
        ro.Contained.Clear();

        ro.Contained.Add(medReq);

        var action = new RequestOrchestration.ActionComponent()
        {
            Type = new(),
            Resource = new ResourceReference($"#{medReq.Id}")
        };

        action.Type.Coding.Add(new() { Code = "create" });

        ro.Action.Add(action);
    }



    private bool ContainedProposalErrorLCVAL07()
    {
        RequestOrchestrationBase(); // initialize the RO correctly

        PrepareOrderMedicationRequest(); // correct initialization of the contained proposal
        medReq.Intent = null;            // intent error

        RequestOrchestrationUpdateContained(); // add the proposal to contained

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

    private bool ContainedProposalErrorLCVAL08()
    {
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED           
        medReq.Status = null;   // status error

        RequestOrchestrationUpdateContained(); // add the proposal to contained

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

    private bool ContainedProposalErrorLCVAL09()
    {
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Subject.Reference = null;

        RequestOrchestrationUpdateContained(); // add the proposal to contained

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

    private bool ContainedProposalErrorLCVAL15()
    {
        medReq.Subject.Reference = $"HL7ATCorePatient/{createdPatient.Id}";
        medReq.Id = "";

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL15");
    }

    private bool ContainedProposalErrorLCVAL16()
    {
        medReq.Id = Guid.NewGuid().ToFhirId();
        medReq.Contained.Add(createdPatient); // this is not allowed
        medReq.BasedOn.Add( new() { Reference = $"{LincaEndpoints.LINCAProposalMedicationRequest}" });

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL16");
    }

    private bool ContainedProposalErrorLCVAL17()
    {
        medReq.Contained.Clear();
        medReq.BasedOn.Clear();
        medReq.Status = MedicationRequest.MedicationrequestStatus.Active;

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL17");
    }

    private bool ContainedProposalErrorLCVAL19()
    {
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Order;

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL19");
    }

    private bool ContainedProposalErrorLCVAL21()
    {
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;
        medReq.PriorPrescription = new ResourceReference() { Reference = $"{LincaEndpoints.LINCAPrescriptionMedicationRequest}" };

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL21");
    }

    private bool ContainedProposalErrorLCVAL22()
    {
        medReq.PriorPrescription = null;
        medReq.GroupIdentifier = new Identifier() { Value = "ANY VALUE" };

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL22");
    }

    private bool ContainedProposalErrorLCVAL23()
    {
        medReq.GroupIdentifier = null;
        medReq.InformationSource.Clear();

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL23");
    }

    private bool ContainedProposalErrorLCVAL24()
    {
        medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // WRONG OID 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        });

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL24");
    }

    private bool ContainedProposalErrorLCVAL25()
    {
        medReq.InformationSource.Clear();
        medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // correct OID 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        });

        medReq.Requester = null;

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL25");
    }

    private bool ContainedProposalErrorLCVAL26()
    {
        medReq.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ALLZEIT_BEREIT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.3"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Susanne Allzeit"
        };

        medReq.Medication = null;

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL26");
    }

    private bool ContainedProposalErrorLCVAL27()
    {
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

        medReq.Subject.Reference = $"HL7ATCorePatient{createdPatient.Id}"; // slash is missing

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL27");
    }

    private bool ContainedProposalErrorLCVAL28()
    {
        medReq.Subject.Reference = $"HL7ATCorePatient/{Guid.NewGuid().ToFhirId()}"; // Id not in Database

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL28");
    }

    private bool ContainedProposalErrorLCVAL29()
    {
        medReq.Subject.Reference = $"HL7ATCorePatient/{createdPatient.Id}";

        medReq.Performer.Clear();

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL29");
    }

    private bool ContainedProposalErrorLCVAL30()
    {
        medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.2",  // OID of caregiver
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Haus Sonnenschein"   // optional
        });

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL30");
    }

    private bool ContainedProposalErrorLCVAL56()
    {
        medReq.Performer.Clear();
        medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Kunibert Kreuzotter"   // optional
        });

        medReq.DispenseRequest.Dispenser.Identifier.Value = null; // this is a doctor, not a pharmacy

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL56");
    }

    private bool ContainedProposalErrorLCVAL65()
    {
        medReq.Performer.Clear();
        medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Kunibert Kreuzotter"   // optional
        });

        medReq.DispenseRequest.Dispenser.Identifier.Value = "2.999.40.0.34.3.1.2"; // this is a doctor, not a pharmacy

        RequestOrchestrationUpdateContained(); // add the proposal to contained

        (var createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL65");
    }

    private bool CreateRequestOrchestrationSuccess()
    {
        medReq.DispenseRequest.Dispenser = null; // create the proposal without pharmacy, but with given quantity

        RequestOrchestrationUpdateContained();

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
