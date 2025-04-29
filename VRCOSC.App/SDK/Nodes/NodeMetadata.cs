// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public static class NodeMetadataBuilder
{
    public static NodeMetadata BuildFrom(Node node)
    {
        var type = node.GetType();

        if (!type.IsAssignableTo(typeof(Node)))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't extend {nameof(Node)}");

        if (!type.TryGetCustomAttribute<NodeAttribute>(out var nodeAttribute))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't use the attribute {nameof(NodeAttribute)}");

        var method = getProcessMethod(type);
        var parameters = method.GetParameters();

        var isFlowInput = type.IsAssignableTo(typeof(IFlowInput));
        var isFlowOutput = type.IsAssignableTo(typeof(IFlowOutput));

        if ((isFlowInput || isFlowOutput) && method.ReturnParameter.ParameterType != typeof(Task))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} as node is a flow node that doesn't return {nameof(Task)}");

        //var allRefsAfterNonRefs = parameters.Select(p => p.ParameterType.IsAssignableTo(typeof(IRef))).SkipWhile(isRef => !isRef).All(isRef => isRef);

        //if (!allRefsAfterNonRefs)
          //  throw new Exception($"Cannot build {nameof(NodeMetadata)} as the defined {nameof(NodeProcessAttribute)} method has non-refs after refs");

        var inputParametersEnumerable = parameters.TakeWhile(p => !p.ParameterType.IsAssignableTo(typeof(IRef)));
        var outputParametersEnumerable = parameters.SkipWhile(p => !p.ParameterType.IsAssignableTo(typeof(IRef)));

        if (isFlowInput || isFlowOutput)
        {
            // skip cancellation token
            inputParametersEnumerable = inputParametersEnumerable.Skip(1);
        }

        var inputParameters = inputParametersEnumerable.ToList();
        var outputParameters = outputParametersEnumerable.ToList();

        var isValueInput = inputParameters.Count != 0;
        var isValueOutput = outputParameters.Count != 0;

        var inputsHaveVariableSize = inputParameters.Any(p => p.HasCustomAttribute<NodeVariableSizeAttribute>());

        if (inputsHaveVariableSize)
        {
            var inputsHaveOneVariableSize = inputParameters.Count(p => p.IsDefined(typeof(NodeVariableSizeAttribute), false)) == 1;

            if (!inputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single {nameof(NodeVariableSizeAttribute)} is allowed for inputs");

            var inputsHaveVariableSizeLast = inputParameters.Last().IsDefined(typeof(NodeVariableSizeAttribute), false);

            if (!inputsHaveVariableSizeLast)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as {nameof(NodeVariableSizeAttribute)} is only allowed to be defined on the last input");
        }

        var outputsHaveVariableSize = outputParameters.Any(p => p.HasCustomAttribute<NodeVariableSizeAttribute>());

        if (outputsHaveVariableSize)
        {
            var outputsHaveOneVariableSize = outputParameters.Count(p => p.IsDefined(typeof(NodeVariableSizeAttribute), false)) == 1;

            if (!outputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single {nameof(NodeVariableSizeAttribute)} is allowed for outputs");

            var outputsHaveVariableSizeLas = outputParameters.Last().IsDefined(typeof(NodeVariableSizeAttribute), false);

            if (!outputsHaveVariableSizeLas)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as {nameof(NodeVariableSizeAttribute)} is only allowed to be defined on the last output");
        }

        var inputVariableSize = inputParameters.LastOrDefault()?.GetCustomAttribute<NodeVariableSizeAttribute>();
        var outputVariableSize = outputParameters.LastOrDefault()?.GetCustomAttribute<NodeVariableSizeAttribute>();

        var inputMetadata = getIoMetadata(inputParameters);
        var outputMetadata = getIoMetadata(outputParameters);

        var metadata = new NodeMetadata
        {
            Title = nodeAttribute.Title,
            Path = nodeAttribute.Path,
            GenericArguments = type.GetGenericArguments(),
            FlowOutputs = type.IsAssignableTo(typeof(IFlowOutput)) ? ((IFlowOutput)node).FlowOutputs : [],
            IsFlowInput = isFlowInput,
            IsFlowOutput = isFlowOutput,
            IsValueInput = isValueInput,
            IsValueOutput = isValueOutput,
            ProcessMethod = method,
            Inputs = inputMetadata,
            Outputs = outputMetadata,
            InputVariableSize = inputVariableSize,
            InputVariableSizeActual = inputVariableSize?.DefaultSize ?? 0,
            OutputVariableSize = outputVariableSize,
            OutputVariableSizeActual = outputVariableSize?.DefaultSize ?? 0
        };

        metadata.ProcessDelegate = generateProcessDelegate(metadata);
        return metadata;
    }

    private static MethodInfo getProcessMethod(Type type)
    {
        var t = type;

        var processMethods = new List<MethodInfo>();

        while (t is not null && t != typeof(object))
        {
            foreach (var methodInfo in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                    processMethods.Add(methodInfo);
            }

            t = t.BaseType;
        }

        return processMethods.Count switch
        {
            0 => throw new Exception($"Cannot build {nameof(NodeMetadata)} as a {nameof(Node)} must have a method that has a {nameof(NodeProcessAttribute)}"),
            > 1 => throw new Exception($"Cannot build {nameof(NodeMetadata)} as a {nameof(Node)} must have only 1 method that has a {nameof(NodeProcessAttribute)}"),
            _ => processMethods.Single()
        };
    }

    private static NodeValueMetadata[] getIoMetadata(List<ParameterInfo> parameters)
    {
        var arr = new NodeValueMetadata[parameters.Count];

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var name = string.Empty;

            if (parameter.TryGetCustomAttribute<NodeValueAttribute>(out var nodeValueAttribute))
            {
                name = nodeValueAttribute.Name;
            }

            arr[i] = new NodeValueMetadata
            {
                Name = name,
                Parameter = parameter
            };
        }

        return arr;
    }

    private static Delegate generateProcessDelegate(NodeMetadata metadata)
    {
        var args = new List<Type> { typeof(Node) };

        if (metadata.IsFlow)
            args.Add(typeof(CancellationToken));

        if (metadata.IsValueInput)
            args.Add(typeof(object?[]));

        if (metadata.IsValueOutput)
            args.Add(typeof(IRef[]));

        var argsArr = args.ToArray();

        var dynamicMethod = new DynamicMethod(
            name: "InvokeProcess",
            returnType: metadata.IsFlow ? typeof(Task) : typeof(void),
            parameterTypes: argsArr,
            m: typeof(Node).Module,
            skipVisibility: true
        );

        var inputTypes = metadata.Inputs.Select(input => input.Type).ToArray();
        var outputTypes = metadata.Outputs.Select(output => output.Type).ToArray();

        generateIl(dynamicMethod, metadata.ProcessMethod, inputTypes, outputTypes);
        var delegateType = createDelegateType(metadata.IsFlow, args);
        return dynamicMethod.CreateDelegate(delegateType);
    }

    private static Type createDelegateType(bool isFlow, List<Type> args)
    {
        if (isFlow)
        {
            args.Add(typeof(Task));

            var argsArr = args.ToArray();

            switch (args.Count)
            {
                case 2:
                    return typeof(Func<,>).MakeGenericType(argsArr);

                case 3:
                    return typeof(Func<,,>).MakeGenericType(argsArr);

                case 4:
                    return typeof(Func<,,,>).MakeGenericType(argsArr);

                case 5:
                    return typeof(Func<,,,,>).MakeGenericType(argsArr);
            }
        }
        else
        {
            var argsArr = args.ToArray();

            switch (argsArr.Length)
            {
                case 1:
                    return typeof(Action<>).MakeGenericType(argsArr);

                case 2:
                    return typeof(Action<,>).MakeGenericType(argsArr);

                case 3:
                    return typeof(Action<,,>).MakeGenericType(argsArr);

                case 4:
                    return typeof(Action<,,,>).MakeGenericType(argsArr);
            }
        }

        throw new Exception();
    }

    private static void generateIl(DynamicMethod dm, MethodInfo method, Type[] inputTypes, Type[] outputTypes)
    {
        var il = dm.GetILGenerator();
        var index = 0;

        il.Emit(OpCodes.Ldarg, index); // load instance
        index++;

        if (method.ReturnParameter.ParameterType != typeof(void))
        {
            il.Emit(OpCodes.Ldarg, index); // load cancellation token
            index++;
        }

        if (inputTypes.Length > 0)
        {
            il.Emit(OpCodes.Ldarg, index);

            for (var i = 0; i < inputTypes.Length; i++)
            {
                insertInputIl(il, i, inputTypes[i]);
            }

            index++;
        }

        if (outputTypes.Length > 0)
        {
            il.Emit(OpCodes.Ldarg, index);

            for (var i = 0; i < outputTypes.Length; i++)
            {
                insertOutputIl(il, i, outputTypes[i]);
            }

            index++;
        }

        il.EmitCall(OpCodes.Callvirt, method, null);
        il.Emit(OpCodes.Ret);
    }

    private static void insertInputIl(ILGenerator il, int index, Type type)
    {
        il.Emit(OpCodes.Ldc_I4, index);

        if (type.IsValueType)
        {
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Unbox_Any, type);
        }
        else
        {
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Castclass, type);
        }
    }

    private static void insertOutputIl(ILGenerator il, int index, Type type)
    {
        il.Emit(OpCodes.Ldc_I4, index);
        il.Emit(OpCodes.Ldelem_Ref);

        var refType = typeof(Ref<>).MakeGenericType(type);
        il.Emit(OpCodes.Castclass, refType);

        /***
        var valueField = refType.GetField("Value", BindingFlags.Instance | BindingFlags.Public)!;
        il.Emit(OpCodes.Ldflda, valueField);
        ***/
    }
}

public sealed class NodeMetadata
{
    public string Title { get; set; } = null!;
    public string? Path { get; set; }
    public Type[] GenericArguments { get; set; } = null!;
    public MethodInfo ProcessMethod { get; set; } = null!;
    public Delegate ProcessDelegate { get; set; } = null!;
    public NodeFlowRef[] FlowOutputs { get; set; } = [];
    public NodeValueMetadata[] Inputs { get; set; } = [];
    public NodeValueMetadata[] Outputs { get; set; } = [];
    public NodeVariableSizeAttribute? InputVariableSize { get; set; }
    public int InputVariableSizeActual { get; set; }
    public NodeVariableSizeAttribute? OutputVariableSize { get; set; }
    public int OutputVariableSizeActual { get; set; }
    public bool IsFlowInput { get; set; }
    public bool IsFlowOutput { get; set; }
    public bool IsValueInput { get; set; }
    public bool IsValueOutput { get; set; }
    public bool IsAsync { get; set; }

    public bool IsTrigger => IsFlowOutput && !IsFlowInput;
    public bool IsFlow => IsFlowInput || IsFlowOutput;
    public bool IsValue => IsValueInput || IsValueOutput;
    public string GenericArgumentsAsString => string.Join(", ", GenericArguments.Select(arg => arg.GetFriendlyName()));

    public bool InputHasVariableSize => InputVariableSize is not null;
    public bool OutputHasVariableSize => OutputVariableSize is not null;

    public int FlowCount => FlowOutputs.Length;

    public int InputsCount => Inputs.Length;

    public int InputsVirtualCount
    {
        get
        {
            if (Inputs.Length == 0) return 0;
            if (InputVariableSize is null) return Inputs.Length;

            return Inputs.Length - 1 + InputVariableSizeActual;
        }
    }

    public int OutputsCount => Outputs.Length;

    public int OutputsVirtualCount
    {
        get
        {
            if (Outputs.Length == 0) return 0;
            if (OutputVariableSize is null) return Outputs.Length;

            return Outputs.Length - 1 + OutputVariableSizeActual;
        }
    }

    public Type GetTypeOfInputSlot(int index)
    {
        if (InputsCount == 0) throw new Exception($"Cannot get type of input slot when there are no inputs {Title}");
        if (!InputHasVariableSize && index >= InputsCount) throw new IndexOutOfRangeException();
        if (InputHasVariableSize && index >= InputsVirtualCount) throw new IndexOutOfRangeException();

        if (InputHasVariableSize)
        {
            if (index >= InputsCount - 1) return Inputs.Last().Type.GetElementType()!;
        }

        return Inputs[index].Type;
    }

    public Type GetTypeOfOutputSlot(int index)
    {
        if (OutputsCount == 0) throw new Exception("Cannot get type of output slot when there are no outputs");
        if (!OutputHasVariableSize && index >= OutputsCount) throw new IndexOutOfRangeException();
        if (OutputHasVariableSize && index >= OutputsVirtualCount) throw new IndexOutOfRangeException();

        if (OutputHasVariableSize)
        {
            if (index >= OutputsCount - 1) return Outputs.Last().Type.GetElementType()!;
        }

        return Outputs[index].Type;
    }
}

public sealed class NodeValueMetadata
{
    public string Name { get; set; } = null!;
    public ParameterInfo Parameter { get; set; } = null!;

    public Type Type => Parameter.ParameterType.IsGenericType && Parameter.ParameterType.GetGenericTypeDefinition() == typeof(Ref<>) ? Parameter.ParameterType.GenericTypeArguments[0] : Parameter.ParameterType;
}