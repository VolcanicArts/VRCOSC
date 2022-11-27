// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.TickerTape;

public class TickerTapeModule : Module
{
    public override string Title => "Ticker Tape";
    public override string Description => "Recreates a ticker tape effect for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int ChatBoxPriority => 2;
    protected override int DeltaUpdate => 2000;

    private const int max_char_length = 15;
    private int startingIndex;

    protected override void CreateAttributes()
    {
        CreateSetting(TickerTapeSetting.Contents, "Contents", "The contents of the ChatBox to tick through", string.Empty);
    }

    protected override Task OnStart(CancellationToken cancellationToken)
    {
        startingIndex = 0;

        return Task.CompletedTask;
    }

    protected override Task OnUpdate()
    {
        var contents = GetSetting<string>(TickerTapeSetting.Contents);
        var length = contents.Length;

        if (length <= 0) return Task.CompletedTask;

        // the +1 on the length is needed for the space
        var startIndex = startingIndex % (length + 1);

        contents += $" {contents}";

        var text = contents[startIndex..(startIndex + max_char_length)];

        SetChatBoxText(text);

        startingIndex++;

        return Task.CompletedTask;
    }

    private enum TickerTapeSetting
    {
        Contents
    }
}
