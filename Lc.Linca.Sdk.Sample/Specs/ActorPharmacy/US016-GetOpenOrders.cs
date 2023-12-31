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

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US016_GetOpenOrders : Spec
{
    public const string UserStory = @"
        Pharmacist Mag. Andreas Amsel, owner of the pharmacy Apotheke 'Zum frühen Vogel' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When he is expected to fullfil medication orders for customers, 
        then he submits a read request for open orders where his pharmacy is mentioned as the designated dispenser.
        He will receive a list of LINCA order position chains, 
            e.g., for orders for clients of Haus Vogelsang because they mentioned his pharmacy as preferred pick-up point,
            and his software can interpret the returned LINCA order position chains, 
            and visually present and import the order and all its positions";

    public US016_GetOpenOrders(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Get prescriptions to dispense", GetPrescriptionsToDispense)
        };
    }

    private bool GetPrescriptionsToDispense()
    {
        (Bundle results, bool received) = LincaDataExchange.GetPrescriptionsToDispense(Connection);

        if (received)
        {
            Console.WriteLine($"Get prescriptions-to-dispense succeeded");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine($"Get prespcriptions-to-dispense failed");
        }

        return received;
    }
}
