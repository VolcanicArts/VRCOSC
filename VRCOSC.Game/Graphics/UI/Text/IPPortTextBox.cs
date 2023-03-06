// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class IPPortTextBox : ValidationTextBox<IPPortTextBox.IPPortResult>
{
    private readonly Regex regex = new(@"^(((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4})(:{1})([0-9]{1,5})$");

    public IPPortTextBox()
    {
        PlaceholderText = "IP:Port";
    }

    protected override bool IsTextValid(string text)
    {
        if (!regex.IsMatch(text)) return false;

        var port = int.Parse(text.Split(':')[1]);
        return port is >= 0 and <= 65535;
    }

    protected override IPPortResult GetConvertedText()
    {
        if (string.IsNullOrEmpty(Current.Value)) return new IPPortResult();

        return new IPPortResult
        {
            IP = Current.Value.Split(':')[0],
            Port = int.Parse(Current.Value.Split(':')[1])
        };
    }

    public class IPPortResult
    {
        public string IP = string.Empty;
        public int Port;
    }
}
