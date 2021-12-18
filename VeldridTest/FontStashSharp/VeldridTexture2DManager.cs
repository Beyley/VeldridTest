using System.Drawing;
using FontStashSharp.Interfaces;

namespace VeldridTest.FontStashSharp {
	public class VeldridTexture2DManager : ITexture2DManager {
		public object CreateTexture(int width, int height) {
			return new Texture2D((uint)width, (uint)height, Program.RenderState.GraphicsDevice.Aniso4xSampler, Program.RenderState);
		}
		
		public Point GetTextureSize(object texture) {
			return (texture as Texture2D)!.Size;
		}
		
		public void SetTextureData(object texture, Rectangle bounds, byte[] data) {
			(texture as Texture2D)!.SetData<byte>(bounds, data, Program.RenderState);
		}
	}
}
