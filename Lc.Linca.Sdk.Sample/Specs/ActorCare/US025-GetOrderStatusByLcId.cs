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

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US025_GetOrderStatus : Spec
{
    public const string UserStory = @"
        Get all chain links for a certain lc_id: proposals, precriptions, dispenses for that lc_id 
        and additionally all initial prescriptions and related chain links ";


    public US025_GetOrderStatus(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Get proposal status", GetProposalStatus)
        };
    }

    private bool GetProposalStatus()
    {
        Bundle results = new();
        bool received = false;  

        (results, received) = LincaDataExchange.GetProposalStatus(Connection, "174e917018cc4305a8708807dd9bfb3d");


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
