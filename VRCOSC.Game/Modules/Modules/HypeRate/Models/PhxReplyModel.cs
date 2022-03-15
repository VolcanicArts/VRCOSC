using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Models;

public class PhxReplyModel
{
    [JsonProperty("payload")]
    public PhxReplyPayload Payload = null!;
}

public class PhxReplyPayload
{
    [JsonProperty("status")]
    public string Status = null!;
}
