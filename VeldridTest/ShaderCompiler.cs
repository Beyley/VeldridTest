using System;
using System.Text;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.SPIRV;

namespace VeldridTest {
	public static class ShaderCompiler {
		public static Shader[] CompileShaderPair(RenderState renderState, ShaderDescription vertex, ShaderDescription fragment) {
			try {
				return renderState.ResourceFactory.CreateFromSpirv(vertex, fragment);
			}
			catch(SpirvCompilationException ex) {
				int lineNumber = -1;
				
				MatchCollection mc = Regex.Matches(ex.Message, @":\d+:");

				if (mc.Count != 0) {
					lineNumber = int.Parse(mc[0].Value.Trim(':'));
				}
				
				StringBuilder message = new();
				
				
				
				throw new Exception(message.ToString());
			}
		}
	}
}
