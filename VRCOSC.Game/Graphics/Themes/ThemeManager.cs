// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Themes;

public static class ThemeManager
{
    public static ColourTheme Theme;

    public static Dictionary<ThemeAttribute, Color4> Current => schemes[Theme];

    private static readonly Dictionary<ColourTheme, Dictionary<ThemeAttribute, Color4>> schemes = new()
    {
        {
            ColourTheme.Dark, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"222222") },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"333333") },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"444444") },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"555555") },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"666666") },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD").Darken(0.25f) },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"FFFFFF") }
            }
        },
        {
            ColourTheme.Light, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"DDDDDD") },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"CCCCCC") },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"AAAAAA") },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"999999") },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"444444") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"000000") }
            }
        },
        {
            ColourTheme.Discord, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"1e2124") },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"282b30") },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"36393e") },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"424549") },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"DDDDDD") },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"7289da") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"999999") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"7289da") }
            }
        },
        {
            ColourTheme.SpaceGrey, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"343d46") },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"4f5b66") },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"65737e") },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"a7adba").Darken(0.25f) },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"c0c5ce") },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"FFFFFF") }
            }
        },
        {
            ColourTheme.NightSky, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"2e4482").Darken(0.25f) },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"2e4482") },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"546bab") },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"546bab").Lighten(0.25f) },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"546bab").Lighten(0.5f) },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"FFFFFF") }
            }
        },
        {
            ColourTheme.Purple, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"660066").Darken(0.25f) },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"800080").Darken(0.25f) },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"be29ec").Darken(0.25f) },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"d896ff").Darken(0.25f) },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"efbbff").Darken(0.25f) },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"FFFFFF") }
            }
        },
        {
            ColourTheme.Teal, new Dictionary<ThemeAttribute, Color4>
            {
                { ThemeAttribute.Darker, Color4Extensions.FromHex(@"003333").Darken(0.25f) },
                { ThemeAttribute.Dark, Color4Extensions.FromHex(@"004444").Darken(0.25f) },
                { ThemeAttribute.Mid, Color4Extensions.FromHex(@"005555").Darken(0.25f) },
                { ThemeAttribute.Light, Color4Extensions.FromHex(@"006666").Darken(0.25f) },
                { ThemeAttribute.Lighter, Color4Extensions.FromHex(@"007777").Darken(0.25f) },
                { ThemeAttribute.Success, Color4Extensions.FromHex(@"88B300") },
                { ThemeAttribute.Pending, Color4Extensions.FromHex(@"FFCC22") },
                { ThemeAttribute.Failure, Color4Extensions.FromHex(@"ED1121") },
                { ThemeAttribute.Action, Color4Extensions.FromHex(@"44AADD") },
                { ThemeAttribute.Text, Color4Extensions.FromHex(@"FFFFFF") },
                { ThemeAttribute.SubText, Color4Extensions.FromHex(@"BBBBBB") },
                { ThemeAttribute.Border, Color4Extensions.FromHex(@"000000") },
                { ThemeAttribute.Accent, Color4Extensions.FromHex(@"FFFFFF") }
            }
        }
    };
}

public enum ColourTheme
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
