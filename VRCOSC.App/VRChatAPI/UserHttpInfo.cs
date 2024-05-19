// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.App.VRChatAPI;

public class UserHttpInfo
{
    [JsonProperty("requiresTwoFactorAuth")]
    public List<string> TwoFactorAuthTypes = null!;
}
