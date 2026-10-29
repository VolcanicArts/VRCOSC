// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using UnitsNet;
using UnitsNet.Units;

namespace VRCOSC.App.Nodes.Types.Quantity;

[Node("Quantity Converter", "Quantity/Quantity")]
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
public sealed class QuantityConverterNode<TUnit>() : SimpleResultComputeNode<IQuantity, TUnit, IQuantity<TUnit>?>((value, unit) => (IQuantity<TUnit>?)value?.ToUnit(unit), "Value", "Unit") where TUnit : struct, Enum;