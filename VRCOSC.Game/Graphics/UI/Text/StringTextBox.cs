// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class StringTextBox : ValidationTextBox<string>
{
    public int MinimumLength { get; init; }

    protected override bool IsTextValid(string text) => text.Length >= MinimumLength;

    protected override string GetConvertedText() => Current.Value;
}
