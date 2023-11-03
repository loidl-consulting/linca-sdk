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

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US017_PartialDispense : Spec
{
    public const string UserStory = @"
        Pharmacist Mag. Andreas Amsel, owner of the pharmacy Apotheke 'Zum frühen Vogel' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When he is expected to fullfil medication orders for a customer, e.g., Renate Rüssel-Olifant, 
        and he has a LINCA order Id to go with a purchase her care giver Susanne Allzeit just made for her, 
        and he did not, or is not able to, dispense all of the product at once         
        then Mag. Andreas Amsel submits a partial dispense record for the order position in question
          and his software will send that to the LINCA server,
          and notify the ordering organization, Pflegedienst Immerdar, about the partial dispense.";

    public US017_PartialDispense(LincaConnection conn) : base(conn) { }
}
