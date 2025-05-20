// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Ports;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Serial Write", "Actions/Serial")]
public sealed class SerialWriteNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Success"), new("On Fail")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Port Name")] string portName,
        [NodeValue("Baud Rate")] int baudRate,
        [NodeValue("Parity")] Parity parity,
        [NodeValue("Data Bits")] int dataBits,
        [NodeValue("Stop Bits")] StopBits stopBits,
        [NodeValue("Command")] string command
    )
    {
        using SerialPort serial = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

        if (string.IsNullOrEmpty(command))
        {
            await TriggerFlow(context, 1);
            return;
        }

        try
        {
            serial.Open();
            serial.WriteLine(command);
        }
        catch (Exception)
        {
            await TriggerFlow(context, 1);
            return;
        }

        await TriggerFlow(context, 0);
    }
}