// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Frameworks;

public abstract class JsonWebSocket : BaseWebSocket
{
    protected JsonWebSocket(string uri)
        : base(uri)
    {
    }

    protected void Send(object data)
    {
        base.Send(JsonConvert.SerializeObject(data));
    }
}
