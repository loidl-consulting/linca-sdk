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

internal class US002_MedOrderRepeat : Spec
{
    public const string UserStory = @"
        User Susanne Allzeit (DGKP) is an employee at the mobile caregiver 
        organization Pflegedienst Immerdar, whose client, Renate Rüssel-Olifant, is 
        already registered as patient in the LINCA system. 
        Susanne Allzeit needs to re-stock prescription medication for Renate Rüssel-Olifant. 
        Hence, she places an order on LINCA referring to the existing patient 
        record of Renate Rüssel-Olifant. 
        Additionally, she specifies her preferred pharmacy, Apotheke 'Klappernder Storch', in advance 
        to collect the order there. ";

    public US002_MedOrderRepeat(LincaConnection conn) : base(conn) { }
}
