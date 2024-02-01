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

internal class Test001_Immerdar_CreatePatientValidation : Spec
{
    public const string UserStory = @"
    Testcase 001: Invoke all Patient validation errors and create one Patient successfully.
    Run this testcase with the certificate of Pflegedienst Immerdar "";
    ";

    protected MedicationRequest medReq = new();

    public Test001_Immerdar_CreatePatientValidation(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create client record: LCVAL01 Name.text missing", ClientRecordErrorLCVAL01),
            new("Create client record: LCVAL02 svnr not valid", ClientRecordErrorLCVAL02),
            new("Create client record: LCVAL03 svnr not unique", ClientRecordErrorLCVAL03),
            new("Create client record: LCVAL04 birthdate and svnr missing", ClientRecordErrorLCVAL04),
            new("Create client record: LCVAL05 gender missing", ClientRecordErrorLCVAL05),
            new("Create client record: multiple LCVALS send empty Patient resource", ClientRecordErrorLCVALMultiple),
            new("Create client record with success", CreateClientRecord),
            new("Create client record, LCVAL06 cast failed", CreateClientRecordLCVAL06)
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
       
        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL01");
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

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL02");
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

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL03");
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

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL04");
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

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL05");
    }

    private bool ClientRecordErrorLCVALMultiple()
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

        OutcomeHelper.PrintOutcome(outcome);

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

        OutcomeHelper.PrintOutcome(outcome);

        return canCue;
    }

    private bool CreateClientRecordLCVAL06()
    {
        var ro = new RequestOrchestration();

        (var created, var canCue, var outcome) = LincaDataExchange.PostRequestOrchestrationToPatientEndpoint(Connection, ro);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        return OutcomeHelper.PrintOutcomeAndCheckLCVAL(outcome, "LCVAL06");
    }
}
