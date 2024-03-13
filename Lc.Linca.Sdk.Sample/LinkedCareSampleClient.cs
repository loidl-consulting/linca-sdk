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

using Lc.Linca.Sdk.Sample.Resources;
using Lc.Linca.Sdk.Scaffolds;
using System.Security.Cryptography.X509Certificates;

namespace Lc.Linca.Sdk.Client;

internal static class LinkedCareSampleClient
{
    /// <summary>
    /// In SDK, this always points to the development system
    /// </summary>
    // internal const string FhirServerBaseUrl = "https://fhir5-q.linkedcare.at";
    internal const string FhirServerBaseUrl = "https://hackathon-r5.pineit.at/pineit/pitdata-fhir/fhir";
    //internal const string FhirServerBaseUrl = "https://localhost:8084";

    internal static CareInformationSystem CareInformationSystemScaffold = new();
    internal static PractitionerInformationSystem PractitionerInformationSystemScaffold = new();
    internal static PharmacyInformationSystem PharmacyInformationSystemScaffold = new();

    private const string CommandLineArgumentUseEmbeddedCert = "--use-embedded-cert";
    private const int ExitCodeCouldNotConnect = 0xaca1;
    private const int ExitCodeIdle = 0x1;
    private const int ExitCodeSuccess = 0x0;

    static int Main(string[] _)
    {
        var exitCode = ExitCodeIdle;
        var connection = Blurb();

        if (connection.Succeeded)
        {
            do
            {
                PrintConnectedSequences();
                var specToRun = SelectSpecToRun();
                if (specToRun == null)
                {
                    Console.WriteLine("Keine Spezifikation ausgewählt.");
                }
                else
                {
                    exitCode = ExitCodeSuccess;
                    Spec.Run(specToRun, connection);
                }
            } while (KeepGoing());
        }
        else
        {
            exitCode = ExitCodeCouldNotConnect;
        }

        if (Environment.UserInteractive)
        {
            Console.ReadKey();
        }

        return exitCode;
    }

    private static LincaConnection Blurb()
    {
        X509Certificate2? clientCertificate = null;

        Console.WriteLine("Linked Care Software Development Kit");
        Console.WriteLine("Verbindung mit FHIR Server wird initiiert...");

        if(Environment.GetCommandLineArgs().Contains(CommandLineArgumentUseEmbeddedCert))
        {
            /* this shows how the certificate can be passed to the connection
             * when it is provided as a byte array (when loaded from database or a file) */
            clientCertificate = new(ResourceProxy.linca_pflegeeinrichtung_001_dev, ResourceProxy.pk_pw);
        }

        var connection = LincaConnector.Connect(FhirServerBaseUrl, clientCertificate);
        var con = new Terminal.Info();
        var outcome = false;
        var (negotiated, capabilityStatement) = LincaConnector.NegotiateCapabilities(connection);
        if (negotiated == CapabilityNegotiationOutcome.Succeeded && capabilityStatement != null)
        {
            Terminal.Info info = new();
            var name = capabilityStatement
                .Name
                .Replace(Constants.ServerProductLead, $"{Constants.AnsiColorFire}{Constants.ServerProductLead}{Constants.AnsiReset}")
                .Replace(Constants.ServerProductTail, $"{Constants.AnsiColorCaat}{Constants.ServerProductTail}{Constants.AnsiReset}");

            var desc = capabilityStatement
                .Description
                .Replace(Constants.ManufacturerName, $"{Constants.AnsiColorMaroon}{Constants.ManufacturerName}{Constants.AnsiReset}");

            info.WriteLine($@"{name}, FHIR version {capabilityStatement.FhirVersion}");
            info.WriteLine($@"Statement from {capabilityStatement.Date} supporting {capabilityStatement.Rest.First().Resource.Count} resources");
            info.HorizontalRule();
            info.WriteLine($@"Connected to {desc}, version {capabilityStatement.Version}");
            Console.Clear();
            info.Show();
            outcome = true;
            con.Flush(Environment.NewLine);
            var row1 = con.WriteLine($"  [ ] Version des LINCA Servers", true);
            var row2 = con.WriteLine($"  [ ] Reauthentifizierung bei Tokenablauf", true);
            var row3 = con.WriteLine($"  [ ] Unterstützte Ressourcen", true);

            var testVersion = connection.Capabilities.FhirVersion?.ToString() == "N5_0_0";
            if (!con.Outcome(testVersion, row1))
            {
                outcome = false;
            }

            var testReauth = TestReauthentication(connection);
            if (!con.Outcome(testReauth, row2))
            {
                outcome = false;
            }

            var testResources = connection.Capabilities.Rest.First().Resource.Count >= 5;
            if (!con.Outcome(testResources, row3))
            {
                outcome = false;
            }
        }
        else
        {
            switch(negotiated)
            {
                case CapabilityNegotiationOutcome.NotConnected:
                    Console.Error.WriteLine("Not connected");
                    break;

                case CapabilityNegotiationOutcome.Unauthorized:
                    Console.Error.WriteLine("Unauthorized");
                    break;

                case CapabilityNegotiationOutcome.CouldNotParse:
                    Console.Error.WriteLine("Could not parse the FHIR Server's capability statement");
                    break;

                default:
                    Console.Error.WriteLine("Failed to negotiate capabilities");
                    break;
            }
        }

        con.Flush(Environment.NewLine);
        if (!outcome)
        {
            Console.Error.WriteLine("Failed to negotiate FHIR capabilities");
        }

        return connection;
    }

    private static bool TestReauthentication(LincaConnection connection)
    {
        connection.JavaWebToken = "test_invalidated_token";
        try
        {
            using var httpFailing = connection.GetAuthenticatedClient();
            var failRequest = new HttpRequestMessage(HttpMethod.Options, $"{connection.ServerBaseUrl}/");
            failRequest.Headers.Accept.Add(Constants.FhirJson);
            using var failResponse = httpFailing.Send(failRequest);

            if(failResponse.IsSuccessStatusCode)
            {
                return false;
            }

            connection.Reauthenticate();
            using var httpGood = connection.GetAuthenticatedClient();
            var goodRequest = new HttpRequestMessage(HttpMethod.Options, $"{connection.ServerBaseUrl}/");
            goodRequest.Headers.Accept.Add(Constants.FhirJson);
            using var goodResponse = httpGood.Send(goodRequest);

            return goodResponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static Type? SelectSpecToRun()
    {
        Console.WriteLine("Wählen Sie eine Spezifikation aus, um einen Testlauf zu starten:\n");
        var availableSpecs = Spec.Choice.ToList();
        foreach ((var number, var name, var _) in availableSpecs)
        {
            Console.WriteLine($"[{number:D2}] {name}");
        }

        Console.Write("\nSpezifikation Nr: [");
        var caretLeft = Console.CursorLeft;
        Console.Write("  ] (Start mit Eingabetaste)");
        Console.SetCursorPosition(caretLeft, Console.CursorTop);
        var selectedNumber = Console.ReadLine();
        Console.Clear();
        if (short.TryParse(selectedNumber, out var storyNumber) && storyNumber >= 0 && storyNumber <= availableSpecs.Count)
        {
            return availableSpecs.FirstOrDefault(s => s.number == storyNumber).spec;
        }

        return null;
    }

    private static bool KeepGoing()
    {
        Console.Write("\nNeuen Durchlauf starten? [");
        var caretLeft = Console.CursorLeft;
        Console.Write(" ] (J/N)");
        Console.SetCursorPosition(caretLeft, Console.CursorTop);
        if(Console.ReadKey().KeyChar.ToString().ToLowerInvariant() == "j")
        {
            Console.Clear();

            return true;
        }

        return false;
    }

    private static void PrintConnectedSequences()
    {
        Console.WriteLine("Folgende Abhängigkeiten bestehen zwischen den Testfällen [00] [07] [08] [09] [10]:");
        Console.WriteLine("[00] (Zertifikat Haus Vogelsang -> [07] (Zertifikat Dr. Spitzmaus) -> [08] (Zertifikat Dr. Spitzmaus) -> [10] (Apotheke Zum Fruehen Vogel)");
        Console.WriteLine("[00] (Zertifikat Haus Vogelsang -> [09] (Zertifikat Dr. Spitzmaus)");
        Console.WriteLine("Alle anderen Testfälle können voneinander unabhängig mit dem entsprechenden Zertifikat ausgeführt werden");
        Console.WriteLine("");
    }
}