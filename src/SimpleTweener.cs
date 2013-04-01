#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SimpleTweener", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class SimpleTweener : IPluginEvaluate
	{
		[Input("Tween To", DefaultValue = 0)]
		ISpread<double> FTweenTo;
		
		[Input("Tween From", DefaultValue = 0)]
		ISpread<double> FTweenFrom;
		
		[Input("Duration", DefaultValue = 1.0)]
		ISpread<double> FDuration;
		
		[Input("Start", DefaultValue = 0, IsBang = true)]
		ISpread<bool> FStartInput;
		
		[Input("Clear", DefaultValue = 0, IsBang = true)]
		ISpread<bool> FClear;
		
		[Input("EasingType", DefaultEnumEntry = "Linear")]
		ISpread<EasingType> FEasing;
		
		[Input("EasingDirection", DefaultEnumEntry = "None")]
		ISpread<EasingDirection> FEasingDirection;
		
		[Input("Wait For Tween End", DefaultValue = 1)]
		ISpread<bool> FWaitForEnd;

		[Output("Output")]
		ISpread<double> FOutput;
		
		[Output("Finished")]
		ISpread<bool> FFinishedOutput;
		
		[Output("Started")]
		ISpread<bool> FStartedOutput;

		readonly Spread<double> FValue;
		readonly Spread<int> FStart;

		readonly Spread<double> FPreviousValue;
		readonly Spread<double> FNextValue;

		readonly Spread<bool> FFinished;

		readonly Spread<DateTime> FStartDate;
		
		[ImportingConstructor]
		public SimpleTweener()
		{
			FStart = new Spread<int>();
			FValue = new Spread<double>();
			
			FStartDate = new Spread<DateTime>();
			
			FPreviousValue = new Spread<double>();
			FNextValue = new Spread<double>();
			
			FFinished = new Spread<bool>();
		}

		public void Evaluate(int spreadMax)
		{
			FOutput.SliceCount = spreadMax;
			FFinished.SliceCount = spreadMax;
			FStart.SliceCount = spreadMax;
			FValue.SliceCount = spreadMax;
			FStartDate.SliceCount = spreadMax;
			FPreviousValue.SliceCount = spreadMax;
			FNextValue.SliceCount = spreadMax;

			FStartedOutput.SliceCount = spreadMax;
			FFinishedOutput.SliceCount = spreadMax;

			for (var i = 0; i < spreadMax; i++)
			{
				FFinishedOutput[i] = false;
				FStartedOutput[i] = false;
				
				var currentTime = DateTime.UtcNow;
				
				if(FStartInput[i])
				{
					if(FStart[i] != 0 && FWaitForEnd[i]) return;
					
					FStart[i] = 1;
					FStartDate[i] = currentTime;
					
					FPreviousValue[i] = FValue[i];
					FNextValue[i] = FTweenTo[i];
					
					FStartedOutput[i] = true;
				}
				
				var timeInterval = currentTime - FStartDate[i];
				var result = (timeInterval.TotalMilliseconds / (1000 * FDuration[i]));
				// clamp result
				result = Math.Max(0, Math.Min(result, 1));
				
				if (FStart[i] == 1)
				{
					FFinished[i] = false;
					switch(FEasingDirection[i])
					{
						case EasingDirection.In:
							result = Easing.EaseIn(result, FEasing[i]);
							break;
						case EasingDirection.Out:
							result = Easing.EaseOut(result, FEasing[i]);
							break;
						case EasingDirection.InOut:
							result = Easing.EaseInOut(result, FEasing[i]);
							break;
					}
					FValue[i] = FPreviousValue[i] * (1 - result) + result * FNextValue[i];
				}
				
				if(Math.Abs(result - 1) < 0.00001){
					FStart[i] = 0;
					if(!FFinished[i])
					{
						FFinished[i] = true;
						FFinishedOutput[i] = true;
					}
				}
				
				//clear tween
				if(FClear[i])
				{
					FStart[i] = 0;
					FValue[i] = FTweenFrom[i];
				}
			}
			
			//FStartedOutput.AssignFrom(FStartInput);
			FOutput.AssignFrom(FValue);
		}
	}
	
	//external easing class
	public static class Easing
    {
        // Adapted from source : http://www.robertpenner.com/easing/

        public static float Ease(double linearStep, float acceleration, EasingType type)
        {
            float easedStep = acceleration > 0 ? EaseIn(linearStep, type) : 
                              acceleration < 0 ? EaseOut(linearStep, type) : 
                              (float) linearStep;

            return MathHelper.Lerp(linearStep, easedStep, Math.Abs(acceleration));
        }

        public static float EaseIn(double linearStep, EasingType type)
        {
            switch (type)
            {
                case EasingType.Step:       return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:     return (float)linearStep;
                case EasingType.Sine:       return Sine.EaseIn(linearStep);
                case EasingType.Quadratic:  return Power.EaseIn(linearStep, 2);
                case EasingType.Cubic:      return Power.EaseIn(linearStep, 3);
                case EasingType.Quartic:    return Power.EaseIn(linearStep, 4);
                case EasingType.Quintic:    return Power.EaseIn(linearStep, 5);
            }
			throw new NotImplementedException();
        }

        public static float EaseOut(double linearStep, EasingType type)
        {
            switch (type)
            {
                case EasingType.Step:       return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:     return (float)linearStep;
                case EasingType.Sine:       return Sine.EaseOut(linearStep);
                case EasingType.Quadratic:  return Power.EaseOut(linearStep, 2);
                case EasingType.Cubic:      return Power.EaseOut(linearStep, 3);
                case EasingType.Quartic:    return Power.EaseOut(linearStep, 4);
                case EasingType.Quintic:    return Power.EaseOut(linearStep, 5);
            }
            throw new NotImplementedException();
        }

        public static float EaseInOut(double linearStep, EasingType easeInType, EasingType easeOutType)
        {
            return linearStep < 0.5 ? EaseInOut(linearStep, easeInType) : EaseInOut(linearStep, easeOutType);
        }
        public static float EaseInOut(double linearStep, EasingType type)
        {
            switch (type)
            {
                case EasingType.Step:       return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:     return (float)linearStep;
                case EasingType.Sine:       return Sine.EaseInOut(linearStep);
                case EasingType.Quadratic:  return Power.EaseInOut(linearStep, 2);
                case EasingType.Cubic:      return Power.EaseInOut(linearStep, 3);
                case EasingType.Quartic:    return Power.EaseInOut(linearStep, 4);
                case EasingType.Quintic:    return Power.EaseInOut(linearStep, 5);
            }
            throw new NotImplementedException();
        }

        static class Sine
        {
            public static float EaseIn(double s)
            {
                return (float)Math.Sin(s * MathHelper.HalfPi - MathHelper.HalfPi) + 1;
            }
            public static float EaseOut(double s)
            {
                return (float)Math.Sin(s * MathHelper.HalfPi);
            }
            public static float EaseInOut(double s)
            {
                return (float)(Math.Sin(s * MathHelper.Pi - MathHelper.HalfPi) + 1) / 2;
            }
        }

        static class Power
        {
            public static float EaseIn(double s, int power)
            {
                return (float)Math.Pow(s, power);
            }
            public static float EaseOut(double s, int power)
            {
                var sign = power % 2 == 0 ? -1 : 1;
                return (float)(sign * (Math.Pow(s - 1, power) + sign));
            }
            public static float EaseInOut(double s, int power)
            {
                s *= 2;
                if (s < 1) return EaseIn(s, power) / 2;
                var sign = power % 2 == 0 ? -1 : 1;
                return (float)(sign / 2.0 * (Math.Pow(s - 2, power) + sign * 2));
            }
        }
    }

    public enum EasingType
    {
        Step,
        Linear,
        Sine,
        Quadratic,
        Cubic,
        Quartic,
        Quintic
    }

    public enum EasingDirection
    {
    	None,
    	In,
    	Out,
    	InOut
    }
    
    public static class MathHelper
    {
        public const float Pi = (float)Math.PI;
        public const float HalfPi = (float)(Math.PI / 2);

        public static float Lerp(double from, double to, double step)
        {
            return (float)((to - from) * step + from);
        }
    }
}
