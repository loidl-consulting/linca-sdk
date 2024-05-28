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
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorPharmacy;

internal class US019_PartialDispense : Spec
{
    public const string UserStory = @"
        Pharmacist Mag. Andreas Amsel, owner of the pharmacy Apotheke 'Zum frühen Vogel' has 
        access to and permission in a pharmacist role in the LINCA system. 
        When he is expected to fullfil medication orders for a customer, e.g., Renate Rüssel-Olifant, 
        and he has a LINCA order Id to go with a purchase her care giver Susanne Allzeit just made for her, 
        and he did not, or is not able to, dispense all of the product at once         
        then Mag. Andreas Amsel submits a partial dispense record for the order position in question
          and his software will send that to the LINCA server,
          and notify the ordering organization, Pflegedienst Immerdar, about the partial dispense.";

    protected MedicationDispense dispenseP = new();
    protected MedicationDispense dispenseC = new();


    public US019_PartialDispense(LincaConnection conn) : base(conn)
    {
        Steps = new Step[]
        {
            new ("Dispense prescription partially", CreatePartialDispense),
            new ("Complete the partial dispense", CreateCompleteDispense)
        };
    }

    private bool CreatePartialDispense()
    {
        (Bundle results, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "WABI0001VVCC");

        if (received)
        {
            List<MedicationRequest> prescriptions = BundleHelper.FilterPrescriptionsToDispense(results);
            MedicationRequest? prescription = prescriptions.Find(x => x.Medication.Concept.Coding.First().Display.Contains("Luxerm"));

            if (prescription != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm = prescription.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine("Prescription for Renate Rüssel-Olifant not found, cannot create dispense");

                return false;
            }



            if (!string.IsNullOrEmpty(LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm))
            {
                dispenseP.AuthorizingPrescription.Add(new()
                {
                    Reference = $"LINCAPrescriptionMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm}"
                });

                dispenseP.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
                dispenseP.Subject = new ResourceReference()
                {
                    Identifier = new Identifier()
                    {
                        Value = "1238100866",
                        System = Constants.WellknownOidSocialInsuranceNr
                    },
                    Display = "Renate Rüssel-Olifant"
                };

                dispenseP.Medication = new()
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

                dispenseP.Quantity = new() { Value = 1 };

                dispenseP.DosageInstruction.Add(new Dosage()
                {
                    Text = "morgens und abends auf die betroffene Stelle auftragen",
                });

                dispenseP.Performer.Add(new()
                {
                    Actor = new()
                    {
                        Identifier = new()
                        {
                            Value = "2.999.40.0.34.5.1.2",  // OID of dispensing pharmacy
                            System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                        },
                        Display = "Apotheke 'Zum frühen Vogel'"
                    }
                });

                dispenseP.Type = new()
                {
                    Coding = new()
                    {
                        new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "FFP")
                    }
                };

                (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispenseP);

                if (canCue)
                {
                    Console.WriteLine($"Linca MedicationDispense (type FFP) transmitted, id {postedMD.Id}");
                }
                else
                {
                    Console.WriteLine($"Failed to transmit Linca MedicationDispense");
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
            else
            {
                Console.WriteLine($"Initial prescription (Luxerm for Renate Rüssel-Oilfant) not found");

                return false;
            }
        }
        else
        {
            Console.WriteLine($"Get prescription-to-dispense failed");

            return false;
        }
    }

    private bool CreateCompleteDispense()
    {
        (Bundle results, bool received) = LincaDataExchange.GetPrescriptionToDispense(Connection, "WABI0001VVCC");

        if (received)
        {
            List<MedicationRequest> prescriptions = BundleHelper.FilterPrescriptionsToDispense(results);
            MedicationRequest? prescription = prescriptions.Find(x => x.Id == LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm);

            // Check if the partially dispensed prescription is still in the result list
            if (prescription == null)
            {
                Console.WriteLine("Prescription for Renate Rüssel-Olifant not found any more, cannot complete dispense");

                return false;
            }

            if (!string.IsNullOrEmpty(LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm))
            {
                dispenseC.AuthorizingPrescription.Add(new()
                {
                    Reference = $"LINCAPrescriptionMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionIdRenateLuxerm}"
                });

                dispenseC.Status = MedicationDispense.MedicationDispenseStatusCodes.Completed;
                dispenseC.Subject = new ResourceReference()
                {
                    Identifier = new Identifier()
                    {
                        Value = "1238100866",
                        System = Constants.WellknownOidSocialInsuranceNr
                    },
                    Display = "Renate Rüssel-Olifant"
                };

                dispenseC.Medication = new()
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

                dispenseC.Quantity = new() { Value = 1 };  // 1 remaining package

                dispenseC.DosageInstruction.Add(new Dosage()
                {
                    Text = "morgens und abends auf die betroffene Stelle auftragen",
                });

                dispenseC.Performer.Add(new()
                {
                    Actor = new()
                    {
                        Identifier = new()
                        {
                            Value = "2.999.40.0.34.5.1.2",  // OID of dispensing pharmacy
                            System = "urn:ietf:rfc:3986"  // Code-System: eHVD
                        },
                        Display = "Apotheke 'Zum frühen Vogel'"
                    }
                });

                dispenseC.Type = new()
                {
                    Coding = new()
                    {
                        new Coding(system: "http://terminology.hl7.org/CodeSystem/v3-ActCode", code: "RFC")
                    }
                };

                (var postedMD, var canCue, var outcome) = LincaDataExchange.CreateMedicationDispense(Connection, dispenseC);

                if (canCue)
                {
                    Console.WriteLine($"Linca MedicationDispense (type RFC) transmitted, id {postedMD.Id}");
                }
                else
                {
                    Console.WriteLine($"Failed to transmit Linca MedicationDispense (type partial dispense)");
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
            else
            {
                Console.WriteLine($"Initial prescription (Luxerm for Renate Rüssel-Oilfant) not found");

                return false;
            }
        }
        else
        {
            Console.WriteLine($"Get prescription-to-dispense failed");

            return false;
        }
    }
}
