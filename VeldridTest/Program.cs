using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using FontStashSharp;
using Gdk;
using Kettu;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using VeldridTest.FontStashSharp;
using Color = System.Drawing.Color;
using Key = Veldrid.Key;

namespace VeldridTest {
	internal class Program {
		public static RenderState RenderState = new();

		public static List<Drawable> DrawableObjects;

		private static DeviceBuffer _ProjectionBuffer;

		private static Stopwatch _Stopwatch;
		
		private static VeldridFontStashRenderer _TextRenderer;
		private static FontSystem               _TestFontSystem;
		private static DynamicSpriteFont        _TestFont;

		private static bool _captureWanted;

		private static void Main(string[] args) {
			Logger.StartLogging();
			Logger.AddLogger(new ConsoleLogger());

			//Set the options for the window on creation
			WindowCreateInfo windowOptions = new() {
				X            = 100,
				Y            = 100,
				WindowWidth  = 960,
				WindowHeight = 540,
				WindowTitle  = "Veldrid Tutorial"
			};

			//Create a SDL2 window
			RenderState.Window = VeldridStartup.CreateWindow(ref windowOptions);

			//Set the options for the graphics device
			GraphicsDeviceOptions graphicsDeviceOptions = new() {
				PreferStandardClipSpaceYDirection = true,
				PreferDepthRangeZeroToOne         = true
			};

			// graphicsDeviceOptions.Debug               = true;
			graphicsDeviceOptions.SyncToVerticalBlank = false;
			
			//Create a graphics device that we will render to
			RenderState.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(RenderState.Window, graphicsDeviceOptions);

			//Log the backend used
			Logger.Log($"Window created with the {RenderState.GraphicsDevice.BackendType.ToString()} backend!");

			_Stopwatch = new();
			_Stopwatch.Start();
			
			Profiler.Initialize();
			
			Profiler.StartCapture("createResources");
			//Create the resources used
			CreateResources();
			Logger.Log($"Creating resources took {Profiler.EndCapture("createResources")!.Length}ms!");
 
			float xtest = 0;
			
			//Basic KeyDown event
			RenderState.Window.KeyDown += delegate(KeyEvent @event) {
				if (@event.Key == Key.Enter) {
					_captureWanted = true;
					return;
				}
				
				Texture2D texture = TextureLoader.LoadTexture("obsoletethisgrab.png", RenderState.GraphicsDevice.Aniso4xSampler, RenderState);
				if (@event.Key == Key.A) {
					
					texture = TextureLoader.LoadTexture("2.png", RenderState.GraphicsDevice.Aniso4xSampler, RenderState);
				}
				
				Logger.Log(@event.Key.ToString());


				DrawableObjects.Add(new DrawableTexture(new Vector2(200 + xtest, 200), texture) {
					Scale = new(0.1f)
				});
				
				xtest += 100;
			};

			double lastFrameTime = 0;
			
			//Game loop
			while (RenderState.Window.Exists) {
				if (lastFrameTime != 0)
					lastFrameTime = Profiler.EndCapture("fps")!.Length;
				else
					lastFrameTime = 1;
				Profiler.StartCapture("fps");
				
				
				Update();
				
				if (!RenderState.Window.Exists)
					continue;
				
				RenderState.Window.PumpEvents();
				
				if(_captureWanted)
					Profiler.StartCapture("draw");
				
				Draw(lastFrameTime);
					
				if (_captureWanted) {
					Logger.Log($"draw took {Profiler.EndCapture("draw")!.Length} ms!");
					_captureWanted = false;
				}
			}
			
			//Dispose resources
			DisposeResources();
		}

		private static void UpdateProjectionBuffer() {
			RenderState.GraphicsDevice.UpdateBuffer(_ProjectionBuffer, 0, Matrix4x4.CreateOrthographicOffCenter(0, RenderState.Window.Width, 0, RenderState.Window.Height, -1f, 1f));
			RenderState.GraphicsDevice.ResizeMainWindow((uint)RenderState.Window.Width, (uint)RenderState.Window.Height);
		}

		public static void DisposeResources() {
			RenderState.TexturedPipeline.Dispose();
			RenderState.Shaders[0].Dispose();
			RenderState.Shaders[1].Dispose();
			RenderState.CommandList.Dispose();
			// _VertexBuffer.Dispose();
			// _IndexBuffer.Dispose();
			RenderState.GraphicsDevice.Dispose();
		}

		public static void CreateResources() {
			RenderState.GraphicsDevice.SyncToVerticalBlank = false;

			DrawableObjects = new();
			
			//Get the graphics factory from the graphics device, this is used to create graphic resources 
			RenderState.ResourceFactory = RenderState.GraphicsDevice.ResourceFactory;
			
			Texture2D.ResourceLayout = RenderState.ResourceFactory.CreateResourceLayout(new(new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
																							new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
			// Texture2D texture = new(new("obsoletethisgrab.png"), RenderState.GraphicsDevice.Aniso4xSampler, RenderState);
			
			_ProjectionBuffer = RenderState.ResourceFactory.CreateBuffer(new((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer));

			RenderState.Window.Resized += UpdateProjectionBuffer;

			ResourceLayoutDescription projectionBufferLayoutDescription = new(
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
			
			ResourceLayout projectionBufferResourceLayout = RenderState.ResourceFactory.CreateResourceLayout(projectionBufferLayoutDescription);

			RenderState.ProjectionBufferResourceSet = RenderState.ResourceFactory.CreateResourceSet(new ResourceSetDescription(projectionBufferResourceLayout, _ProjectionBuffer));

			//Defines how the vertex struct is layed out 
			VertexLayoutDescription vertexLayout = new(
				new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
				new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4),
				new VertexElementDescription("TexturePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

			//Generates the vertex and fragment shaders
			ShaderDescription vertexShaderDesc = new(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(ReadStringFromEmbeddedResource("TexturedVertexShader.glsl")),
				"main");
			ShaderDescription fragmentShaderDesc = new(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(ReadStringFromEmbeddedResource("TexturedFragmentShader.glsl")),
				"main");

			//Creates the shaders
			RenderState.Shaders = RenderState.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

			//Create a new pipeline description 
			GraphicsPipelineDescription pipelineDescription = new() {
				BlendState        = BlendStateDescription.SingleAlphaBlend,
				//Set the stencil state 
				DepthStencilState = new DepthStencilStateDescription(
					true,
					true,
					ComparisonKind.LessEqual),
				//Set the rasterizer state
				RasterizerState = new RasterizerStateDescription(
					FaceCullMode.None,
					PolygonFillMode.Solid,
					FrontFace.Clockwise,
					true,
					false),
				//Sets the primitive topology to a list of triangles
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				ResourceLayouts   = new []{ projectionBufferResourceLayout, Texture2D.ResourceLayout },
				//Sets the shaders of the pipeline
				ShaderSet         = new ShaderSetDescription(
					new[] { vertexLayout },
					RenderState.Shaders),
				//Set the thing to render to to the screen
				Outputs = RenderState.GraphicsDevice.SwapchainFramebuffer.OutputDescription
			};

			RenderState.TexturedPipeline = RenderState.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

			RenderState.CommandList = RenderState.ResourceFactory.CreateCommandList();
			
			UpdateProjectionBuffer();
			
			_TextRenderer = new VeldridFontStashRenderer(RenderState);

			_TestFontSystem = new(new FontSystemSettings());
			_TestFontSystem.AddFont(File.ReadAllBytes("default-font.ttf"));

			_TestFont = _TestFontSystem.GetFont(100);
		}

		public static void Update() {
			// DrawableObjects.ForEach(x => x.Position.X = ((Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency) % 1) * 1000);
		}

		public static void Draw(double lastFrameTime) {
			Renderer.Begin(RenderState);

			Renderer.ClearColor(RgbaFloat.Black);

			foreach (Drawable drawable in DrawableObjects) {
				drawable.Draw(RenderState);
			}
			
			// _TestFont.DrawText(_TextRenderer, (1000d / lastFrameTime).ToString(), new(10), Color.White);

			Renderer.End();
		}

		public static string ReadStringFromEmbeddedResource(string name) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			
			name = $"VeldridTest.{name}";
			
			using (Stream stream = assembly.GetManifestResourceStream(name))
				using (StreamReader reader = new(stream))
					return reader.ReadToEnd();
		}
	}
}
