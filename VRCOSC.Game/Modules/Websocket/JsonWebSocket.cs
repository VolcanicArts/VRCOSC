// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Websocket;

public class JsonWebSocket : BaseWebSocket
{
    public JsonWebSocket(string uri)
        : base(uri)
    {
    }

    public void SendAsJson(object data)
    {
        Send(JsonConvert.SerializeObject(data));
    }
}
