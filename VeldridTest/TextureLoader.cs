using Kettu;
using Veldrid;
using Veldrid.ImageSharp;

namespace VeldridTest {
	public class TextureLoader {
		public static Texture2D LoadTexture(string path, Sampler sampler, RenderState renderState) {
			Profiler.StartCapture("load_texture");
			ImageSharpTexture rawTexture = new(path);
			Logger.Log($"Loading texture {path} took {Profiler.EndCapture($"load_texture").Length} ms!");

			return new(rawTexture, sampler, renderState);
		}
	}
}
