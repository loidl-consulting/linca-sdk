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

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US011_PrescribeWithChanges : Spec
{
    protected MedicationRequest prescription = new();

    public const string UserStory = @"
        Practitioner Dr. Silvia Spitzmaus is responsible for the LINCA registered care giver client Günter Gürtelthier. 
        She has received a LINCA order position requesting medication prescription for him.
        She decides that the medication intended by a particular order position needs to be adjusted.  
        Hence, she submits a prescription for that position with the eMedId and eRezeptId she got, with changed medication/quantity,
          and her software will send that to the LINCA server,
          and the ordering care giver organization Haus Vogelsang will be informed that the order position has been 
          prescribed with modified medication/quantity";

    public US011_PrescribeWithChanges(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create PrescriptionMedicationRequest with changed medication", CreatePrescriptionRecord)
        };
    }

    private bool CreatePrescriptionRecord()
    {
        prescription.BasedOn.Add(new ResourceReference()
        {
            Reference = "LINCAOrderMedicationRequest/3249246cd3774134abeafdfe6189e8e7"
        });
        prescription.Status = MedicationRequest.MedicationrequestStatus.Active;      // REQUIRED
        prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
        prescription.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = "HL7ATCorePatient/6800bda462034a9a8123e3dc48c61d53"     // relative path to Linca Fhir patient resource, copy from order
        };
        prescription.Medication = new()      // the doctor changes the medication to a ready-to-use ointment
        {
            Concept = new()
            {
                Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0059714",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Ultralan - Salbe"
                        }
                    }
            }
        };
        prescription.DosageInstruction.Add(new Dosage()
        {
            Text = "1x täglich auf die betroffene Stelle auftragen"
        });
        /*
        prescription.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.1",  // OID of the ordering care organization
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Haus Vogelsang"   // optional
        });
        prescription.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ECHT_SPECHT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.1"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Walter Specht"
        };
        */
        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.3",  // OID of prescribing practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Silvia Spitzmaus"   // optional
        });
        prescription.DispenseRequest = new()
        {
            Dispenser = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.3",  // OID of designated pharmacy
                    System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                },
                Display = "Apotheke 'Zum Linden Wurm'"
            }
        };
        prescription.Identifier.Add(new Identifier()
        {
            Value = "CVF1 23ER USW1",
            System = "eMed-ID"
        });
        prescription.GroupIdentifier = new()
        {
            Value = "ABCD 1234 EFGH",
            System = "eRezept-ID"
        };

        (var postedPMR, var canCue) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, prescription);

        if (canCue)
        {
            Console.WriteLine($"Linca PrescriptionMedicationRequest transmitted, id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
        }

        return canCue;
    }
}
