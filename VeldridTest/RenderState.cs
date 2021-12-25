using Veldrid;
using Veldrid.Sdl2;

namespace VeldridTest {
	public class RenderState {
		public GraphicsDevice  GraphicsDevice;
		public CommandList     CommandList;
		public ResourceFactory ResourceFactory;
		
		public Shader[]   Shaders;
		public Pipeline   TexturedPipeline;
		public Sdl2Window Window;
		
		public ResourceSet ProjectionBufferResourceSet;
	}
}
