// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using UnitsNet;

namespace VRCOSC.App.Nodes.Types.Quantity;

[Node("Quantity Value", "Quantity/Quantity")]
[NodeCollapsed]
public sealed class QuantityValueNode<T>() : SimpleValueTransformNode<IQuantity, T>(value => value is not null ? T.CreateChecked(value.As(value.Unit)) : default!) where T : IFloatingPoint<T>;