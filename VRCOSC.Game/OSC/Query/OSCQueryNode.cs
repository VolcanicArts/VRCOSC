// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.Game.OSC.Query;

public class OSCQueryNode
{
    [JsonProperty("FULL_PATH")]
    public string FullPath = null!;

    [JsonProperty("TYPE")]
    public string OscType = null!;

    [JsonProperty("ACCESS")]
    public OSCQueryNodeAccess Access;

    [JsonProperty("CONTENTS")]
    public Dictionary<string, OSCQueryNode> Contents = new();

    [JsonProperty("VALUE")]
    public object[] Value = Array.Empty<object>();

    [JsonConstructor]
    public OSCQueryNode()
    {
    }
}

public enum OSCQueryNodeAccess
{
    NoValue = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    ReadWrite = Read | Write
}
