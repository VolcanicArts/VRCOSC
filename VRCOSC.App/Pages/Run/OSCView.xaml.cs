// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.Pages.Run;

public partial class OSCView
{
    public ObservableDictionary<string, object> OutgoingMessages { get; } = new();
    public ObservableDictionary<string, object> IncomingMessages { get; } = new();

    public OSCView()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
        AppManager.GetInstance().VRChatOscClient.OnParameterReceived += OnParameterReceived;
        AppManager.GetInstance().State.Subscribe(OnAppManagerStateChange);

        DataContext = this;
    }

    private void OnParameterSent(VRChatOscMessage e) => Dispatcher.Invoke(() => OutgoingMessages[e.ParameterName] = e.ParameterValue);
    private void OnParameterReceived(VRChatOscMessage e) => Dispatcher.Invoke(() => IncomingMessages[e.ParameterName] = e.ParameterValue);

    private void OnAppManagerStateChange(AppManagerState newState)
    {
        if (newState == AppManagerState.Starting) OutgoingMessages.Clear();
    }
}

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = System.Convert.ToInt32(value);
        return index % 2 == 0 ? Application.Current.Resources["CBackground3"] : Application.Current.Resources["CBackground2"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
