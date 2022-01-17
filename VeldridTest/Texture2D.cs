using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EeveeTools.Helpers;
using Kettu;
using Veldrid;
using Veldrid.ImageSharp;
using VeldridTest.FontStashSharp;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace VeldridTest {
	public class Texture2D : IDisposable {
		public ImageSharpTexture RawTexture;
		public Texture           Texture;
		public ResourceSet       ResourceSet;
		public ResourceSet[]     ResourceSets = new ResourceSet[8];
		public Sampler           Sampler;

		public ResourceSet GetResourceSet(RenderState renderState, int i) {
			return this.ResourceSets[i] ?? (this.ResourceSets[i] = renderState.ResourceFactory.CreateResourceSet(
				new ResourceSetDescription(
					BatchedResourceLayouts[i],
					this.Texture,
					this.Sampler)
			));
		}
		
		public Point Size => new((int)this.Texture.Width, (int)this.Texture.Height);
		
		public static ResourceLayout ResourceLayout;
		public static ResourceLayout BatchedResourceLayout;
		public static ResourceLayout[] BatchedResourceLayouts = new ResourceLayout[8];

		public unsafe void SetData<T>(Rectangle bounds, ReadOnlySpan<T> data, RenderState renderState) where T : unmanaged {
			fixed (T* ptr = data) {
				uint byteCount = (uint)(data.Length * Unsafe.SizeOf<T>());
				
				renderState.GraphicsDevice.UpdateTexture(this.Texture, (IntPtr)ptr, byteCount, (uint)bounds.X, (uint)bounds.Y, 0, (uint)bounds.Width, (uint)bounds.Height, 1, 0, 0);
			}
		}
		
		public Texture2D(uint width, uint height, Sampler sampler, RenderState renderState) {
			Profiler.StartCapture($"texture_create{Thread.CurrentThread.Name}");

			this.Sampler = sampler;
			
			this.Texture = renderState.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled | TextureUsage.RenderTarget));

			this.ResourceSet = renderState.ResourceFactory.CreateResourceSet(
				new ResourceSetDescription(
					ResourceLayout,
					this.Texture,
					sampler)
			);
			
			Logger.Log($"blank texture creation took {Profiler.EndCapture($"texture_create{Thread.CurrentThread.Name}").Length} ms!");
		}
		
		public Texture2D(ImageSharpTexture texture, Sampler sampler, RenderState renderState) {
			Profiler.StartCapture($"texture_create{Thread.CurrentThread.Name}");
			this.RawTexture = texture;
			
			this.Sampler = sampler;

			this.Texture = this.RawTexture.CreateDeviceTexture(renderState.GraphicsDevice, renderState.ResourceFactory);

			this.ResourceSet = renderState.ResourceFactory.CreateResourceSet(
				new ResourceSetDescription(
					ResourceLayout,
					this.Texture,
					sampler)
			);
			Logger.Log($"texture creation took {Profiler.EndCapture($"texture_create{Thread.CurrentThread.Name}").Length} ms!");
		}
		
		public void Dispose() {
			this.Texture?.Dispose();
			this.ResourceSet?.Dispose();
			for (int i = 0; i < this.ResourceSets.Length; i++) {
				this.ResourceSets[i]?.Dispose();
			}
			this.Sampler?.Dispose();
		}

		~Texture2D() {
			this.Dispose();
		}
	}
}
