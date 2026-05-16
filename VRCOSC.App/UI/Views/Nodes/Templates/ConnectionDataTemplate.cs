// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Nodes;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Views.Nodes.ViewModels;

namespace VRCOSC.App.UI.Views.Nodes.Templates;

public class ConnectionDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate FlowOutput { get; set; }
    public DataTemplate FlowOutputList { get; set; }
    public DataTemplate FlowInput { get; set; }
    public DataTemplate FlowInputList { get; set; }
    public DataTemplate ValueOutput { get; set; }
    public DataTemplate ValueOutputList { get; set; }
    public DataTemplate ValueInput { get; set; }
    public DataTemplate ValueInputList { get; set; }
    public DataTemplate CheckBoxValueInput { get; set; }
    public DataTemplate TextBoxValueInput { get; set; }
    public DataTemplate ComboBoxValueInput { get; set; }
    public DataTemplate KeybindValueInput { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        switch (item)
        {
            case NodeFlowOutputViewModel:
                return FlowOutput;

            case NodeFlowOutputListViewModel:
                return FlowOutputList;

            case NodeFlowInputViewModel:
                return FlowInput;

            case NodeFlowInputListViewModel:
                return FlowInputList;

            case NodeValueOutputViewModel:
                return ValueOutput;

            case NodeValueOutputListViewModel:
                return ValueOutputList;

            case NodeValueInputViewModel vivm:
            {
                var element = vivm.Element;
                var type = element.Metadata.Shared.ValueType;

                if (type == typeof(bool)) return CheckBoxValueInput;
                if (NodeConstants.TEXTBOX_TYPES.Contains(type)) return TextBoxValueInput;
                if (type == typeof(Keybind)) return KeybindValueInput;
                if (type.IsEnum) return ComboBoxValueInput;

                return ValueInput;
            }

            case NodeValueInputListViewModel:
                return ValueInputList;

            default:
                return null;
        }
    }
}