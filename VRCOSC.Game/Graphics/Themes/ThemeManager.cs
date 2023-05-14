// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;

namespace VRCOSC.Game.Graphics.Themes;

public static class ThemeManager
{
    public static VRCOSCTheme VRCOSCTheme;

    public static ColourTheme Current => schemes[VRCOSCTheme];

    private static readonly Dictionary<VRCOSCTheme, ColourTheme> schemes = new()
    {
        { VRCOSCTheme.Dark, new DarkTheme() },
        { VRCOSCTheme.Light, new LightTheme() },
        { VRCOSCTheme.Discord, new DiscordTheme() },
        { VRCOSCTheme.SpaceGrey, new SpaceGreyTheme() },
        { VRCOSCTheme.NightSky, new NightSkyTheme() },
        { VRCOSCTheme.Purple, new PurpleTheme() },
        { VRCOSCTheme.Teal, new TealTheme() }
    };
}

public class ColourTheme
{
    protected readonly Dictionary<ThemeAttribute, Colour4> Colours = new();

    protected ColourTheme()
    {
        Colours[ThemeAttribute.Success] = Color4Extensions.FromHex(@"88B300").Darken(0.25f);
        Colours[ThemeAttribute.Pending] = Color4Extensions.FromHex(@"FFCC22").Darken(0.25f);
        Colours[ThemeAttribute.Failure] = Color4Extensions.FromHex(@"ED1121").Darken(0.25f);
        Colours[ThemeAttribute.Action] = Color4Extensions.FromHex(@"44AADD").Darken(0.25f);
        Colours[ThemeAttribute.Text] = Color4Extensions.FromHex(@"FFFFFF");
        Colours[ThemeAttribute.SubText] = Color4Extensions.FromHex(@"BBBBBB");
        Colours[ThemeAttribute.Border] = Color4Extensions.FromHex(@"000000");
        Colours[ThemeAttribute.Accent] = Color4Extensions.FromHex(@"FFFFFF");
    }

    public Colour4 this[ThemeAttribute themeAttribute] => Colours[themeAttribute];
}

public class DarkTheme : ColourTheme
{
    public DarkTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"222222");
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"333333");
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"444444");
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"555555");
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"AAAAAA");
    }
}

public class LightTheme : DarkTheme
{
    public LightTheme()
    {
        Colours[ThemeAttribute.Darker] = Colours[ThemeAttribute.Darker].Invert();
        Colours[ThemeAttribute.Dark] = Colours[ThemeAttribute.Dark].Invert();
        Colours[ThemeAttribute.Mid] = Colours[ThemeAttribute.Mid].Invert();
        Colours[ThemeAttribute.Light] = Colours[ThemeAttribute.Light].Invert();
        Colours[ThemeAttribute.Lighter] = Colours[ThemeAttribute.Lighter].Invert();
        Colours[ThemeAttribute.Text] = Colours[ThemeAttribute.Text].Invert();
        Colours[ThemeAttribute.SubText] = Colours[ThemeAttribute.SubText].Invert();
        Colours[ThemeAttribute.Border] = Colours[ThemeAttribute.Border].Invert();
        Colours[ThemeAttribute.Accent] = Colours[ThemeAttribute.Accent].Invert();
    }
}

public class DiscordTheme : ColourTheme
{
    public DiscordTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"1e2124");
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"282b30");
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"36393e");
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"424549");
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"DDDDDD");
        Colours[ThemeAttribute.Action] = Color4Extensions.FromHex(@"7289da");
        Colours[ThemeAttribute.Text] = Color4Extensions.FromHex(@"FFFFFF");
        Colours[ThemeAttribute.SubText] = Color4Extensions.FromHex(@"999999");
        Colours[ThemeAttribute.Accent] = Color4Extensions.FromHex(@"7289da");
    }
}

public class SpaceGreyTheme : ColourTheme
{
    public SpaceGreyTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"343d46");
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"4f5b66");
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"65737e");
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"a7adba");
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"c0c5ce");
    }
}

public class NightSkyTheme : ColourTheme
{
    public NightSkyTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"2e4482").Darken(0.25f);
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"2e4482");
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"546bab");
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"546bab").Lighten(0.15f);
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"546bab").Lighten(0.25f);
    }
}

public class PurpleTheme : ColourTheme
{
    public PurpleTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"660066").Darken(0.25f);
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"800080").Darken(0.25f);
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"be29ec").Darken(0.25f);
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"d896ff").Darken(0.5f);
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"efbbff").Darken(0.5f);
    }
}

public class TealTheme : ColourTheme
{
    public TealTheme()
    {
        Colours[ThemeAttribute.Darker] = Color4Extensions.FromHex(@"003333").Darken(0.25f);
        Colours[ThemeAttribute.Dark] = Color4Extensions.FromHex(@"004444").Darken(0.25f);
        Colours[ThemeAttribute.Mid] = Color4Extensions.FromHex(@"005555").Darken(0.25f);
        Colours[ThemeAttribute.Light] = Color4Extensions.FromHex(@"006666").Darken(0.25f);
        Colours[ThemeAttribute.Lighter] = Color4Extensions.FromHex(@"007777").Darken(0.25f);
    }
}

public enum VRCOSCTheme
{
    Dark,
    Light,
    Discord,
    SpaceGrey,
    NightSky,
    Purple,
    Teal
}

public enum ThemeAttribute
{
    Darker,
    Dark,
    Mid,
    Light,
    Lighter,
    Success,
    Pending,
    Failure,
    Action,
    Text,
    SubText,
    Border,
    Accent
}
