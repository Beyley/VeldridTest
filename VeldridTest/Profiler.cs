using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace VeldridTest {
	public class ProfilerCapture {
		public readonly double Start;
		public double End {
			get;
			private set;
		}
		/// <summary>
		/// The length in miliseconds
		/// </summary>
		public double Length {
			get;
			private set;
		}

		public void SetEnd(double end) {
			this.End = end;

			this.Length = this.End - this.Start;
		}
		
		public ProfilerCapture(double start) {
			this.Start  = start;
			this.End    = 0;
			this.Length = 0;
		}
	}
	
	public static class Profiler {
		private static Dictionary<string, ProfilerCapture> _Results;

		public static void Initialize() {
			_Results = new();
		}

		public static double GetTimestamp() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000d;
		
		public static void StartCapture(string name) {
			ProfilerCapture capture = new (GetTimestamp());

			_Results.Remove(name);
			
			_Results.Add(name, capture);
		}

		[CanBeNull]
		public static ProfilerCapture EndCapture(string name) {
			if(_Results.TryGetValue(name, out ProfilerCapture capture))
				capture.SetEnd(GetTimestamp());

			return capture;
		}
	}
}
