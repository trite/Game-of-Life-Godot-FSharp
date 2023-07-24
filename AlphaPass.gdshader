shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
  vec4 texColor = texture(screen_texture, SCREEN_UV);
  if (texColor.rgb != vec3(0.0, 0.0, 0.0)) { // If the pixel is not black
    COLOR = texColor; // Write the original color
//	COLOR = vec4(1.0);
  } else {
    COLOR = vec4(0.0, 0.0, 0.0, 0.0); // If the pixel is black, make it transparent
  }
//
//	COLOR = texColor;
}
