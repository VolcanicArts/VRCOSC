// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules.Modules.ChatBoxText;

public class ChatBoxTextModule : ChatBoxModule
{
    public override string Title => "ChatBox Text";
    public override string Description => "Display custom text in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int ChatBoxPriority => 2;

    protected override bool DefaultChatBoxDisplay => true;

    protected override string? GetChatBoxText() => GetSetting<string>(ChatBoxTextSetting.ChatBoxText);

    protected override void CreateAttributes()
    {
        CreateSetting(ChatBoxTextSetting.ChatBoxText, "ChatBox Text", "What text should be displayed in the ChatBox?", string.Empty);
        base.CreateAttributes();
    }

    private enum ChatBoxTextSetting
    {
        ChatBoxText
    }
}
