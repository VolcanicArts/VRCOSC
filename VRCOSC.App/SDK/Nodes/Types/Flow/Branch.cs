// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("If", "Flow")]
[NodeFlowInput]
[NodeFlowOutput("True", "False")]
[NodeValueInput("Condition")]
public class IfNode : Node
{
    [NodeProcess]
    private void process(bool condition) => SetFlow(condition ? 0 : 1);
}

[Node("If With State", "Flow")]
[NodeFlowInput]
[NodeFlowOutput("Became True", "Became False", "Stayed True", "Stayed False")]
[NodeValueInput("Condition")]
public class IfWithStateNode : Node
{
    private const int became_true_slot = 0;
    private const int became_false_slot = 1;
    private const int true_slot = 2;
    private const int false_slot = 3;

    private bool prevCondition;

    [NodeProcess]
    private void process(bool condition)
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            SetFlow(became_true_slot);
            return;
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            SetFlow(became_false_slot);
            return;
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            SetFlow(true_slot);
            return;
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            SetFlow(false_slot);
            return;
        }
    }
}