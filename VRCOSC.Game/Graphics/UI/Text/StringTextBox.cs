// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class StringTextBox : ValidationTextBox<string>
{
    /// <summary>
    /// Allows for setting a minimum length
    /// </summary>
    public BindableNumber<int> MinimumLength { get; set; } = new();

    protected override bool IsTextValid(string text) => text.Length >= MinimumLength.Value;

    protected override string GetConvertedText() => Current.Value;
}
