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
using Lc.Linca.Sdk.Client;
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;
using System.Security.Cryptography;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class Test004_Immerdar_BasedOnProposalValidation : Spec
{
    public const string UserStory = @"
        First create a RequestOrchestration with one contained proposal, 
        then try to post updates for that proposal and invoke all error codes for that use case.
        Run this testcase with the certificate of Pflegedienst Immerdar";

    protected Patient createdPatient = new();
    protected RequestOrchestration ro = new();
    protected MedicationRequest medReq = new();
    protected RequestOrchestration createdRO = new();
    protected MedicationRequest? createdMedReq = new();
    protected MedicationRequest? updateMedReq = new();
    

    public Test004_Immerdar_BasedOnProposalValidation(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create patient record successfully", CreateClientRecord),
            new("Create RequestOrchestration with one contained proposal successfully", CreateRequestOrchestrationSuccess),
            new("Create Proposal based on existing proposal, LCVAL 18: basedOn is emtpty", ProposalMedicationRequestUpdateLCVAL18A),
            new("Create Proposal based on existing proposal, LCVAL 18: basedOn not unique", ProposalMedicationRequestUpdateLCVAL18B),
            new("Create Proposal based on existing proposal, LCVAL 19: intent invalid", ProposalMedicationRequestUpdateLCVAL19),
            new("Create Proposal based on existing proposal, LCVAL 20: status invalid", ProposalMedicationRequestUpdateLCVAL20),
            new("Create Proposal based on existing proposal, LCVAL 31: refString invalid", ProposalMedicationRequestUpdateLCVAL31),
            new("Create Proposal based on existing proposal, LCVAL 32: refId not found", ProposalMedicationRequestUpdateLCVAL32),
            new("Create Proposal based on existing proposal, LCVAL 34: informationSource not unique", ProposalMedicationRequestUpdateLCVAL34),
             new("Create Proposal based on existing proposal, LCVAL 36: supportingInformation differs", ProposalMedicationRequestUpdateLCVAL36),
            new("Create Proposal based on existing proposal successfully", ProposalMedicationRequestUpdateSuccess),
            new("Create Proposal based on existing proposal, LCVAL 33: ref in basedOn is not latest chain link", ProposalMedicationRequestUpdateLCVAL33),
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

    private bool CreateRequestOrchestrationSuccess()
    {
        PrepareOrderMedicationRequest();

        ro.Status = RequestStatus.Active;      // REQUIRED
        ro.Intent = RequestIntent.Proposal;    // REQUIRED
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

        (createdRO, var canCue, var outcome) = LincaDataExchange.CreateRequestOrchestrationWithOutcome(Connection, ro);

        if (canCue)
        {
            Console.WriteLine($"Linca Request Orchestration with id '{createdRO.Id}' successfully created");
            
            // fetch the created ProposalMedicationRequest
            (var results, var received) = LincaDataExchange.GetProposalStatus(Connection, $"{createdRO.Id}");
            if (received && results.Entry.Count == 1)
            {
                createdMedReq = results.Entry.First().Resource as MedicationRequest;

                if (createdMedReq == null )
                {
                    Console.WriteLine("Failed to get created ProposalMedicationRequest from Server");
                    return false;
                }
                else
                {
                    Console.WriteLine($"LINCAProposalMedicationRequest with id {createdMedReq.Id} successfully created");
                }
            }
            else
            {
                Console.WriteLine("Failed to get created ProposalMedicationRequest from Server");
                return false;
            }
        }
        else
        {
            Console.WriteLine("Validation failed, result:");
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

    private bool ProposalMedicationRequestUpdateLCVAL18A()
    {
        if (createdMedReq != null)
        {
            updateMedReq = createdMedReq.DeepCopy() as MedicationRequest;
            updateMedReq!.Id = null;

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"Validation Result");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL18B()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{createdMedReq.Id}"
            });
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{createdMedReq.Id}"
            });


            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL19()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.BasedOn.Clear();
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{createdMedReq.Id}"
            });

            updateMedReq.Intent = MedicationRequest.MedicationRequestIntent.Order; // this is not allowed in proposals

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL20()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.Intent = MedicationRequest.MedicationRequestIntent.Proposal; // this is not allowed in proposals

            updateMedReq.Status = MedicationRequest.MedicationrequestStatus.Draft;

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL31()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.BasedOn.Clear();
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedication/{createdMedReq.Id}" // wrong refString: request is missing
            });

            updateMedReq.Status = MedicationRequest.MedicationrequestStatus.Cancelled; // that is allowed

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL32()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.BasedOn.Clear();
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{createdRO.Id}" // this is the Id of the RequestOrchestration, not the  Proposal
            });

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL34()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.BasedOn.Clear();
            updateMedReq!.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{createdMedReq.Id}"
            });

            updateMedReq!.InformationSource.Add( medReq.Requester ); // Error: add requester as second informationSource

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL36()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.InformationSource.Clear();
            updateMedReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Pflegedienst Immerdar"   // optional
            });

            updateMedReq.SupportingInformation.Clear();
            updateMedReq.SupportingInformation.Add(new ResourceReference()
            {
                Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}"
            });

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateSuccess()
    {
        if (createdMedReq != null)
        {
            updateMedReq!.SupportingInformation.Clear();

            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine($"Successfully created LINCAProposalMedicationRequest with id {postedOMR.Id}");
            }
            else
            {
                Console.WriteLine($"Create LINCAProposalMedicationRequest failed");
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
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }

    private bool ProposalMedicationRequestUpdateLCVAL33()
    {
        if (createdMedReq != null)
        {
            (var postedOMR, var canCue, var outcome) = LincaDataExchange.PostProposalMedicationRequest(Connection, updateMedReq!);

            if (canCue)
            {
                Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            }
            else
            {
                Console.WriteLine($"ValidationResult");
            }

            if (outcome != null)
            {
                foreach (var item in outcome.Issue)
                {
                    Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
                }
            }

            return !canCue;

        }
        else
        {
            Console.WriteLine("Basic ProposalMedicationRequest is missing");

            return false;
        }
    }
}
