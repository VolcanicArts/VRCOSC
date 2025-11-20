// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VRCOSC.App.OpenVR;

public class OpenVRPaths
{
    [JsonPropertyName("config")]
    public List<string> Config { get; set; } = [];

    [JsonPropertyName("runtime")]
    public List<string> Runtime { get; set; } = [];
}