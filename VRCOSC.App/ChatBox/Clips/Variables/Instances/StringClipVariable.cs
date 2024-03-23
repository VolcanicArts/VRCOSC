// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class StringClipVariable : ClipVariable
{
    public StringClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    protected override string Format(object? value)
    {
        return (string)value;
    }
}
