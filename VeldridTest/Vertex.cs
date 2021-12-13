using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace VeldridTest {
	public struct Vertex {
		public Vector2   Position;        // This is the position, in pixels.
		public RgbaFloat Color;           // This is the color of the vertex.
		public Vector2   TexturePosition; //This is the position of the texture.
		
		public Vertex(Vector2 position, RgbaFloat color, Vector2 texturePosition) {
			this.Position        = position;
			this.Color           = color;
			this.TexturePosition = texturePosition;
		}
		public static uint SizeInBytes => (uint)Unsafe.SizeOf<Vertex>();
	}
}
