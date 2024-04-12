// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class BoolClipVariable : ClipVariable
{
    public BoolClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("When True", "when_true")]
    public string WhenTrue = "True";

    [ClipVariableOption("When False", "when_false")]
    public string WhenFalse = "False";

    protected override string Format(object value)
    {
        var trueString = string.IsNullOrEmpty(WhenTrue) ? "1" : WhenTrue;
        var falseString = string.IsNullOrEmpty(WhenFalse) ? "0" : WhenFalse;

        return (bool)value ? trueString : falseString;
    }
}
