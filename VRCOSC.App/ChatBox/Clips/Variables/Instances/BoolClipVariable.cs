// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class BoolClipVariable : ClipVariable
{
    [ClipVariableOption("Format As String")]
    [JsonProperty("format_as_string")]
    public bool FormatAsString { get; set; }

    public override string Format(object value)
    {
        if (FormatAsString)
        {
            return ((bool)value).ToString();
        }
        else
        {
            return ((bool)value) ? "1" : "0";
        }
    }
}
