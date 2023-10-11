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

namespace Lc.Linca.Sdk.Client;

internal class Program
{
    /// <summary>
    /// In SDK, this always points to the development system
    /// </summary>
    // internal const string FhirServerBaseUrl = "https://localhost:8084";
    internal const string FhirServerBaseUrl = "https://fhir5-d.linkedcare.at";

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
        Console.WriteLine("Linked Care Software Development Kit");
        Console.WriteLine("Verbindung mit FHIR Server wird initiiert...");

        var connection = LincaConnector.Connect(FhirServerBaseUrl);
        var con = new Terminal.Info();
        var outcome = false;

        if (LincaConnector.NegotiateCapabilities(connection))
        {
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
        if (short.TryParse(selectedNumber, out var storyNumber) && storyNumber > 0 && storyNumber <= availableSpecs.Count)
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
}