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

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US011_PrescribeAsOrdered : Spec
{
    public const string UserStory = @"
        Practitioner Dr. Wibke Würm is responsible for the LINCA registered mobile caregiver client Renate Rüssel-Olifant. 
        She has received a LINCA order position requesting medication prescription for her.
        She decides to issue a prescription for the medication for Renate Rüssel-Olifant intended by that order position. 
        Hence, she submits a prescription for that position with the eMedId and eRezeptId she got
          and her software will send that to the LINCA server,
          and the ordering mobile caregiver organization Pflegedienst Immerdar will be informed that the order position has been prescribed as ordered,
          and they will inform DGKP Susanne Allzeit.";

    protected MedicationRequest prescription = new();

    public US011_PrescribeAsOrdered(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
            {
            new("Create PrescriptionMedicationRequest as ordered", CreatePrescriptionRecord)
            };
    }

    private bool CreatePrescriptionRecord()
    {
        prescription.BasedOn.Add(new()
        {
            Reference = "LINCAProposalMedicationRequest/d2359858cb6640faa9d1f8a69214bada"
        });
        prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
        prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
        prescription.Subject = new ResourceReference()                             // REQUIRED
        {
            Reference = "HL7ATCorePatient/c42a921b95004b87a7d19fa59cf64c81"     // relative path to Linca Fhir patient resource
        };
        prescription.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                    {
                        new Coding()
                        {
                            Code = "0031130",
                            System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                            Display = "Lasix 40 mg Tabletten"
                        }
                    }
            }
        };
        prescription.DosageInstruction.Add(new Dosage()
        {
            Sequence = 1,
            Text = "1 Tablette täglich",
            Timing = new Timing()
            {
                Repeat = new()
                {
                    Bounds = new Duration
                    {
                        Value = 1,
                        Code = "d",

                    },
                    Frequency = 1,
                    Period = 1,
                    PeriodUnit = Timing.UnitsOfTime.D
                }
            },
            DoseAndRate = new()
            {
                new Dosage.DoseAndRateComponent()
                {
                    Dose = new Quantity(value: 1, unit: "St")
                }
            }
        });

        // prescription.InformationSource will be copied from resource in basedOn by the Fhir server
        // prescription.Requester will be copied from resource in basedOn by the Fhir server
        // prescription.SupportingInformation will be copied from resource in basedOn by the Fhir server

        prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
        prescription.Identifier.Add(new Identifier()
        {
            Value = "XYZ1 ABC2 UVW3",
            System = "eMed-ID"
        });
        prescription.GroupIdentifier = new()
        {
            Value = "ASDF GHJ4 KL34",
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
