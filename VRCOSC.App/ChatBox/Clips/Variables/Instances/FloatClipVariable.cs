// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class FloatClipVariable : ClipVariable
{
    public FloatClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("Float Format", "float_format")]
    public string FloatFormat { get; set; } = string.Empty;

    protected override string Format(object value)
    {
        if (string.IsNullOrEmpty(FloatFormat))
        {
            return ((float)value).ToString("F1", CultureInfo.CurrentCulture);
        }
        else
        {
            return ((float)value).ToString(FloatFormat, CultureInfo.CurrentCulture);
        }
    }
}
