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

internal class US003_MedOrderStationaryCare : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He needs to collectively order prescription medication for several clients, amongst others 
        for Günter Gürtelthier and Patrizia Platypus. Patrizia's practitioner is 
        Dr. Kunibert Kreuzotter, Günter's practitioner is Dr. Silvia Spitzmaus. 
        Walter Specht places an order for all needed client prescription medication on LINCA 
        and specifies in advance the pharmacy Apotheke 'Zum frühen Vogel' that ought 
        to prepare the order";
}
