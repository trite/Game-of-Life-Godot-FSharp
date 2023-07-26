shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform mat4 global_transform;

uniform vec2 inside_top_left;
uniform vec2 inside_bottom_right;

uniform vec2 ball_center;
uniform float ball_radius;

uniform bool ball_enabled;

const float vBarrier = 100.0;
const float vBarrierWidthHalf = 25.0;

// This has to have a fixed size, for now I'm just setting a
//   decently large value (50) and only populating the array with
//   values when they exist. It seems to work so far :fingers-crossed:
uniform vec3[50] ball_positions_and_sizes;

highp float rand(vec2 co)
{
//  highp float a = 12.9898;
//  highp float b = 78.233;
//  highp float c = 43758.5453;
  highp float a = 15.353;
  highp float b = 59.4932;
  highp float c = 1736.295;
  highp float dt= dot(co.xy ,vec2(a,b));
  highp float sn= mod(dt,3.14);
  return fract(sin(sn) * c);
}

float getCellContent(vec4 cc) {
	return max(cc.r, max(cc.g, cc.b)) > 0. ? 1.0 : 0.0;
}

int ca_count(in sampler2D tex, vec2 uv, vec2 pixel_size) {
	vec2 s = pixel_size;

	float modifier;
	float neighbours = 0.;

	for (float x = -1.; x < 2.; x++) {
		for (float y = -1.; y < 2.; y++) {
		    // We don't want to add our current square
		    // modifier will equal zero only when we have x=0 & y=0
			modifier = min(1., abs(x) + abs(y));

			// Add any neighbours x value (we could also use y or z)
			neighbours += getCellContent(textureLod(tex, uv + vec2(x, y) * pixel_size, 0.0)) * modifier;
		}
	}

	return int(neighbours);
}

int barrier_count(in sampler2D tex, vec2 uv, vec2 pixel_size) {
	vec2 s = pixel_size;

	float modifier;
	float neighbours = 0.;

	float above = 0.0;
	float sides = 0.0;
	float below = 0.0;
  
	for (float x = -1.; x < 2.; x++) {
		for (float y = -1.; y < 2.; y++) {
			// We don't want to add our current square
			// modifier will equal zero only when we have x=0 & y=0
			modifier = min(1., abs(x) + abs(y));

			// Add any neighbours x value (we could also use y or z)
			float result = getCellContent(textureLod(tex, uv + vec2(x, y) * pixel_size, 0.0)) * modifier;
			neighbours += result;
			
			if (y > 0.5) {
				below += result;
			} else if (y > -0.5) {
				sides += result;
			} else {
				above += result;
			}
		}
	}
	
	// Logic for horizontal barrier that only allows travel downward
	if (below == 3.0) {
		return int(0.0);
	} else if (below == 1.0 && above == 2.0) {
		return int(0.0);
	}

	return int(neighbours);
}

// by anastadunbar - source: https://www.shadertoy.com/view/XljGzV
vec3 hsl2rgb( in vec3 c )
{
	vec3 rgb = clamp( abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0 );

	return c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
}

vec4 getColor(vec4 cc, int count, float time) {
	float wasCellAlive = getCellContent(cc); 
	
	// When cell is alive & has either 2 or 3 neighbours
	// it will survive until the next iteration
	if (wasCellAlive > 0. && (count == 2 || count == 3)) {
		return cc;
	}
	
	else if (count == 3) {
		return vec4(hsl2rgb(vec3(time * 0.05, 0.5, 0.5)), 1.0);
	}
	
	// For all other cases the cells are dead
	return vec4(0.0, 0.0, 0.0, 1.0);
}

varying vec2 worldPosition;

void vertex() {
	// Get world position to allow calculating position in pixels
	worldPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;
	

	
	int count;
	
	// Create a 10 pixel horizontal band across the screen that restricts travel in one direction
	if (worldPosition.y >= vBarrier - vBarrierWidthHalf && worldPosition.y < vBarrier + vBarrierWidthHalf) {
		count = barrier_count(screen_texture, uv, sz);
	} else {
		count = ca_count(screen_texture, uv, sz);
	}
	
//	if (worldPosition.x >= inside_top_left.x
//		&& worldPosition.y >= inside_top_left.y
//		&& worldPosition.x < inside_bottom_right.x
//		&& worldPosition.y < inside_bottom_right.y)
//	{
//		// Logic that applies to the inner zone would live here, but that's not what I needed -_-
//	}
	
	COLOR = getColor(textureLod(screen_texture, uv, 0.0), count, TIME);
	
	if (ball_enabled) {
		for (int i = 0; i < ball_positions_and_sizes.length(); i++) {
			vec2 distance_to_ball_center = (uv / sz) - ball_positions_and_sizes[i].xy;
			float current_ball_radius = ball_positions_and_sizes[i].z;
			
			if (length(distance_to_ball_center) <= current_ball_radius) {
				float col = rand(distance_to_ball_center * uv * 2.);
			
				float col_del = smoothstep(0.1, 0.11, col);
				if (col_del == 0.0) {
					COLOR = vec4(vec3(0.0), 1.0);
				}

				// Turning random cells on at a threshhold
				float col_add = smoothstep(0.94, 0.95, col);
				if (col_add == 1.0) {
					COLOR = vec4(vec3(1.0), 1.0);
				}
			}
		}
//		vec2 distance_to_ball_center = (uv / sz) - ball_center;
//
//		if (length(distance_to_ball_center) <= ball_radius) {
//	//		COLOR = vec4(vec3(0.0), 1.0);
//			float col = rand(distance_to_ball_center * uv * 2.);
//
//			// Turning random cells off at a threshhold
//			float col_del = smoothstep(0.1, 0.11, col);
//			if (col_del == 0.0) {
//				COLOR = vec4(vec3(0.0), 1.0);
//			}
//
//			// Turning random cells on at a threshhold
//			float col_add = smoothstep(0.94, 0.95, col);
//			if (col_add == 1.0) {
//				COLOR = vec4(vec3(1.0), 1.0);
//			}
//		}
	}
}
