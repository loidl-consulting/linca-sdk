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
using static Lc.Linca.Sdk.Scaffolds.CareInformationSystem;
using System.Reflection;
using System.Xml.Linq;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class Test001_CreatePatientValidation : Spec
{
    public const string UserStory = @"
    Testcase 001: Invoke all Patient validation errors and create one Patient successfully.
    Run this testcase with the certificate of Pflegedienst Immerdar "";
    ";

    protected MedicationRequest medReq = new();

    public Test001_CreatePatientValidation(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create client record: LCVAL01 Name.text missing", ClientRecordErrorLCVAL01),
            new("Create client record: LCVAL02 svnr not valid", ClientRecordErrorLCVAL02),
            new("Create client record: LCVAL03 svnr not unique", ClientRecordErrorLCVAL03),
            new("Create client record: LCVAL04 birthdate and svnr missing", ClientRecordErrorLCVAL04),
            new("Create client record: LCVAL05 gender missing", ClientRecordErrorLCVAL05),
            new("Create client record: multiple LCVALS send empty Patient resource", ClientRecordErrorLCVAL06),
            new("Create client record with success", CreateClientRecord)
        };
    }


    private bool ClientRecordErrorLCVAL01()
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
            // Text = client.Firstname + " " + client.Lastname
        });

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: client.SocInsNumber
        ));

        patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);
       
        if(canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

    private bool ClientRecordErrorLCVAL02()
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
            value: "123456789"
        ));

        patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

    private bool ClientRecordErrorLCVAL03()
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

        patient.Identifier.Add(new Identifier(
           system: Constants.WellknownOidSocialInsuranceNr,
           value: "1122100648"
       ));

        patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

    private bool ClientRecordErrorLCVAL04()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        var client = new CareInformationSystem.Client();
        var patient = new Patient();

        patient.Name.Add(new()
        {
            Family = client.Lastname,
            Given = new[] { client.Firstname },
            Text = client.Firstname + " " + client.Lastname
        });

        /*
        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: client.SocInsNumber
        ));
        */

        patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

    private bool ClientRecordErrorLCVAL05()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        var client = new CareInformationSystem.Client();
        var patient = new Patient();

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

        //patient.Gender = AdministrativeGender.Female;

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

    private bool ClientRecordErrorLCVAL06()
    {
        var patient = new Patient();

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
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

        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatientWithOutcome(Connection, patient);

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

    /*
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
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
    }
    */
    /*
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
    */
}
