shader_type canvas_item;

uniform sampler2D tex : hint_screen_texture;

const mat4 dither_matrix = mat4(
	vec4(0, 8, 2, 10),
	vec4(12, 4, 14, 6),
	vec4(3, 11, 1, 9),
	vec4(15, 7, 13, 5)
);

const float res = 420.0;

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 resolution = 1.0 / SCREEN_PIXEL_SIZE;

	uv *= res;
	uv.x *= resolution.x/resolution.y;
	uv = floor(uv);
	uv.x /= resolution.x/resolution.y;
	uv /= res;

	COLOR = texture(tex, uv);

	uv *= res;
	uv.x *= resolution.x/resolution.y;

	COLOR *= 32.0;
	COLOR += dither_matrix[int(mod(uv.x, 4.0))][int(mod(uv.y, 4.0))] / 24.0;
	COLOR = round(COLOR);
	COLOR /= 32.0;
}