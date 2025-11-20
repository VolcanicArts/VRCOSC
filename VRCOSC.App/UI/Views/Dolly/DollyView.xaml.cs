// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Dolly;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Dolly;

public partial class DollyView
{
    public DollyManager DollyManager => DollyManager.GetInstance();

    public Observable<int> Delay { get; } = new();
    public Observable<Visibility> ShowPlay { get; } = new();
    public Observable<Visibility> ShowStop { get; } = new(Visibility.Collapsed);

    public DollyView()
    {
        InitializeComponent();
        DataContext = this;

        AppManager.GetInstance().State.Subscribe(newState => Dispatcher.Invoke(() =>
        {
            if (newState == AppManagerState.Started)
            {
                Overlay.FadeOutFromOne(250);
            }
            else
            {
                Overlay.FadeIn(250);
            }
        }));

        DollyManager.OnPlay += () =>
        {
            ShowPlay.Value = Visibility.Collapsed;
            ShowStop.Value = Visibility.Visible;
        };

        DollyManager.OnStop += () =>
        {
            ShowPlay.Value = Visibility.Visible;
            ShowStop.Value = Visibility.Collapsed;
        };
    }

    private void LoadDolly_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var element = (FrameworkElement)sender;
            var dolly = (App.Dolly.Dolly)element.Tag;

            await DollyManager.GetInstance().Import(dolly);
        }
    }

    private void DeleteDolly_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var dolly = (App.Dolly.Dolly)element.Tag;

        var result = MessageBox.Show("Are you sure you want to delete this dolly?", "Dolly Delete Warning", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        DollyManager.GetInstance().Dollies.Remove(dolly);
    }

    private void ImportVRChat_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            await DollyManager.GetInstance().Export();
        }
    }

    private void ImportFile_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var filePath = await Platform.PickFileAsync(".json");
            if (filePath is null) return;

            DollyManager.GetInstance().ImportFile(filePath);
        }
    }

    private void Play_OnClick(object sender, RoutedEventArgs e)
    {
        DollyManager.GetInstance().Play();
    }

    private void PlayDelayed_OnClick(object sender, RoutedEventArgs e)
    {
        DollyManager.GetInstance().PlayDelayed(Delay.Value);
    }

    private void Stop_OnClick(object sender, RoutedEventArgs e)
    {
        DollyManager.GetInstance().Stop();
    }
}