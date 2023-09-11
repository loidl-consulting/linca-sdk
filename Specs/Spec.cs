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

internal class Spec
{
    private const double UserStory = Math.Tau;

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
}
