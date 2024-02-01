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
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class Test005_Vogelsang_DeleteRequestOrchestrationValidation : Spec
{
    public const string UserStory = @"
        First a RequestOrchestration with contained proposals is successfully created. 
        Run this test with the certificate of Haus Vogelsang.";

    protected Patient createdGuenter = new();
    protected Patient createdPatrizia = new();
    protected MedicationRequest medReq1 = new();
    protected MedicationRequest medReq2 = new();
    protected MedicationRequest medReq3 = new();
    protected MedicationRequest medReq4 = new();
    protected RequestOrchestration createdRO = new();

    public Test005_Vogelsang_DeleteRequestOrchestrationValidation(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create client record Günter Gürtelthier", CreateClientRecord1),
            new("Create client record Patrizia Platypus", CreateClientRecord2),
            new("Place orders for two patients with pharmacy specified", CreateRequestOrchestrationRecord),
            new("Cancel a single proposal", PostProposalMedicationRequestCancel),
            new("Cancel the same proposal again, LCVAL33, not latest chain link", PostProposalMedicationRequestCancelLCVAL33),
            new("Delete RequestOrchestration, LCVAL62, one proposal already processed", DeleteRequestOrchestrationLCVAL62),
            new("Place orders for two patients with pharmacy specified", CreateAnotherRequestOrchestrationRecord),
            new("Delete RequestOrchestration completely with success", DeleteRequestOrchestrationSuccess),
            new("Delete RequestOrchestration, LCVAL63, RequestOrchestration has already been revoked", DeleteRequestOrchestrationLCVAL63)

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

        (createdGuenter, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine($"Client information transmitted, id {createdGuenter.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        OutcomeHelper.PrintOutcome(outcome);

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

        (createdPatrizia, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine($"Client information transmitted, id {createdPatrizia.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool CreateRequestOrchestrationRecord() 
    {  
        PrepareMedicationRequests();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
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
        ro.Contained.Add(medReq3);  

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool PostProposalMedicationRequestCancel()
    {
        (Bundle results, bool received) = LincaDataExchange.GetProposalStatus(Connection, $"{createdRO.Id}");

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

            MedicationRequest proposalPatrizia = proposals.Find(x => x.Subject.Reference.Contains($"{createdPatrizia.Id}"))!;

            // post order medication request for Patrizia Platypus based on an existing order medication request
            // set Status to cancelled 
            medReq4 = (MedicationRequest)medReq3.DeepCopy();
            medReq4.Id = null;
            medReq4.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{proposalPatrizia.Id}"
            });

            medReq4.Status = MedicationRequest.MedicationrequestStatus.Cancelled;    // REQUIRED
            medReq4.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, medReq4);

            if (canCue)
            {
                Console.WriteLine($"Linca ProposalMedicationRequest transmitted, id {postedOMR.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca ProposalMedicationRequest");
            }

            OutcomeHelper.PrintOutcome(outcome);

            return canCue;
        }
        else
        {
            Console.WriteLine($"Failed to retrieve id of ProposalMedicationRequest for update");

            return false;
        }
    }

    private bool PostProposalMedicationRequestCancelLCVAL33()
    {
        (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, medReq4);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL33");
    }

    private bool DeleteRequestOrchestrationLCVAL62()
    {
        (var oo, var deleted) = LincaDataExchange.DeleteRequestOrchestration(Connection, $"{createdRO.Id}");

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(oo, "LCVAL62");
    }

    private bool CreateAnotherRequestOrchestrationRecord()
    {
        //PrepareMedicationRequests();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,    // REQUIRED
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
        ro.Contained.Add(medReq3);

        foreach (var item in ro.Contained)
        {
            var action = new RequestOrchestration.ActionComponent()
            {
                //Type =
                Resource = new ResourceReference($"#{item.Id}")
            };
            ro.Action.Add(action);
        }

        (createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
        }

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool DeleteRequestOrchestrationSuccess()
    {
        (var oo, var deleted) = LincaDataExchange.DeleteRequestOrchestration(Connection, $"{createdRO.Id}");

        OutcomeHelper.PrintOutcome(oo);

        return deleted;
    }
    private bool DeleteRequestOrchestrationLCVAL63()
    {
        (var oo, var deleted) = LincaDataExchange.DeleteRequestOrchestration(Connection, $"{createdRO.Id}");

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(oo, "LCVAL63");
    }

    private void PrepareMedicationRequests()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        
        // medication request 1 for Günter Gürtelthier
        medReq1.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq1.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq1.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq1.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdGuenter.Id}"     // relative path to Linca Fhir patient resource
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

        // medication request 2 for Günter Gürtelthier
        medReq2.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq2.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq2.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq2.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdGuenter.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq2.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "4460951",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "Granpidam 20 mg Filmtabletten"
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
                Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Silvia Spitzmaus"   // optional
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

        /***********************************************************************************/

        // medication request for Patricia Platypus
        medReq3.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED
        medReq3.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq3.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq3.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdPatrizia.Id}"     // relative path to Linca Fhir patient resource
        };

        medReq3.Medication = new()
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

        medReq3.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });

        medReq3.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };

        medReq3.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Kunibert Kreuzotter"   // optional
        });

        medReq3.DispenseRequest = new()
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
