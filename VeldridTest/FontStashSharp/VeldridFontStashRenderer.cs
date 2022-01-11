using System.Drawing;
using System.Numerics;
using FontStashSharp.Interfaces;
using GLib;
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

			Vector2 size = new(src.Value.Width * scale.X, src.Value.Height * scale.Y);
			
			BatchedRenderer.DrawTexture(texture2D, new(pos - origin, depth), convertedColor, size, new(src.Value.X, src.Value.Y, src.Value.Width, src.Value.Height));
		}
		
		public ITexture2DManager TextureManager {
			get;
		}
	}
}
