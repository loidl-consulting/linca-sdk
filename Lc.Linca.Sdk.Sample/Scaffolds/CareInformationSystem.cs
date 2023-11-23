namespace Lc.Linca.Sdk.Scaffolds;

/// <summary>
/// A minimal implementation of a fictitious care information
/// software that acts as a Linked Care client for test and
/// demonstration purposes
/// </summary>
internal class CareInformationSystem : ActorSoftwareScaffold
{
    public CareInformationSystem() : base(nameof(CareInformationSystem)) { }

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
