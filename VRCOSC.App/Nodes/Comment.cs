// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public interface IComment : IGraphElement
{
    Observable<string> Text { get; }
    Observable<Vector2> Position { get; }
}

public class Comment : GraphElement, IComment
{
    public Observable<string> Text { get; } = new("New Comment");
    public Observable<Vector2> Position { get; } = new();
}