// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

public static class NodeGraphBaseHelper
{
    public static string RunTypeMigration(string type)
    {
        if (type.Contains("IndirectSendParameterNode"))
            return type.Replace("IndirectSendParameterNode", "SendParameterNode");

        if (type.Contains("DirectSendParameterNode"))
            return type.Replace("DirectSendParameterNode", "SendParameterNode");

        if (type.Contains("RichTextNode"))
            return type.Replace("RichTextNode", "ValueNode<System.String>");

        return type;
    }

    public static bool RunPropertyMigration(INode node, string originalType, string propertyKey, object? propertyValue)
    {
        var migrations = new Dictionary<string, (string PropertyKey, Type? TargetType, int Slot)>
        {
            { "DirectSendParameterNode", ("text", typeof(string), 0) },
            { "DriveParameterNode", ("text", typeof(string), 0) },
            { "ToggleParameterNode", ("text", typeof(string), 0) },
            { "ParameterSourceNode", ("text", typeof(string), 0) },
            { "PhysboneParameterSourceNode", ("text", typeof(string), 0) },
            { "RaycastParameterSourceNode", ("text", typeof(string), 0) },
            { "WildcardParameterSourceNode", ("text", typeof(string), 0) },
            { "SteamVRTrackedDeviceSourceNode", ("text", typeof(string), 0) },
            { "RegexMatchNode", ("text", typeof(string), 0) },
            { "RegexMatchesNode", ("text", typeof(string), 0) },
            { "RegexIsMatchNode", ("text", typeof(string), 0) },
            { "RegexReplaceNode", ("text", typeof(string), 0) },
            { "RegexSplitNode", ("text", typeof(string), 0) },
            { "ImpulseReceiveNode", ("text", typeof(string), 0) },
            { "ImpulseSendNode", ("text", typeof(string), 0) },
            //{ "GamepadSourceNode", ("text", typeof(string), 0) },
            { "KeybindSourceNode", ("keybind", typeof(Keybind), 0) },
            { "ValueNode", ("value", null, 0) },
            { "RichTextNode", ("value", null, 0) }
        };

        var matching = migrations.FirstOrDefault(m => originalType.Contains(m.Key) && propertyKey == m.Value.PropertyKey);
        if (matching.Key == null) return false;

        // Since migrations are from single properties, take the first generic type if the type we're converting is null
        var targetType = matching.Value.TargetType ?? node.Metadata.Shared.TypeGenerics[0];

        if (!tryConvertToTargetType(propertyValue, targetType, out var convertedValue))
            return false;

        var propertyElement = (IValueInput)node.Metadata.Elements[ConnectionPoint.ValueInput][matching.Value.Slot].Instance;
        propertyElement.SetField(convertedValue);
        return true;
    }

    private static bool tryConvertToTargetType(object? value, Type targetType, out object? outValue)
    {
        try
        {
            switch (value)
            {
                case null:
                    outValue = null;
                    return true;

                case JToken token:
                    outValue = token.ToObject(targetType)!;
                    return true;

                case string strValue when targetType == typeof(Guid):
                    outValue = Guid.Parse(strValue);
                    return true;

                case var subValue when targetType.IsAssignableTo(typeof(Enum)):
                    outValue = Enum.ToObject(targetType, subValue);
                    return true;

                case long utcTicks when targetType == typeof(DateTimeOffset):
                    var utcDateTime = new DateTime(utcTicks, DateTimeKind.Utc);
                    var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
                    outValue = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
                    return true;

                case string timeSpanStr when targetType == typeof(TimeSpan) && TimeSpan.TryParse(timeSpanStr, out var parsedTimeSpan):
                    outValue = parsedTimeSpan;
                    return true;

                default:
                    outValue = Convert.ChangeType(value, targetType);
                    return true;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Unable to convert {value?.GetType().GetFriendlyName() ?? "null"} to {targetType.GetFriendlyName()}");
            outValue = null;
            return false;
        }
    }

    public static IEnumerable<Guid> Deserialise(SerialisableNodeGraphBase baseElements, NodeGraph targetGraph, bool remapIds = false, Vector2 offset = new())
    {
        var nodeIds = new List<Guid>();
        var idMapping = new Dictionary<Guid, Guid>();

        foreach (var sV in baseElements.Variables)
        {
            try
            {
                if (remapIds && idMapping.ContainsKey(sV.Id)) continue;

                if (!TypeResolver.TryConstruct(sV.Type, out var variableType))
                {
                    Logger.Log($"Unable to construct variable type {sV.Type}");
                    continue;
                }

                var valueParseSuccessful = tryConvertToTargetType(sV.Value, variableType, out var variableValue);
                var variableId = remapIds ? Guid.NewGuid() : sV.Id;
                if (remapIds) idMapping[sV.Id] = variableId;

                var variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [variableId, sV.Name, sV.Persistent, valueParseSuccessful ? variableValue : variableType.CreateDefault()])!;
                targetGraph.GraphVariables.TryAdd(variableId, variable);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a variable when deserialising");
            }
        }

        foreach (var sN in baseElements.Nodes)
        {
            try
            {
                var serialisedType = RunTypeMigration(sN.Type);

                if (!TypeResolver.TryConstruct(serialisedType, out var nodeType))
                {
                    Logger.Log($"Unable to construct node type {serialisedType}");
                    continue;
                }

                var nodeId = remapIds ? Guid.NewGuid() : sN.Id;
                if (remapIds) idMapping.Add(sN.Id, nodeId);

                var nodeResult = targetGraph.AddNode(nodeType, nodeId);
                if (!nodeResult.IsSuccess) continue;

                var node = nodeResult.Value;
                nodeIds.Add(node.Id);
                node.Metadata.Position = sN.Position + offset;

                if (sN.Sizes is not null)
                {
                    foreach (var (point, sizes) in sN.Sizes)
                    {
                        var elements = node.Metadata.Elements[(ConnectionPoint)point];

                        for (var i = 0; i < elements.Length; i++)
                        {
                            elements[i].Size = sizes[i];
                        }
                    }
                }

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var migrationResult = RunPropertyMigration(node, sN.Type, propertyKey, propertyValue);

                        if (migrationResult)
                        {
                            // Since we've migrated from a property to a ValueInput, increase the target slot on all connections to the value inputs
                            foreach (var connection in baseElements.Connections.Where(c => c.Type == "v" && c.InputId == sN.Id))
                            {
                                connection.InputSlot++;
                            }

                            continue;
                        }

                        var (_, propertyInfo) = node.Metadata.Shared.Properties.SingleOrDefault(property => property.Key == propertyKey);
                        if (propertyInfo is null) continue;

                        if (tryConvertToTargetType(propertyValue, propertyInfo.PropertyType, out var convertedValue))
                        {
                            if (remapIds && propertyKey == "variable_id" && baseElements.Variables.Count > 0)
                                convertedValue = idMapping[(Guid)convertedValue!];

                            propertyInfo.SetValue(node, convertedValue);
                        }
                    }
                }

                if (sN.Inlines is not null)
                {
                    var inputs = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput);

                    for (var i = 0; i < sN.Inlines.Count; i++)
                    {
                        var input = inputs[i];
                        if (input.Metadata.Shared.IsList) continue;

                        var inline = sN.Inlines[i];

                        if (input.Metadata.Shared.IsInlineable && tryConvertToTargetType(inline, input.Metadata.Shared.ValueType, out var value))
                            ((IValueInput)input).SetField(value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error creating a node when deserialising. Node is {sN.Type}");
            }
        }

        foreach (var sC in baseElements.Connections)
        {
            var outputId = remapIds ? idMapping[sC.OutputId] : sC.OutputId;
            var inputId = remapIds ? idMapping[sC.InputId] : sC.InputId;

            var outputNodeResult = targetGraph.GetNode(outputId);
            if (!outputNodeResult.IsSuccess) continue;

            var inputNodeResult = targetGraph.GetNode(inputId);
            if (!inputNodeResult.IsSuccess) continue;

            var outputNode = outputNodeResult.Value;
            var inputNode = inputNodeResult.Value;

            try
            {
                if (sC.Type == "f")
                {
                    var outputElement = (IFlowOutputBase)outputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowOutput)[sC.OutputSlot];
                    var inputElement = (IFlowInputBase)inputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowInput)[sC.InputSlot];

                    targetGraph.CreateConnection(outputElement, sC.OutputSlotIndex, inputElement, sC.InputSlotIndex);
                }

                if (sC.Type == "v")
                {
                    var outputElement = (IValueOutputBase)outputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput)[sC.OutputSlot];
                    var inputElement = (IValueInputBase)inputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[sC.InputSlot];

                    if (inputElement.Metadata.Shared.Modes == InputModes.Inline) continue;

                    targetGraph.CreateConnection(outputElement, sC.OutputSlotIndex, inputElement, sC.InputSlotIndex);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a connection when deserialising");
            }
        }

        foreach (var sG in baseElements.Groups)
        {
            try
            {
                var groupNodeIds = sG.Nodes.Select(nodeId => remapIds ? idMapping[nodeId] : nodeId).ToList();
                groupNodeIds.RemoveAll(nodeId => !targetGraph.Elements.ContainsKey(nodeId));
                if (groupNodeIds.Count == 0) continue;

                var groupId = remapIds ? Guid.NewGuid() : sG.Id;
                if (remapIds) idMapping[sG.Id] = groupId;

                var group = targetGraph.AddGroup(groupNodeIds, groupId);
                group.Title.Value = sG.Title;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a group when deserialising");
            }
        }

        foreach (var sC in baseElements.Comments)
        {
            try
            {
                var commentId = remapIds ? Guid.NewGuid() : sC.Id;
                if (remapIds) idMapping[sC.Id] = commentId;

                var comment = targetGraph.AddComment(commentId);
                comment.Position.Value = sC.Position + offset;
                comment.Text.Value = sC.Text;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a comment when deserialising");
            }
        }

        return nodeIds;
    }
}