shader_type canvas_item;

float getCellContent(vec4 cc) {
  return max(cc.r, max(cc.g, cc.b)) > 0. ? 1.0 : 0.0;
}

void fragment() {
	vec2 s = TEXTURE_PIXEL_SIZE;

	float tl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, -s.y)));
	float cl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, 0)));
	float bl = getCellContent(texture(TEXTURE, UV + vec2(-s.x, s.y)));

	float tc = getCellContent(texture(TEXTURE, UV + vec2(0, -s.y)));
	float cc = getCellContent(texture(TEXTURE, UV + vec2(0, 0)));
	float bc = getCellContent(texture(TEXTURE, UV + vec2(0, s.y)));

	float tr = getCellContent(texture(TEXTURE, UV + vec2(s.x, -s.y)));
	float cr = getCellContent(texture(TEXTURE, UV + vec2(s.x, 0)));
	float br = getCellContent(texture(TEXTURE, UV + vec2(s.x, s.y)));

	int count = int(tl) + int(cl) + int(bl) + int(tc) + int(bc) + int(tr) + int(cr) + int(br);

	// Stay
	vec4 output = vec4(cc, cc, cc, 1.0);
	
	// Death
	if (count < 2 || count > 3)
		output = vec4(cc - 0.1, cc - 0.1, cc - 0.1, 1); // Some fading
	
	// Life
	if (count == 3)
		output = vec4(1, 1, 1, 1);
	
	COLOR = output;
}

