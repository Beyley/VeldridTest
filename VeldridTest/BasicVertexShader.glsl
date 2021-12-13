#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
	mat4 projectionMatrix;
};

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = projectionMatrix * vec4(Position, 0, 1);

	gl_Position.y *= -1;

    fsin_Color = Color;
}