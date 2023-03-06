// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class IPTextBox : ValidationTextBox<string>
{
    private readonly Regex regex = new(@"^(((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4})$");

    protected override bool IsTextValid(string text) => regex.IsMatch(text);

    protected override string GetConvertedText() => Current.Value;
}
