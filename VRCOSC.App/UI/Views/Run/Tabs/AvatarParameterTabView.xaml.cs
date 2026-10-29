// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        refill(OutgoingListView, OutgoingFiltered, OutgoingStore, text);
        refill(IncomingListView, IncomingFiltered, IncomingStore, text);
    }

    private static void refill(ListView listView, ObservableCollection<AvatarParameterStore> target, ObservableDictionary<string, AvatarParameterStore> source, string text)
    {
        var selectedNames = listView.SelectedItems.OfType<AvatarParameterStore>().Select(s => s.Name).ToHashSet();

        target.Clear();
        target.AddRange(source.Values.Where(s => string.IsNullOrWhiteSpace(text) || s.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase)).OrderBy(s => s.Name));

        if (selectedNames.Count == 0) return;

        foreach (var store in target.Where(s => selectedNames.Contains(s.Name)))
        {
            listView.SelectedItems.Add(store);
        }
    }

    private static string formatValue(AvatarParameterStore store) => Convert.ToString(store.Value.Value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static void copyToClipboard(ListView listView, Func<AvatarParameterStore, string> selector)
    {
        var lines = listView.SelectedItems.OfType<AvatarParameterStore>().OrderBy(s => s.Name).Select(selector).ToList();
        if (lines.Count == 0) return;

        try
        {
            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to copy parameters to the clipboard");
        }
    }

    private static ListView? getListViewFromMenuItem(object sender) => (sender as MenuItem)?.Parent is ContextMenu { PlacementTarget: ListView listView } ? listView : null;

    private void ListView_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ListView listView || Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.C) return;

        copyToClipboard(listView, s => $"{s.Name}={formatValue(s)}");
        e.Handled = true;
    }

    private void CopyName_OnClick(object sender, RoutedEventArgs e)
    {
        if (getListViewFromMenuItem(sender) is { } listView) copyToClipboard(listView, s => s.Name);
    }

    private void CopyValue_OnClick(object sender, RoutedEventArgs e)
    {
        if (getListViewFromMenuItem(sender) is { } listView) copyToClipboard(listView, formatValue);
    }

    private void CopyNameAndValue_OnClick(object sender, RoutedEventArgs e)
    {
        if (getListViewFromMenuItem(sender) is { } listView) copyToClipboard(listView, s => $"{s.Name}={formatValue(s)}");
    }

    private void SelectAll_OnClick(object sender, RoutedEventArgs e)
    {
        getListViewFromMenuItem(sender)?.SelectAll();
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