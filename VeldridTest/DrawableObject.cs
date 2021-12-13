using System;
using Veldrid;

namespace VeldridTest {
	public class DrawableObject : IDisposable {
		public DeviceBuffer VertexBuffer;
		public DeviceBuffer IndexBuffer;

		public Vertex[] Vertices;
		public ushort[]              Indices;

		public DrawableObject() {
			
		}

		public DrawableObject(Vertex[] vertices, ushort[] indices, RenderState renderState) {
			this.Vertices = vertices;
			this.Indices  = indices;

			this.CreateBuffers(renderState);
			this.UpdateBuffers(renderState);
		}

		public void CreateBuffers(RenderState renderState) {
			ResourceFactory factory = renderState.GraphicsDevice.ResourceFactory;
			
			//Create the vertex buffer
			this.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(this.Vertices.Length * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
			//Create the index buffer
			this.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(this.Indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
		}

		public void UpdateBuffers(RenderState renderState) {
			//Fill the vertex buffer and make it viewable to the GPU
			renderState.GraphicsDevice.UpdateBuffer(this.VertexBuffer, 0, this.Vertices);
			//Fill the index buffer and make it viewable to the GPU
			renderState.GraphicsDevice.UpdateBuffer(this.IndexBuffer, 0, this.Indices);
		}

		public void Dispose() {
			this.VertexBuffer?.Dispose();
			this.IndexBuffer?.Dispose();
			
			GC.SuppressFinalize(this);
		}

		public void Draw(RenderState renderState) {
			//Set the vertex buffer for the test rectangle
			renderState.CommandList.SetVertexBuffer(0, this.VertexBuffer);
			//Set the index buffer for the test rectangle
			renderState.CommandList.SetIndexBuffer(this.IndexBuffer, IndexFormat.UInt16);
			renderState.CommandList.DrawIndexed(
				(uint)this.Indices.Length,
				1,
				0,
				0,
				0);
		}
	}
}
