// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OSC.Query;

public class OSCQueryRootNode : OSCQueryNode
{
    private readonly Dictionary<string, OSCQueryNode> pathStore;

    public OSCQueryRootNode()
    {
        pathStore = new Dictionary<string, OSCQueryNode>()
        {
            { "/", this }
        };
    }

    private OSCQueryNode? getNodeWithPath(string path) => pathStore.TryGetValue(path, out var node) ? node : null;

    public OSCQueryNode? AddNode(OSCQueryNode node)
    {
        var parent = getNodeWithPath(node.ParentPath) ?? AddNode(new OSCQueryNode(node.ParentPath));
        if (parent is null) return null;

        if (parent.Contents == null)
        {
            parent.Contents = new Dictionary<string, OSCQueryNode>();
        }
        else if (parent.Contents.ContainsKey(node.Name))
        {
            Logger.Log($"Child node {node.Name} already exists on {FullPath}, you need to remove the existing entry first");
            return null;
        }

        parent.Contents.Add(node.Name, node);
        pathStore.Add(node.FullPath, node);

        return node;
    }
}

public class OSCQueryNode
{
    [JsonProperty("FULL_PATH")]
    public string FullPath = null!;

    [JsonProperty("TYPE")]
    public string? OscType;

    [JsonProperty("ACCESS")]
    public OSCQueryNodeAccess? Access;

    [JsonProperty("CONTENTS")]
    public Dictionary<string, OSCQueryNode>? Contents;

    [JsonProperty("VALUE")]
    public object[]? Value;

    [JsonConstructor]
    public OSCQueryNode()
    {
    }

    public OSCQueryNode(string fullPath)
    {
        FullPath = fullPath;
    }

    [JsonIgnore]
    public string ParentPath
    {
        get
        {
            var length = Math.Max(1, FullPath.LastIndexOf("/", StringComparison.Ordinal));
            return FullPath[..length];
        }
    }

    [JsonIgnore]
    public string Name => FullPath[(FullPath.LastIndexOf('/') + 1)..];
}

public enum OSCQueryNodeAccess
{
    NoValue = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    ReadWrite = Read | Write
}
