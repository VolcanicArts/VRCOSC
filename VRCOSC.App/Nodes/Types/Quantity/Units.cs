// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using UnitsNet;

namespace VRCOSC.App.Nodes.Types.Quantity;

[Node("Quantity Unit", "Quantity/Quantity")]
[NodeCollapsed]
public sealed class QuantityUnitNode() : SimpleValueTransformNode<IQuantity, UnitInfo?>(value => value is not null ? UnitsNet.Quantity.GetUnitInfo(value.Unit) : null);

[Node("Unit Info Unpack", "Quantity/Units")]
public sealed class UnitInfoUnpackNode : Node
{
    public ValueInput<UnitInfo?> UnitInfo = new();

    public ValueOutput<string> Name = new();
    public ValueOutput<string> PluralName = new();
    public ValueOutput<string?> QuantityName = new();
    public ValueOutput<Enum> Value = new();

    protected override Task Process(IPulseContext c)
    {
        var unitInfo = UnitInfo.Read(c);
        if (unitInfo is null) return Task.CompletedTask;

        Name.Write(unitInfo.Name, c);
        PluralName.Write(unitInfo.PluralName, c);
        QuantityName.Write(unitInfo.QuantityName, c);
        Value.Write(unitInfo.Value, c);

        return Task.CompletedTask;
    }
}