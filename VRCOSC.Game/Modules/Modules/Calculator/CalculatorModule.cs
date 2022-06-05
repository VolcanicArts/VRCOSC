// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform.Windows;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Calculator;

public class CalculatorModule : IntegrationModule
{
    public override string Title => "Calculator";
    public override string Description => "Integrate with the Windows calculator for efficient maths";
    public override string Author => "Buckminsterfullerene";
    public override Colour4 Colour => Color4Extensions.FromHex(@"ff2600").Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;
    public override bool Experimental => true;

    private bool isCalculatorOpen;
    private float calculatorResult;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        CalculatorInputParameters.CalculatorOpen,
        CalculatorInputParameters.CalculatorClose,
        CalculatorInputParameters.CalculatorClear,
        CalculatorInputParameters.CalculatorCalculate,
        CalculatorInputParameters.CalculatorCopyValue,
        CalculatorInputParameters.CalculatorAdd,
        CalculatorInputParameters.CalculatorSubtract,
        CalculatorInputParameters.CalculatorMultiply,
        CalculatorInputParameters.CalculatorDivide,
        CalculatorInputParameters.CalculatorNumber
    };

    protected override string TargetProcess => "calc";

    protected override IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>
    {
        { CalculatorInputParameters.CalculatorClear, new[] { WindowsVKey.VK_ESCAPE } },
        { CalculatorInputParameters.CalculatorCalculate, new[] { WindowsVKey.VK_RETURN } },
        { CalculatorInputParameters.CalculatorCopyValue, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_C } }, // TODO: Maybe the value should be sent every time something is executed
        { CalculatorInputParameters.CalculatorAdd, new[] { WindowsVKey.VK_ADD } },
        { CalculatorInputParameters.CalculatorSubtract, new[] { WindowsVKey.VK_SUBTRACT } },
        { CalculatorInputParameters.CalculatorMultiply, new[] { WindowsVKey.VK_MULTIPLY } },
        { CalculatorInputParameters.CalculatorDivide, new[] { WindowsVKey.VK_DIVIDE } },
        { CalculatorNumbers.CalculatorNumber0, new[] { WindowsVKey.VK_NUMPAD0 } },
        { CalculatorNumbers.CalculatorNumber1, new[] { WindowsVKey.VK_NUMPAD1 } },
        { CalculatorNumbers.CalculatorNumber2, new[] { WindowsVKey.VK_NUMPAD2 } },
        { CalculatorNumbers.CalculatorNumber3, new[] { WindowsVKey.VK_NUMPAD3 } },
        { CalculatorNumbers.CalculatorNumber4, new[] { WindowsVKey.VK_NUMPAD4 } },
        { CalculatorNumbers.CalculatorNumber5, new[] { WindowsVKey.VK_NUMPAD5 } },
        { CalculatorNumbers.CalculatorNumber6, new[] { WindowsVKey.VK_NUMPAD6 } },
        { CalculatorNumbers.CalculatorNumber7, new[] { WindowsVKey.VK_NUMPAD7 } },
        { CalculatorNumbers.CalculatorNumber8, new[] { WindowsVKey.VK_NUMPAD8 } },
        { CalculatorNumbers.CalculatorNumber9, new[] { WindowsVKey.VK_NUMPAD9 } }
    };

    protected override IReadOnlyDictionary<Enum, ProcessCommand> ProcessCommands => new Dictionary<Enum, ProcessCommand>
    {
        { CalculatorInputParameters.CalculatorOpen, ProcessCommand.Start },
        { CalculatorInputParameters.CalculatorClose, ProcessCommand.Stop }
    };

    public override void CreateAttributes()
    {
        CreateParameter(CalculatorAttributes.CalculatorSendValue, "Send Value", "Send the current value of the calculator", "/avatar/parameters/CalculatorResult");
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        switch (key)
        {
            case CalculatorInputParameters.CalculatorOpen:
                if (!isCalculatorOpen) ExecuteProcessCommand(ProcessCommand.Start);
                isCalculatorOpen = true;
                break;

            case CalculatorInputParameters.CalculatorClose:
                if (isCalculatorOpen) ExecuteProcessCommand(ProcessCommand.Stop);
                isCalculatorOpen = false;
                break;

            case CalculatorInputParameters.CalculatorCopyValue:
                calculatorResult = returnClipboardValue();
                SendParameter(CalculatorAttributes.CalculatorSendValue, calculatorResult);
                break;
        }

        ExecuteShortcut(key);
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        if (!key.Equals(CalculatorInputParameters.CalculatorNumber) || !isCalculatorOpen) return;

        var number = (int)Math.Round(value * 9);
        ExecuteShortcut(CalculatorNumbers.CalculatorNumber0 + number); // Holy shit if this works then I'm so fucking lucky
        ExecuteShortcut(CalculatorInputParameters.CalculatorCopyValue);
        calculatorResult = returnClipboardValue();
        SendParameter(CalculatorAttributes.CalculatorSendValue, calculatorResult);
    }

    private float returnClipboardValue()
    {
        var clipboard = new WindowsClipboard().GetText();
        if (clipboard.Length == 0) return 0;

        if (!float.TryParse(clipboard, out float value)) return 0;

        Terminal.Log($"Received clipboard value of {value}");
        return value;
    }
}

public enum CalculatorNumbers
{
    CalculatorNumber0,
    CalculatorNumber1,
    CalculatorNumber2,
    CalculatorNumber3,
    CalculatorNumber4,
    CalculatorNumber5,
    CalculatorNumber6,
    CalculatorNumber7,
    CalculatorNumber8,
    CalculatorNumber9
}

// TODO: Remove the need for open/close inputs and do it automatically based on whether or not it's already open
public enum CalculatorInputParameters
{
    CalculatorOpen,
    CalculatorClose,
    CalculatorClear,
    CalculatorCalculate,
    CalculatorCopyValue,
    CalculatorAdd,
    CalculatorSubtract,
    CalculatorMultiply,
    CalculatorDivide,
    CalculatorNumber
}

public enum CalculatorAttributes
{
    CalculatorSendValue
}
