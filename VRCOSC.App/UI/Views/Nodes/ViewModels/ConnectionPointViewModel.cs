// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public abstract class ConnectionPointViewModel : ObservableObject
{
    public int Index { get; }
    public bool RenderName { get; }
    public abstract INodeElement Element { get; }

    protected ConnectionPointViewModel(int index, bool renderName)
    {
        Index = index;
        RenderName = renderName;
    }
}

public interface IConnectionPointListViewModel
{
    ObservableCollection<ConnectionPointViewModel> ItemsSource { get; }
    void AddItem();
    void RemoveItem();
}

public abstract class ConnectionPointListViewModel : ObservableObject
{
    public abstract INodeElement Element { get; }
    public abstract void AddItem();
    public abstract void RemoveItem();
}

public partial class NodeFlowOutputViewModel : ConnectionPointViewModel
{
    public override IFlowOutputBase Element { get; }

    public NodeFlowOutputViewModel(IFlowOutputBase element, int index = 0, bool renderName = true) : base(index, renderName)
    {
        Element = element;
    }
}

public partial class NodeFlowOutputListViewModel : ConnectionPointListViewModel, IConnectionPointListViewModel
{
    public override IFlowOutputList Element { get; }

    public ObservableCollection<ConnectionPointViewModel> ItemsSource { get; } = [];

    public NodeFlowOutputListViewModel(IFlowOutputList element)
    {
        Element = element;
        ItemsSource.AddRange(Enumerable.Range(0, element.Metadata.Size).Select(i => new NodeFlowOutputViewModel(element, i, i == 0)));
    }

    public override void AddItem()
    {
        ItemsSource.Add(new NodeFlowOutputViewModel(Element, Element.Metadata.Size - 1, false));
    }

    public override void RemoveItem()
    {
        ItemsSource.RemoveAt(ItemsSource.Count - 1);
    }
}

public partial class NodeFlowInputViewModel : ConnectionPointViewModel
{
    public override IFlowInputBase Element { get; }

    public NodeFlowInputViewModel(IFlowInputBase element, int index = 0, bool renderName = true) : base(index, renderName)
    {
        Element = element;
    }
}

public partial class NodeFlowInputListViewModel : ConnectionPointListViewModel, IConnectionPointListViewModel
{
    public override IFlowInputList Element { get; }

    public ObservableCollection<ConnectionPointViewModel> ItemsSource { get; } = [];

    public NodeFlowInputListViewModel(IFlowInputList element)
    {
        Element = element;
        ItemsSource.AddRange(Enumerable.Range(0, element.Metadata.Size).Select(i => new NodeFlowInputViewModel(element, i, i == 0)));
    }

    public override void AddItem()
    {
        ItemsSource.Add(new NodeFlowInputViewModel(Element, Element.Metadata.Size - 1, false));
    }

    public override void RemoveItem()
    {
        ItemsSource.RemoveAt(ItemsSource.Count - 1);
    }
}

public partial class NodeValueInputViewModel : ConnectionPointViewModel
{
    public override IValueInputBase Element { get; }
    public string TypeName { get; }

    [ObservableProperty]
    private bool renderConnection;

    [ObservableProperty]
    private bool renderInline;

    public NodeValueInputViewModel(IValueInputBase element, int index = 0, bool renderName = true, bool? renderInlineOverride = null) : base(index, renderName)
    {
        Element = element;
        TypeName = element.Metadata.Shared.ValueType.GetFriendlyName();

        if (element is IValueInput)
        {
            var canInline = element.Metadata.Shared.Modes.HasFlag(InputModes.Inline);
            var canConnection = element.Metadata.Shared.Modes.HasFlag(InputModes.Connection);

            RenderConnection = canConnection;

            if (renderInlineOverride is null)
            {
                RenderInline = canInline && !element.IsConnected;

                element.OnIsConnectedChanged += () => { RenderInline = canInline && !element.IsConnected; };
            }
            else
            {
                RenderInline = element.Metadata.Shared.Modes == InputModes.Inline || renderInlineOverride.Value;
            }
        }
    }
}

public partial class NodeValueInputListViewModel : ConnectionPointListViewModel, IConnectionPointListViewModel
{
    public override IValueInputList Element { get; }

    public ObservableCollection<ConnectionPointViewModel> ItemsSource { get; } = [];

    public NodeValueInputListViewModel(IValueInputList element)
    {
        Element = element;
        ItemsSource.AddRange(Enumerable.Range(0, element.Metadata.Size).Select(i => new NodeValueInputViewModel(element, i, i == 0)));
    }

    public override void AddItem()
    {
        ItemsSource.Add(new NodeValueInputViewModel(Element, Element.Metadata.Size - 1, false));
    }

    public override void RemoveItem()
    {
        ItemsSource.RemoveAt(ItemsSource.Count - 1);
    }
}

public partial class NodeValueOutputViewModel : ConnectionPointViewModel
{
    public override IValueOutputBase Element { get; }
    public string TypeName { get; }

    public NodeValueOutputViewModel(IValueOutputBase element, int index = 0, bool renderName = true) : base(index, renderName)
    {
        Element = element;
        TypeName = element.Metadata.Shared.ValueType.GetFriendlyName();
    }
}

public partial class NodeValueOutputListViewModel : ConnectionPointListViewModel, IConnectionPointListViewModel
{
    public override IValueOutputList Element { get; }

    public ObservableCollection<ConnectionPointViewModel> ItemsSource { get; } = [];

    public NodeValueOutputListViewModel(IValueOutputList element)
    {
        Element = element;
        ItemsSource.AddRange(Enumerable.Range(0, element.Metadata.Size).Select(i => new NodeValueOutputViewModel(element, i, i == 0)));
    }

    public override void AddItem()
    {
        ItemsSource.Add(new NodeValueOutputViewModel(Element, Element.Metadata.Size - 1, false));
    }

    public override void RemoveItem()
    {
        ItemsSource.RemoveAt(ItemsSource.Count - 1);
    }
}