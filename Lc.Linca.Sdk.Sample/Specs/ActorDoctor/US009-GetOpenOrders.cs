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
using Lc.Linca.Sdk;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US009_GetOpenOrders : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered care giver clients 
        Patrizia Platypus and Renate Rüssel-Olifant, who are two of his patients. 
        Dr. Kunibert Kreuzotter has access to and permission in a practitioner role in the LINCA system, 
        hence he is expected to prescribe orders via the LINCA system. 
        When he submits a read request to the LINCA system he gets all open orders where he is mentioned as the designated practitioner, 
        e.g., all open orders for Patrizia Platypus and Renate Rüssel-Olifant.
        Dr. Kreuzotters software can interpret the returned LINCA order position chains 
        and visually present the status of the order and all its positions.";

    public US009_GetOpenOrders(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Get proposals to prescribe", GetProposalsToPrescribe)
        };
    }

    private bool GetProposalsToPrescribe()
    {
        (Bundle results, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            Console.WriteLine($"Get proposals-to-prescribe succeeded");

            foreach (var item in results.Entry)
            {
                Console.WriteLine(item.FullUrl);
            }
        }
        else
        {
            Console.WriteLine($"Get proposals-to-prescribe failed");
        }

        return received;
    }
}
