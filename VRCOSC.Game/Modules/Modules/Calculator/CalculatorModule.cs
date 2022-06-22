// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform.Windows;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Calculator;

public class CalculatorModule : IntegrationModule
{
    public override string Title => "Calculator";
    public override string Description => "Integrate with the Windows calculator for efficient maths";
    public override string Author => "Buckminsterfullerene";
    public override Colour4 Colour => Color4Extensions.FromHex(@"ff2600").Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string TargetProcess => "calc";

    private bool isCalculatorOpen;
    private float calculatorResult;

    public override void CreateAttributes()
    {
        CreateOutputParameter(CalculatorOutputParameter.CalculatorSendValue, "Send Value", "Send the current value of the calculator", "/avatar/parameters/CalculatorResult");

        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorOpen, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorClose, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorClear, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorCalculate, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorCopyValue, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorAdd, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorSubtract, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorMultiply, ActionMenu.Button);
        RegisterInputParameter<bool>(CalculatorInputParameter.CalculatorDivide, ActionMenu.Button);
        RegisterInputParameter<float>(CalculatorInputParameter.CalculatorNumber);

        RegisterKeyCombination(CalculatorInputParameter.CalculatorClear, WindowsVKey.VK_ESCAPE);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorCalculate, WindowsVKey.VK_RETURN);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorCopyValue, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_C);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorAdd, WindowsVKey.VK_ADD);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorSubtract, WindowsVKey.VK_SUBTRACT);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorMultiply, WindowsVKey.VK_MULTIPLY);
        RegisterKeyCombination(CalculatorInputParameter.CalculatorDivide, WindowsVKey.VK_DIVIDE);
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

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case CalculatorInputParameter.CalculatorOpen:
                if (!isCalculatorOpen) StartTarget();
                isCalculatorOpen = true;
                break;

            case CalculatorInputParameter.CalculatorClose:
                if (isCalculatorOpen) StopTarget();
                isCalculatorOpen = false;
                break;

            case CalculatorInputParameter.CalculatorCopyValue:
                sendResult();
                break;
        }

        ExecuteKeyCombination(key);
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        if (!key.Equals(CalculatorInputParameter.CalculatorNumber) || !isCalculatorOpen) return;

        var number = (int)Math.Round(value * 9);
        ExecuteKeyCombination(CalculatorNumbers.CalculatorNumber0 + number); // Holy shit if this works then I'm so fucking lucky
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
        ExecuteKeyCombination(CalculatorInputParameter.CalculatorCopyValue);
        calculatorResult = returnClipboardValue();
        SendParameter(CalculatorOutputParameter.CalculatorSendValue, calculatorResult);
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

    private enum CalculatorInputParameter
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

    private enum CalculatorOutputParameter
    {
        CalculatorSendValue
    }
}
