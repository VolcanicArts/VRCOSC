// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class DateTimeClipVariable : ClipVariable
{
    [ClipVariableOption("Date/Time Format")]
    [JsonProperty("datetime_format")]
    public string DateTimeFormat { get; set; } = string.Empty;

    public override string Format(object value)
    {
        if (string.IsNullOrEmpty(DateTimeFormat))
        {
            return ((DateTime)value).ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            return ((DateTime)value).ToString(DateTimeFormat, CultureInfo.CurrentCulture);
        }
    }
}
