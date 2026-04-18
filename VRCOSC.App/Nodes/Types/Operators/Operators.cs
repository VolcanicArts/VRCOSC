// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Equals", "Operators", EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T>() : SimpleResultComputeNode<T, bool>(EqualityComparer<T>.Default.Equals);

[Node("Not Equals", "Operators", EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => !EqualityComparer<T>.Default.Equals(a, b));