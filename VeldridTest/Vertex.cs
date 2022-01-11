using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace VeldridTest {
	public struct Vertex {
		public Vector3   Position;        // This is the position, in pixels.
		public RgbaFloat Color;           // This is the color of the vertex.
		public Vector2   TexturePosition; // This is the position of the texture.
		public byte      TextureId;
		
		public Vertex(Vector3 position, RgbaFloat color, Vector2 texturePosition) {
			this.Position        = position;
			this.Color           = color;
			this.TexturePosition = texturePosition;
			this.TextureId       = 0;
		}
		public static uint SizeInBytes => (uint)Unsafe.SizeOf<Vertex>();
	}
}
