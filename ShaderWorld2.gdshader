shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform mat4 global_transform;

const float vBarrier = 250.0;

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

//	float tl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, -s.y)));
//	float cl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, 0)));
//	float bl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, s.y)));
//
//	float tc = getCellContent(texture(TEXTURE, UV + vec2(0, -s.y)));
//	float cc = getCellContent(texture(TEXTURE, UV + vec2(0, 0)));
//	float bc = getCellContent(texture(TEXTURE, UV + vec2(0, s.y)));
//
//	float tr = getCellContent(texture(TEXTURE, UV + vec2(s.x, -s.y)));
//	float cr = getCellContent(texture(TEXTURE, UV + vec2(s.x, 0)));
//	float br = getCellContent(texture(TEXTURE, UV + vec2(s.x, s.y)));

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
//	worldPosition = (MODEL_MATRIX * vec4(VERTEX, 1.0, 1.0)).xy;
	worldPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;
	
	int count;

	if (worldPosition.y >= 45.0 && worldPosition.y < 55.0) {
		count = barrier_count(screen_texture, uv, sz);
	} else {
		count = ca_count(screen_texture, uv, sz);
	}

	
	COLOR = getColor(textureLod(screen_texture, uv, 0.0), count, TIME);
	
//	if (worldPosition.x <= vBarrier && worldPosition.x >= vBarrier + 1.0) {
//		COLOR = vec4(1.0);
//	}
//
//	if (worldPosition.y > 50.0 && worldPosition.y < 51.0) {
//		COLOR = vec4(1.0);
//	}

//	COLOR = vec4(worldPosition.x, worldPosition.y, 1.0, 1.0);
	
//	if (floor(uv.x * 1000.0) == floor(vBarrier * TEXTURE_PIXEL_SIZE.x * 1000.0)) {
//		COLOR = vec4(1.0);
//	}

//	float upperBound = vBarrier * TEXTURE_PIXEL_SIZE.y + (0.5 * TEXTURE_PIXEL_SIZE.y);
//	float lowerBound = vBarrier * TEXTURE_PIXEL_SIZE.y - (0.5 * TEXTURE_PIXEL_SIZE.y);
	
//	float upperBound = vBarrier * sz.y + (sz.y * 0.5);
//	float lowerBound = vBarrier * sz.y;
//
//	if (uv.x <= upperBound && uv.x >= lowerBound) {
//		COLOR = vec4(1.0);
//	}

//	COLOR = vec4(uv.y, 1.0, 1.0, 1.0);
	
}
