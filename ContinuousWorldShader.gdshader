shader_type canvas_item;

// float col_add = smoothstep(0.94, 0.95, col);

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float kernel[49];
uniform int kernel_size;
uniform float kernel_sum;

uniform float aliveness_func_curve_peak;
uniform float aliveness_func_curve_center;
uniform float aliveness_func_curve_stdev;

// By using HSL we just have to rotate the H (Hue) value to span the color spectrum
// Function from https://www.shadertoy.com/view/XljGzV
vec3 hsl2rgb( in vec3 c )
{
	vec3 rgb = clamp( abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0 );

	return c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
}

// Function from https://www.shadertoy.com/view/XljGzV
vec3 rgb2hsl( in vec3 c ){
  float h = 0.0;
	float s = 0.0;
	float l = 0.0;
	float r = c.r;
	float g = c.g;
	float b = c.b;
	float cMin = min( r, min( g, b ) );
	float cMax = max( r, max( g, b ) );

	l = ( cMax + cMin ) / 2.0;
	if ( cMax > cMin ) {
		float cDelta = cMax - cMin;
        
		s = l < .0 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) );
        
		if ( r == cMax ) {
			h = ( g - b ) / cDelta;
		} else if ( g == cMax ) {
			h = 2.0 + ( b - r ) / cDelta;
		} else {
			h = 4.0 + ( r - g ) / cDelta;
		}

		if ( h < 0.0) {
			h += 6.0;
		}
		h = h / 6.0;
	}
	return vec3( h, s, l );
}



// Get lightness of a cell
float getCellLightness(vec4 cell) {
	return rgb2hsl(cell.rgb).b;
}

float kernelVal(int x, int y) {
    int index = y * kernel_size + x;
    return kernel[index];
}

float neighborCellLifeAvg(in sampler2D tex, vec2 uv, vec2 pixel_size) {
	int limit = int(kernel_size / 2);
	
	float result = 0.;
	
	for (int x = -limit; x <= limit; x++) {
		for (int y = -limit; y <= limit; y++) {
			result += getCellLightness(textureLod(
				tex, uv + vec2(float(x), float(y)) * pixel_size, 0.0))
				* kernelVal(x, y);
		}
	}
	
	return clamp(result / kernel_sum, 0.0, 1.0);
}


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

//uniform float aliveness_func_curve_peak;
//uniform float aliveness_func_curve_center;
//uniform float aliveness_func_curve_stdev;

//float gaussianFunc(float x) {
//	// Convert FWHM to std dev
//	float sigma = aliveness_func_curve_stdev / (2.0 * sqrt(2.0 * log(2.0)));
//
//	// Compute Gaussian
//	float gaussian = exp(-0.5  * pow((x - aliveness_func_curve_center) / sigma, 2.0));
//
//	// Normalize and return
//	return gaussian / aliveness_func_curve_peak / (sigma * sqrt(2.0 * PI));
//
////	return pow(1.0, -(pow(x - 0.375, 2.0) / (2.0 * pow(0.0625, 2.0))));
//}

vec4 getColorCont(vec4 cc, float nearbyPopulation, float time) {
	// L in HSL corresponds to nearbyPopulation value
//	float l = gaussianFunc(nearbyPopulation);

	if (nearbyPopulation >= 0.4 && nearbyPopulation < 0.5) {
		return vec4(hsl2rgb(vec3(time * 0.05, 0.75, 1.0)), 1.0);
	} else if (nearbyPopulation >= 0.05 && nearbyPopulation < 0.95) {
		return cc;
	}
	
	return vec4(0.0, 0.0, 0.0, 1.0);
	
	
//	// H is based on either time or last color depending on l
//	if (l > 0.75) {
//		return vec4(hsl2rgb(vec3(time * 0.05, 0.75, l)), 1.0);
//	} else {
//		vec3 hsl = rgb2hsl(cc.rgb);
//		return vec4(hsl2rgb(vec3(hsl.r, hsl.g, l)), 1.0);
//	}
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;
	
//	// Normal Conway's Game of Life
//	int count = ca_count(screen_texture, uv, sz);
//	COLOR = getColor(textureLod(screen_texture, uv, 0.0), count, TIME);
	
	float nearbyPopulation = neighborCellLifeAvg(screen_texture, uv, sz);
	COLOR = getColorCont(textureLod(screen_texture, uv, 0.0), nearbyPopulation, TIME);
//    TODO: pick up here by making a replacement for `getColor` that works off of floats
	
}
