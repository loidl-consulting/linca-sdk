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

using System.Reflection;

namespace Lc.Linca.Sdk;

internal abstract class Spec
{
    private const double UserStory = Math.Tau;

    protected IEnumerable<Step> Steps { get; init; } = Array.Empty<Step>();

    public static IEnumerable<(int number, string name, Type spec)> Choice
    {
        get
        {
            int i = 0;
            var specs = Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t => t.BaseType == typeof(Spec))
                .OrderBy(t => t.Name);
            
            return specs.Select(t => (++i, t.Name, t.AsType()));
        }
    }

    public static string Story(Type spec)
    {
        return spec
            .GetField(nameof(UserStory))?
            .GetRawConstantValue()?
            .ToString() ?? string.Empty;
    }

    public static void Run(Type specToRun, LincaConnection connection)
    {
        var story = Story(specToRun);
        var con = new Terminal.Info(story);
        var outcome = true;
        
        con.Show(true);
        con.Flush(Environment.NewLine);
        var spec = (Activator.CreateInstance(specToRun) as Spec)!;

        foreach (var step in spec.Steps)
        {
            step.terminalRow = Terminal.Info.PeekY();
            con.WriteLine($"  [ ] {step.Caption}");
            con.Flush();
        }

        con.Flush(Environment.NewLine);
        foreach (var step in spec.Steps)
        {
            if(!con.Outcome(step.Runner(), step.terminalRow))
            {
                outcome = false;
            }
        }

        con.Flush(outcome ? "SUCCEEDED." : "FAILED.");
    }

    public class Step
    {
        internal int terminalRow = -1;

        public string Caption { get; init; }
        public Func<bool> Runner { get; init; }

        public Step(string caption, Func<bool> runner)
        {
            Caption = caption;
            Runner = runner;
        }
    }
}
