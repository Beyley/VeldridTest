using Veldrid;
using Veldrid.Sdl2;

namespace VeldridTest {
	public class RenderState {
		public GraphicsDevice GraphicsDevice;
		public CommandList    CommandList;
		
		public Shader[]   Shaders;
		public Pipeline   Pipeline;
		public Sdl2Window Window;
	}
}
