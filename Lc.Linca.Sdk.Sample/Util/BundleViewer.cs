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
using Hl7.Fhir.Utility;

namespace Lc.Linca.Sdk;

/// <summary>
/// Utilities for filtering and printing Bundles
/// </summary>
public static class BundleHelper
{
    public static List<MedicationRequest> FilterProposalsToPrescribe(Bundle orderchains)
    {
        List<MedicationRequest> openProposals = new();
        List<MedicationRequest> proposals = new();
        List<MedicationRequest> prescriptions = new();

        foreach (var item in orderchains.Entry)
        {
            if (item.FullUrl.Contains("LINCAProposal"))
            {
                proposals.Add((item.Resource as MedicationRequest)!);
            }

            if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!) ;
            }
        }

        foreach (var item in proposals) 
        {
            if (proposals.Find(x => !x.BasedOn.IsNullOrEmpty() && x.BasedOn.First().Reference.Contains(item.Id)) ==  null
                && prescriptions.Find(x => !x.BasedOn.IsNullOrEmpty() && x.BasedOn.First().Reference.Contains(item.Id)) == null)
            {
                openProposals.Add(item);
            }        
        }

        return openProposals;
    }

    public static List<MedicationRequest> FilterPrescriptionsToDispense(Bundle orderchains)
    {
        List<MedicationRequest> openPrescriptions = new();
        List<MedicationRequest> prescriptions = new();
        List<MedicationDispense> dispenses = new();

        foreach (var item in orderchains.Entry)
        {
            if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!);

            }

            if (item.FullUrl.Contains("LINCAMedicationDispense"))
            {
                dispenses.Add((item.Resource as MedicationDispense)!);
            }
        }

        foreach (var item in prescriptions)
        {
            if (prescriptions.Find(x => x.PriorPrescription != null && x.PriorPrescription.Reference.Contains(item.Id)) == null
               && dispenses.Find(x => !x.AuthorizingPrescription.IsNullOrEmpty()
                                        && x.AuthorizingPrescription.First().Reference.Contains(item.Id)
                                        && x.Type.Coding.First().Code.EndsWith("C")
                                        && x.Status == MedicationDispense.MedicationDispenseStatusCodes.Completed) == null)
            {
                openPrescriptions.Add(item);
            }
        }

        return openPrescriptions;
    }

    public static void ShowOrderChains (Bundle orderchains)
    {
        List<MedicationRequest> proposals = new();
        List<MedicationRequest> prescriptions = new();
        List<MedicationDispense> dispenses = new();

        Console.WriteLine("Bundle Entries:");

        foreach (var item in orderchains.Entry)
        {
            Console.WriteLine(item.FullUrl);

            if (item.FullUrl.Contains("LINCAProposal"))
            {
                proposals.Add((item.Resource as MedicationRequest)!);
            }
            else if (item.FullUrl.Contains("LINCAPrescription"))
            {
                prescriptions.Add((item.Resource as MedicationRequest)!);
            }
            else if (item.FullUrl.Contains("LINCAMedicationDispense"))
            {
                dispenses.Add((item.Resource as MedicationDispense)!);
            }
        }

        Console.WriteLine();

        foreach (var item in dispenses)
        {
            Console.WriteLine($"Dispense Id: {item.Id} --> authorizingPrescription: {item.AuthorizingPrescription.First().Reference}");
        }

        foreach (var item in prescriptions)
        {
            if (item.BasedOn.Count == 1)
            {
                Console.WriteLine($"Prescription Id: {item.Id} --> is based on proposal: {item.BasedOn.First().Reference}");
            }
            else if (item.PriorPrescription != null) 
            {
                Console.WriteLine($"Prescription Id: {item.Id} --> refers to prior prescription: {item.PriorPrescription.Reference}");
            }
            else
            {
                Console.WriteLine($"Prescription Id: {item.Id} is an initial prescription");
            }
        }

        foreach (var item in proposals)
        {
            if (item.BasedOn.Count == 1)
            {
                Console.WriteLine($"Proposal Id: {item.Id} --> is based on proposal: {item.BasedOn.First().Reference}");
            }
            else
            {
                Console.WriteLine($"Proposal Id: {item.Id} --> is a starting link");
            }
        }
    }
}
