namespace Lc.Linca.Sdk.Scaffolds;

/// <summary>
/// An abstract representation of a software product
/// that implementers provide and where they host
/// the Linked Care client in, and with which they
/// make the Linked Care system available to their users
/// </summary>
internal abstract class ActorSoftwareScaffold
{
    internal enum PseudoDatabaseField
    {
        PatientId = 0
    }

    internal static void PseudoDatabaseStore(string db, PseudoDatabaseField field, Guid value)
    {
        var record = new byte[16 * typeof(PseudoDatabaseField).GetFields().Length];

        value.ToByteArray().CopyTo(record, 16 * (short)field);
        File.WriteAllBytes($".{db}.tmp.db", record);
    }

    internal static Guid PseudoDatabaseRetrieve(string db, PseudoDatabaseField field)
    {
        try
        {
            var content = File.ReadAllBytes($".{db}.tmp.db");

            return new(content.Skip((short)field * 16).Take(16).ToArray());
        }
        catch
        {
            return Guid.Empty;
        }
    }
}
