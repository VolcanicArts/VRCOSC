// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public static class DefaultTimeline
{
    public static IEnumerable<Clip> GenerateDefaultTimeline(ChatBoxManager chatBoxManager)
    {
        yield return generateClockClip(chatBoxManager);
        yield return generateHwsClip(chatBoxManager);
        yield return generateWeatherClip(chatBoxManager);
        yield return generateHeartrateClip(chatBoxManager);
        yield return generateChatBoxTextClip(chatBoxManager);
        yield return generateMediaClip(chatBoxManager);
    }

    private static Clip generateClockClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"Clock" },
            Priority = { Value = 0 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"clockmodule");
        clip.GetStateFor(@"clockmodule", @"default").Enabled.Value = true;

        return clip;
    }

    private static Clip generateHwsClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"Hardware Stats" },
            Priority = { Value = 1 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"hardwarestatsmodule");
        clip.GetStateFor(@"hardwarestatsmodule", @"default").Enabled.Value = true;

        return clip;
    }

    private static Clip generateWeatherClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"Weather" },
            Priority = { Value = 2 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"weathermodule");
        clip.GetStateFor(@"weathermodule", @"default").Enabled.Value = true;

        return clip;
    }

    private static Clip generateHeartrateClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"Heartrate" },
            Priority = { Value = 3 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"hyperatemodule");
        clip.AssociatedModules.Add(@"pulsoidmodule");
        clip.GetStateFor(@"hyperatemodule", @"default").Enabled.Value = true;
        clip.GetStateFor(@"pulsoidmodule", @"default").Enabled.Value = true;

        return clip;
    }

    private static Clip generateChatBoxTextClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"ChatBox Text" },
            Priority = { Value = 4 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"chatboxtextmodule");
        clip.GetStateFor(@"chatboxtextmodule", @"default").Enabled.Value = true;

        return clip;
    }

    private static Clip generateMediaClip(ChatBoxManager chatBoxManager)
    {
        var clip = new Clip(chatBoxManager)
        {
            Name = { Value = @"Media" },
            Priority = { Value = 5 },
            Start = { Value = 0 },
            End = { Value = 60 }
        };

        clip.AssociatedModules.Add(@"mediamodule");
        clip.GetStateFor(@"mediamodule", @"playing").Enabled.Value = true;

        return clip;
    }
}
