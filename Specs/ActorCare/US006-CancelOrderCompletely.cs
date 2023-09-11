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

internal class US006_CancelOrderCompletely : Spec
{
    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to cancel the complete order due to a major mistake.
        He submits a delete request on the order number, providing a reason for cancellation, such as a human error. 
        Then, 
        either the whole order will be cancelled by the LINCA system, if for none of its positions the designated 
            practitioner has already issued a prescription and the status is set to 'revoked' by LINCA,
        or the whole order will remain active if for any of its positions the designated practitioner 
            has already issued a prescription. And positions for which the designated practitioner has not yet issued a prescription, 
            will be promoted to the status 'cancelled' by the LINCA system";
}
