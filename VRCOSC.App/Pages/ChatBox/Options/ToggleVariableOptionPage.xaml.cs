// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using VRCOSC.App.ChatBox.Clips.Variables;

namespace VRCOSC.App.Pages.ChatBox.Options;

public partial class ToggleVariableOptionPage
{
    public ToggleVariableOptionPage(ClipVariable instance, PropertyInfo propertyInfo)
    {
        InitializeComponent();

        var isCheckedBinding = new Binding(propertyInfo.Name)
        {
            Source = instance
        };

        ValueCheckBox.SetBinding(ToggleButton.IsCheckedProperty, isCheckedBinding);
    }
}
