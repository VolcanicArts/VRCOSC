// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.ChatBoxText;

public partial class ChatBoxTextModule : ChatBoxModule
{
    public override string Title => "ChatBox Text";
    public override string Description => "Display custom text in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(1.5f);
    protected override bool ShouldUpdateImmediately => false;
    protected override int ChatBoxPriority => 1;

    private int index;
    private int speed = 1;

    protected override string GetChatBoxText()
    {
        var text = GetSetting<string>(ChatBoxTextSetting.ChatBoxText);

        if (!GetSetting<bool>(ChatBoxTextSetting.Animate)) return text;

        var splitter = GetSetting<string>(ChatBoxTextSetting.Splitter);

        var tickerText = $"{text}{splitter}{text}";
        var maxLength = Math.Min(GetSetting<int>(ChatBoxTextSetting.MaxLength), text.Length);
        speed = GetSetting<int>(ChatBoxTextSetting.ScrollSpeed);

        if (index > text.Length + splitter.Length - 1) index = 0;

        tickerText = tickerText[index..(maxLength + index)];

        return tickerText;
    }

    protected override void OnModuleStart()
    {
        index = 0;
    }

    protected override void OnModuleUpdate()
    {
        index += speed;
    }

    protected override void CreateAttributes()
    {
        CreateSetting(ChatBoxTextSetting.ChatBoxText, "ChatBox Text", "What text should be displayed in the ChatBox?", string.Empty);
        CreateSetting(ChatBoxTextSetting.Animate, "Animate", "Should the text animate like a ticker tape?", false);
        CreateSetting(ChatBoxTextSetting.ScrollSpeed, "Scroll Speed", "How fast should the text scroll? Measured in characters per update.", 5, () => GetSetting<bool>(ChatBoxTextSetting.Animate));
        CreateSetting(ChatBoxTextSetting.Splitter, "Splitter", "The splitter that goes between loops of the text", " | ", () => GetSetting<bool>(ChatBoxTextSetting.Animate));
        CreateSetting(ChatBoxTextSetting.MaxLength, "Max Length", "The maximum length to show at one time when animating", 16, () => GetSetting<bool>(ChatBoxTextSetting.Animate));
        base.CreateAttributes();
    }

    private enum ChatBoxTextSetting
    {
        ChatBoxText,
        Animate,
        ScrollSpeed,
        Splitter,
        MaxLength
    }
}
