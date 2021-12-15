using EeveeTools.Helpers;
using Kettu;
using Veldrid;
using Veldrid.ImageSharp;

namespace VeldridTest {
	public class Texture2D {
		public ImageSharpTexture RawTexture;
		public Texture           Texture;
		public TextureView       TextureView;
		public ResourceSet       ResourceSet;
		
		public static ResourceLayout    ResourceLayout;

		public Texture2D(ImageSharpTexture texture, Sampler sampler, RenderState renderState) {
			long timeStart = UnixTime.Now();
			Profiler.StartCapture($"texture_create{timeStart}");
			this.RawTexture = texture;

			this.Texture     = this.RawTexture.CreateDeviceTexture(renderState.GraphicsDevice, renderState.ResourceFactory);
			this.TextureView = renderState.ResourceFactory.CreateTextureView(this.Texture);

			this.ResourceSet = renderState.ResourceFactory.CreateResourceSet(
				new ResourceSetDescription(
					ResourceLayout,
					this.TextureView,
					sampler)
			);
			Logger.Log($"texture creation took {Profiler.EndCapture($"texture_create{timeStart}").Length} ms!");
		}
	}
}
