// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;

namespace VRCOSC.Graphics.UI.Text;

public partial class IPEndPointTextBox : ValidationTextBox<IPEndPoint>
{
    public IPEndPointTextBox()
    {
        PlaceholderText = "IP:Port";
    }

    protected override bool IsTextValid(string text) => IPEndPoint.TryParse(text, out _);

    protected override IPEndPoint GetConvertedText() => IPEndPoint.TryParse(Current.Value, out var parsedValue) ? parsedValue : new IPEndPoint(IPAddress.None, 0);
}
