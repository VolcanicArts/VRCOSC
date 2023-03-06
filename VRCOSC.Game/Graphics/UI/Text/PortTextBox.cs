// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class PortTextBox : ValidationTextBox<int>
{
    private readonly Regex regex = new(@"^([0-9]{1,5})$");

    protected override bool IsTextValid(string text)
    {
        if (!regex.IsMatch(text)) return false;

        var port = int.Parse(text);
        return port is >= 0 and <= 65535;
    }

    protected override int GetConvertedText() => int.Parse(Current.Value);
}
