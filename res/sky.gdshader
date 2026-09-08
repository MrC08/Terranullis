shader_type sky;

uniform sampler2D star_noise;

#include "sample_sky_color.gdshaderinc"

void sky() {
	COLOR = sample_sky_color(EYEDIR, LIGHT0_DIRECTION);

	vec2 star_coords = (SKY_COORDS * 4.0);
	COLOR = mix(COLOR, COLOR + texture(star_noise, star_coords).rgb, clamp(
		pow(1.0 - max(COLOR.r, max(COLOR.g, COLOR.b)), 16.0)
		, 0.0, 1.0));
}
