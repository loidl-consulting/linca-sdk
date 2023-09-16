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

    public US001_MedOrderSingleArticle(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Create client record", CreateClientRecord),
            new("Place order with no pharmacy specified", () => false)
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
            Given = new[] { client.Firstname }
        });

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: client.SocInsNumber
        ));

        (var createdPatient, var canCue) = LincaDataExchange.CreatePatient(Connection, patient);
       
        if(canCue)
        {
            Console.WriteLine($"Client information transmitted, id {createdPatient.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit client information");
        }

        return canCue;
    }
}
