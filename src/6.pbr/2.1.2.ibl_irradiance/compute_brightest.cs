#version 430 core

layout(local_size_x = 16, local_size_y = 16) in;

layout(binding = 0) uniform samplerCube environmentMap;
layout(std430, binding = 0) buffer BrightestSpotData 
{
    vec4 brightestSpot; // RGB is the color, A is the intensity
    vec3 direction; // Direction of the brightest spot
};

shared float sharedMaxIntensity;
shared vec4 sharedColor;
shared vec3 sharedDir;

vec3 computeDirection(int face, ivec2 id, ivec2 size) 
{
    vec2 uv = (vec2(id) + 0.5) / vec2(size);
    uv = uv * 2.0 - 1.0;
    vec3 dir;
    if (face == 0) // +X
    { 
        dir = vec3(1.0, -uv.y, -uv.x);
    }
    else if (face == 1) // -X
    { 
        dir = vec3(-1.0, -uv.y, uv.x);
    } 
    else if (face == 2) // +Y
    { 
        dir = vec3(uv.x, 1.0, uv.y);
    } 
    else if (face == 3) // -Y
    { 
        dir = vec3(uv.x, -1.0, -uv.y);
    } 
    else if (face == 4) // +Z
    { 
        dir = vec3(uv.x, -uv.y, 1.0);
    } 
    else if (face == 5) // -Z
    { 
        dir = vec3(-uv.x, -uv.y, -1.0);
    }
    return normalize(dir);
}

void main() 
{
    if (gl_LocalInvocationIndex == 0) 
    {
        sharedMaxIntensity = 0.0;
    }
    barrier();

    int face = int(gl_GlobalInvocationID.z); 
    ivec2 id = ivec2(gl_GlobalInvocationID.xy);

    vec3 dir = computeDirection(face, id, ivec2(512, 512));

    vec4 color = texture(environmentMap, dir);

    // Using luminance coefficients
    float intensity = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722)); 

    if (intensity > sharedMaxIntensity) 
    {
        sharedMaxIntensity = intensity;
        sharedColor = color;
        sharedDir = dir;
    }
    barrier();

    if (gl_LocalInvocationIndex == 0 && sharedMaxIntensity > brightestSpot.a) 
    {
        brightestSpot = sharedColor;
        brightestSpot.a = sharedMaxIntensity;
        direction = sharedDir;
    }
}