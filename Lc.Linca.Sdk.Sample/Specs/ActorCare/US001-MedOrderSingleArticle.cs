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
using Hl7.Fhir.Support;
using Lc.Linca.Sdk.Scaffolds;
using System.Globalization;

namespace Lc.Linca.Sdk.Specs.ActorCare;

internal class US001_MedOrderSingleArticle : Spec
{
    public const string UserStory = @"
        User Susanne Allzeit (DGKP) is an employee at the mobile caregiver organization Pflegedienst Immerdar, 
        whose client, Renate Rüssel-Olifant, is not in the LINCA system yet. 
        Hence, Susanne Allzeit creates a client record in the system.
        Now, it is possible to order prescriptions for Renate Rüssel-Olifant. 
        As Susanne Allzeit will pick up the medication on the go, she places the order 
        without specifying a pharmacy.";

    public Patient createdPatient = new Patient();
    protected MedicationRequest medReq = new();

    public US001_MedOrderSingleArticle(LincaConnection conn) : base(conn)
    {


        Steps = new Step[]
        {
            new("Create client record", CreateClientRecord),
            new("Place order with no pharmacy specified", CreateRequestOrchestrationRecord)
        };
    }

    /// <summary>
    /// As an actor who is an order placer,
    /// it is necessary to ensure that all patient records
    /// which later occur in the order position(s), are present
    /// as FHIR resources on the linked care server.
    /// 
    /// This is where an actual care information system
    /// would fetch the client data from its database, 
    /// and convert it into a FHIR R5 resource
    /// </summary>
    private bool CreateClientRecord()
    {
        var client = CareInformationSystem.GetClient();
        var patient = new Patient
        {
            BirthDate = DateTime.ParseExact(
                client.DoB,
                Constants.DobFormat,
                CultureInfo.InvariantCulture
            ).ToFhirDate()
        };

        patient.Name.Add(new()
        {
            Family = client.Lastname,
            Given = new[] { client.Firstname },
            Text = client.Firstname + " " + client.Lastname
        });

        patient.Identifier.Add(new Identifier(
            system: Constants.WellknownOidSocialInsuranceNr,
            value: client.SocInsNumber
        ));
        patient.Gender = AdministrativeGender.Female;

        (createdPatient, var canCue) = LincaDataExchange.CreatePatient(Connection, patient);
       
        if(canCue)
        {
            Console.WriteLine($"Client information transmitted, id {createdPatient.Id}");
        }
        else
        {
            Console.WriteLine("Failed to transmit client information");
        }

        return canCue;
    }

    private bool OrderAnyPharmacy()
    {
        var client = CareInformationSystem.GetClient();
        if(client.ClientId == Guid.Empty)
        {
            Console.WriteLine("No client Id has been registered in the care information system");

            return false;
        }

        // populate the order position
        var orderPosition = new MedicationRequest()
        {
            // the client for whom the medication is ordered
            Subject = new()
            {

            },
            // the practitioner who will be asked to issue the prescription
            Performer = new()
            {

            },
            // the product ordered
            Medication = new()
            {

            }
        };


        // populate the order header
        var order = new RequestOrchestration()
        {
            Contained = new(new[] { orderPosition })
        };

        (var createdOrder, var canCue) = LincaDataExchange.PlaceOrder(Connection, order);

        if (canCue)
        {
            Console.WriteLine($"Order created, id {createdOrder.Id}");
        }
        else
        {
            Console.WriteLine("Failed to create order");
        }

        return canCue;
    }

    private void PrepareOrderMedicationRequest()
    {
        medReq.Id = Guid.NewGuid().ToFhirId();                                  // REQUIRED    
        medReq.Status = MedicationRequest.MedicationrequestStatus.Unknown;      // REQUIRED
        medReq.Intent = MedicationRequest.MedicationRequestIntent.Proposal;     // REQUIRED
        medReq.Subject = new ResourceReference()                                // REQUIRED
        {
            Reference = $"HL7ATCorePatient/{createdPatient.Id}"     // relative path to Linca Fhir patient resource
            //Reference = "HL7ATCorePatient/c4313cca3e5b4cda89053630b5caae8d"
        };
        medReq.Medication = new()
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
        medReq.InformationSource.Add(new ResourceReference()  // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Pflegedienst Immerdar"   // optional
        });
        medReq.Requester = new ResourceReference()  // REQUIRED
        {
            Identifier = new()
            {
                Value = "ALLZEIT_BEREIT",               // e.g., org internal username or handsign of Susanne Allzeit
                System = "urn:oid:2.999.40.0.34.1.1.3"  // Code-System: Care-Org Pflegedienst Immerdar
            },
            Display = "DGKP Susanne Allzeit"
        };
        medReq.Performer.Add(new ResourceReference()   // REQUIRED, cardinality 1..1 in LINCA
        {
            Identifier = new()
            {
                Value = "2.999.40.0.34.3.1.1",  // OID of designated practitioner 
                System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
            },
            Display = "Dr. Wibke Würm"   // optional
        });
    }

    private bool CreateRequestOrchestrationRecord ()
    {
        // first prepare a LincaOrderMedicationRequest to be contained in the LincaRequestOrchestration
        PrepareOrderMedicationRequest();

        RequestOrchestration ro = new()
        {
            Status = RequestStatus.Active,      // REQUIRED
            Intent = RequestIntent.Proposal,       // REQUIRED
            Subject = new ResourceReference()   // REQUIRED
            {
                Identifier = new()
                {
                    Value = "2.999.40.0.34.1.1.3",  // OID of the ordering care organization from certificate
                    System = "urn:oid:1.2.40.0.34.5.2"  // Code-System: eHVD
                },
                Display = "Pflegedienst Immerdar"   // optional
            }
        };

        ro.Contained.Add(medReq);

        var action = new RequestOrchestration.ActionComponent()
        {
            //Type = 
            Resource = new ResourceReference($"#{medReq.Id}")
        };

        ro.Action.Add( action );

       
        (var createdRO, var canCue) = LincaDataExchange.CreateRequestOrchestration(Connection, ro);

        if (canCue)
        {
            Console.WriteLine($"Linca Request Orchestration transmitted, id {createdRO.Id}");
        }
        else
        {
            Console.WriteLine($"Failed to transmit Linca Request Orchestration");
        }

        return canCue;
    }
}
