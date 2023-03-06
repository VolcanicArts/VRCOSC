// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Graphics.UI.Text;

public partial class IntTextBox : ValidationTextBox<int>
{
    public IntTextBox()
    {
        EmptyIsValid = false;
    }

    protected override bool IsTextValid(string text)
    {
        try
        {
            _ = int.Parse(text);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    protected override int GetConvertedText() => int.Parse(Current.Value);
}
