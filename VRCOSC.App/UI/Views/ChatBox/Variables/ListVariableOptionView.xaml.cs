// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.ChatBox.Variables;

public partial class ListVariableOptionView
{
    private readonly ClipVariable clipVariable;
    private readonly PropertyInfo propertyInfo;

    public ObservableCollection<Observable<string>> Items { get; } = new();

    public ListVariableOptionView(ClipVariable clipVariable, PropertyInfo propertyInfo)
    {
        this.clipVariable = clipVariable;
        this.propertyInfo = propertyInfo;
        InitializeComponent();

        var items = (List<string>)propertyInfo.GetValue(clipVariable)!;
        Items.AddRange(items.Select(item => new Observable<string>(item)));

        Items.OnCollectionChanged((newItems, _) =>
        {
            foreach (var newItem in newItems)
            {
                newItem.Subscribe(updateItems, true);
            }

            updateItems();
        }, true);

        DataContext = this;
    }

    private void updateItems()
    {
        propertyInfo.SetValue(clipVariable, Items.Select(item => item.Value).ToList());
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        Items.Add(new Observable<string>(string.Empty));
    }

    private void RemoveButton_ButtonClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var item = (Observable<string>)element.Tag;

        Items.Remove(item);
    }
}
