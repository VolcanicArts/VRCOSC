// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.Game.Modules.Modules;

public abstract partial class ChatBoxModule : Module
{
    protected virtual bool DefaultChatBoxDisplay => true;
    protected virtual IEnumerable<string> ChatBoxFormatValues => Array.Empty<string>();
    protected virtual string DefaultChatBoxFormat => string.Empty;

    private DateTimeOffset nextSendTime;

    protected override void CreateAttributes()
    {
        var chatboxFormat = ChatBoxFormatValues.Aggregate("How should details about this module be displayed in the ChatBox? Available values: ", (current, value) => current + $"{value}, ").Trim().TrimEnd(',');
        CreateSetting(ChatBoxSetting.ChatBoxDisplay, "ChatBox Display", "If details about this module should be displayed in the ChatBox", DefaultChatBoxDisplay);

        if (ChatBoxFormatValues.Any())
        {
            CreateSetting(ChatBoxSetting.ChatBoxFormat, "ChatBox Format", chatboxFormat, DefaultChatBoxFormat,
                () => GetSetting<bool>(ChatBoxSetting.ChatBoxDisplay));
        }

        CreateSetting(ChatBoxSetting.ChatBoxMode, "ChatBox Mode", "Should this module display every X seconds or always show?", ChatBoxMode.Always,
            () => GetSetting<bool>(ChatBoxSetting.ChatBoxDisplay));
        CreateSetting(ChatBoxSetting.ChatBoxTimer, "ChatBox Display Timer", $"How long should this module wait between showing details in the ChatBox? (sec)\nRequires ChatBox Mode to be {ChatBoxMode.Timed}", 60,
            () => (GetSetting<bool>(ChatBoxSetting.ChatBoxDisplay) && GetSetting<ChatBoxMode>(ChatBoxSetting.ChatBoxMode) == ChatBoxMode.Timed));
        CreateSetting(ChatBoxSetting.ChatBoxLength, "ChatBox Display Length", $"How long should this module's details be shown in the ChatBox (sec)\nRequires ChatBox Mode to be {ChatBoxMode.Timed}", 5,
            () => (GetSetting<bool>(ChatBoxSetting.ChatBoxDisplay) && GetSetting<ChatBoxMode>(ChatBoxSetting.ChatBoxMode) == ChatBoxMode.Timed));
    }

    protected override void OnModuleStart()
    {
        nextSendTime = DateTimeOffset.Now;
    }

    protected override void Update()
    {
        if (State.Value == ModuleState.Started) trySend();
    }

    private void trySend()
    {
        if (!GetSetting<bool>(ChatBoxSetting.ChatBoxDisplay)) return;
        if (GetSetting<ChatBoxMode>(ChatBoxSetting.ChatBoxMode) == ChatBoxMode.Timed && nextSendTime > DateTimeOffset.Now) return;

        var text = GetChatBoxText();

        var displayTimerTimeSpan = TimeSpan.FromSeconds(GetSetting<int>(ChatBoxSetting.ChatBoxTimer));
        var displayLengthTimeSpan = GetSetting<ChatBoxMode>(ChatBoxSetting.ChatBoxMode) == ChatBoxMode.Timed ? TimeSpan.FromSeconds(GetSetting<int>(ChatBoxSetting.ChatBoxLength)) : TimeSpan.Zero;

        var closestNextSendTime = SetChatBoxText(text, displayLengthTimeSpan);

        if (GetSetting<ChatBoxMode>(ChatBoxSetting.ChatBoxMode) == ChatBoxMode.Always) nextSendTime = DateTimeOffset.Now;

        if (closestNextSendTime >= DateTimeOffset.Now + displayTimerTimeSpan)
            nextSendTime = closestNextSendTime;
        else
            nextSendTime = closestNextSendTime + (displayTimerTimeSpan - displayLengthTimeSpan);
    }

    /// <summary>
    /// Called to gather text to be put into the ChatBox.
    /// <para>Text is allowed to be empty. Null indicates that the module is in an invalid state to send text and will be denied ChatBox time.</para>
    /// </summary>
    /// <returns></returns>
    protected abstract string? GetChatBoxText();

    protected enum ChatBoxSetting
    {
        ChatBoxDisplay,
        ChatBoxFormat,
        ChatBoxMode,
        ChatBoxTimer,
        ChatBoxLength
    }

    private enum ChatBoxMode
    {
        Timed,
        Always
    }
}
