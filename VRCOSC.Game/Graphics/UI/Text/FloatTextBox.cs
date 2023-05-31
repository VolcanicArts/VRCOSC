// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class FloatTextBox : ValidationTextBox<float>
{
    public FloatTextBox()
    {
        EmptyIsValid = false;
    }

    protected override bool IsTextValid(string text) => float.TryParse(text, out _);

    protected override float GetConvertedText() => float.Parse(Current.Value);
}
