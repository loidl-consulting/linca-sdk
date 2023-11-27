using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lc.Linca.Sdk.Scaffolds;

/// <summary>
/// An abstract representation of a software product
/// that implementers provide and where they host
/// the Linked Care client in, and with which they
/// make the Linked Care system available to their users
/// </summary>
internal abstract class ActorSoftwareScaffold
{
    internal PseudoStore Data;

    protected string db { init; get; }

    protected internal ActorSoftwareScaffold(string db)
    {
        Data = new();
        this.db = db;
    }

    internal void PseudoDatabaseStore()
    {
        File.WriteAllText($".{db}.tmp.lcdb", JsonSerializer.Serialize(Data));
    }

    internal void PseudoDatabaseRetrieve()
    {
        var fileName = $".{db}.tmp.lcdb";

        if (File.Exists(fileName))
        {
            Data = JsonSerializer.Deserialize<PseudoStore>(File.ReadAllText(fileName))!;
        }
        else
        {
            Data = new();
        }
    }

    internal class PseudoStore
    {
        public string ClientIdRenate { get; set; } = string.Empty;
        public string ClientIdGuenter { get; set; } = string.Empty;
        public string ClientIdPatrizia { get; set; } = string.Empty;
        public string LcIdImmerdar001 { get; set; } = string.Empty;
        public string LcIdImmerdar002 { get; set; } = string.Empty;
        public string LcIdVogelsang { get; set; } = string.Empty;
        public string OrderProposalIdRenateAtKreuzotter { get; set; } = string.Empty;
        public string OrderProposalIdRenateAtWuerm { get; set; } = string.Empty;
        public string OrderProposalIdGuenter { get; set; } = string.Empty;
        public string OrderProposalIdGuenterGranpidam { get; set; } = string.Empty;
        public string OrderProposalIdPatrizia { get; set; } = string.Empty;
        public string UpdateOrderProposalGuenter { get; set; } = string.Empty;
        public string CancelledOrderProposalPatricia { get; set; } = string.Empty;
        public string PrescriptionWithChangesGuenter { get; set; } = string.Empty;
    }
}
