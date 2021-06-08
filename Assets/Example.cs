using System;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Example : MonoBehaviour
{
    /// <summary>
    /// This method gives a slightly different result (depending on the input) when
    /// compiled with Burst, compared to C#. This seems to be a problem with constant
    /// folding.
    /// </summary>
    private static float Calculate(float input)
    {
        return 0.1f * (input + 0.1f);
    }

    /// <summary>
    /// Non-burst job that runs <see cref="Example.Calculate"/> on a bunch of inputs.
    /// </summary>
    private struct Job1 : IJob
    {
        public int Count;

        [ReadOnly] public NativeArray<float> Input;
        [WriteOnly] public NativeArray<float> Result;

        public void Execute()
        {
            for (var i = 0; i < Count; ++i)
            {
                Result[i] = Calculate(Input[i]);
            }
        }
    }

    /// <summary>
    /// Burst job that runs <see cref="Example.Calculate"/> on a bunch of inputs.
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    private struct Job2 : IJob
    {
        public int Count;

        [ReadOnly] public NativeArray<float> Input;
        [WriteOnly] public NativeArray<float> Result;

        public void Execute()
        {
            for (var i = 0; i < Count; ++i)
            {
                Result[i] = Calculate(Input[i]);
            }
        }
    }

    public List<float> Inputs;

    public int Seed = 12824;
    public int InputsToGenerate = 10;

    private void Start()
    {
        PrepareInputs();

        // Allocate native arrays

        var count = Inputs.Count;

        var input = new NativeArray<float>(count, Allocator.TempJob);

        for (var i = 0; i < count; ++i)
        {
            input[i] = Inputs[i];
        }

        var result1 = new NativeArray<float>(count, Allocator.TempJob);
        var result2 = new NativeArray<float>(count, Allocator.TempJob);

        // Create jobs

        var job1 = new Job1
        {
            Count = count,
            Input = input,
            Result = result1
        };

        var job2 = new Job2
        {
            Count = count,
            Input = input,
            Result = result2
        };

        // Schedule / complete jobs

        var dep1 = job1.Schedule();
        var dep2 = job2.Schedule();

        JobHandle.CompleteAll(ref dep1, ref dep2);

        // Print results

        for (var i = 0; i < count; ++i)
        {
            var writer = new StringWriter();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            writer.WriteLine($"#{i + 1}: {(result1[i] == result2[i] ? "Pass" : "Fail")}");
            writer.WriteLine($"      Input: {FormatSingle(input[i])}");
            writer.WriteLine($"    Result1: {FormatSingle(result1[i])}");
            writer.WriteLine($"    Result2: {FormatSingle(result2[i])}");

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (result1[i] == result2[i])
            {
                Debug.Log(writer);
            }
            else
            {
                Debug.LogError(writer);
            }
        }

        // Clean up

        input.Dispose();
        result1.Dispose();
        result2.Dispose();
    }

    private void PrepareInputs()
    {
        Inputs = Inputs ?? new List<float>();

        if (InputsToGenerate <= 0) return;

        var rand = new System.Random(Seed);

        for (var i = 0; i < InputsToGenerate; ++i)
        {
            Inputs.Add((float)rand.NextDouble());
        }
    }

    private string FormatSingle(float value)
    {
        var asUint = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);

        return $"{(value > 0 ? '+' : '-')}{(value > 0 ? value : -value):F10} ({asUint:x8})";
    }
}
