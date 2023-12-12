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

internal class US000_InternalTestPutPatient : Spec
{
    public const string UserStory = @"
        Caregivers can create patients with externally assigned ids by sending them with http put";

    protected MedicationRequest medReq = new();

    public US000_InternalTestPutPatient(LincaConnection conn) : base(conn)
    {


        Steps = new Step[]
        {
            new("Create client record with externally assigned id", CreateClientRecord),
            new("Update client record", UpdateClientRecord)
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
            Id = Guid.NewGuid().ToFhirId(),
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
        (var createdPatient, var canCue, var outcome) = LincaDataExchange.CreatePatient(Connection, patient);
       
        if (canCue)
        {
                Console.WriteLine($"Client information with external id transmitted, id {createdPatient.Id}");

                LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdRenate = createdPatient.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information with external id");
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

    private bool UpdateClientRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();
        var client = new CareInformationSystem.Client();
        var patient = new Patient
        {
            Id = LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdRenate,
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

        patient.Gender = AdministrativeGender.Other;

        (var updatedPatient, var canCue, var outcome) = LincaDataExchange.CreatePatient(Connection, patient);

        if (canCue)
        {
            if (updatedPatient.Id == patient.Id)
            {
                Console.WriteLine($"Updated client, id {updatedPatient.Id}");
            }
            else
            {
                Console.WriteLine($"Client update information for id {patient.Id} transmitted, but the server assigned the id {updatedPatient.Id}");
            }
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
}
