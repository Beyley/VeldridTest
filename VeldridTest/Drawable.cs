using System;
using System.Numerics;
using Veldrid;

namespace VeldridTest {
	public abstract class Drawable {
		public Vector3 Position = new();
		public RgbaFloat Color = RgbaFloat.White;
		
		public abstract void Draw(RenderState renderState);
		public abstract void Dispose();

		public float LastDepth;
	}
}
