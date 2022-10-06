// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Platform.Windows;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Calculator;

public sealed class CalculatorModule : IntegrationModule
{
    public override string Title => "Calculator";
    public override string Description => "Integrate with the Windows calculator for efficient maths";
    public override string Author => "Buckminsterfullerene";
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string TargetProcess => "calc";

    private bool isCalculatorOpen;
    private float calculatorResult;

    protected override void CreateAttributes()
    {
        CreateOutgoingParameter(CalculatorOutgoingParameter.CalculatorResult, "Result Value", "The current result of the calculator", "/avatar/parameters/CalculatorResult");

        RegisterButtonInput(CalculatorIncomingParameter.CalculatorOpen);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorClose);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorClear);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorCalculate);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorCopyValue);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorAdd);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorSubtract);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorMultiply);
        RegisterButtonInput(CalculatorIncomingParameter.CalculatorDivide);
        RegisterRadialInput(CalculatorIncomingParameter.CalculatorNumber);

        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorClear, WindowsVKey.VK_ESCAPE);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorCalculate, WindowsVKey.VK_RETURN);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorCopyValue, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_C);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorAdd, WindowsVKey.VK_ADD);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorSubtract, WindowsVKey.VK_SUBTRACT);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorMultiply, WindowsVKey.VK_MULTIPLY);
        RegisterKeyCombination(CalculatorIncomingParameter.CalculatorDivide, WindowsVKey.VK_DIVIDE);
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

    protected override void OnStart()
    {
        isCalculatorOpen = IsTargetProcessOpen();
        EnsureSingleTargetProcess();
        if (isCalculatorOpen) sendResult();
    }

    protected override void OnButtonPressed(Enum key)
    {
        switch (key)
        {
            case CalculatorIncomingParameter.CalculatorOpen:
                if (!isCalculatorOpen) StartTarget();
                isCalculatorOpen = true;
                break;

            case CalculatorIncomingParameter.CalculatorClose:
                if (isCalculatorOpen) StopTarget();
                isCalculatorOpen = false;
                break;

            case CalculatorIncomingParameter.CalculatorCopyValue:
                sendResult();
                break;
        }

        ExecuteKeyCombination(key);
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        if (!key.Equals(CalculatorIncomingParameter.CalculatorNumber) || !isCalculatorOpen) return;

        var number = (int)Math.Round(value * 9);
        ExecuteKeyCombination((CalculatorNumbers)number);
        sendResult();
    }

    private float returnClipboardValue()
    {
        var clipboard = new WindowsClipboard().GetText();
        if (clipboard.Length == 0) return 0;

        if (!float.TryParse(clipboard, out float value)) return 0;

        Log($"Received clipboard value of {value}");
        return value;
    }

    private void sendResult()
    {
        ExecuteKeyCombination(CalculatorIncomingParameter.CalculatorCopyValue);
        calculatorResult = returnClipboardValue();
        SendParameter(CalculatorOutgoingParameter.CalculatorResult, calculatorResult);
    }

    private enum CalculatorNumbers
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

    private enum CalculatorIncomingParameter
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

    private enum CalculatorOutgoingParameter
    {
        CalculatorResult
    }
}
