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

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US012_ModifyPrescribedDosage : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Silvia Spitzmaus is responsible for the LINCA registered care giver client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him and has already 
        submitted a prescription for that order position.
        She decides that the dosage instructions in the prior prescription need to be defined or modified. 
        Hence, she submits an update to that prescription with new dosage instructions,
          and her software will send that to the LINCA server,
          and the ordering care giver organization Haus Vogelsang will be informed that the prescription has been 
          updated with altered dosage";

    public US012_ModifyPrescribedDosage(LincaConnection conn) : base(conn) { }
}
