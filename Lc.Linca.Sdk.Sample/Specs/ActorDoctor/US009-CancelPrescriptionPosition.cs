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

internal class US009_CancelPrescriptionPosition : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Silvia Spitzmaus is responsible for the LINCA registered care giver client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him.
        She decides that Günter Gürtelthier shall no longer take the medication intended by that order position. 
        Hence, she submits an update on that order position with the status set to 'stopped' or 'ended',
          and her software will send that to the LINCA server,
          and the ordering care giver organization Haus Vogelsang will be informed that this position will not be prescribed further on";
    public US009_CancelPrescriptionPosition(LincaConnection conn) : base(conn) { }
}
