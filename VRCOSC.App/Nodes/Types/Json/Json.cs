// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("Json Parse Object", "Json")]
public sealed class JsonParseObjectNode : Node, IFlowInput
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true
    };

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> Input = new();
    public ValueOutput<JsonObject> Json = new();

    protected override async Task Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (string.IsNullOrEmpty(input))
        {
            await OnFail.Execute(c);
            return;
        }

        var result = JsonSerializer.Deserialize<JsonObject>(input, options);

        if (result is null)
        {
            await OnFail.Execute(c);
            return;
        }

        Json.Write(result, c);
        await OnSuccess.Execute(c);
    }
}

[Node("Json Parse Array", "Json")]
public sealed class JsonParseArrayNode : Node, IFlowInput
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true
    };

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> Input = new();
    public ValueOutput<JsonArray> Json = new();

    protected override async Task Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (string.IsNullOrEmpty(input))
        {
            await OnFail.Execute(c);
            return;
        }

        var result = JsonSerializer.Deserialize<JsonArray>(input, options);

        if (result is null)
        {
            await OnFail.Execute(c);
            return;
        }

        Json.Write(result, c);
        await OnSuccess.Execute(c);
    }
}

[Node("Json Node Metadata", "Json")]
public sealed class JsonNodeMetadataNode : Node
{
    public ValueInput<JsonNode> Input = new();
    public ValueOutput<JsonValueKind> Kind = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        Kind.Write(input.GetValueKind(), c);
        return Task.CompletedTask;
    }
}

[Node("Json Object To Dictionary", "Json")]
public sealed class JsonObjectToDictionaryNode<T> : Node
{
    public ValueInput<JsonObject> Input = new();
    public ValueOutput<Dictionary<string, T>> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        try
        {
            var dict = input.ToDictionary(p => p.Key, p =>
            {
                if (p.Value is null) return default!;

                if (typeof(T) == typeof(JsonArray)) return (T)(object)p.Value.AsArray();
                if (typeof(T) == typeof(JsonObject)) return (T)(object)p.Value.AsObject();

                return p.Value.GetValue<T>();
            });

            Output.Write(dict, c);
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}

[Node("Json Array To Enumerable", "Json")]
public sealed class JsonArrayToEnumerableNode<T> : Node
{
    public ValueInput<JsonArray> Input = new();
    public ValueOutput<IEnumerable<T>> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        try
        {
            if (typeof(T) == typeof(JsonArray) || typeof(T) == typeof(JsonObject))
                Output.Write(input.OfType<T>(), c);
            else
                Output.Write(input.GetValues<T>(), c);
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}

[Node("Json Child Value", "Json")]
public sealed class JsonValueNode<T> : Node, IHasTextProperty
{
    public ValueInput<JsonObject> Input = new();
    public ValueOutput<T> Output = new();

    [NodeProperty("text")]
    public string Text { get; set; }

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        try
        {
            if (input.TryGetPropertyValue(Text, out var node) && node is not null)
            {
                var value = node.GetValue<T>();
                Output.Write(value, c);
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}

[Node("Json Child Array", "Json")]
public sealed class JsonArrayNode : Node, IHasTextProperty
{
    public ValueInput<JsonObject> Input = new();
    public ValueOutput<JsonArray> Output = new();

    [NodeProperty("text")]
    public string Text { get; set; }

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        try
        {
            if (input.TryGetPropertyValue(Text, out var node) && node is not null)
            {
                var value = node.AsArray();
                Output.Write(value, c);
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}

[Node("Json Child Object", "Json")]
public sealed class JsonObjectNode : Node, IHasTextProperty
{
    public ValueInput<JsonObject> Input = new();
    public ValueOutput<JsonObject> Output = new();

    [NodeProperty("text")]
    public string Text { get; set; }

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        try
        {
            if (input.TryGetPropertyValue(Text, out var node) && node is not null)
            {
                var value = node.AsObject();
                Output.Write(value, c);
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}

[Node("Json To String", "Json")]
public sealed class JsonToStringNode : Node
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true
    };

    public ValueInput<JsonNode> Input = new();
    public ValueOutput<string> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        Output.Write(input.ToJsonString(options), c);
        return Task.CompletedTask;
    }
}

[Node("Dictionary To Json Object", "Json")]
public sealed class DictionaryToJsonObjectNode<T> : Node
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true
    };

    public ValueInput<Dictionary<string, T>> Input = new();
    public ValueOutput<JsonObject> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        var value = (JsonObject)JsonSerializer.SerializeToNode(input, options)!;

        Output.Write(value, c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable to Json Array", "Json")]
public sealed class EnumerableToJsonArrayNode<T> : Node
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true
    };

    public ValueInput<IEnumerable<T>> Input = new();
    public ValueOutput<JsonArray> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Task.CompletedTask;

        var value = (JsonArray)JsonSerializer.SerializeToNode(input, options)!;

        Output.Write(value, c);
        return Task.CompletedTask;
    }
}