shader_type canvas_item;

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
      neighbours += getCellContent(texture(tex, uv + vec2(x, y) * pixel_size)) * modifier;
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
//	vec2 s = TEXTURE_PIXEL_SIZE;

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
//
//	int count = int(tl) + int(cl) + int(bl) + int(tc) + int(bc) + int(tr) + int(cr) + int(br);

//	// Stay
//	vec4 output = vec4(cc, cc, cc, 1.0);
//
//	// Death
//	if (count < 2 || count > 3)
//		output = vec4(cc - 0.1, cc - 0.1, cc - 0.1, 1); // Some fading
//
//	// Life
//	if (count == 3)
//		output = vec4(1, 1, 1, 1);
//
//	COLOR = output;


	vec2 uv = SCREEN_UV;
	vec2 sz = SCREEN_PIXEL_SIZE;

	int count = ca_count(TEXTURE, uv, sz);
	
	COLOR = getColor(texture(TEXTURE, uv), count, TIME);
}

