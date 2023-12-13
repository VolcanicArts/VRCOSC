// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Graphics.UI.Text;

public partial class IntTextBox : ValidationTextBox<int>
{
    public bool AllowNegative { get; init; } = false;
    public int Minimum { get; init; } = int.MinValue;
    public int Maximum { get; init; } = int.MaxValue;

    protected override bool IsTextValid(string text) => int.TryParse(text, out var intText) && (intText >= 0 || AllowNegative) && intText >= Minimum && intText <= Maximum;
    protected override int GetConvertedText() => int.Parse(Current.Value);
}
