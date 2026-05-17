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
    public required DataTemplate FlowOutput { get; set; }
    public required DataTemplate FlowOutputList { get; set; }
    public required DataTemplate FlowInput { get; set; }
    public required DataTemplate FlowInputList { get; set; }
    public required DataTemplate ValueOutput { get; set; }
    public required DataTemplate ValueOutputList { get; set; }
    public required DataTemplate ValueInput { get; set; }
    public required DataTemplate ValueInputList { get; set; }
    public required DataTemplate CheckBoxValueInput { get; set; }
    public required DataTemplate TextBoxValueInput { get; set; }
    public required DataTemplate ComboBoxValueInput { get; set; }
    public required DataTemplate KeybindValueInput { get; set; }
    public required DataTemplate DateTimeValueInput { get; set; }
    public required DataTemplate TimeSpanValueInput { get; set; }

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
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (type == typeof(bool)) return CheckBoxValueInput;
                if (NodeConstants.TEXTBOX_TYPES.Contains(type)) return TextBoxValueInput;
                if (type == typeof(Keybind)) return KeybindValueInput;
                if (type.IsEnum) return ComboBoxValueInput;
                if (type == typeof(DateTime)) return DateTimeValueInput;
                if (type == typeof(TimeSpan)) return TimeSpanValueInput;

                return ValueInput;
            }

            case NodeValueInputListViewModel:
                return ValueInputList;

            default:
                return null;
        }
    }
}