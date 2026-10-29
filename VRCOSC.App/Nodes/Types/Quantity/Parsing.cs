// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace VRCOSC.App.Nodes.Types.Quantity;

[Node("Quantity Parse", "Quantity/Quantity")]
[NodeGenerics(
    typeof(AccelerationUnit),
    typeof(AmountOfSubstanceUnit),
    typeof(AngleUnit),
    typeof(AreaUnit),
    typeof(BitRateUnit),
    typeof(ElectricCapacitanceUnit),
    typeof(ElectricChargeUnit),
    typeof(ElectricConductanceUnit),
    typeof(ElectricCurrentUnit),
    typeof(ElectricInductanceUnit),
    typeof(ElectricPotentialUnit),
    typeof(ElectricResistanceUnit),
    typeof(ElectricResistivityUnit),
    typeof(EnergyUnit),
    typeof(EntropyUnit),
    typeof(ForceUnit),
    typeof(FrequencyUnit),
    typeof(IlluminanceUnit),
    typeof(InformationUnit),
    typeof(IrradianceUnit),
    typeof(LengthUnit),
    typeof(LinearDensityUnit),
    typeof(MassUnit),
    typeof(PowerUnit),
    typeof(PressureUnit),
    typeof(RatioUnit),
    typeof(ReactiveEnergyUnit),
    typeof(ReactivePowerUnit),
    typeof(RotationalSpeedUnit),
    typeof(SpecificEnergyUnit),
    typeof(SpecificEntropyUnit),
    typeof(SpecificVolumeUnit),
    typeof(SpeedUnit),
    typeof(TemperatureUnit),
    typeof(ThermalConductivityUnit),
    typeof(TorqueUnit),
    typeof(VolumeUnit),
    typeof(VolumeFlowUnit),
    typeof(DurationUnit),
    typeof(DensityUnit),
    typeof(DynamicViscosityUnit),
    typeof(KinematicViscosityUnit),
    typeof(RadioactivityUnit),
    typeof(RelativeHumidityUnit)
)]
public sealed class QuantityParse<TUnit> : Node where TUnit : struct, Enum
{
    public ValueInput<double> Value = new();
    public ValueInput<TUnit> Unit = new();
    public ValueOutput<IQuantity<TUnit>> Quantity = new();

    protected override Task Process(IPulseContext c)
    {
        var value = Value.Read(c);
        var unit = Unit.Read(c);
        var unitInfo = UnitsNet.Quantity.GetUnitInfo(unit);
        Quantity.Write((IQuantity<TUnit>)UnitsNet.Quantity.From(value, unitInfo.Value), c);
        return Task.CompletedTask;
    }
}
