// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using OneOf;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.Nodes.Types.Parameters;

[Node("On Parameter Received", "Parameters")]
[NodeFlowInput(true)]
[NodeFlowOutput("")]
[NodeValueInput("Name")]
[NodeValueOutput("Type", "Value")]
public class OnParameterReceived : Node
{
    [NodeProcess]
    private (ParameterType, OneOf<bool, int, float>) process(string parameterName)
    {
        var receivedParameter = new ReceivedParameter(parameterName, true);

        switch (receivedParameter.Type)
        {
            case ParameterType.Bool:
                return (receivedParameter.Type, OneOf<bool, int, float>.FromT0((bool)receivedParameter.Value));

            case ParameterType.Int:
                return (receivedParameter.Type, OneOf<bool, int, float>.FromT1((int)receivedParameter.Value));

            case ParameterType.Float:
                return (receivedParameter.Type, OneOf<bool, int, float>.FromT2((float)receivedParameter.Value));

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

[Node("On Registered Parameter Received", "Parameters")]
[NodeFlowInput(true)]
[NodeFlowOutput("")]
[NodeValueOutput("Value")]
public class OnRegisteredParameterReceived : Node
{
    private ModuleParameter? selectedParameter;

    public ModuleParameter? SelectedParameter
    {
        get => selectedParameter;
        set
        {
            selectedParameter = value;
            OnOptionChanged?.Invoke();
        }
    }

    [NodeProcess]
    private OneOf<bool, int, float>? process()
    {
        var parameterArrived = true;
        var parameterBool = true;
        var parameterInt = 1;
        var parameterFloat = 0.5f;
        var parameterType = ParameterType.Bool;

        if (parameterArrived)
        {
            switch (parameterType)
            {
                case ParameterType.Bool:
                    return OneOf<bool, int, float>.FromT0(parameterBool);

                case ParameterType.Int:
                    return OneOf<bool, int, float>.FromT1(parameterInt);

                case ParameterType.Float:
                    return OneOf<bool, int, float>.FromT2(parameterFloat);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return null;
    }
}