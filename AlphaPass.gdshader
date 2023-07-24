shader_type canvas_item;

void fragment() {
  vec4 texColor = texture(TEXTURE, UV);
  if (texColor.rgb != vec3(0.0, 0.0, 0.0)) { // If the pixel is not black
    COLOR = texColor; // Write the original color
  } else {
    COLOR = vec4(0.0, 0.0, 0.0, 0.0); // If the pixel is black, make it transparent
  }
}
