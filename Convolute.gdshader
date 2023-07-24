shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Kernels
#define identity mat3(vec3(0, 0, 0), vec3(0, 1, 0), vec3(0, 0, 0))
#define edge0 mat3(vec3(1, 0, -1), vec3(0, 0, 0), vec3(-1, 0, 1))
#define edge1 mat3(vec3(0, 1, 0), vec3(1, -4, 1), vec3(0, 1, 0))
#define edge2 mat3(vec3(-1, -1, -1), vec3(-1, 8, -1), vec3(-1, -1, -1))
#define sharpen mat3(vec3(0, -1, 0), vec3(-1, 5, -1), vec3(0, -1, 0))
#define box_blur mat3(vec3(1, 1, 1), vec3(1, 1, 1), vec3(1, 1, 1)) * 0.1111
#define gaussian_blur mat3(vec3(1, 2, 1), vec3(2, 4, 2), vec3(1, 2, 1)) * 0.0625
#define emboss mat3(vec3(-2, -1, 0), vec3(-1, 1, 1), vec3(0, 1, 2))

#define brighten_blur mat3(vec3(1, 1, 1)) * 10.0

// Iterate over all pixels in 3x3 kernel area
vec4 convolute(vec2 uv, mat3 kernel, vec2 iResolution)
{
    vec4 color = vec4(0);
    
    const float direction[3] = float[3](-1.0, 0.0, 1.0);    
    for (int x = 0; x < 3; x++)
    {
        for (int y = 0; y < 3; y++)
        {
            vec2 offset = vec2(direction[x], direction[y]) / iResolution.xy;
            color += texture(screen_texture, uv+offset) * kernel[x][y];
        }
    }
    return color;
}

void fragment()
{
	vec2 fragCoord = FRAGCOORD.xy;
	vec2 iResolution = 1.0 / SCREEN_PIXEL_SIZE;
	
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = fragCoord / iResolution.xy;
	
	COLOR = convolute(uv, gaussian_blur, iResolution);

//    if (uv.x > iMouse.x / iResolution.x)
//    {
//        // Pixels to the right of cursor remain untouched
//        fragColor = texture(iChannel0, uv);
//    }
//    else
//    {
//        // Pixels to the left of cursor get convolution kernel applied to them
//        fragColor = convolute(uv, emboss, iResolution);
//    }
}
