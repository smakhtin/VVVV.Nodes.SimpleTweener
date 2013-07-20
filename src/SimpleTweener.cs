#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SimpleTweener", Category = "Value", Help = "Tweens Values", Tags = "")]
	#endregion PluginInfo
	public class SimpleTweener : IPluginEvaluate
	{
		[Input("Tween To", DefaultValue = 0)]
		ISpread<ISpread<double>> FTweenToIn;
		
		[Input("Tween From", DefaultValue = 0)]
		ISpread<double> FTweenFromIn;
		
		[Input("Duration", DefaultValue = 1.0)]
		ISpread<double> FDurationIn;
		
		[Input("Start", DefaultValue = 0, IsBang = true)]
		ISpread<bool> FStartInputIn;
		
		[Input("Clear", DefaultValue = 0, IsBang = true)]
		ISpread<bool> FClearIn;

        [Input("EasingType", DefaultEnumEntry = "Elastic")]
		ISpread<EasingType> FEasingTypeIn;
		
		[Input("EasingDirection", DefaultEnumEntry = "None")]
		ISpread<EasingDirection> FEasingDirection;
		
		[Input("Wait For Tween End", DefaultValue = 1)]
		ISpread<bool> FWaitForEndIn;

		[Output("Output")]
		ISpread<double> FOutput;
		
		[Output("Finished")]
		ISpread<bool> FFinishedOut;
		
		[Output("Started")]
		ISpread<bool> FStartedOut;

		private readonly Spread<double> FValue;

		private readonly Spread<double> FPreviousValue;
		private readonly Spread<double> FNextValue;

	    private readonly Spread<int> FStep;

		private readonly Spread<DateTime> FStartDate;

	    private readonly Dictionary<EasingDirection, Dictionary<EasingType, Func<double, double>>> FEasingByDirection;
        
        [ImportingConstructor]
		public SimpleTweener()
		{
			FValue = new Spread<double>();
            FStep = new Spread<int>();
			
			FStartDate = new Spread<DateTime>();
			
			FPreviousValue = new Spread<double>();
			FNextValue = new Spread<double>();
			
            FEasingByDirection = new Dictionary<EasingDirection, Dictionary<EasingType, Func<double, double>>>();

            FillEasings();
		}

	    private void FillEasings()
	    {
	        FEasingByDirection.Add(EasingDirection.In, new Dictionary<EasingType, Func<double, double>>());
            FEasingByDirection.Add(EasingDirection.InOut, new Dictionary<EasingType, Func<double, double>>());
            FEasingByDirection.Add(EasingDirection.Out, new Dictionary<EasingType, Func<double, double>>());
            FEasingByDirection.Add(EasingDirection.OutIn, new Dictionary<EasingType, Func<double, double>>());

            FEasingByDirection[EasingDirection.In].Add(EasingType.Back, Tweener.BackEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Bounce, Tweener.BounceEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Circular, Tweener.CircularEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Cubic, Tweener.CubicEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Elastic, Tweener.ElasticEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Exponential, Tweener.ExponentialEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Quad, Tweener.QuadEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Quartic, Tweener.QuarticEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Quintic, Tweener.QuinticEaseIn);
            FEasingByDirection[EasingDirection.In].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseIn);

            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Back, Tweener.BackEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Bounce, Tweener.BounceEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Circular, Tweener.CircularEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Cubic, Tweener.CubicEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Elastic, Tweener.ElasticEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Exponential, Tweener.ExponentialEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quad, Tweener.QuadEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quartic, Tweener.QuarticEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quintic, Tweener.QuinticEaseInOut);
            FEasingByDirection[EasingDirection.InOut].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseInOut);

            FEasingByDirection[EasingDirection.Out].Add(EasingType.Back, Tweener.BackEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Bounce, Tweener.BounceEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Circular, Tweener.CircularEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Cubic, Tweener.CubicEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Elastic, Tweener.ElasticEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Exponential, Tweener.ExponentialEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Quad, Tweener.QuadEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Quartic, Tweener.QuarticEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Quintic, Tweener.QuinticEaseOut);
            FEasingByDirection[EasingDirection.Out].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseOut);

            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Back, Tweener.BackEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Bounce, Tweener.BounceEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Circular, Tweener.CircularEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Cubic, Tweener.CubicEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Elastic, Tweener.ElasticEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Exponential, Tweener.ExponentialEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quad, Tweener.QuadEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quartic, Tweener.QuarticEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quintic, Tweener.QuinticEaseOutIn);
            FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseOutIn);
	    }

	    public void Evaluate(int spreadMax)
	    {
	        var sMax = Math.Max(FTweenToIn.SliceCount, FTweenFromIn.SliceCount);
            FOutput.SliceCount = sMax;
            FValue.SliceCount = sMax;
            FStartDate.SliceCount = sMax;
            FPreviousValue.SliceCount = sMax;
            FNextValue.SliceCount = sMax;

            FStartedOut.SliceCount = sMax;
            FFinishedOut.SliceCount = sMax;

            FStep.SliceCount = sMax;

			for (var i = 0; i < spreadMax; i++)
			{
				var currentTime = DateTime.UtcNow;
				
				if(FStartInputIn[i])
				{
					if(FStartedOut[i] && FWaitForEndIn[i]) return;
					
					FStartedOut[i] = true;
                    FStartedOut[i] = true;
					
                    FStartDate[i] = currentTime;
					FPreviousValue[i] = FValue[i];
					FNextValue[i] = FTweenToIn[i][0];
				}
				
				var timeInterval = currentTime - FStartDate[i];
				var result = (timeInterval.TotalMilliseconds / (1000 * FDurationIn[i]));

                // clamp result
                result = Math.Max(0, Math.Min(result, 1));

                if (Math.Abs(result - 1) < 0.00001 && FStartedOut[i])
                {
                    FStep[i]++;
                    //next step
                    if (FTweenToIn[i].SliceCount > FStep[i])
                    {
                        FStartDate[i] = currentTime;

                        FPreviousValue[i] = FValue[i];
                        FNextValue[i] = FTweenToIn[i][FStep[i]];
                    }
                    else
                    {
                        FStartedOut[i] = false;
                        if (!FFinishedOut[i])
                        {
                            FFinishedOut[i] = true;
                        } 
                    }
                }
				
				if (FStartedOut[i])
				{
					FFinishedOut[i] = false;
                    if (FEasingDirection[i] != EasingDirection.None)
                    {
                        var easingFunction = FEasingByDirection[FEasingDirection[i]][FEasingTypeIn[i]];
                        result = easingFunction(result);
                    }
					
					FValue[i] = FPreviousValue[i] * (1 - result) + result * FNextValue[i];
				}
				
				//clear tween
				if(FClearIn[i])
				{
					FStartedOut[i] = false;
				    FFinishedOut[i] = false;
				    FStep[i] = 0;
					FValue[i] = FTweenFromIn[i];
				}

                FFinishedOut[i] = FFinishedOut[i];
                FStartedOut[i] = FStartedOut[i];
			}
			
			FOutput.AssignFrom(FValue);
		}
	}

    public enum EasingType
    {
        Back,
        Bounce,
        Circular,
        Cubic,
        Elastic,
        Exponential,
        Quad,
        Quartic,
        Quintic,
        Sinusoidal
    }

    public enum EasingDirection
    {
    	None,
    	In,
    	InOut,
        Out,
        OutIn
    }

    public enum Mode
    {
        None,
        Bounce
    }

}
