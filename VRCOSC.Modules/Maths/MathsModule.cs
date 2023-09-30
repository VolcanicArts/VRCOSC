// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using org.mariuszgromada.math.mxparser;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Maths;

[ModuleTitle("Maths")]
[ModuleDescription("Allows you to write equations to execute on a parameter, and set the output's value into another parameter")]
[ModuleAuthor("VolcanicArts")]
public class MathsModule : AvatarModule
{
    private readonly Dictionary<string, ReceivedParameter> parameterValues = new();
    private readonly Dictionary<string, MathsEquationInstance> instances = new();
    private readonly List<PrimitiveElement> elements = new();

    public MathsModule()
    {
        License.iConfirmNonCommercialUse("VolcanicArts");
    }

    protected override void CreateAttributes()
    {
        CreateSetting(MathsSetting.Constants, "Constants", "Define your own constants to reuse in your equations", Array.Empty<string>());
        CreateSetting(MathsSetting.Functions, "Functions", "Define your own functions to reuse in your equations", Array.Empty<string>());

        CreateSetting(MathsSetting.Equations, new MathsEquationInstanceListAttribute
        {
            Name = "Equations",
            Description = "Here you can write equations to run on a parameter and output to another parameter\nValues will be automatically converted to best fit the output parameter\nYou can access any parameter value by writing its name\nChanges to this setting requires a module restart",
            Default = new List<MathsEquationInstance>()
        });
    }

    protected override void OnModuleStart()
    {
        parameterValues.Clear();
        instances.Clear();
        elements.Clear();

        GetSettingList<MathsEquationInstance>(MathsSetting.Equations).ForEach(instance => instances.TryAdd(instance.TriggerParameter.Value, instance));
        elements.AddRange(GetSettingList<string>(MathsSetting.Constants).Select(constant => new Constant(constant)));
        elements.AddRange(GetSettingList<string>(MathsSetting.Functions).Select(function => new Function(function)));
    }

    protected override void OnAnyParameterReceived(ReceivedParameter parameter)
    {
        parameterValues[parameter.Name] = parameter;

        if (!instances.TryGetValue(parameter.Name, out var instance)) return;

        var expression = new Expression(instance.Equation.Value, parameterValues.Values.Select(createArgumentForParameterValue).Concat(elements).ToArray());
        expression.disableImpliedMultiplicationMode();

        if (expression.getMissingUserDefinedArguments().Any())
        {
            Log($"Missing argument value for equation {instance.TriggerParameter.Value}");
            return;
        }

        var output = expression.calculate();

        SendParameter(instance.OutputParameter.Value, convertToOutputType(output, instance.OutputType.Value));
    }

    private static PrimitiveElement createArgumentForParameterValue(ReceivedParameter parameter)
    {
        if (parameter.IsValueType<bool>()) return new Argument(parameter.Name, parameter.ValueAs<bool>() ? 1 : 0);
        if (parameter.IsValueType<int>()) return new Argument(parameter.Name, parameter.ValueAs<int>());
        if (parameter.IsValueType<float>()) return new Argument(parameter.Name, parameter.ValueAs<float>());

        throw new InvalidOperationException("Unknown parameter type");
    }

    private object convertToOutputType(double value, MathsEquationValueType valueType)
    {
        try
        {
            return valueType switch
            {
                MathsEquationValueType.Bool => Convert.ToBoolean(value),
                MathsEquationValueType.Int => Convert.ToInt32(value),
                MathsEquationValueType.Float => Convert.ToSingle(value),
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
        catch (Exception e)
        {
            Log($"Warning. Value {value}. " + e.Message);

            return valueType switch
            {
                MathsEquationValueType.Bool => default(bool),
                MathsEquationValueType.Int => default(int),
                MathsEquationValueType.Float => default(float),
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
    }

    private enum MathsSetting
    {
        Constants,
        Functions,
        Equations
    }
}
