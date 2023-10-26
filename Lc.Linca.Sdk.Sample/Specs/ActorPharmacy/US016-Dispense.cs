﻿/***********************************************************************************
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

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US016_Dispense : Spec
{
    protected MedicationDispense dispense = new();

    public const string UserStory = @"
        Pharmacist Mag. Andreas Amsel, owner of the pharmacy Apotheke 'Zum frühen Vogel' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When he is expected to fullfil medication orders for a customer, e.g., Renate Rüssel-Olifant, 
        and he has a LINCA order Id to go with a purchase her care giver Susanne Allzeit just made for her, 
        then Mag. Andreas Amsel submits a dispense record for the order position in question
          and his software will send that to the LINCA server,
          and notify the ordering organization, Pflegedienst Immerdar, about the thus completed order position.";

    public US016_Dispense(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
            {
            new("Create MedicationDispense", CreateMedicationDispenseRecord)
            };

    }

    private bool CreateMedicationDispenseRecord()
    {
        dispense.AuthorizingPrescription.Add(new()
        {
            Reference = "LINCAPrescriptionMedicationRequest/61b25ab21d4e4492ab460127fbb03b1f"
        });
        dispense.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
        dispense.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = "HL7ATCorePatient/5f16757cdbb04496b586862b7d6159f2"    // relative path to Linca Fhir patient resource
        };
        dispense.Medication = new()
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
        dispense.DosageInstruction.Add(new Dosage()
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
                },
            }
        });
        dispense.Performer.Add(new()
        {
            Actor = new()
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.5.1.2",  // OID of dispensing pharmacy
                    System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
                },
                Display = "Apotheke 'Zum frühen Vogel'"
            }
        });
        dispense.Type = new()
        {
            Coding = new()
            {
                new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFC")
            }
        };

        (var postedMD, var canCue) = LincaDataExchange.CreateMedicationDispense(Connection, dispense);

        if (canCue)
        {
            Console.WriteLine($"Linca MedicationDispense transmitted, id {postedMD.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca MedicationDispense");
        }

        return canCue;
    }
}
