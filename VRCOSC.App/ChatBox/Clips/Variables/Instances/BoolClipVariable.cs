// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class BoolClipVariable : ClipVariable
{
    public BoolClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("Format As String", "format_as_string")]
    public bool FormatAsString { get; set; }

    protected override string Format(object value)
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
