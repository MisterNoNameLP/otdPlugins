using System;
using System.Threading;
using System.Numerics;
using System.Diagnostics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace VoiDPlugins.Filter {
	[PluginName("TestPlugin24")]
	public class TestPlugin : IPositionedPipelineElement<IDeviceReport> {
		public event Action<IDeviceReport>? Emit;

		[SliderProperty("Scale", 0.0f, 10f, 1f), DefaultPropertyValue(1f)]
		public float Scale { get; set; }
		[SliderProperty("Test2", 0.0f, 10f, 1f), DefaultPropertyValue(1f)]
		public float Test2 { get; set; }

		public PipelinePosition Position => PipelinePosition.PostTransform;

		public float accel = 1f;
		
		public Stopwatch stopwatch = new Stopwatch();

		public int count = 0;
		public int MAX_COUNT = 100;

		public Vector2 calcTestAccel(Vector2 delta) {

			
			return delta;
		}

		public void Consume(IDeviceReport value){
			if (value is ITabletReport report){
				//Thread.Sleep(300);
				stopwatch.Stop();
				float deltaTime = (float) stopwatch.Elapsed.TotalMilliseconds;

				if (report.Position != new Vector2(0f, 0f)){
					Console.WriteLine("=== Before ===");
					Console.WriteLine(deltaTime);
					//Console.WriteLine("value: " + value);
					//Console.WriteLine("report: " + report);
					Console.WriteLine("report.Position: " + report.Position);
				}


				if (count < MAX_COUNT) {
					report.Position = new Vector2(-1, 0);
				} else {
					report.Position = new Vector2(0, 0);
				}
				count ++;
				Console.WriteLine(count);


				var steps = 10;
				for (float i = 0; i <= steps; i++) {
					var testDelta = calcTestAccel(new Vector2(i / steps, 0f));

					Console.WriteLine(testDelta.X);

					/*
					var maxDelta = 1;
					var maxChars = 10;
					var visualisationString = "";
					for ( i2 = 0; i <= maxDelta; i++ ) {
						visualisationString = visualisationString + "#"
					}
					*/
				}


				var delta = report.Position * .5f;
				var tt = delta * accel;

				//delta = delta * Vector2.Abs(( delta * accel ));


				//var delta = report.Position * Scale;
				
				//report.Position = new System.Numerics.Vector2(1f, 0f);



				if (report.Position != new Vector2(0f, 0f)){
					Console.WriteLine("=== After ===");
					Console.WriteLine("delta: " + delta);
				}

				report.Position = delta;
				value = report;
				stopwatch.Reset();
				stopwatch.Start();
			}
			


			Emit?.Invoke(value);
			//Console.WriteLine("Scale: " + Scale + ", Test2: " + Test2);
		}
	}
}
