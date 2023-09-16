using System.Runtime.CompilerServices;

namespace Lc.Linca.Sdk.Scaffolds;

/// <summary>
/// A minimal implementation of a fictitious care information
/// software that acts as a Linked Care client for test and
/// demonstration purposes
/// </summary>
internal class CareInformationSystem : ActorSoftwareScaffold
{
    public static Client GetClient()
    {
        var clientInfo = new Client()
        {
            ClientId = GetClientIdFromDb()
        };

        return clientInfo;
    }

    private static Guid GetClientIdFromDb()
    {
        var clientId = Guid.NewGuid();

        try
        {
            clientId = new(File.ReadAllBytes(CareDb));
        } 
        catch
        {
            RegisterClientAsStored(clientId);
        }

        return clientId;
    }

    private static void RegisterClientAsStored(Guid clientId)
    {
        File.WriteAllBytes(CareDb, clientId.ToByteArray());
    }

    private static string CareDb => $".{nameof(CareInformationSystem)}.tmp.db";

    public class Client
    {
        public Guid ClientId = Guid.Empty;
        public string ClientNumber = "24601";
        public string Firstname = "Renate";
        public string Lastname = "Rüssel-Olifant";
        public string DoB = "19660810";
        public string SocInsNumber = "1238100866";
    }
}
