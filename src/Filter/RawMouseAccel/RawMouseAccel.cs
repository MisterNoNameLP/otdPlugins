using System;
using System.Threading;
using System.Numerics;
using System.Diagnostics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace RawMouseAccel.Filter {
	[PluginName("RawMouseAccel")]
	public class RawMouseAccelBindings : IStateBinding{
		internal static bool bypass { set; get; }

		public static string[] ValidModes => new[] { "Toggle", "Hold" };

		[Property("Mode"), PropertyValidated(nameof(ValidModes))]
		public string? mode { set; get; }

		public void Press(TabletReference tablet, IDeviceReport report) {
			if (mode == "Toggle")
				bypass = !bypass;
			else if (mode == "Hold")
				bypass = true;
		}

		public void Release(TabletReference tablet, IDeviceReport report) {
			if (mode == "Hold")
				bypass = false;
		}
	}

	[PluginName("RawMouseAccel")]
	public class RawMouseAccel : IPositionedPipelineElement<IDeviceReport> {
		public event Action<IDeviceReport>? Emit;
		public PipelinePosition Position => PipelinePosition.PostTransform;

		public bool isDev = false;

		[SliderProperty("Scale", 0.1f, 10f, 1f), DefaultPropertyValue(1f)]
		public float Scale { get; set; }
		[SliderProperty("Acceleration", 1f, 10f, 1.2f), DefaultPropertyValue(1.2f)]
		public float Accel { get; set; }
		
		public Stopwatch stopwatch = new Stopwatch();

		public Vector2 calcTestAccel(Vector2 delta) {
			Vector2 newDelta = new Vector2(0f, 0f);
			bool xIsNegative = false;
			bool yIsNegative = false;

			if (delta.X < 0) {
				xIsNegative = true;
			}
			if (delta.Y < 0) {
				yIsNegative = true;
			}

			newDelta.X = (float)Math.Pow(Math.Abs(delta.X), Accel);
			newDelta.Y = (float)Math.Pow(Math.Abs(delta.Y), Accel);
			
			if (xIsNegative) {
				newDelta.X *= -1;
			}
			if (yIsNegative) {
				newDelta.Y *= -1;
			}

			return newDelta;
		}

		public void Consume(IDeviceReport value){
			if (value is ITabletReport report){
				var newPosition = report.Position;
				var reportPosition = report.Position;

				//Thread.Sleep(300);
				stopwatch.Stop();
				float deltaTime = (float) stopwatch.Elapsed.TotalMilliseconds;

				if (reportPosition != new Vector2(0f, 0f) && isDev){
					Console.WriteLine("=== Before ===");
					Console.WriteLine(deltaTime);
					//Console.WriteLine("value: " + value);
					//Console.WriteLine("report: " + report);
					Console.WriteLine("reportPosition: " + reportPosition);

					var steps = 30;
					for (float i = 0; i <= steps; i++) {
						var testDelta = calcTestAccel(new Vector2((i * 1) / steps, 0f)) * 10f;	
						// calc graph
						var maxDelta = 30;
						var maxChars = steps;
						var visualisationString = "";
						var charsToPrint = maxChars / maxDelta * testDelta.X;
						for (int i2 = 0; i2 <= maxChars; i2++){
							if (i2 <= charsToPrint) {
								visualisationString = visualisationString + "##";
							} else {
								visualisationString = visualisationString + "..";
							}
						}
						Console.WriteLine(visualisationString);
					}
				}

				newPosition = calcTestAccel(reportPosition);

				if (reportPosition != new Vector2(0f, 0f) && isDev){
					Console.WriteLine("=== After ===");
					Console.WriteLine(newPosition);
				}

				if (!RawMouseAccelBindings.bypass){
					reportPosition = newPosition * Scale;
					report.Position = reportPosition;
				}

				//reportPosition = delta;
				value = report;
				stopwatch.Reset();
				stopwatch.Start();
			}
			

			Emit?.Invoke(value);
			//Console.WriteLine("Scale: " + Scale + ", Test2: " + Test2);
		}
	}
}
