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

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US017_GetOrderPositionInfo : Spec
{ 
    public const string UserStory = @"
        Pharmacist Mag. Andreas Amsel, owner of the pharmacy Apotheke 'Zum frühen Vogel' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When care giver Susanne Allzeit (DGKP) presents a barcode representing a prescription of a 
        LINCA order for her client Renate Rüssel-Olifant,
        Then Mag. Andreas Amsel can scan that code at his POS,
          and his software can fetch the corresponding records from LINCA,
          and interpret the returned LINCA order position chains
          and visually present and import the positions included in that prescription for Renate Rüssel-Olifant";

    public US017_GetOrderPositionInfo(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new("Get prescription to dispense by eRezeptId", GetPrescriptionToDispense)
        };
    }

    private bool GetPrescriptionToDispense()
    {
        (Bundle results, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "ABCD 1234 EFGH");

        if (received)
        {
            Console.WriteLine($"Get prescription-to-dispense succeeded");

            BundleViewer.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine($"Get prescription-to-dispense failed");
        }

        return received;
    }
}
