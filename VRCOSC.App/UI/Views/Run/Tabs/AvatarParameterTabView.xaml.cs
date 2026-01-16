// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public class AvatarParameterStore
{
    public string Name { get; }
    public Observable<object?> Value { get; }

    public AvatarParameterStore(string name, object? value)
    {
        Name = name;
        Value = new Observable<object?>(value);
    }
}

public partial class AvatarParameterTabView
{
    private readonly ConcurrentDictionary<string, object> outgoingLocal = new();
    private readonly ConcurrentDictionary<string, object> incomingLocal = new();

    public ObservableDictionary<string, AvatarParameterStore> OutgoingStore { get; } = [];
    public ObservableDictionary<string, AvatarParameterStore> IncomingStore { get; } = [];

    public ObservableCollection<AvatarParameterStore> OutgoingFiltered { get; } = [];
    public ObservableCollection<AvatarParameterStore> IncomingFiltered { get; } = [];

    private readonly DispatcherTimer timer;

    public AvatarParameterTabView()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageSent += onVRChatOSCMessageSent;
        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageReceived += onVRChatOSCMessageReceived;
        AppManager.GetInstance().State.Subscribe(OnAppManagerStateChange);

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        timer.Tick += update;
        timer.Start();

        DataContext = this;
        SearchTextBox.TextChanged += (_, _) => filter(SearchTextBox.Text);
        filter(string.Empty);
    }

    private void filter(string text)
    {
        OutgoingFiltered.Clear();
        OutgoingFiltered.AddRange(OutgoingStore.Values.Where(s => string.IsNullOrWhiteSpace(text) || s.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase)).OrderBy(s => s.Name));
        IncomingFiltered.Clear();
        IncomingFiltered.AddRange(IncomingStore.Values.Where(s => string.IsNullOrWhiteSpace(text) || s.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase)).OrderBy(s => s.Name));
    }

    private void update(object? sender, EventArgs e)
    {
        var newStores = false;

        foreach (var pair in outgoingLocal)
        {
            if (OutgoingStore.TryGetValue(pair.Key, out var store))
            {
                store.Value.Value = pair.Value;
            }
            else
            {
                OutgoingStore[pair.Key] = new AvatarParameterStore(pair.Key, pair.Value);
                newStores = true;
            }
        }

        outgoingLocal.Clear();

        foreach (var pair in incomingLocal)
        {
            if (IncomingStore.TryGetValue(pair.Key, out var store))
            {
                store.Value.Value = pair.Value;
            }
            else
            {
                IncomingStore[pair.Key] = new AvatarParameterStore(pair.Key, pair.Value);
                newStores = true;
            }
        }

        incomingLocal.Clear();

        if (newStores)
            filter(SearchTextBox.Text);
    }

    private void onVRChatOSCMessageSent(VRChatOSCMessage e)
    {
        if (!e.IsAvatarParameter) return;

        outgoingLocal[e.ParameterName] = e.ParameterValue;
    }

    private Task onVRChatOSCMessageReceived(VRChatOSCMessage e)
    {
        if (!e.IsAvatarParameter) return Task.CompletedTask;

        incomingLocal[e.ParameterName] = e.ParameterValue;

        return Task.CompletedTask;
    }

    private void OnAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        if (newState != AppManagerState.Starting) return;

        filter(SearchTextBox.Text);
    });
}