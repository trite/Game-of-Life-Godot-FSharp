shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

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
//      neighbours += getCellContent(texture(tex, uv + vec2(x, y) * pixel_size)) * modifier;
	  neighbours += getCellContent(textureLod(tex, uv + vec2(x, y) * pixel_size, 0.0)) * modifier;
    }
  }

  return int(neighbours);
}

vec4 getColor(vec4 cc, int count, float time) {
  float wasCellAlive = getCellContent(cc); 
  
  // When cell is alive & has either 2 or 3 neighbours
  // it will survive until the next iteration
  if (wasCellAlive > 0. && (count == 2 || count == 3)) {
    return cc;
  }

  // If the cell is dead but has 3 neighbours
  // it will spring into life
  else if (count == 3) {
    return vec4(sin(time), cos(time), 1.0, 1.0);
  }

  // For all other cases the cells are dead
  return vec4(0.0, 0.0, 0.0, 1.0);
}

void fragment() {
//	vec3 c = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;

//	vec3 newc = vec3(getNext(c.r), getNext(c.g), getNext(c.b));
	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;

	int count = ca_count(screen_texture, uv, sz);
	
//	COLOR = getColor(texture(TEXTURE, uv), count, TIME);
	COLOR = getColor(textureLod(screen_texture, uv, 0.0), count, TIME);
	
	// Place fragment code here.
	
//	COLOR = texture(TEXTURE, UV);
//	float grey = .21 * COLOR.r + .71 * COLOR.g + .07 * COLOR.b;
//    COLOR.rgb = vec3(grey); // Change the color value

//	COLOR.rgb = vec3(0.5, 0.8, 0.8);
}
