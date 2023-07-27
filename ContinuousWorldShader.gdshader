shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

float getCellContent(vec4 cc) {
	return max(cc.r, max(cc.g, cc.b)) > 0. ? 1.0 : 0.0;
}

int ca_count(in sampler2D tex, vec2 uv, vec2 pixel_size) {
	float modifier;
	float neighbours = 0.;

	for (float x = -1.; x < 2.; x++) {
		for (float y = -1.; y < 2.; y++) {
		    // Removes self from consideration
			modifier = min(1., abs(x) + abs(y));

			// Add 1 for each living neighbor
			neighbours += getCellContent(textureLod(tex, uv + vec2(x, y) * pixel_size, 0.0)) * modifier;
		}
	}

	return int(neighbours);
}

// By using HSL we just have to rotate the H (Hue) value to span the color spectrum
// Function from https://www.shadertoy.com/view/XljGzV
vec3 hsl2rgb( in vec3 c )
{
	vec3 rgb = clamp( abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0 );

	return c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
}

vec4 getColor(vec4 cc, int count, float time) {
	// If count is 2 return current state
	if (count == 2) {
		return cc;
	}
	
	// If count is 3 then either stay alive or spontaneously come to life
	else if (count == 3) {
		return vec4(hsl2rgb(vec3(time * 0.05, 0.5, 0.5)), 1.0);
	}
	
	// Others die off from over- or under-population
	return vec4(0.0, 0.0, 0.0, 1.0);
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;
	
	int count = ca_count(screen_texture, uv, sz);
	
	COLOR = getColor(textureLod(screen_texture, uv, 0.0), count, TIME);
}
