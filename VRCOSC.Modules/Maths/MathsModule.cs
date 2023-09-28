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
    private readonly Dictionary<string, MathsEquationInstance> instances = new();

    public MathsModule()
    {
        License.iConfirmNonCommercialUse("VolcanicArts");
    }

    protected override void CreateAttributes()
    {
        CreateSetting(MathsSetting.Equations, new MathsEquationInstanceListAttribute
        {
            Name = "Equations",
            Description = "Here you can write equations to run on a parameter and output to another parameter\nValues will be automatically converted to best fit the output parameter\nChanges to this require a module restart\nTo access the input parameter's value, use the argument 'p'",
            Default = new List<MathsEquationInstance>()
        });
    }

    protected override void OnModuleStart()
    {
        instances.Clear();

        GetSettingList<MathsEquationInstance>(MathsSetting.Equations).ForEach(instance => { instances.Add(instance.InputParameter.Value, instance); });
    }

    protected override void OnAnyParameterReceived(ReceivedParameter parameter)
    {
        if (!instances.TryGetValue(parameter.Name, out var instance)) return;

        var parameterArgument = createArgumentForParameterValue(parameter, instance.InputType.Value);
        var expression = new Expression(instance.Equation.Value, parameterArgument);

        var output = expression.calculate();

        SendParameter(instance.OutputParameter.Value, convertToOutputType(output, instance.OutputType.Value));
    }

    private static Argument createArgumentForParameterValue(ReceivedParameter parameter, MathsEquationValueType valueType) => valueType switch
    {
        MathsEquationValueType.Bool => new Argument("p", parameter.ValueAs<bool>() ? 1 : 0),
        MathsEquationValueType.Int => new Argument("p", parameter.ValueAs<int>()),
        MathsEquationValueType.Float => new Argument("p", parameter.ValueAs<float>()),
        _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
    };

    private static object convertToOutputType(double value, MathsEquationValueType valueType) => valueType switch
    {
        MathsEquationValueType.Bool => Convert.ToBoolean(value),
        MathsEquationValueType.Int => Convert.ToInt32(value),
        MathsEquationValueType.Float => Convert.ToSingle(value),
        _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
    };

    private enum MathsSetting
    {
        Equations
    }
}
