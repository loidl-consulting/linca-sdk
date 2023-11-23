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

using System.Drawing.Text;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US010_CancelPrescriptionPosition : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered care giver client Renate Rüssel-Olifant. 
        He has received a LINCA order position requesting medication prescription for her.
        He decides that Renate Rüssel-Olifant shall no longer take the medication intended by that order position. 
        Hence, he submits an update on that order position with the status set to 'stopped' or 'ended',
          and his software will send that to the LINCA server,
          and the ordering care giver organization Pflegedienst Immerdar will be informed that this position will not be prescribed further on, 
          and their software system will inform Susanne Allzeit(DGKP)";
    public US010_CancelPrescriptionPosition(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Stop the intake of an ordered medication", SetProposalStatusEnded )
        };

        
    }

    private bool SetProposalStatusEnded()
    {
        return true;
    }
}
