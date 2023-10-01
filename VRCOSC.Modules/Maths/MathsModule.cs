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
        CreateSetting(MathsSetting.Constants, "Constants", "Define your own constants to reuse in your equations\nChanges to this setting requires a module restart", Array.Empty<string>());
        CreateSetting(MathsSetting.Functions, "Functions", "Define your own functions to reuse in your equations\nChanges to this setting requires a module restart", Array.Empty<string>());

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

    protected override async void OnAnyParameterReceived(ReceivedParameter parameter)
    {
        parameterValues[parameter.Name] = parameter;

        if (!instances.TryGetValue(parameter.Name, out var instance)) return;

        var expression = new Expression(instance.Equation.Value, parameterValues.Values.Select(createArgumentForParameterValue).Concat(elements).ToArray());
        expression.disableImpliedMultiplicationMode();

        foreach (var missingArgument in expression.getMissingUserDefinedArguments())
        {
            var missingArgumentValue = await FindParameterValue(missingArgument);

            if (missingArgumentValue is null)
            {
                Log($"Could not retrieve missing argument value '{missingArgument}'");
                continue;
            }

            if (missingArgumentValue is bool boolValue)
                expression.addArguments(new Argument(missingArgument, boolValue));
            else if (missingArgumentValue is int intValue)
                expression.addArguments(new Argument(missingArgument, intValue));
            else if (missingArgumentValue is float floatValue)
                expression.addArguments(new Argument(missingArgument, floatValue));
        }

        var outputType = await FindParameterType(instance.OutputParameter.Value);

        if (outputType is null)
        {
            Log($"Could not find output parameter '{instance.OutputParameter.Value}'");
            return;
        }

        var output = expression.calculate();

        var finalValue = convertToOutputType(output, outputType.Value);
        SendParameter(instance.OutputParameter.Value, finalValue);
    }

    private static PrimitiveElement createArgumentForParameterValue(ReceivedParameter parameter)
    {
        if (parameter.IsValueType<bool>()) return new Argument(parameter.Name, parameter.ValueAs<bool>() ? 1 : 0);
        if (parameter.IsValueType<int>()) return new Argument(parameter.Name, parameter.ValueAs<int>());
        if (parameter.IsValueType<float>()) return new Argument(parameter.Name, parameter.ValueAs<float>());

        throw new InvalidOperationException("Unknown parameter type");
    }

    private object convertToOutputType(double value, TypeCode valueType)
    {
        try
        {
            return valueType switch
            {
                TypeCode.Boolean => Convert.ToBoolean(value),
                TypeCode.Int32 => Convert.ToInt32(value),
                TypeCode.Single => Convert.ToSingle(value),
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
        catch (Exception e)
        {
            Log($"Warning. Value {value}. " + e.Message);

            return valueType switch
            {
                TypeCode.Boolean => default(bool),
                TypeCode.Int32 => default(int),
                TypeCode.Single => default(float),
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
