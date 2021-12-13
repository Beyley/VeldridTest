using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Kettu;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ShaderGen;
using ShaderGen.Glsl;
using SharpText.Core;
using SharpText.Veldrid;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace VeldridTest {
	internal class Program {
		private static RenderState RenderState = new();

		public static List<DrawableObject> DrawableObjects;

		private static DeviceBuffer _ProjectionBuffer;
		private static ResourceSet  _GlobalResourceSet;
		private static ResourceSet  _TextureResourceSet;

		private static Stopwatch _Stopwatch;

		public static Texture TestTexture;
		public static TextureView TestTextureView;

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

			//Create a graphics device that we will render to
			RenderState.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(RenderState.Window, graphicsDeviceOptions, GraphicsBackend.Vulkan);

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
				Logger.Log(@event.Key.ToString());

				DrawableObjects.Add(new PrimitiveQuadDrawable(new Vector2(200 + xtest, 200), new(400), RenderState));
				
				xtest += 300;
			};

			//Game loop
			while (RenderState.Window.Exists) {
				RenderState.Window.PumpEvents();
				Update();
				Draw();
			}
			
			//Dispose resources
			DisposeResources();
		}

		private static void UpdateProjectionBuffer() {
			RenderState.GraphicsDevice.UpdateBuffer(_ProjectionBuffer, 0, Matrix4x4.CreateOrthographicOffCenter(0, RenderState.Window.Width, 0, RenderState.Window.Height, -1f, 1f));
			RenderState.GraphicsDevice.ResizeMainWindow((uint)RenderState.Window.Width, (uint)RenderState.Window.Height);
		}

		public static void DisposeResources() {
			RenderState.Pipeline.Dispose();
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
			ResourceFactory factory = RenderState.GraphicsDevice.ResourceFactory; 

			ImageSharpTexture textureRaw = new("obsoletethisgrab.png");
			
			TestTexture     = textureRaw.CreateDeviceTexture(RenderState.GraphicsDevice, factory);
			TestTextureView = factory.CreateTextureView(TestTexture);
			
			_ProjectionBuffer = factory.CreateBuffer(new((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer));

			RenderState.Window.Resized += UpdateProjectionBuffer;

			ResourceLayoutDescription resourceLayoutDescription = new(
				new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
			
			ResourceLayout globalResourceSetLayout = factory.CreateResourceLayout(resourceLayoutDescription);

			ResourceLayout textureLayout = factory.CreateResourceLayout(new(new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
																			new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

			_GlobalResourceSet = factory.CreateResourceSet(new ResourceSetDescription(globalResourceSetLayout, _ProjectionBuffer));
			_TextureResourceSet = factory.CreateResourceSet(
				new ResourceSetDescription(
					textureLayout, 
					TestTextureView, 
					RenderState.GraphicsDevice.Aniso4xSampler)
				);

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
			RenderState.Shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

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
					FaceCullMode.Front,
					PolygonFillMode.Solid,
					FrontFace.Clockwise,
					true,
					false),
				//Sets the primitive topology to a list of triangles
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				ResourceLayouts   = new []{ globalResourceSetLayout, textureLayout },
				//Sets the shaders of the pipeline
				ShaderSet         = new ShaderSetDescription(
					new[] { vertexLayout },
					RenderState.Shaders),
				//Set the thing to render to to the screen
				Outputs = RenderState.GraphicsDevice.SwapchainFramebuffer.OutputDescription
			};

			RenderState.Pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

			RenderState.CommandList = factory.CreateCommandList();
			
			UpdateProjectionBuffer();

			// Font font = new("default-font.ttf", 20);

			// _TextRenderer = new VeldridTextRenderer(_GraphicsDevice, _CommandList, font);

			// _TextRenderer.DrawText("The quick brown fox jumps over the lazy dog", new Vector2(5, 5), new Color(255, 255, 255, 1), 2);
		}

		public static void Update() {
			
		}

		public static void Draw() {
			RenderState.CommandList.Begin();
			
			//Set the framebuffer to render to
			RenderState.CommandList.SetFramebuffer(RenderState.GraphicsDevice.SwapchainFramebuffer);

			//Clear the screen to black
			RenderState.CommandList.ClearColorTarget(0, RgbaFloat.Black);

			RenderState.CommandList.SetFullViewports();
			
			//Set the pipeline
			RenderState.CommandList.SetPipeline(RenderState.Pipeline);
			//Set the graphics resource set
			RenderState.CommandList.SetGraphicsResourceSet(0, _GlobalResourceSet);
			RenderState.CommandList.SetGraphicsResourceSet(1, _TextureResourceSet);

			foreach (DrawableObject drawable in DrawableObjects) {
				drawable.Draw(RenderState);
			}
			
			// _TextRenderer.Draw();
			
			RenderState.CommandList.End();

			//Submit commands to the GPU
			RenderState.GraphicsDevice.SubmitCommands(RenderState.CommandList);
			
			// ?
			RenderState.GraphicsDevice.SwapBuffers();
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
