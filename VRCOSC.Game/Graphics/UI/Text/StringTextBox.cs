// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Graphics.UI.Text;

public partial class StringTextBox : ValidationTextBox<string>
{
    public bool EmptyIsValid { get; init; } = false;
    public int MinimumLength { get; init; } = 0;

    protected override bool IsTextValid(string text) => (text.Length > 0 || EmptyIsValid) && text.Length >= MinimumLength;
    protected override string GetConvertedText() => Current.Value;
}
