﻿namespace BlockEngine.Client.Threading.Jobs;

/// <summary>
/// Test job with an integer result equal to the product of 7 and 6.
/// </summary>
public class ExampleJob : VektorJob<int>
{
    public override void Execute()
    {
        // Do some work.
        int result = 7 * 6;

        // Set the result of the job.
        SetResult(result);
    }
}