// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FontAwesome6;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Metadata;

public enum ConnectionPoint
{
    FlowOutput,
    FlowInput,
    ValueOutput,
    ValueInput
}

public interface INodeElementSharedMetadata
{
    FieldInfo FieldInfo { get; }
    int Slot { get; }
    bool IsList { get; }
    Type ValueType { get; }
    bool IsInlineable { get; }
}

public record NodeElementSharedMetadata : INodeElementSharedMetadata
{
    public required FieldInfo FieldInfo { get; init; }
    public required int Slot { get; init; }
    public required bool IsList { get; init; }
    public required bool IsInlineable { get; init; }

    /// <summary>
    /// Should only be called on <see cref="IValueInput{T}"/> or <see cref="IValueInputList{T}"/>
    /// </summary>
    public Type ValueType => FieldInfo.FieldType.GetGenericArguments()[0];
}

public interface INodeElementMetadata
{
    INodeElementSharedMetadata Shared { get; }
    INodeElement Instance { get; }
    int Size { get; set; }
    int WorkingSize { get; }
}

public sealed class NodeElementMetadata : INodeElementMetadata
{
    public required INodeElementSharedMetadata Shared { get; init; }
    public required INodeElement Instance { get; init; }
    public int Size { get; set; }
    public int WorkingSize => Shared.IsList ? Size : 1;
}

public interface INodeSharedMetadata
{
    Type Type { get; }
    string Name { get; }
    string Path { get; }
    string PathRoot { get; }
    EFontAwesomeIcon[] Icons { get; }
    Type[] GenericTypes { get; }
    Type[] GenericsFilter { get; }
    Dictionary<ConnectionPoint, INodeElementSharedMetadata[]> Elements { get; }
    Dictionary<string, PropertyInfo> Properties { get; }
    bool Reprocess { get; }
    bool IsCollapsed { get; }
    bool NoCancel { get; }
    bool IsContinuous { get; }
    bool IsActiveUpdate { get; }

    int FlowOutputCount { get; }
    int FlowInputCount { get; }
    int ValueOutputCount { get; }
    int ValueInputCount { get; }

    bool IsFlowOutput { get; }
    bool IsFlowInput { get; }
    bool IsValueOutput { get; }
    bool IsValueInput { get; }
    bool HasListElements { get; }
    bool HasProperties { get; }

    bool IsFlow { get; }
    bool IsValue { get; }
    bool IsFlowOutputTrigger { get; }
    bool IsValueInputTrigger { get; }
    bool IsAnyTrigger { get; }
}

public sealed class NodeSharedMetadata : INodeSharedMetadata
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string PathRoot { get; init; }
    public required EFontAwesomeIcon[] Icons { get; init; }
    public required Type[] GenericTypes { get; init; }
    public required Type[] GenericsFilter { get; init; }
    public required Dictionary<ConnectionPoint, INodeElementSharedMetadata[]> Elements { get; init; }
    public required Dictionary<string, PropertyInfo> Properties { get; init; }
    public required bool Reprocess { get; init; }
    public required bool IsCollapsed { get; init; }
    public required bool NoCancel { get; init; }
    public required bool IsContinuous { get; init; }
    public required bool IsActiveUpdate { get; init; }

    public int FlowOutputCount => Elements[ConnectionPoint.FlowOutput].Length;
    public int FlowInputCount => Elements[ConnectionPoint.FlowInput].Length;
    public int ValueOutputCount => Elements[ConnectionPoint.ValueOutput].Length;
    public int ValueInputCount => Elements[ConnectionPoint.ValueInput].Length;

    public bool IsFlowOutput => FlowOutputCount != 0;
    public bool IsFlowInput => FlowInputCount != 0;
    public bool IsValueOutput => ValueOutputCount != 0;
    public bool IsValueInput => ValueInputCount != 0;
    public bool HasListElements => Elements.Any(p => p.Value.Any(e => e.IsList));
    public bool HasProperties => Properties.Count != 0;

    public bool IsFlow => IsFlowInput || IsFlowOutput;
    public bool IsValue => IsValueInput || IsValueOutput;
    public bool IsFlowOutputTrigger => IsFlowOutput && !IsFlowInput && IsValueInput;
    public bool IsValueInputTrigger => !IsFlow && IsValueInput && !IsValueOutput;
    public bool IsAnyTrigger => IsFlowOutputTrigger || IsValueInputTrigger;
}

public interface INodeMetadata
{
    INodeSharedMetadata Shared { get; }
    Vector2 Position { get; set; }
    int ZIndex { get; set; }
    Dictionary<ConnectionPoint, INodeElementMetadata[]> Elements { get; }
    bool IsSourceNode { get; }
    bool IsDriveNode { get; }

    INodeElementMetadata ElementMetadataFor(INodeElement element)
    {
        try
        {
            return Elements.Values.SelectMany(a => a).Single(m => m.Instance == element);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{Shared.Name} has encountered an error");
            return null;
        }
    }

    int[] ElementSizesFor(ConnectionPoint point) => Elements[point].Select(i => i.Size).ToArray();
    INodeElement[] ElementInstancesFor(ConnectionPoint point) => Elements[point].Select(i => i.Instance).ToArray();
}

public sealed class NodeMetadata : INodeMetadata
{
    public required INodeSharedMetadata Shared { get; init; }
    public Vector2 Position { get; set; }
    public int ZIndex { get; set; }
    public required Dictionary<ConnectionPoint, INodeElementMetadata[]> Elements { get; init; }
    public bool IsSourceNode => !Shared.IsFlow && Shared.ValueInputCount == 1 && Shared.ValueOutputCount == 1 && ((IValueInputBase)Elements[ConnectionPoint.ValueInput][0].Instance).Modes == ValueInputMode.Inline;
    public bool IsDriveNode => !Shared.IsFlow && !Shared.IsValueOutput && Shared.ValueInputCount == 2 && ((IValueInputBase)Elements[ConnectionPoint.ValueInput][0].Instance).Modes == ValueInputMode.Inline;
}