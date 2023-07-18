shader_type canvas_item;

// uniform vec4 alive_color = vec4(float(int(0.0)));

uniform vec4 dead_color = vec4(vec3(0.0), 1.0);

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	COLOR = tex;
	if (tex.g == 1.0) {
		COLOR = vec4(float(int(1)));
		COLOR.r = 0.6;
	} else {
		COLOR = dead_color;
	}
	
	
}

void vertex() {
	// VERTEX += vec2(10.0, 0.0);
	// VERTEX += vec2(cos(TIME)*100.0, sin(TIME)*100.0);
}