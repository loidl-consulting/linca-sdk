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

    public US001_MedOrderSingleArticle()
    {
        Steps = new Step[]
        {
            new("Create client record", CreateClientRecord),
            new("Place order with no pharmacy specified", () => false)
        };
    }

    private bool CreateClientRecord()
    {
        return true;
    }
}
