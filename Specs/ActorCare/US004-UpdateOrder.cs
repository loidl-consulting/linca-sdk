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

internal class US004_UpdateOrder : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to modify details of that order, in particular he wants to update one 
        individual order position for his client Günter Gürtelthier.
        The LINCA systems prevents Walter Specht from updating such a position 
        if Günter's practitioner, Dr. Silvia Spitzmaus, has already issued a prescription for that order position";

    public US004_UpdateOrder(LincaConnection conn) : base(conn) { }
}
