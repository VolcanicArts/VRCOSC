// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class IntClipVariable : ClipVariable
{
    public IntClipVariable()
    {
    }

    public IntClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    protected override string Format(object value)
    {
        var intValue = (int)value;

        return intValue == int.MaxValue ? "\u221e" : intValue.ToString(CultureInfo.CurrentCulture);
    }
}
