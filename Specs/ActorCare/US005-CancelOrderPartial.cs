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

internal class US005_CancelOrder : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to cancel individual order positions for his client Patrizia Platypus.
        He submits updates on those positions, providing a reason for cancellation, such as a medical reason, 
        and sets their status to 'cancelled'. 
        The LINCA systems prevents Walter Specht from submitting such cancellations
        if Patrizia's practitioner, Dr. Kunibert Kreuzotter, has already issued a prescription for the original order position";
}
