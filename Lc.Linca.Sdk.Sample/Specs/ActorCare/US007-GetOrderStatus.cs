﻿/***********************************************************************************
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

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US007_GetOrderStatus : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Walter Specht needs to know the status of that order, and he has the permission to read the 
        entire order. 
        Hence, he submits a read request on the order number,
          and his care software can use the returned LINCA order position chains,
          and visually present the status of the order and all its positions";

    public US007_GetOrderStatus(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Get proposal status", GetProposalStatus)
        };
    }

    private bool GetProposalStatus()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        Bundle results = new();
        bool received = false;  

        if (! string.IsNullOrEmpty(LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang)) 
        {
            (results, received) = LincaDataExchange.GetProposalStatus(Connection, $"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdVogelsang}");
        }

        // use the following line to run the sequence [01] Immerdar -> [11] Würm -> [18] Klappernder Storch -> [07] Immerdar
        // (results, received) = LincaDataExchange.GetProposalStatus(Connection, $"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.LcIdImmerdar001}");

        if (received)
        {
            Console.WriteLine("Get proposal-status succeeded");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine($"Get proposal-status failed");
        }

        return received;
    }
}
