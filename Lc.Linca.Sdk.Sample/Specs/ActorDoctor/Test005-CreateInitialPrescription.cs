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
using Hl7.Fhir.Support;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class Test005_CreateInitialPrescriptionBundle : Spec
{
    public const string UserStory = @"
        Run this testcase with the certificate of Dr. Wibke Würm";

    MedicationRequest initialPresc1 = new();
    MedicationRequest initialPresc2 = new();

    public Test005_CreateInitialPrescriptionBundle(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
            {
            new ("Create initial prescription bundle, LCVAL29, performer required", InitialPrescriptionLCVAL29),
            new ("Create initial prescription bundle, LCVAL55, performer OID required", InitialPrescriptionLCVAL55),
            new ("Create initial prescription bundle, LCVAL66, performer OID different to certificate", InitialPrescriptionLCVAL66),
            new ("Create initial prescription bundle, LCVAL26, medication required", InitialPrescriptionLCVAL26),
            new ("Create initial prescription bundle, LCVAL07, intent required", InitialPrescriptionLCVAL07),
            new ("Create initial prescription bundle, LCVAL67, intent originalOrder, but priorPrescription not null", InitialPrescriptionLCVAL67),
            new ("Create initial prescription bundle, LCVAL08, status required", InitialPrescriptionLCVAL08),
            new ("Create initial prescription bundle, LCVAL59, status active required", InitialPrescriptionLCVAL59),
            new ("Create initial prescription bundle, LCVAL09A, subject required", InitialPrescriptionLCVAL09A),
            new ("Create initial prescription bundle, LCVAL09B, subject svnr required", InitialPrescriptionLCVAL09B),
            new ("Create initial prescription bundle, LCVAL68, subject cannot differ in Bundle", InitialPrescriptionLCVAL68),
            new ("Create initial prescription bundle, LCVAL69, groupIdentifier cannot differ in Bundle", InitialPrescriptionLCVAL69),
            new ("Create initial prescription bundle, LCVAL70, initial prescription cannot have lc_id", InitialPrescriptionLCVAL70),
            new ("Create initial prescription bundle, LCVAL71, initial prescriptions must be sent in Bundle", InitialPrescriptionLCVAL71),
            new ("Create Bundle of initial prescriptions successfully", CreateInitialPrescriptionSuccess)
            };
    }

    private bool InitialPrescriptionLCVAL29()
    {
        // create initial Linca Prescription Medication Request
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
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        /*
        initialPresc1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
        */ 

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

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return ! canCue;
    }

    private bool InitialPrescriptionLCVAL55()
    {
        initialPresc1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                // Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
        
        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL66()
    {
        initialPresc1.Performer.Clear();
        initialPresc1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.2",  // a doctors OID, but the wrong one
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL26()
    {
        initialPresc1.Performer.Clear();
        initialPresc1.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of Wibke Würm
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc1.Medication = null;

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL07()
    {
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

        initialPresc1.Intent = null;

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL67()
    {
        initialPresc1.Intent = MedicationRequest.MedicationRequestIntent.OriginalOrder;

        initialPresc1.PriorPrescription = new() { Reference = $"{LincaEndpoints.LINCAPrescriptionMedicationRequest}/{Guid.NewGuid().ToFhirId()}" };

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL08()
    {
        initialPresc1.PriorPrescription = null;

        initialPresc1.Status = null;

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL59()
    {
        initialPresc1.Status = MedicationRequest.MedicationrequestStatus.Stopped; // this is not allowed in initial prescriptions

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL09A()
    {
        initialPresc1.Status = MedicationRequest.MedicationrequestStatus.Active; // this is not allowed in initial prescriptions

        initialPresc1.Subject = null;

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL09B()
    {
        initialPresc1.Subject = new ResourceReference()   // REQUIRED
        {
            Identifier = new Identifier()
            {
                // Value = "1238100866",
                System = Constants.WellknownOidSocialInsuranceNr
            },
            Display = "Renate Rüssel-Olifant"
        };

        /***** add the Linca Prescription Medication Request to a Bundle for transaction ****************/
        Bundle prescriptions = new()
        {
            Type = Bundle.BundleType.Transaction,
            Entry = new()
        };

        prescriptions.AddResourceEntry(initialPresc1, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

        (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL68()
    {
        initialPresc1.Subject = new ResourceReference()                                      // REQUIRED
        {
            Identifier = new Identifier()
            {
                Value = "1148070771",                  // Patient SVNR cannot differ from second prescription in the Bundle
                System = Constants.WellknownOidSocialInsuranceNr
            },
            Display = "Patrizia Platypus"
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
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc2.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });

        initialPresc2.Identifier.Add(new Identifier()
        {
            Value = "1231 RSTO 345G",
            System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"    // OID: eMed-Id
        });

        initialPresc2.GroupIdentifier = new()
        {
            Value = "WABI 0001 VVCC",
            System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
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
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL69()
    {
        initialPresc1.Subject = new ResourceReference()                                      // REQUIRED
        {
            Identifier = new Identifier()
            {
                Value = "1238100866",                  // Patient SVNR cannot differ from second prescription in the Bundle
                System = Constants.WellknownOidSocialInsuranceNr
            },
            Display = "Renate Rüssel-Olifant"
        };

        // Linca Prescription Medication Request for drug 2
        
        initialPresc2.GroupIdentifier = new()
        {
            Value = "WABI 0001 AAAA",        // eRezeptId different to second prescription in Bundle
            System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
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
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL70()
    {
        // Linca Prescription Medication Request for drug 2
        initialPresc2.GroupIdentifier = new()
        {
            Value = "WABI 0001 VVCC",        
            System = "urn:oid:1.2.40.0.10.1.4.3.3"       // OID: Rezeptnummer
        };

        initialPresc2.SupportingInformation.Add(new() { Reference = $"{LincaEndpoints.LINCARequestOrchestration}/{Guid.NewGuid().ToFhirId()}" });

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
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");

            BundleHelper.ShowOrderChains(results);
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }

    private bool InitialPrescriptionLCVAL71()
    {
        (var postedPMR, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionMedicationRequest(Connection, initialPresc1);

        //(Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

        if (canCue)
        {
            Console.WriteLine("Validation did not work properly: OperationOutcome excpected");
            Console.WriteLine($"Created PrescriptionMedicationRequest with Id {postedPMR.Id}");
        }
        else
        {
            Console.WriteLine("Validation result:");
        }

        if (outcome != null)
        {
            foreach (var item in outcome.Issue)
            {
                Console.WriteLine($"Outcome Issue Code: '{item.Details.Coding?.FirstOrDefault()?.Code}', Text: '{item.Details.Text}'");
            }
        }

        return !canCue;
    }
    private bool CreateInitialPrescriptionSuccess()
    {
        // Linca Prescription Medication Request for drug 2
        initialPresc2.SupportingInformation.Clear();

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
