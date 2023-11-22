using System.Dynamic;
using System.Reflection;
using System.Security;

namespace Lc.Linca.Sdk.Sample.Resources;

internal class ResourceProxy
{
    /// <summary>
    /// Returns #1 of the actor: care pseudocertificates for development and test purposes,
    /// as a byte array - to show by example, how a client certificate that is loaded from 
    /// a database could be passed to the SDK's Connect() function
    /// (This certificate will not work in a staging or production environment)
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    internal static byte[] linca_pflegeeinrichtung_001_dev
#pragma warning restore IDE1006 // Naming Styles
    {
        get
        {
            using var certBytes = new MemoryStream();

            Assembly
                .GetExecutingAssembly()!
                .GetManifestResourceStream(typeof(ResourceProxy), $"{nameof(linca_pflegeeinrichtung_001_dev)}.pfx")!
                .CopyTo(certBytes);

            return certBytes.ToArray();
        }
    }

    /// <summary>
    /// This private key password is not actually secret for the purposes
    /// of the Linked Care Software Development Kit, because it is used with
    /// the temporary pseudo certificates for test purposes. In an actual
    /// client implementation this would have to come from some kind of
    /// secure password store (or use the operating system's certificate store,
    /// preferably).
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    internal static SecureString pk_pw
#pragma warning restore IDE1006 // Naming Styles
    {
        get
        {
            var pkPw = new SecureString();

            pkPw.AppendChar('V');
            pkPw.AppendChar('W');
            pkPw.AppendChar('n');
            pkPw.AppendChar('C');
            pkPw.AppendChar('t');
            pkPw.AppendChar('T');
            pkPw.AppendChar('6');
            pkPw.AppendChar('9');
            pkPw.AppendChar('4');
            pkPw.AppendChar('n');
            pkPw.AppendChar('k');
            pkPw.AppendChar('3');
            pkPw.AppendChar('T');
            pkPw.AppendChar('e');
            pkPw.AppendChar('L');

            return pkPw;
        }
    }
}
