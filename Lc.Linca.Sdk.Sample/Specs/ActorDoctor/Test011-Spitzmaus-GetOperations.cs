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
using Hl7.Fhir.Model.Extensions;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class Test011_Spitzmaus_GetOperations : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Kunibert Kreuzotter is responsible for the LINCA registered care giver clients 
        Patrizia Platypus and Renate Rüssel-Olifant, who are two of his patients. 
        Dr. Kunibert Kreuzotter has access to and permission in a practitioner role in the LINCA system, 
        hence he is expected to prescribe orders via the LINCA system. 
        When he submits a read request to the LINCA system he gets all open orders where he is mentioned as the designated practitioner, 
        e.g., all open orders for Patrizia Platypus and Renate Rüssel-Olifant.
        Dr. Kreuzotters software can interpret the returned LINCA order position chains 
        and visually present the status of the order and all its positions.";

    public Test011_Spitzmaus_GetOperations(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Get '$' is undefined", GetDollarSign),
            new ("Get '$test-operation' is undefined", GetUndefinedOperation),
            new ("Get '$prescriptions-to-dispense' with doctors certificate", GetPrescriptionsToDispenseWithDoctorCertificate),
            new ("Get '$proposals-to-prescribe' with success", GetProposalsToPrescribeSuccess),
        };
    }

    private bool GetWithEmptyString()
    {
        //GET ""
        (Bundle proposalChains, bool canCue) = LincaDataExchange.GetWithAnyOperationName(Connection, string.Empty);

        if (canCue)
        {
            Console.WriteLine("Error: Get '' succeeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(proposalChains);
        }
        else
        {
            Console.WriteLine("Get '' failed, this is the expected outcome");
        }

        return !canCue;
    }

    private bool GetDollarSign()
    {
        //GET ""
        (Bundle proposalChains, bool canCue) = LincaDataExchange.GetWithAnyOperationName(Connection, "$");

        if (canCue)
        {
            Console.WriteLine("Error: Get '$' succeeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(proposalChains);
        }
        else
        {
            Console.WriteLine("Get '$' failed, this is the expected outcome");
        }

        return !canCue;
    }

    private bool GetUndefinedOperation()
    {
        //GET ""
        (Bundle proposalChains, bool canCue) = LincaDataExchange.GetWithAnyOperationName(Connection, "$test-operation");

        if (canCue)
        {
            Console.WriteLine("Error: Get '$test-operation' succeeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(proposalChains);
        }
        else
        {
            Console.WriteLine("Get '$test-operation' failed, this is the expected outcome");
        }

        return !canCue;
    }

    private bool GetPrescriptionsToDispenseWithDoctorCertificate()
    {
        //GET ""
        (Bundle proposalChains, bool canCue) = LincaDataExchange.GetWithAnyOperationName(Connection, LincaEndpoints.prescriptions_to_dispense);

        if (canCue)
        {
            Console.WriteLine("Error: Get '$prescriptions-to-dispense' with Doctor's certificate succeeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(proposalChains);
        }
        else
        {
            Console.WriteLine("Get '$prescriptions-to-dispense' failed, this is the expected outcome");
        }

        return !canCue;
    }

    private bool GetProposalsToPrescribeSuccess()
    {
        //GET ""
        (Bundle proposalChains, bool canCue) = LincaDataExchange.GetWithAnyOperationName(Connection, LincaEndpoints.proposals_to_prescribe);

        if (canCue)
        {
            Console.WriteLine("Get '$proposals-to-prescribe' with Doctor's certificate succeeded, resulting Bundle:");

            BundleHelper.ShowOrderChains(proposalChains);
        }
        else
        {
            Console.WriteLine("Error: Get '$proposals-to-prescribe' failed");
        }

        return canCue;
    }
}
