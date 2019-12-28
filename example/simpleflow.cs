using System;
using FlowManager;

/// <summary>
/// A 'Hello world' example of how to use FlowManager class.
/// In this example the flow only contains 3 steps and run in feedforward fashion
/// </summary>
public class SimpleFlow
{
	public static void Main()
	{
        FlowSteps steps = new FlowSteps();
		FlowManager flow = new FlowManager(steps.getFirstStep());
        
        while (flow.NextAction != null) flow.Continue();

        /* Expected output: 
            Step1
            Step2
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

    public virtual Action<FlowManager> getFirstStep() {
        return Step1;
    }

    protected virtual void Step1(FlowManager fm) {
        Console.WriteLine("Step1");
        fm.NextAction = Step2;
    }

    protected virtual void Step2(FlowManager fm) {
        Console.WriteLine("Step2");
        fm.NextAction = Step3;
    }

    protected virtual void Step3(FlowManager fm) {
        Console.WriteLine("Step3");
        fm.NextAction = null;
    }
}
