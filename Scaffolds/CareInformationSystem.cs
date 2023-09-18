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
        var existingClientId = PseudoDatabaseRetrieve(nameof(CareInformationSystem), PseudoDatabaseField.PatientId);

        if (existingClientId == Guid.Empty)
        {
            /* what a caregiver calls a client,
             * a doctor calls a patient. */
            PseudoDatabaseStore(
                nameof(CareInformationSystem),
                PseudoDatabaseField.PatientId,
                clientId
            );
        }
        else
        {
            clientId = existingClientId;
        }

        return clientId;
    }

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
