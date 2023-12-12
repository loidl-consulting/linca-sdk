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
using Hl7.Fhir.Utility;
using Lc.Linca.Sdk.Client;

namespace Lc.Linca.Sdk.Specs.ActorDoctor;

internal class US012_PrescribeWithChanges : Spec
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

    public US012_PrescribeWithChanges(LincaConnection conn) : base(conn) 
    {
        Steps = new Step[]
        {
            new("Create PrescriptionMedicationRequest with changed medication", CreatePrescriptionRecord)
        };
    }

    private bool CreatePrescriptionRecord()
    {
        LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseRetrieve();

        (Bundle orders, bool received) = LincaDataExchange.GetProposalsToPrescribe(Connection);

        if (received)
        {
            List<MedicationRequest> proposals = BundleHelper.FilterProposalsToPrescribe(orders);


            MedicationRequest? proposalsGuenterNotGranpidam = proposals.Find(x => x.Subject.Reference.Contains($"{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}") 
                                                                                        && ! x.Medication.Concept.Coding.First().Display.Contains("Granpidam"));

            if (proposalsGuenterNotGranpidam != null)
            {
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter = proposalsGuenterNotGranpidam.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();
            }
            else
            {
                Console.WriteLine($"Linca ProposalMedicationRequest for Guenter not found, prescription cannot be created");

                return false;
            }

            prescription.BasedOn.Add(new ResourceReference()
            {
                Reference = $"LINCAProposalMedicationRequest/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.OrderProposalIdGuenter}"
            });

            prescription.Status = MedicationRequest.MedicationrequestStatus.Active;    // REQUIRED
            prescription.Intent = MedicationRequest.MedicationRequestIntent.Order;     // REQUIRED
            prescription.Subject = new ResourceReference()                             // REQUIRED
            {
                Reference = $"HL7ATCorePatient/{LinkedCareSampleClient.CareInformationSystemScaffold.Data.ClientIdGuenter}"     // relative path to Linca Fhir patient resource, copy from order
            };

            prescription.Medication = new() // the doctor changes the medication to a ready-to-use ointment
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

            // prescription.InformationSource.Add(new ResourceReference()  // will be copied from reference in basedOn
            // prescription.Requester = new ResourceReference()  //will be copied from reference in basedOn
            // prescription.DispenseRequest.Dispenser // will be copied from reference in basedOn, if available

            prescription.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.3.1.3",  // OID of designated practitioner 
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Dr. Silvia Spitzmaus"   // optional
            });

            prescription.Identifier.Add(new Identifier()
            {
                Value = "CVF1 23ER USW1",
                System = "urn:oid:1.2.40.0.10.1.4.3.4.2.1"     // OID: eMed-Id
            });

            prescription.GroupIdentifier = new()
            {
                Value = "ABCD 1234 EFGH",
                System = "urn:oid:1.2.40.0.10.1.4.3.3"        // OID: Rezeptnummer
            };

            Bundle prescriptions = new()
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new()
            };

            prescriptions.AddResourceEntry(prescription, $"{Connection.ServerBaseUrl}/{LincaEndpoints.LINCAPrescriptionMedicationRequest}");

            (Bundle results, var canCue, var outcome) = LincaDataExchange.CreatePrescriptionBundle(Connection, prescriptions);

            if (canCue)
            {
                Console.WriteLine($"Linca PrescriptionMedicationRequestBundle transmitted, created Linca PrescriptionMedicationRequest");
                LinkedCareSampleClient.CareInformationSystemScaffold.Data.PrescriptionWithChangesGuenter = results.Entry.First().Resource.Id;
                LinkedCareSampleClient.CareInformationSystemScaffold.PseudoDatabaseStore();

                BundleHelper.ShowOrderChains(results);
            }
            else
            {
                Console.WriteLine($"Failed to transmit Linca PrescriptionMedicationRequest");
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
            Console.WriteLine($"Failed to receive Linca ProposalMedicationRequest for Guenter");

            return false;
        }
    }
}
