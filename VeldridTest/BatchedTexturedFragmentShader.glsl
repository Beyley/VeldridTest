#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_texCoords;
layout(location = 2) flat in int fsin_texIndex;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D SurfaceTexture_0;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler_0;
layout(set = 2, binding = 0) uniform texture2D SurfaceTexture_1;
layout(set = 2, binding = 1) uniform sampler SurfaceSampler_1;
layout(set = 3, binding = 0) uniform texture2D SurfaceTexture_2;
layout(set = 3, binding = 1) uniform sampler SurfaceSampler_2;
layout(set = 4, binding = 0) uniform texture2D SurfaceTexture_3;
layout(set = 4, binding = 1) uniform sampler SurfaceSampler_3;
layout(set = 5, binding = 0) uniform texture2D SurfaceTexture_4;
layout(set = 5, binding = 1) uniform sampler SurfaceSampler_4;
layout(set = 6, binding = 0) uniform texture2D SurfaceTexture_5;
layout(set = 6, binding = 1) uniform sampler SurfaceSampler_5;
layout(set = 7, binding = 0) uniform texture2D SurfaceTexture_6;
layout(set = 7, binding = 1) uniform sampler SurfaceSampler_6;
layout(set = 8, binding = 0) uniform texture2D SurfaceTexture_7;
layout(set = 8, binding = 1) uniform sampler SurfaceSampler_7;

void main()
{
    int index = int(fsin_texIndex);

    switch (index) {
        case 0:
        fsout_Color = texture(sampler2D(SurfaceTexture_0, SurfaceSampler_0), fsin_texCoords) * fsin_Color;
        break;
        case 1:
        fsout_Color = texture(sampler2D(SurfaceTexture_1, SurfaceSampler_1), fsin_texCoords) * fsin_Color;
        break;
        case 2:
        fsout_Color = texture(sampler2D(SurfaceTexture_2, SurfaceSampler_2), fsin_texCoords) * fsin_Color;
        break;
        case 3:
        fsout_Color = texture(sampler2D(SurfaceTexture_3, SurfaceSampler_3), fsin_texCoords) * fsin_Color;
        break;
        case 4:
        fsout_Color = texture(sampler2D(SurfaceTexture_4, SurfaceSampler_4), fsin_texCoords) * fsin_Color;
        break;
        case 5:
        fsout_Color = texture(sampler2D(SurfaceTexture_5, SurfaceSampler_5), fsin_texCoords) * fsin_Color;
        break;
        case 6:
        fsout_Color = texture(sampler2D(SurfaceTexture_6, SurfaceSampler_6), fsin_texCoords) * fsin_Color;
        break;
        case 7:
        fsout_Color = texture(sampler2D(SurfaceTexture_7, SurfaceSampler_7), fsin_texCoords) * fsin_Color;
        break;
    }
}