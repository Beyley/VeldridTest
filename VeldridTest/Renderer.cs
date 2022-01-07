using System;
using System.Numerics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Veldrid;

namespace VeldridTest {
	public static class Renderer {
		public static bool Begun { get; private set; }

		private static RenderState _RenderState;

		public static void Begin(RenderState renderState) {
			if (Begun) throw new Exception("Renderer already begun!");

			_RenderState = renderState;
			
			_RenderState.CommandList.Begin();
			
			//Set the framebuffer to render to
			_RenderState.CommandList.SetFramebuffer(_RenderState.GraphicsDevice.SwapchainFramebuffer);
			
			_RenderState.CommandList.SetFullViewports();
			
			//Set the pipeline
			_RenderState.CommandList.SetPipeline(_RenderState.TexturedPipeline);
			//Set the graphics resource set that contains the projection buffer
			_RenderState.CommandList.SetGraphicsResourceSet(0, _RenderState.ProjectionBufferResourceSet);

			Begun = true;
		}

		public static void ClearColor(RgbaFloat color) {
			if (!Begun) throw new Exception("Renderer not begun!");
			
			_RenderState.CommandList.ClearColorTarget(0, color);
		} 

		public static void DrawTexture(Texture2D texture, Vector2 position, RgbaFloat color, Point size, Rectangle? src = null) => DrawTexture(texture, position, color, new Vector2(size.X, size.Y), src);

		public static void DrawTexture(Texture2D texture, Vector2 position, RgbaFloat color, System.Drawing.Point size, Rectangle? src = null) => DrawTexture(texture, position, color, new Vector2(size.X, size.Y), src);
		
		private static DeviceBuffer _IndexBuffer;
		private static DeviceBuffer _VertexBuffer;
		
		public static void DrawTexture(Texture2D texture, Vector2 position, RgbaFloat color, Vector2 size, Rectangle? src = null) {
			

			Vector2 texBL = new(0, 1);
			Vector2 texBR = new(1, 1);
			Vector2 texTR = new(1, 0);
			Vector2 texTL = new(0, 0);

			if (src.HasValue) {
				Vector2 texelSize = new(1f / texture.Size.X, 1f / texture.Size.Y);
				
				texBL = new(src.Value.X     * texelSize.X, src.Value.Bottom * texelSize.Y);
				texBR = new(src.Value.Right * texelSize.X, src.Value.Bottom * texelSize.Y);
				texTR = new(src.Value.Right * texelSize.X, src.Value.Y      * texelSize.Y);
				texTL = new(src.Value.X     * texelSize.X, src.Value.Y      * texelSize.Y);
			}
			
			//Create an array of Vertex's for the quad pos and colours
			Vertex[] vertices = {
				//Bottom left
				new(new Vector2(position.X, position.Y + size.Y), color, texBL),
				//Bottom right
				new(position + size, color, texBR),
				//Top right
				new(new Vector2(position.X + size.X, position.Y), color, texTR),
				//Top left
				new(position, color, texTL)
			};

			//Set the indicies for the quad
			ushort[] indices = { 
				//Tri 1
				0, 1, 2,
				//Tri 2
				2, 3, 0 };
			
			ResourceFactory factory = _RenderState.GraphicsDevice.ResourceFactory;

			if(_VertexBuffer == null || _IndexBuffer == null) {
				_VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(vertices.Length * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
				_IndexBuffer  = factory.CreateBuffer(new BufferDescription((uint)(indices.Length  * sizeof(ushort)), BufferUsage.IndexBuffer));
			}
			
			//Fill the vertex buffer and make it viewable to the GPU
			_RenderState.CommandList.UpdateBuffer(_VertexBuffer, 0, vertices);
			//Fill the index buffer and make it viewable to the GPU
			_RenderState.CommandList.UpdateBuffer(_IndexBuffer, 0, indices);
			
			_RenderState.CommandList.SetGraphicsResourceSet(1, texture.ResourceSet);
			
			//Set the vertex buffer for the test rectangle
			_RenderState.CommandList.SetVertexBuffer(0, _VertexBuffer);
			//Set the index buffer for the test rectangle
			_RenderState.CommandList.SetIndexBuffer(_IndexBuffer, IndexFormat.UInt16);
			_RenderState.CommandList.DrawIndexed(
				(uint)indices.Length,
				1,
				0,
				0,
				0);
		}

		public static void RenderText() {
			
		}

		public static void End() {
			Begun = false;
			
			_RenderState.CommandList.End();
			
			_RenderState.GraphicsDevice.SubmitCommands(_RenderState.CommandList);
			
			_RenderState.GraphicsDevice.SwapBuffers();
		}
	}
}
