using System.Drawing;
using System.Numerics;
using FontStashSharp.Interfaces;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace VeldridTest.FontStashSharp {
	public class VeldridFontStashRenderer : IFontStashRenderer {
		public DeviceBuffer IndexBuffer;
		public DeviceBuffer VertexBuffer;
		
		private RenderState  _renderState;
		
		public VeldridFontStashRenderer(RenderState state) {
			this.TextureManager = new VeldridTexture2DManager();
			this._renderState   = state;
		}
		
		public void Draw(object texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 origin, Vector2 scale, float depth) {
			Texture2D texture2D = (Texture2D)texture;

			RgbaFloat convertedColor = new(color.R, color.G, color.B, color.A);

			src ??= new(0, 0, texture2D.Size.X, texture2D.Size.Y);

			Vector2 texelSize = new(1f / texture2D.Size.X, 1f / texture2D.Size.Y);
			
			Vector2 size = new(src.Value.Width * scale.X, src.Value.Height * scale.Y);

			// Create an array of Vertex's for the quad pos and colours
			Vertex[] quadVertices = {
			 	//Bottom left
			 	new(new Vector2(pos.X, pos.Y + size.Y) - origin, convertedColor, new(src.Value.X * texelSize.X, src.Value.Bottom * texelSize.Y)),
			 	//Bottom right
			 	new(pos + size - origin, convertedColor, new(src.Value.Right * texelSize.X, src.Value.Bottom * texelSize.Y)),
			 	//Top right
			 	new(new Vector2(pos.X + size.X, pos.Y) - origin, convertedColor, new(src.Value.Right * texelSize.X, src.Value.Y * texelSize.Y)),
			 	//Top left
			 	new(pos - origin, convertedColor, new(src.Value.X * texelSize.X, src.Value.Y * texelSize.Y))
			};
			// Vertex[] quadVertices = {
			// 	//Bottom left
			// 	new(new(000, 500), RgbaFloat.Red, new(0, 1)),
			// 	//Bottom right
			// 	new(new(500), RgbaFloat.Green, new(1, 1)),
			// 	//Top right
			// 	new(new(500, 000), RgbaFloat.Blue, new(1, 0)),
			// 	//Top left
			// 	new(new(500), RgbaFloat.Yellow, new(0, 0))
			// };

			//Set the indicies for the quad
			ushort[] quadIndices = { 
				//Tri 1
				0, 1, 2,
				//Tri 2
				2, 3, 0 };
			
			ResourceFactory factory = this._renderState.GraphicsDevice.ResourceFactory;

			//Create the vertex buffer
			this.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(quadVertices.Length * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
			//Create the index buffer
			this.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(quadIndices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
			
			//Fill the vertex buffer and make it viewable to the GPU
			this._renderState.GraphicsDevice.UpdateBuffer(this.VertexBuffer, 0, quadVertices);
			//Fill the index buffer and make it viewable to the GPU
			this._renderState.GraphicsDevice.UpdateBuffer(this.IndexBuffer, 0, quadIndices);
			
			this._renderState.CommandList.SetGraphicsResourceSet(1, texture2D.ResourceSet);
			
			//Set the vertex buffer for the test rectangle
			this._renderState.CommandList.SetVertexBuffer(0, this.VertexBuffer);
			//Set the index buffer for the test rectangle
			this._renderState.CommandList.SetIndexBuffer(this.IndexBuffer, IndexFormat.UInt16);
			this._renderState.CommandList.DrawIndexed(
				(uint)quadIndices.Length,
				1,
				0,
				0,
				0);
		}
		
		public ITexture2DManager TextureManager {
			get;
		}
	}
}
