shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform int size : hint_range(1, 32) = 4;

void fragment() {
  vec2 pixel_size = 1.0 / vec2(textureSize(screen_texture, 0));
  vec4 color = vec4(0.0);
  for (int y = -size; y <= size; y++) {
    for (int x = -size; x <= size; x++) {
      color += texture(screen_texture, SCREEN_UV + vec2(float(x), float(y)) * pixel_size);
    }
  }
  color /= (2.0 * float(size) + 1.0) * (2.0 * float(size) + 1.0);
  COLOR = color;
}