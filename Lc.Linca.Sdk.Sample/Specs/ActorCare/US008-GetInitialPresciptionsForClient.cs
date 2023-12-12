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
using Lc.Linca.Sdk.Client;
using Lc.Linca.Sdk.Scaffolds;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US008_GetInitialPrescriptionsForClient : Spec
{
    public const string UserStory = @"
        Renate Rüssel-Olifant had a check-up with her general practitioner Dr. Wibke Würm and 
        got an initial prescription for new medication that she has not been taking so far. 
        Her caregiver Susanne Allzeit (DGKP) can see the new prescription for Renate Rüssel-Olifant 
        on her mobile device and can pick the new medication at any pharmacy of her choice by presenting the
        corresponding data matrix code";

    public US008_GetInitialPrescriptionsForClient(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Get initial prescriptions by a patient's social security number", GetPrescriptionsBySvnr)
        };
    }

    private bool GetPrescriptionsBySvnr()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle results, bool received) = LincaDataExchange.GetInitialPrescription(Connection, $"{new CareInformationSystem.Client().SocInsNumber}");

        if (received)
        {
            Console.WriteLine("Get initial prescriptions succeeded");
            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine($"Get initial prescriptions failed");
        }

        return received;
    }
}
