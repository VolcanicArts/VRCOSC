// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Is Null", "Utility")]
[NodeCollapsed]
public sealed class IsNullNode<T>() : SimpleValueTransformNode<T, bool>(v => v is null);

[Node("Is Not Null", "Utility")]
[NodeCollapsed]
public sealed class IsNotNullNode<T>() : SimpleValueTransformNode<T, bool>(v => v is not null);