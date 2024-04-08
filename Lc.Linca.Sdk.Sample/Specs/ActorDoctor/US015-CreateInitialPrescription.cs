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

internal class US015_CreateInitialPrescription : Spec
{
    public const string UserStory = @"
        Renate Rüssel-Olifant had a check-up with her general practitioner Dr. Wibke Würm. 
        Dr. Würm decides to prescribe new medication for Renate Rüssel-Olifant she has not been taking so far. 
        So, Dr. Würm creates an initial prescription for Renate Rüssel-Olifant for two products and sends it to the LINCA system.";

    MedicationRequest initialPresc1 = new();
    MedicationRequest initialPresc2 = new();

    public US015_CreateInitialPrescription(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
            {
            new ("Create an initial prescription", CreateInitialPrescriptionRecord)
            };
    }

    private bool CreateInitialPrescriptionRecord()
    {
        // Linca Prescription Medication Request for drug 1
        initialPresc1.Status = MedicationRequest.MedicationrequestStatus.Active;             // REQUIRED
        initialPresc1.Intent = MedicationRequest.MedicationRequestIntent.OriginalOrder;      // REQUIRED
        initialPresc1.Subject = new ResourceReference()                                      // REQUIRED
        {
            Identifier = new Identifier()
            {
                Value = "1238100866",
                System = Constants.WellknownOidSocialInsuranceNr
            },
            Display = "Renate Rüssel-Olifant"
        };

        initialPresc1.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "1256718",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "Sotacor 80 mg Tabletten"
                    }
                }
            }
        };

        initialPresc1.DosageInstruction.Add(new Dosage()
        {
            Sequence = 1,
            Text = "0-0-1-0",
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

        initialPresc1.InformationSource.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc1.Identifier.Add(new Identifier()
        {
            Value = "1231 RSTO 345G",
            System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"    // OID: eMed-Id
        });

        initialPresc1.GroupIdentifier = new()
        {
            Value = "WABI 0001 VVCC",
            System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
        };

        initialPresc1.DispenseRequest = new()
        {
            Quantity = new()
            {
                Value = 1
            }
        };

        // Linca Prescription Medication Request for drug 2
        initialPresc2.Status = MedicationRequest.MedicationrequestStatus.Active;             // REQUIRED
        initialPresc2.Intent = MedicationRequest.MedicationRequestIntent.OriginalOrder;      // REQUIRED
        initialPresc2.Subject = new ResourceReference()                                      // REQUIRED
        {
            Identifier = new Identifier()
            {
                Value = "1238100866",
                System = Constants.WellknownOidSocialInsuranceNr
            },
            Display = "Renate Rüssel-Olifant"
        };

        initialPresc2.Medication = new()
        {
            Concept = new()
            {
                Coding = new()
                {
                    new Coding()
                    {
                        Code = "4450562",
                        System = "https://termgit.elga.gv.at/CodeSystem/asp-liste",
                        Display = "Luxerm 160 mg/g Creme"
                    }
                }
            }
        };

        initialPresc2.DosageInstruction.Add(new Dosage()
        {
            Text = "morgens und abends auf die betroffene Stelle auftragen",
        });

        initialPresc2.InformationSource.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:ietf:rfc:3986"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc2.Identifier.Add(new Identifier()
        {
            Value = "1231RSTO345G",
            System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"    // OID: eMed-Id
        });

        initialPresc2.GroupIdentifier = new()
        {
            Value = "WABI0001VVCC",
            System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
        };

        initialPresc2.DispenseRequest = new()
        {
            Quantity = new()
            {
                Value = 2
            }
        };

        /***** add the Linca Prescription Medication Requests to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");
        prescriptions.AddResourceEntry(initialPresc2, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine($"Linca PrescriptionMedicationRequestBundle transmitted, created Linca PrescriptionMedicationRequests");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequestBundle");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return canCue;
    }
}
