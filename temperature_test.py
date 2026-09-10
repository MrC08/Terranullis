import math
import sys
from matplotlib import pyplot as plt


DECL_MAX = 23.4 / 90  # axial tilt, normalized to the same -1..1 scale as l


def declination(s):
	# Subsolar latitude: -0.26 (23.4S) around Dec/Jan, +0.26 (23.4N) around Jun.
	return -DECL_MAX * math.cos(2 * math.pi * s)


def insolation(l, s):
	# Peaks at 1 when l equals the current subsolar latitude, falls off as a
	# cosine of the angular distance from it.
	return math.cos((l - declination(s)) * math.pi / 2)


# Piecewise-linear through the calibration anchors (coldest at t=0, moderate
# 45N climate at t=0.4, hottest at t=1) so the base climate curve doesn't
# saturate before t=1 the way a single quadratic through those points does.
BASE_POINTS = [(0.0, -45.17), (0.4, -9.06), (1.0, 14.46)]


def base_temp(t):
	for (t0, v0), (t1, v1) in zip(BASE_POINTS, BASE_POINTS[1:]):
		if t0 <= t <= t1:
			frac = (t - t0) / (t1 - t0)
			return v0 + (v1 - v0) * frac
	return BASE_POINTS[-1][1] if t > BASE_POINTS[-1][0] else BASE_POINTS[0][1]


# Mirrors Generator.cs TemperatureMap (lines 465-480): a latitude-driven
# base, an elevation penalty (mountains cool regardless of latitude), then
# pulled 10% of the way toward 0.5 per unit of humidity, and clamped.
def derive_temperature(l, h, elevation):
	t = (90 - 90 * abs(l)) / 105
	t -= (max(-0.1, elevation) * 0.8) ** 2
	t = t + (0.5 - t) * (h * 0.1)
	return max(0.0, min(1.0, t))


def temperature(l, h, elevation, s):
	l = float(l)
	h = float(h)
	elevation = float(elevation)
	s = float(s)

	t = derive_temperature(l, h, elevation)
	return 92.54 * insolation(l, s) + base_temp(t)


def d(l, h, elevation, s):
	l = float(l)
	h = float(h)
	elevation = float(elevation)
	s = float(s)
	return temperature(l, h, elevation, s) + 4 + 14 * (1 - h)


def n(l, h, elevation, s):
	l = float(l)
	h = float(h)
	elevation = float(elevation)
	s = float(s)
	return temperature(l, h, elevation, s) - 4 - 14 * (1 - h)


fig, ax = plt.subplots(3, 4)

for i, biome in enumerate([
	{"l": 0 / 90, "h": 0.7176822, "e": 0.2664257, "b": "Forest"},
	{"l": -89 / 90, "h": 0.71632206, "e": -0.45505726, "b": "Sea Ice"},
	{"l": 6 / 90, "h": 0.8663108, "e": -0.44906774, "b": "Coral Reef"},
	{"l": -13 / 90, "h": 0.52560794, "e": 0.03160987, "b": "Prarie"},
	{"l": -85 / 90, "h": 0.65239465, "e": 0.57699645, "b": "Ice Sheet"},
	{"l": 24 / 90, "h": 0.05834421, "e": 0.095327355, "b": "Desert"},
	{"l": -2.5 / 90, "h": 0.18463406, "e": 0.1581705, "b": "Extreme Desert"},
	{"l": 62 / 90, "h": 0.14183111, "e": 0.047315426, "b": "Tundra"},
	{"l": 53.5 / 90, "h": 0.32098752, "e": 0.47587755, "b": "Boreal Forest"},
	{"l": -11 / 90, "h": 0.36483622, "e": 0.091829345, "b": "Shrublands"},
	{"l": -25.5 / 90, "h": 0.8150707, "e": 0.7042397, "b": "Magellanic Rainforest"},
	{"l": -62.5 / 90, "h": 0.7401704, "e": 0.049162693, "b": "Boglands"},
]):
	if sys.argv[-1] == "graph":
		ax[i % 3, i // 3].set_title(str(biome["b"]) + f" Lat={int(biome['l'] * 90)}°")
		ax[i % 3, i // 3].plot([float(s)/12.0 for s in range(13)], [d(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)])  # Plot some data on the Axes.
		ax[i % 3, i // 3].plot([float(s)/12.0 for s in range(13)], [n(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)])  # Plot some data on the Axes.

		minimum = min(40, min([d(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)]))
		maximum = max([d(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)])

		for gridline in range(int(minimum / 10) * 10 - 10, int(maximum / 10) * 10 + 11, 10):
			ax[i % 3, i // 3].axhline(y=gridline, color="gray", linewidth=0.5, linestyle="--", zorder=0)
		ax[i % 3, i // 3].axhline(y=32, color="blue", linewidth=0.8, linestyle="--", zorder=0)
	else:
		print("\n")
		print(str(biome["b"]) + f" Lat={int(biome['l'] * 90)}°")
		print("Daytime (season, temperature):", [f"({a[0]:.2f} {a[1]:.2f})" for a in zip([float(s)/12.0 for s in range(13)], [d(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)])])
		print("Nighttime (season, temperature):", [f"({a[0]:.2f} {a[1]:.2f})" for a in zip([float(s)/12.0 for s in range(13)], [n(biome["l"], biome["h"], biome["e"], s/12.0) for s in range(13)])])

fig.tight_layout()

if sys.argv[-1] == "graph":
	plt.show()