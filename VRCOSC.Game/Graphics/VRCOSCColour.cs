using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global

namespace VRCOSC.Game.Graphics;

// Taken and modified from https://github.com/ppy/osu/blob/master/osu.Game/Graphics/OsuColour.cs
public static class VRCOSCColour
{
    public static readonly Color4 Invisible = new(0, 0, 0, 0);

    public static readonly Color4 PurpleLighter = Color4Extensions.FromHex(@"eeeeff");
    public static readonly Color4 PurpleLight = Color4Extensions.FromHex(@"aa88ff");
    public static readonly Color4 PurpleLightAlternative = Color4Extensions.FromHex(@"cba4da");
    public static readonly Color4 Purple = Color4Extensions.FromHex(@"8866ee");
    public static readonly Color4 PurpleDark = Color4Extensions.FromHex(@"6644cc");
    public static readonly Color4 PurpleDarkAlternative = Color4Extensions.FromHex(@"312436");
    public static readonly Color4 PurpleDarker = Color4Extensions.FromHex(@"441188");

    public static readonly Color4 PinkLighter = Color4Extensions.FromHex(@"ffddee");
    public static readonly Color4 PinkLight = Color4Extensions.FromHex(@"ff99cc");
    public static readonly Color4 Pink = Color4Extensions.FromHex(@"ff66aa");
    public static readonly Color4 PinkDark = Color4Extensions.FromHex(@"cc5288");
    public static readonly Color4 PinkDarker = Color4Extensions.FromHex(@"bb1177");

    public static readonly Color4 BlueLighter = Color4Extensions.FromHex(@"ddffff");
    public static readonly Color4 BlueLight = Color4Extensions.FromHex(@"99eeff");
    public static readonly Color4 Blue = Color4Extensions.FromHex(@"66ccff");
    public static readonly Color4 BlueDark = Color4Extensions.FromHex(@"44aadd");
    public static readonly Color4 BlueDarker = Color4Extensions.FromHex(@"2299bb");

    public static readonly Color4 YellowLighter = Color4Extensions.FromHex(@"ffffdd");
    public static readonly Color4 YellowLight = Color4Extensions.FromHex(@"ffdd55");
    public static readonly Color4 Yellow = Color4Extensions.FromHex(@"ffcc22");
    public static readonly Color4 YellowDark = Color4Extensions.FromHex(@"eeaa00");
    public static readonly Color4 YellowDarker = Color4Extensions.FromHex(@"cc6600");

    public static readonly Color4 GreenLighter = Color4Extensions.FromHex(@"eeffcc");
    public static readonly Color4 GreenLight = Color4Extensions.FromHex(@"b3d944");
    public static readonly Color4 Green = Color4Extensions.FromHex(@"88b300");
    public static readonly Color4 GreenDark = Color4Extensions.FromHex(@"668800");
    public static readonly Color4 GreenDarker = Color4Extensions.FromHex(@"445500");

    public static readonly Color4 Sky = Color4Extensions.FromHex(@"6bb5ff");
    public static readonly Color4 GreySkyLighter = Color4Extensions.FromHex(@"c6e3f4");
    public static readonly Color4 GreySkyLight = Color4Extensions.FromHex(@"8ab3cc");
    public static readonly Color4 GreySky = Color4Extensions.FromHex(@"405461");
    public static readonly Color4 GreySkyDark = Color4Extensions.FromHex(@"303d47");
    public static readonly Color4 GreySkyDarker = Color4Extensions.FromHex(@"21272c");

    public static readonly Color4 SeaFoam = Color4Extensions.FromHex(@"05ffa2");
    public static readonly Color4 GreySeaFoamLighter = Color4Extensions.FromHex(@"9ebab1");
    public static readonly Color4 GreySeaFoamLight = Color4Extensions.FromHex(@"4d7365");
    public static readonly Color4 GreySeaFoam = Color4Extensions.FromHex(@"33413c");
    public static readonly Color4 GreySeaFoamDark = Color4Extensions.FromHex(@"2c3532");
    public static readonly Color4 GreySeaFoamDarker = Color4Extensions.FromHex(@"1e2422");

    public static readonly Color4 Cyan = Color4Extensions.FromHex(@"05f4fd");
    public static readonly Color4 GreyCyanLighter = Color4Extensions.FromHex(@"77b1b3");
    public static readonly Color4 GreyCyanLight = Color4Extensions.FromHex(@"436d6f");
    public static readonly Color4 GreyCyan = Color4Extensions.FromHex(@"293d3e");
    public static readonly Color4 GreyCyanDark = Color4Extensions.FromHex(@"243536");
    public static readonly Color4 GreyCyanDarker = Color4Extensions.FromHex(@"1e2929");

    public static readonly Color4 Lime = Color4Extensions.FromHex(@"82ff05");
    public static readonly Color4 GreyLimeLighter = Color4Extensions.FromHex(@"deff87");
    public static readonly Color4 GreyLimeLight = Color4Extensions.FromHex(@"657259");
    public static readonly Color4 GreyLime = Color4Extensions.FromHex(@"3f443a");
    public static readonly Color4 GreyLimeDark = Color4Extensions.FromHex(@"32352e");
    public static readonly Color4 GreyLimeDarker = Color4Extensions.FromHex(@"2e302b");

    public static readonly Color4 Violet = Color4Extensions.FromHex(@"bf04ff");
    public static readonly Color4 GreyVioletLighter = Color4Extensions.FromHex(@"ebb8fe");
    public static readonly Color4 GreyVioletLight = Color4Extensions.FromHex(@"685370");
    public static readonly Color4 GreyViolet = Color4Extensions.FromHex(@"46334d");
    public static readonly Color4 GreyVioletDark = Color4Extensions.FromHex(@"2c2230");
    public static readonly Color4 GreyVioletDarker = Color4Extensions.FromHex(@"201823");

    public static readonly Color4 Carmine = Color4Extensions.FromHex(@"ff0542");
    public static readonly Color4 GreyCarmineLighter = Color4Extensions.FromHex(@"deaab4");
    public static readonly Color4 GreyCarmineLight = Color4Extensions.FromHex(@"644f53");
    public static readonly Color4 GreyCarmine = Color4Extensions.FromHex(@"342b2d");
    public static readonly Color4 GreyCarmineDark = Color4Extensions.FromHex(@"302a2b");
    public static readonly Color4 GreyCarmineDarker = Color4Extensions.FromHex(@"241d1e");

    public static readonly Color4 RedLighter = Color4Extensions.FromHex(@"ffeded");
    public static readonly Color4 RedLight = Color4Extensions.FromHex(@"ed7787");
    public static readonly Color4 Red = Color4Extensions.FromHex(@"ed1121");
    public static readonly Color4 RedDark = Color4Extensions.FromHex(@"ba0011");
    public static readonly Color4 RedDarker = Color4Extensions.FromHex(@"870000");

    public static readonly Color4 Gray0 = Color4Extensions.FromHex(@"000");
    public static readonly Color4 Gray1 = Color4Extensions.FromHex(@"111");
    public static readonly Color4 Gray2 = Color4Extensions.FromHex(@"222");
    public static readonly Color4 Gray3 = Color4Extensions.FromHex(@"333");
    public static readonly Color4 Gray4 = Color4Extensions.FromHex(@"444");
    public static readonly Color4 Gray5 = Color4Extensions.FromHex(@"555");
    public static readonly Color4 Gray6 = Color4Extensions.FromHex(@"666");
    public static readonly Color4 Gray7 = Color4Extensions.FromHex(@"777");
    public static readonly Color4 Gray8 = Color4Extensions.FromHex(@"888");
    public static readonly Color4 Gray9 = Color4Extensions.FromHex(@"999");
    public static readonly Color4 GrayA = Color4Extensions.FromHex(@"aaa");
    public static readonly Color4 GrayB = Color4Extensions.FromHex(@"bbb");
    public static readonly Color4 GrayC = Color4Extensions.FromHex(@"ccc");
    public static readonly Color4 GrayD = Color4Extensions.FromHex(@"ddd");
    public static readonly Color4 GrayE = Color4Extensions.FromHex(@"eee");
    public static readonly Color4 GrayF = Color4Extensions.FromHex(@"fff");
}
