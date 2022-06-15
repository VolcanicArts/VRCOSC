// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
    public override string TargetProcess => "calc";

    private bool isCalculatorOpen;
    private float calculatorResult;

    public override void CreateAttributes()
    {
        CreateOutputParameter(CalculatorAttributes.CalculatorSendValue, "Send Value", "Send the current value of the calculator", "/avatar/parameters/CalculatorResult");

        RegisterInputParameter(CalculatorInputParameters.CalculatorOpen, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorClose, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorClear, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorCalculate, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorCopyValue, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorAdd, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorSubtract, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorMultiply, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorDivide, typeof(bool));
        RegisterInputParameter(CalculatorInputParameters.CalculatorNumber, typeof(float));

        RegisterKeyCombination(CalculatorInputParameters.CalculatorClear, WindowsVKey.VK_ESCAPE);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorCalculate, WindowsVKey.VK_RETURN);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorCopyValue, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_C);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorAdd, WindowsVKey.VK_ADD);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorSubtract, WindowsVKey.VK_SUBTRACT);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorMultiply, WindowsVKey.VK_MULTIPLY);
        RegisterKeyCombination(CalculatorInputParameters.CalculatorDivide, WindowsVKey.VK_DIVIDE);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber0, WindowsVKey.VK_NUMPAD0);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber1, WindowsVKey.VK_NUMPAD1);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber2, WindowsVKey.VK_NUMPAD2);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber3, WindowsVKey.VK_NUMPAD3);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber4, WindowsVKey.VK_NUMPAD4);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber5, WindowsVKey.VK_NUMPAD5);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber6, WindowsVKey.VK_NUMPAD6);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber7, WindowsVKey.VK_NUMPAD7);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber8, WindowsVKey.VK_NUMPAD8);
        RegisterKeyCombination(CalculatorNumbers.CalculatorNumber9, WindowsVKey.VK_NUMPAD9);
    }

    public override void Start()
    {
        isCalculatorOpen = IsProcessOpen(); // TODO: What if there are multiple calculator processes open at once?
        if (isCalculatorOpen) sendResult();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        switch (key)
        {
            case CalculatorInputParameters.CalculatorOpen:
                if (!isCalculatorOpen) StartTarget();
                isCalculatorOpen = true;
                break;

            case CalculatorInputParameters.CalculatorClose:
                if (isCalculatorOpen) StopTarget();
                isCalculatorOpen = false;
                break;

            case CalculatorInputParameters.CalculatorCopyValue:
                sendResult();
                break;
        }

        ExecuteShortcut(key);
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        if (!key.Equals(CalculatorInputParameters.CalculatorNumber) || !isCalculatorOpen) return;

        var number = (int)Math.Round(value * 9);
        ExecuteShortcut(CalculatorNumbers.CalculatorNumber0 + number); // Holy shit if this works then I'm so fucking lucky
        sendResult();
    }

    private float returnClipboardValue()
    {
        var clipboard = new WindowsClipboard().GetText();
        if (clipboard.Length == 0) return 0;

        if (!float.TryParse(clipboard, out float value)) return 0;

        Terminal.Log($"Received clipboard value of {value}");
        return value;
    }

    private void sendResult()
    {
        ExecuteShortcut(CalculatorInputParameters.CalculatorCopyValue);
        calculatorResult = returnClipboardValue();
        SendParameter(CalculatorAttributes.CalculatorSendValue, calculatorResult);
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
