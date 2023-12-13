// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.Graphics.UI.Text;

public partial class FloatTextBox : ValidationTextBox<float>
{
    protected override bool IsTextValid(string text) => float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
    protected override float GetConvertedText() => float.Parse(Current.Value, NumberStyles.Any, CultureInfo.CurrentCulture);
}
