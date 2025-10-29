// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.App.SDK.VRChat;

public class Instance
{
    public string? WorldId { get; internal set; }
    public List<User> Users { get; } = [];
}