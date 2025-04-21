// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("If", "Flow")]
public sealed class IfNode : Node
{
    private readonly NodeFlowRef trueFlow;
    private readonly NodeFlowRef falseFlow;

    public IfNode()
    {
        AddFlow("*", ConnectionSide.Input);
        trueFlow = AddFlow("On True", ConnectionSide.Output);
        falseFlow = AddFlow("On False", ConnectionSide.Output);
    }

    [NodeProcess(["Condition"], [])]
    private void process(bool condition) => SetFlow(condition ? trueFlow : falseFlow);
}

[Node("If With State", "Flow")]
public class IfWithStateNode : Node
{
    private readonly NodeFlowRef becameTrueFlow;
    private readonly NodeFlowRef becameFalseFlow;
    private readonly NodeFlowRef stillTrueFlow;
    private readonly NodeFlowRef stillFalseFlow;

    private bool prevCondition;

    public IfWithStateNode()
    {
        AddFlow("*", ConnectionSide.Input);
        becameTrueFlow = AddFlow("On Became True", ConnectionSide.Output);
        becameFalseFlow = AddFlow("On Became False", ConnectionSide.Output);
        stillTrueFlow = AddFlow("On Still True", ConnectionSide.Output);
        stillFalseFlow = AddFlow("On Still False", ConnectionSide.Output);
    }

    [NodeProcess(["Condition"], [])]
    private void process(bool condition)
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            SetFlow(becameTrueFlow);
            return;
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            SetFlow(becameFalseFlow);
            return;
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            SetFlow(stillTrueFlow);
            return;
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            SetFlow(stillFalseFlow);
            return;
        }
    }
}