using System.Numerics;
using Veldrid;

namespace VeldridTest {
	public class PrimitiveQuadDrawable : DrawableObject {
		public PrimitiveQuadDrawable(Vector2 position, Vector2 size, RenderState renderState) {
			//Create an array of Vertex's for the quad pos and colours
			Vertex[] quadVertices = {
				//Bottom left
				new(new Vector2(position.X, position.Y + size.Y), RgbaFloat.Red, new(0, 1)),
				//Bottom right
				new(position + size, RgbaFloat.Green, new(1, 1)),
				//Top right
				new(new Vector2(position.X + size.X, position.Y), RgbaFloat.Blue, new(1, 0)),
				//Top left
				new(position, RgbaFloat.Yellow, new(0, 0))
			};

			//Set the indicies for the quad
			ushort[] quadIndices = { 
				//Tri 1
				0, 1, 2,
				//Tri 2
				2, 3, 0 };
			
			this.Vertices = quadVertices;
			this.Indices  = quadIndices;

			this.CreateBuffers(renderState);
			this.UpdateBuffers(renderState);
		}
	}
}
