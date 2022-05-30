using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Calculator;

public class CalculatorModule : IntegrationModule
{
    public override string Title => "Calculator";
    public override string Description => "Integrate with the Windows calculator for efficient maths.";
    public override string Author => "Buckminsterfullerene";
    public override Colour4 Colour => Color4Extensions.FromHex(@"ff2600").Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;

    private CalculatorModuleValues values;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        CalculatorInputParameters.CalculatorOpen,
        CalculatorInputParameters.CalculatorClose,
        CalculatorInputParameters.CalculatorClear,
        CalculatorInputParameters.CalculatorCalculate,
        CalculatorInputParameters.CalculatorSendValue,
        CalculatorInputParameters.CalculatorAdd,
        CalculatorInputParameters.CalculatorSubtract,
        CalculatorInputParameters.CalculatorMultiply,
        CalculatorInputParameters.CalculatorDivide,
        CalculatorInputParameters.CalculatorNumber
    };

    protected override string TargetProcess => "calc";
    protected override string TargetWindow => "calc.exe";

    protected override IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>()
    {
        { CalculatorInputParameters.CalculatorClear, new[] { WindowsVKey.VK_ESCAPE } },
        { CalculatorInputParameters.CalculatorCalculate, new[] { WindowsVKey.VK_RETURN } },
        { CalculatorInputParameters.CalculatorSendValue, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_C } }, // TODO: Maybe the value should be sent every time something is executed
        { CalculatorInputParameters.CalculatorAdd, new[] { WindowsVKey.VK_ADD } },
        { CalculatorInputParameters.CalculatorSubtract, new[] { WindowsVKey.VK_SUBTRACT } },
        { CalculatorInputParameters.CalculatorMultiply, new[] { WindowsVKey.VK_MULTIPLY } },
        { CalculatorInputParameters.CalculatorDivide, new[] { WindowsVKey.VK_DIVIDE } },
        { CalculatorInputParameters.CalculatorNumber, new[] { WindowsVKey.VK_NUMPAD0 } }
    };

    protected override IReadOnlyDictionary<Enum, ProcessCommand> ProcessCommands => new Dictionary<Enum, ProcessCommand>()
    {
        { CalculatorInputParameters.CalculatorOpen, ProcessCommand.Start },
        { CalculatorInputParameters.CalculatorClose, ProcessCommand.Stop }
    };

    public override void CreateAttributes()
    {
        CreateParameter(CalculatorInputParameters.CalculatorSendValue, "Send Value", "Send the current value of the calculator.", "/avatar/parameters/CalculatorResult");
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        switch (key)
        {
            case CalculatorInputParameters.CalculatorOpen:
                if (!values.IsCalculatorOpen) ExecuteProcessCommand(ProcessCommand.Start);
                break;

            case CalculatorInputParameters.CalculatorClose:
                if (values.IsCalculatorOpen) ExecuteProcessCommand(ProcessCommand.Stop);
                break;

            case CalculatorInputParameters.CalculatorSendValue:
                switchToTarget();
                SendParameter(CalculatorInputParameters.CalculatorSendValue, values.CalculatorResult);
                switchToReturn();
                // ExecuteFunctionInTarget(SendParameter, CalculatorInputParameters.CalculatorSendValue, values.CalculatorResult);
                break;
        }

        ExecuteShortcut(key);
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {

    }
}

public struct CalculatorModuleValues
{
    public bool IsCalculatorOpen;
    public float CalculatorResult;
}

// TODO: Remove the need for open/close inputs and do it automatically based on whether or not it's already open
public enum CalculatorInputParameters
{
    CalculatorOpen,
    CalculatorClose,
    CalculatorClear,
    CalculatorCalculate,
    CalculatorSendValue,
    CalculatorAdd,
    CalculatorSubtract,
    CalculatorMultiply,
    CalculatorDivide,
    CalculatorNumber
}
