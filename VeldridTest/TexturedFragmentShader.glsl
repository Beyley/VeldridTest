#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_texCoords;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords) * fsin_Color;
}