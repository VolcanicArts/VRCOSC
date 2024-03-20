// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using Newtonsoft.Json;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class FloatClipVariable : ClipVariable
{
    [ClipVariableOption("Float Format")]
    [JsonProperty("float_format")]
    public string FloatFormat { get; set; } = string.Empty;

    public override string Format(object value)
    {
        if (string.IsNullOrEmpty(FloatFormat))
        {
            return ((float)value).ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            return ((float)value).ToString(FloatFormat, CultureInfo.CurrentCulture);
        }
    }
}
