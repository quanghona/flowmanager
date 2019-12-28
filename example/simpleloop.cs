using System;
using FlowManager;

/// <summary>
/// An example showing how to use loop in the flow.
/// In this example, the flow contains 3 steps and the second step is being several times
/// Further note: loop can be nested so you can defined loop inside another depend on the flow specific requirement
/// </summary>
public class SimpleLoop
{
    public static void Main()
    {
        FlowSteps steps = new FlowSteps();
        Dictionary<string, object> init_data = new Dictionary<string, object>(3);
		init_data.Add("repeat", 3);
		FlowManager flow = new FlowManager(steps.getFirstStep(), init_data);
        
        while (flow.NextAction != null) flow.Continue();

        /* Expected output: 
            Step1
            Step2_0
            Step2_1
            Step2_2
            Step3
        */
    }
}

/// <summary>
/// Define flow step in a class. But steps can be defined in other way(s) as well
/// </summary>
public class FlowSteps
{
    public FlowSteps() { }

    public virtual Action<FlowManager> getFirstStep()
    {
        return Step1;
    }

    protected virtual void Step1(FlowManager fm)
    {
        Console.WriteLine("Step1");
        int repeat = (int)fm.getData("repeat");
        fm.NextAction = Step2;
        /* The variable 'i' will be visible inside a loop */
        fm.StartLoop(Enumerable.Range(0, repeat).GtEnumerator(), "i", Step3);
    }

    protected virtual void Step2(FlowManager fm)
    {
        int i = (int)fm.getData("i");
        Console.WriteLine("Step2_" + i);

        fm.EndLoop();
    }

    protected virtual void Step3(FlowManager fm)
    {
        Console.WriteLine("Step3");
        fm.NextAction = null;
    }
}
