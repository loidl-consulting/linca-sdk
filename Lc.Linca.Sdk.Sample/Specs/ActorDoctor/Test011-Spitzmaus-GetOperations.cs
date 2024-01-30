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
        Run this test with the certificate of Dr. Spitzmaus.";

    public Test011_Spitzmaus_GetOperations(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Get '' is undefined", GetWithEmptyString),
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
