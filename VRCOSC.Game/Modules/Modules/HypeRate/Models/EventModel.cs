using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Models;

public class EventModel
{
    [JsonProperty("event")]
    public string Event = null!;
}
