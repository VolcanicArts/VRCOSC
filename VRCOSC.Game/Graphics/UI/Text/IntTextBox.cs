// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class IntTextBox : ValidationTextBox<int>
{
    public IntTextBox()
    {
        EmptyIsValid = false;
    }

    protected override bool IsTextValid(string text) => int.TryParse(text, out _);

    protected override int GetConvertedText() => int.Parse(Current.Value);
}
