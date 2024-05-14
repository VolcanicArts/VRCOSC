// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Run.Tabs;

public partial class RuntimeView
{
    public Observable<IEnumerable<RuntimePage>> Pages { get; } = new(new List<RuntimePage>());
    public Observable<Visibility> PageListVisibility { get; } = new(Visibility.Collapsed);

    public RuntimeView()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().State.Subscribe(_ =>
        {
            Pages.Value = ModuleManager.GetInstance().Modules.Values.SelectMany(moduleList => moduleList).Where(module => module.RuntimePage is not null).Select(module => new RuntimePage(module.Title, module.RuntimePage!));
            PageListVisibility.Value = AppManager.GetInstance().State.Value == AppManagerState.Started ? Visibility.Visible : Visibility.Collapsed;

            Console.WriteLine(Pages.Value.Count());
        });
    }
}

public record RuntimePage(string Title, Page Page);
