using System;

namespace VeldridTest {
	public class RenderBatch {
		public const int MAX_TEXTURES = 8;
		
		public const int MAX_COUNT    = 2;

		public readonly Texture2D[] Textures = new Texture2D[MAX_TEXTURES];
		public readonly Vertex[]    Vertexes = new Vertex[4 * MAX_COUNT];
		public readonly ushort[]    Indicies = new ushort[6 * MAX_COUNT];
		
		public int UsedVertexes { get; private set; }
		public int UsedIndicies { get; private set; }

		private ushort[] _baseIndices = { 
			//Tri 1
			0, 1, 2,
			//Tri 2
			2, 3, 0 };
		
		/// <summary>
		/// Batches a quad
		/// </summary>
		/// <param name="vertexes"></param>
		/// <param name="texture"></param>
		/// <returns>false means there is no more texture space and you need to create a new batch</returns>
		public bool BatchTexturedQuad(Vertex[] vertexes, Texture2D texture) {
			if (vertexes.Length != 4) throw new Exception();
			
			int texId = this.GetTextureId(texture);
			if (texId == -1) return false;

			//Set the texture id of all the quads
			vertexes[0].TextureId = (byte)texId;
			vertexes[1].TextureId = (byte)texId;
			vertexes[2].TextureId = (byte)texId;
			vertexes[3].TextureId = (byte)texId;
			
			Array.Copy(vertexes, 0, this.Vertexes, this.UsedVertexes, 4);
			Array.Copy(this._baseIndices, 0, this.Indicies, this.UsedIndicies, 6);
			
			for (int i = 0; i < this._baseIndices.Length; i++) {
				this._baseIndices[i] += 4;
			}

			this.UsedIndicies += 6;
			this.UsedVertexes += 4;
			
			return true;
		}

		public bool CanFitNew() => this.UsedIndicies < this.Indicies.Length && this.UsedVertexes < this.Vertexes.Length;

		public int GetTextureId(Texture2D tex) {
			for (int i = 0; i < this.Textures.Length; i++) {
				Texture2D texture2D = this.Textures[i];

				if (tex == texture2D) {
					return i;
				}
				
				if (texture2D == null) {
					this.Textures[i] = tex;
					return i;
				}
			}

			return -1;
		}

		public void Clear() {
			this.UsedIndicies = 0;
			this.UsedVertexes = 0;
			this._baseIndices = new ushort[] {
				//Tri 1
				0, 1, 2,
				//Tri 2
				2, 3, 0
			};
		}
	}
}
