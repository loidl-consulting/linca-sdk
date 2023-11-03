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

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US010_PrescribeAsOrdered : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Silvia Spitzmaus is responsible for the LINCA registered care giver client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him.
        She decides to issue a prescription for the medication for Günter Gürtelthier intended by that order position. 
        Hence, she submits a prescription for that position with the eMedId and eRezeptId she got
          and her software will send that to the LINCA server,
          and the ordering care giver organization Haus Vogelsang will be informed that the order position has been prescribed as ordered";

    public US010_PrescribeAsOrdered(LincaConnection conn) : base(conn) { }
}