using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Numerics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Veldrid;

namespace VeldridTest {
	public static class BatchedRenderer {
		public static bool Begun { get; private set; }

		private static RenderState _RenderState;

		private static int BatchCount = 16;
		
		private static RenderBatch[] _ActiveBatches = new RenderBatch[BatchCount];
		
		public static int UsedBatches { get; private set; }

		public static void Begin(RenderState renderState) {
			if (Begun) throw new Exception("Renderer already begun!");

			_RenderState = renderState;
			
			_RenderState.CommandList.Begin();
			
			// Console.WriteLine(_RenderState.GraphicsDevice.SwapchainFramebuffer.DepthTarget.HasValue);
			
			//Set the framebuffer to render to
			_RenderState.CommandList.SetFramebuffer(_RenderState.GraphicsDevice.SwapchainFramebuffer);
			
			_RenderState.CommandList.SetFullViewports();
			
			//Set the pipeline
			_RenderState.CommandList.SetPipeline(_RenderState.TexturedPipeline);
			//Set the graphics resource set that contains the projection buffer
			_RenderState.CommandList.SetGraphicsResourceSet(0, _RenderState.ProjectionBufferResourceSet);

			// _IndexBuffer = _RenderState.ResourceFactory.CreateBuffer(new BufferDescription((uint)(INDICES.Length * sizeof(ushort)), BufferUsage.IndexBuffer));

			//Fill the index buffer and make it viewable to the GPU
			// _RenderState.CommandList.UpdateBuffer(_IndexBuffer, 0, INDICES);
			
			Begun = true;

			// _LastBoundTexture = null;

			UsedBatches = 0;
		}

		public static void ClearColor(RgbaFloat color) {
			if (!Begun) throw new Exception("Renderer not begun!");
			
			_RenderState.CommandList.ClearColorTarget(0, color);
			// _RenderState.CommandList.ClearDepthStencil(1);
		}
		
		public static void DrawTexture(Texture2D texture, Vector3 position, RgbaFloat color, Point size, Rectangle? src = null) => DrawTexture(texture, position, color, new Vector2(size.X, size.Y), src);

		public static void DrawTexture(Texture2D texture, Vector3 position, RgbaFloat color, System.Drawing.Point size, Rectangle? src = null) => DrawTexture(texture, position, color, new Vector2(size.X, size.Y), src);

		private static readonly Vertex[] VERTICES = new Vertex[4];
		
		public static void DrawTexture(Texture2D texture, Vector3 position, RgbaFloat color, Vector2 size, Rectangle? src = null) {
			Vector2 texBL = new(0, 1);
			Vector2 texBR = new(1, 1);
			Vector2 texTR = new(1, 0);
			Vector2 texTL = new(0, 0);

			Vector3 size3 = new(size, 0);

			if (src.HasValue) {
				Vector2 texelSize = new(1f / texture.Size.X, 1f / texture.Size.Y);
				
				texBL = new(src.Value.X     * texelSize.X, src.Value.Bottom * texelSize.Y);
				texBR = new(src.Value.Right * texelSize.X, src.Value.Bottom * texelSize.Y);
				texTR = new(src.Value.Right * texelSize.X, src.Value.Y      * texelSize.Y);
				texTL = new(src.Value.X     * texelSize.X, src.Value.Y      * texelSize.Y);
			}

			//Bottom left
			VERTICES[0] = new(new Vector3(position.X, position.Y + size.Y, position.Z), color, texBL);
			//Bottom right
			VERTICES[1] = new(position + size3, color, texBR);
			//Top right
			VERTICES[2] = new(new Vector3(position.X + size.X, position.Y, position.Z), color, texTR);
			//Top left
			VERTICES[3] = new(position, color, texTL);
			
			RenderBatch batch;
			if (CreatedBatches == 0) {
				batch = CreateNewRenderBatch();
			}
			else {
				RenderBatch found = null;

				for (int i = 0; i < CreatedBatches; i++) {
					RenderBatch b = _ActiveBatches[i];

					if (!b.CanFitNew())
						continue;
					
					found = b;
					break;
				}
				
				// if (_ActiveBatches[CreatedBatches - 1].CanFitNew())
					// found = _ActiveBatches[CreatedBatches - 1];

				//If no batch was found, create a new one
				if (found == null) {
					batch = CreateNewRenderBatch();
				}
				// else, use the open batch
				else {
					batch = found;
				}
			}

			batch.BatchTexturedQuad(VERTICES, texture);
		}

		/// <summary>
		/// Creates a new renderbatch and returns it to you
		/// </summary>
		/// <returns></returns>
		private static RenderBatch CreateNewRenderBatch() {
			int used = CreatedBatches;

			if (used == BatchCount) {
				BatchCount *= 2;
				Array.Resize(ref _ActiveBatches, BatchCount);
			}

			CreatedBatches++;
			return _ActiveBatches[used] = new();
		}

		private static int CreatedBatches;
		
		private static DeviceBuffer _IndexBuffer;
		private static DeviceBuffer _VertexBuffer;
		
		private static BindableResource[] resources = new BindableResource[RenderBatch.MAX_TEXTURES * 2];

		private static void Flush() {
			for (int i = 0; i < _ActiveBatches.Length; i++) {
				RenderBatch batch = _ActiveBatches[i];
				
				if (batch == null) continue;

				_VertexBuffer ??= _RenderState.ResourceFactory.CreateBuffer(new BufferDescription((uint)(batch.Vertexes.Length * Vertex.SizeInBytes), BufferUsage.VertexBuffer));
				_IndexBuffer  ??= _RenderState.ResourceFactory.CreateBuffer(new BufferDescription((uint)(batch.Indicies.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
				
				_RenderState.CommandList.UpdateBuffer(_VertexBuffer, 0, batch.Vertexes);
				_RenderState.CommandList.UpdateBuffer(_IndexBuffer, 0, batch.Indicies);

				for (int i2 = 0; i2 < batch.UsedTextures; i2++) {
					_RenderState.CommandList.SetGraphicsResourceSet((uint)i2 + 1, batch.Textures[i2].GetResourceSet(_RenderState, i2));
				}
				
				_RenderState.CommandList.SetVertexBuffer(0, _VertexBuffer);
				_RenderState.CommandList.SetIndexBuffer(_IndexBuffer, IndexFormat.UInt16);
				
				_RenderState.CommandList.DrawIndexed(
					(uint)batch.UsedIndicies,
					1,
					0,
					0,
					0);
				
				// resourceSet.Dispose();
			}
		}

		public static void End() {
			Begun = false;

			Flush();

			
			_RenderState.CommandList.End();
			
			_RenderState.GraphicsDevice.SubmitCommands(_RenderState.CommandList);
			
			_RenderState.GraphicsDevice.SwapBuffers();
			
			// CreatedBatches = 0;

			// _ActiveBatches = new RenderBatch[BatchCount];
			for (int i = 0; i < _ActiveBatches.Length; i++) {
				if (_ActiveBatches[i] != null && _ActiveBatches[i].UsedIndicies != 0) UsedBatches++;
				_ActiveBatches[i]?.Clear();
			}
		}
	}
}
