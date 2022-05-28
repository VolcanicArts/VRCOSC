// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Modules;

public abstract class HeartrateProvider : JsonWebSocket
{
    public Action<int>? OnHeartRateUpdate;

    protected HeartrateProvider(string uri)
        : base(uri)
    {
    }
}
