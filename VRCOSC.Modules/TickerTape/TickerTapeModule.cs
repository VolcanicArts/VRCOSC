// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.TickerTape;

[ModuleTitle("Ticker Tape")]
[ModuleDescription("Display and animate text for the ChatBox")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
[ModuleLegacy("chatboxtextmodule")]
public class TickerTapeModule : ChatBoxModule
{
    private readonly Dictionary<string, int> indexes = new();

    protected override void CreateAttributes()
    {
        CreateSetting(TickerTapeSetting.TextList, new TickerTapeInstanceListAttribute
        {
            Default = new List<TickerTapeInstance>
            {
                new()
                {
                    Name = { Value = "Example" },
                    Text = { Value = "ExampleText" },
                    Direction = { Value = TickerTapeDirection.Right },
                    ScrollSpeed = { Value = 1 },
                    MaxLength = { Value = 8 }
                }
            },
            Name = "Texts",
            Description = "Each text instance to register for the ChatBox\nText instances can be accessed with the '_Name' suffix\nScroll speed is the number of characters to scroll each update, defined by the chatbox update rate"
        });

        CreateVariable(TickerTapeVariable.Text, "Text", "text");

        CreateState(TickerTapeState.Default, "Default", $"{GetVariableFormat(TickerTapeVariable.Text, "Example")}");
    }

    protected override void OnModuleStart()
    {
        indexes.Clear();

        GetSettingList<TickerTapeInstance>(TickerTapeSetting.TextList).ForEach(instance =>
        {
            if (string.IsNullOrEmpty(instance.Name.Value)) return;

            if (!indexes.TryAdd(instance.Name.Value, 0)) indexes[instance.Name.Value] = 0;
            SetVariableValue(TickerTapeVariable.Text, instance.Text.Value, instance.Name.Value);
        });

        ChangeStateTo(TickerTapeState.Default);
    }

    [ModuleUpdate(ModuleUpdateMode.ChatBox)]
    private void updateVariables()
    {
        GetSettingList<TickerTapeInstance>(TickerTapeSetting.TextList).ForEach(instance =>
        {
            if (string.IsNullOrEmpty(instance.Name.Value) || !indexes.ContainsKey(instance.Name.Value) || string.IsNullOrEmpty(instance.Text.Value)) return;

            var position = indexes[instance.Name.Value].Modulo(instance.Text.Value.Length);
            var text = cropAndWrapText(instance.Text.Value, position, instance.MaxLength.Value);

            SetVariableValue(TickerTapeVariable.Text, text, instance.Name.Value);

            switch (instance.Direction.Value)
            {
                case TickerTapeDirection.Right:
                    indexes[instance.Name.Value] += instance.ScrollSpeed.Value;
                    break;

                case TickerTapeDirection.Left:
                    indexes[instance.Name.Value] -= instance.ScrollSpeed.Value;
                    break;
            }
        });
    }

    private static string cropAndWrapText(string text, int position, int maxLength)
    {
        var endPos = Math.Min(position + maxLength, text.Length);
        var subText = text.Substring(position, endPos - position);
        return endPos != text.Length ? subText : subText + text[..Math.Min(maxLength - subText.Length, position)];
    }

    private enum TickerTapeSetting
    {
        TextList
    }

    private enum TickerTapeState
    {
        Default
    }

    private enum TickerTapeVariable
    {
        Text
    }
}
