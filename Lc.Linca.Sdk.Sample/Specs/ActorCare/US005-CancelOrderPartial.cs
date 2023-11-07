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

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US005_CancelOrder : Spec
{
    protected MedicationRequest medReq2 = new MedicationRequest();

    public const string UserStory = @"
        User Walter Specht (DGKP) is a caregiver in the inpatient care facility Haus Vogelsang. 
        He has already placed a collective order for prescription medication for several clients on LINCA.
        Now, he needs to cancel individual order positions for his client Patrizia Platypus.
        He submits updates on those positions, providing a reason for cancellation, such as a medical reason, 
        and sets their status to 'cancelled'. 
        The LINCA systems prevents Walter Specht from submitting such cancellations
        if Patrizia's practitioner, Dr. Kunibert Kreuzotter, has already issued a prescription for the original order position";

    public US005_CancelOrder(LincaConnection conn) : base(conn) 
    {
       Steps = new Step[]
       {
            new("Post ProposalMedicationRequest for cancellation", PostProposalMedicationRequestCancel)
       };
    }

    private bool PostProposalMedicationRequestCancel()
    {
        // post order medication request for Patrizia Platypus based on an existing order medication request
        // set Status to cancelled 
        medReq2.BasedOn.Add(new ResourceReference()
        {
            Reference = "LINCAProposalMedicationRequest/1c6c3d78ab384c52aebc030eb6e92131"
        });
        // medication request for Patricia Platypus
        medReq2.Status = MedicationRequest.MedicationrequestStatus.Cancelled;      // REQUIRED
        medReq2.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq2.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/73305590f6b14686911b9aae2f245605"     // relative path to Linca Fhir patient resource
        };
        medReq2.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0028903",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Isoptin 80 mg - Dragees"
                        }
                    }
            }
        };
        medReq2.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });
        medReq2.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };
        medReq2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Kunibert Kreuzotter"   // optional
        });
        medReq2.DispenseRequest = new()
        {
            Dispenser = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.2",  // OID of designated pharmacy
                    System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                },
                Display = "Apotheke 'Zum frühen Vogel'"
            }
        };

        (var postedOMR, var canCue) = LincaDataExchange.PostProposalMedicationRequest(Connection, medReq2);

        if (canCue)
        {
            Console.WriteLine($"Linca ProposalMedicationRequest transmitted, id {postedOMR.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca ProposalMedicationRequest");
        }

        return canCue;
    }
 }
