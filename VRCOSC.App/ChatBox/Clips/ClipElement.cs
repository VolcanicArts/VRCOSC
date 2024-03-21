// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipElement
{
    public Observable<string> Format { get; set; } = new();
    public Observable<bool> Enabled { get; set; } = new();

    public List<ClipVariable> Variables = new();

    public virtual string DisplayName => string.Empty;
    public virtual bool IsDefault => Format.IsDefault && Enabled.IsDefault;
    public virtual bool ShouldBeVisible => true;

    public string RunFormatting()
    {
        // go through each variable and format it
        return string.Empty;
    }
}
