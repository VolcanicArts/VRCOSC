// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class RuntimeTabView
{
    public ObservableCollection<RuntimeView> Pages { get; } = new();
    public Observable<Visibility> PageListVisibility { get; } = new(Visibility.Collapsed);

    public RuntimeTabView()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().State.Subscribe(_ => Dispatcher.Invoke(() =>
        {
            Pages.Clear();
            Pages.AddRange(ModuleManager.GetInstance().RunningModules.Where(module => module.RuntimeView is not null).Select(module => new RuntimeView(module.Title, module.RuntimeView!)));
            PageListVisibility.Value = AppManager.GetInstance().State.Value == AppManagerState.Started ? Visibility.Visible : Visibility.Collapsed;
        }));
    }
}

public record RuntimeView(string Title, UserControl View);
