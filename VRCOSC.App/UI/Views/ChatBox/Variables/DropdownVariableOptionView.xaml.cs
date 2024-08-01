// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Reflection;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.ChatBox.Options;

public partial class DropdownVariableOptionView
{
    public Observable<int> SelectedIndex { get; } = new();

    public DropdownVariableOptionView(ClipVariable instance, PropertyInfo propertyInfo)
    {
        InitializeComponent();

        DataContext = this;

        ValueComboBox.ItemsSource = Enum.GetValues(propertyInfo.PropertyType);

        SelectedIndex.Value = (int)Convert.ChangeType(propertyInfo.GetValue(instance), typeof(int))!;
        SelectedIndex.Subscribe(newIndex => propertyInfo.SetValue(instance, Enum.ToObject(propertyInfo.PropertyType, newIndex)));
    }
}
