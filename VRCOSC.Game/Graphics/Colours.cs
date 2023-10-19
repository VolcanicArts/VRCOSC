// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics;

public static class Colours
{
    public static Color4 Black = new(0, 0, 0, 255);
    public static Color4 Dark = new(30, 31, 34, 255);
    public static Color4 Mid = new(43, 45, 49, 255);
    public static Color4 Light = new(49, 51, 56, 255);
    public static Color4 Highlight = new(56, 58, 64, 255);
    public static Color4 OffWhite = new(242, 243, 245, 255);
    public static Color4 Red = Color4Extensions.FromHex("#e74c3c");

    public static Color4 Transparent = new(0, 0, 0, 0);

    public static Color4 Blue0 = Color4Extensions.FromHex("2980B9");
    public static Color4 Red1 = Color4Extensions.FromHex("E74C3C");
    public static Color4 White0 = Color4Extensions.FromHex("FFFFFF");
    public static Color4 White1 = Color4Extensions.FromHex("808080");
    public static Color4 Gray0 = Color4Extensions.FromHex("1F1F1F");
    public static Color4 Gray1 = Color4Extensions.FromHex("292929");
    public static Color4 Gray2 = Color4Extensions.FromHex("808080");
}
