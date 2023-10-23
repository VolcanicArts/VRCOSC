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

    public static readonly Color4 BLUE0 = Color4Extensions.FromHex("2980B9");
    public static readonly Color4 RED1 = Color4Extensions.FromHex("E74C3C");
    public static readonly Color4 WHITE0 = Color4Extensions.FromHex("FFFFFF");
    public static readonly Color4 WHITE1 = Color4Extensions.FromHex("808080");
    public static readonly Color4 WHITE2 = Color4Extensions.FromHex("BFBFBF");
    public static readonly Color4 GRAY0 = Color4Extensions.FromHex("0F0F0F");
    public static readonly Color4 GRAY1 = Color4Extensions.FromHex("1F1F1F");
    public static readonly Color4 GRAY2 = Color4Extensions.FromHex("292929");
    public static readonly Color4 GRAY3 = Color4Extensions.FromHex("808080");
}
