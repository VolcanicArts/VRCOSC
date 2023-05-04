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
        yield return generateChatBoxTextClip(chatBoxManager);
        yield return generateHeartrateClip(chatBoxManager);
        yield return generateMediaClip(chatBoxManager);
        yield return generateSpeechToTextClip(chatBoxManager);
    }

    private static Clip generateClockClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Clock";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 7;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"clockmodule");
        clip.GetStateFor(@"clockmodule", @"default")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateHwsClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Hardware Stats";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 6;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"hardwarestatsmodule");
        clip.GetStateFor(@"hardwarestatsmodule", @"default")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateWeatherClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Weather";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 5;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"weathermodule");
        clip.GetStateFor(@"weathermodule", @"default")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateChatBoxTextClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"ChatBox Text";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 4;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"chatboxtextmodule");
        clip.GetStateFor(@"chatboxtextmodule", @"default")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateHeartrateClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Heartrate";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 3;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"hyperatemodule");
        clip.AssociatedModules.Add(@"pulsoidmodule");
        clip.GetStateFor(@"hyperatemodule", @"default")!.Enabled.Value = true;
        clip.GetStateFor(@"pulsoidmodule", @"default")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateMediaClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Media";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 2;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"mediamodule");
        clip.GetStateFor(@"mediamodule", @"playing")!.Enabled.Value = true;

        return clip;
    }

    private static Clip generateSpeechToTextClip(ChatBoxManager chatBoxManager)
    {
        var clip = chatBoxManager.CreateClip();
        clip.Name.Value = @"Speech To Text";
        clip.Priority.Value = chatBoxManager.PriorityCount.Value - 1;
        clip.Start.Value = 0;
        clip.End.Value = 60;
        clip.AssociatedModules.Add(@"speechtotextmodule");
        clip.GetStateFor(@"speechtotextmodule", @"textgenerating")!.Enabled.Value = true;
        clip.GetEventFor(@"speechtotextmodule", @"textgenerated")!.Enabled.Value = true;

        return clip;
    }
}
