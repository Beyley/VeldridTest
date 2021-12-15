using System.Numerics;
using ShaderGen;
using Veldrid;
using static ShaderGen.ShaderBuiltins;

namespace VeldridTest {
	public class BasicShader {
		public Matrix4x4 ProjectionBuffer;
		// public Texture2DResource SurfaceTexture;
		// public SamplerResource   Sampler;

		public struct VertexInput
		{
			[PositionSemantic] public Vector2   Position;
			[ColorSemantic]    public RgbaFloat Color;
		}

		[VertexShader]
		public FragmentInput VertexShaderFunc(VertexInput input)
		{
			// FragmentInput output;
			// Vector4       worldPosition = Mul(World, new Vector4(input.Position, 1));
			// Vector4       viewPosition  = Mul(View, worldPosition);
			// output.Position     = Mul(Projection, viewPosition);
			// output.TextureCoord = input.TextureCoord;
			// return output;

			FragmentInput output = new() {
				Position = Mul(this.ProjectionBuffer, new(input.Position, 0, 1))
			};

			output.Position.Y *= -1;

			output.Color = input.Color;

			return output;
		}
		
		public struct FragmentInput
		{
			[SystemPositionSemanticAttribute] public Vector4   Position;
			[ColorSemantic]                   public RgbaFloat Color;
		}


		[FragmentShader]
		public RgbaFloat FragmentShaderFunc(FragmentInput input)
		{
			// return Sample(SurfaceTexture, Sampler, input.TextureCoord);
			return input.Color;
		}
	}
}
