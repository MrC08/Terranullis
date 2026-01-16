using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;


public static class Generator
{
	const int LINES_OF_LON = 360;
	const int LINES_OF_LAT = 180;
	const int LINES_OF_LON_DOUBLE = LINES_OF_LON * 2;
	const int LINES_OF_LAT_DOUBLE = LINES_OF_LAT * 2;
	const int LINES_OF_LON_HALF = LINES_OF_LON / 2;
	const int LINES_OF_LAT_HALF = LINES_OF_LAT / 2;

	public static FastNoiseLite noise;
	public static ImageTexture globalNoiseTex;

	public static bool WorldGenerated = false;
	public static Texture2D LatestProgress = ImageTexture.CreateFromImage(Image.CreateEmpty(LINES_OF_LON, LINES_OF_LAT, false, Image.Format.Rgb8));

	public static bool TectonicsGenerated = false;
	public static float[][] TectonicActivityMap;
	public static bool[][] TectonicTypeMap;

	public static bool ElevationGenerated = false;
	public static float[][] ElevationMap;

	public static bool ClimateGenerated = false;
	public static Vector2[][] AirCurrentMap;

	public static void Init()
	{
		noise = new FastNoiseLite();
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		noise.Frequency = 0.003f;

		// Parrallels lat * 4 * parrellels lon * 4 for 500x500 block resolution, about ~16 chunk resolution
		float[] globalNoise = new float[LINES_OF_LAT * 4 * LINES_OF_LON * 4];

		for (float lat = -LINES_OF_LAT_HALF; lat < LINES_OF_LAT_HALF; lat += 0.25f)
		{
			for (float lon = -LINES_OF_LON_HALF; lon < LINES_OF_LON_HALF; lon += 0.25f)
			{
				Vector2 pos = Util.CoordsToWorldPos(lat, lon);
				int height = HeightAtPos(pos, true);
				globalNoise[GetGlobalNoiseIndex(lat, lon)] = height;
			}
		}

		Image img = Image.CreateFromData(LINES_OF_LON * 4, LINES_OF_LAT * 4, false, Image.Format.Rf, MemoryMarshal.AsBytes<float>(globalNoise));
		globalNoiseTex = ImageTexture.CreateFromImage(img);
	}


	public static void StepGenerateWorld()
	{
		double t = Time.GetTicksUsec();
		if (!TectonicsGenerated)
			GenerateTectonicMap();
		else if (!ElevationGenerated)
			GenerateElevation();
		else if (!ClimateGenerated)
			GenerateClimate();
		else
			WorldGenerated = true;

		GD.Print("Took msec: ", Mathf.Round((Time.GetTicksUsec() - t) * 0.001));
	}


	public static void GenerateTectonicMap()
	{
		GD.Print("Generating tectonics");

		GD.Seed(0);

		Vector2[][] tectonicMap = new Vector2[LINES_OF_LON][];
		Vector2[][] tectonicCollisionMap = new Vector2[72][];
		float[][] tectonicActivityMap = new float[LINES_OF_LON][];
		TectonicTypeMap = new bool[LINES_OF_LON][];
		Vector2[] points = new Vector2[64];

		byte[] debugImage = new byte[LINES_OF_LON * LINES_OF_LAT * 3];

		FastNoiseLite noise = new FastNoiseLite();


		for (int x = 0; x < tectonicCollisionMap.Length; x++)
			tectonicCollisionMap[x] = new Vector2[36];


		for (int i = 0; i < points.Length; i++)
		{
			points[i] = new Vector2(GD.Randf() * LINES_OF_LON, GD.Randf() * LINES_OF_LAT);
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			tectonicMap[x] = new Vector2[LINES_OF_LAT];
			TectonicTypeMap[x] = new bool[LINES_OF_LAT];
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				float shortestDist = float.PositiveInfinity;
				int shortestIndex = 0;

				Vector2 offset = new Vector2(
					noise.GetNoise2D(x, y),
					noise.GetNoise2D(x + LINES_OF_LON_DOUBLE, y + LINES_OF_LAT_DOUBLE)
				) * 30f;

				for (int i = 0; i < points.Length; i++)
				{
					float dist = Mathf.Min(
						new Vector2(x, y).DistanceSquaredTo(points[i] + offset),
						new Vector2(x + (x < LINES_OF_LAT ? LINES_OF_LON : -LINES_OF_LON), y).DistanceSquaredTo(points[i] + offset));
					if (dist < shortestDist)
					{
						shortestDist = dist;
						shortestIndex = i;
					}
				}

				GD.Seed((ulong) shortestIndex);
				tectonicMap[x][y] = new Vector2(GD.RandRange(-1, 1), GD.RandRange(-1, 1));
				tectonicCollisionMap[x / 5][y / 5] += tectonicMap[x][y] / 25f;

				TectonicTypeMap[x][y] = GD.Randf() < 0.4;
			}
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			tectonicActivityMap[x] = new float[LINES_OF_LAT];
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				Vector2I offset = new Vector2I(
					(int) (noise.GetNoise2D(x, y) * 2f - 1f),
					(int) (noise.GetNoise2D(x + LINES_OF_LON_DOUBLE, y + LINES_OF_LAT_DOUBLE) * 2f - 1f)
				);

				int samples = 0;
				for (int x2 = Mathf.Max(0, offset.X + x - 2); x2 <= Mathf.Min(359, offset.X + x + 2); x2++)
				{
					for (int y2 = Mathf.Max(0, offset.Y + y - 2); y2 <= Mathf.Min(179, offset.Y + y + 2); y2++)
					{
						//tectonicActivityMap[x][y] += tectonicMap[x2][y2].Normalized().Dot(tectonicCollisionMap[x / 5][y / 5].Normalized());
						tectonicActivityMap[x][y] += Mathf.Abs(tectonicMap[x2][y2].X - tectonicCollisionMap[x / 5][y / 5].X);
						samples++;
					}
				}

				tectonicActivityMap[x][y] /= samples;
				tectonicActivityMap[x][y] = tectonicActivityMap[x][y];
			}
		}

		TectonicActivityMap = new float[LINES_OF_LON][];
		for (int x = 0; x < LINES_OF_LON; x++)
		{
			TectonicActivityMap[x] = new float[LINES_OF_LAT];
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				float samples = 0;

				for (int x2 = Mathf.Max(0, x - 1); x2 <= Mathf.Min(359, x + 1); x2++)
				{
					for (int y2 = Mathf.Max(0, y - 1); y2 <= Mathf.Min(179, y + 1); y2++)
					{
						TectonicActivityMap[x][y] += tectonicActivityMap[x2][y2];
						samples++;
					}
				}

				TectonicActivityMap[x][y] /= samples;
				TectonicActivityMap[x][y] = Mathf.Clamp(TectonicActivityMap[x][y], 0, 1);
			}
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				//debugImage[(x + y * LINES_OF_LON) * 2] = (byte) (Mathf.Abs(tectonicMap[x][y].X - tectonicCollisionMap[x / 5][y / 5].X) * 127f);
				//debugImage[(x + y * LINES_OF_LON) * 2] = (byte) (Mathf.Abs(tectonicMap[x][y].Y - tectonicCollisionMap[x / 5][y / 5].Y) * 127f);
				
				debugImage[(x + y * LINES_OF_LON) * 3] = (byte) (Mathf.Abs(TectonicActivityMap[x][y]) * 255f);

				debugImage[(x + y * LINES_OF_LON) * 3 + 2] = (byte) (TectonicTypeMap[x][y] ? 255 : 0);
			}
		}

		LatestProgress = new ImageTexture();
		((ImageTexture) LatestProgress).SetImage(Image.CreateFromData(LINES_OF_LON, LINES_OF_LAT, false, Image.Format.Rgb8, debugImage));

		TectonicsGenerated = true;
	}


	public static void GenerateElevation()
	{
		GD.Print("Generating elevation");

		byte[] debugImage = new byte[LINES_OF_LON * LINES_OF_LAT * 3];

		ElevationMap = new float[LINES_OF_LON][];

		FastNoiseLite noise = new FastNoiseLite();
		noise.Frequency = 0.005f;

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			ElevationMap[x] = new float[LINES_OF_LAT];
			for (int y = 0; y < ElevationMap[x].Length; y++)
			{
				if (TectonicTypeMap[x][y])
				{ // Terrestrial
					ElevationMap[x][y] = 0.1f;
					ElevationMap[x][y] += (noise.GetNoise2D(x * 10, y * 10) * 0.5f + 0.5f) * 2f;

					ElevationMap[x][y] *= 0.1f + TectonicActivityMap[x][y];

					ElevationMap[x][y] += noise.GetNoise2D(x, y) * 0.1f - 0.035f;

					ElevationMap[x][y] = Mathf.Min(1f, ElevationMap[x][y]);
				} else
				{ // Oceanic
					ElevationMap[x][y] = -0.1f;
					ElevationMap[x][y] -= noise.GetNoise2D(x * 10, y * 10) * 0.5f + 0.4f;

					ElevationMap[x][y] *= TectonicActivityMap[x][y];

					ElevationMap[x][y] = Mathf.Max(-1f, ElevationMap[x][y]);
				}
			}
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				if (ElevationMap[x][y] > 0) {
					debugImage[(x + y * LINES_OF_LON) * 3 + 1] = 255;

					debugImage[(x + y * LINES_OF_LON) * 3 + 0] = (byte) (255 * ElevationMap[x][y]);
					debugImage[(x + y * LINES_OF_LON) * 3 + 2] = (byte) (255 * ElevationMap[x][y]);
				} else
					debugImage[(x + y * LINES_OF_LON) * 3 + 2] = (byte) (255 * (1 + ElevationMap[x][y]));
			}
		}

		LatestProgress = new ImageTexture();
		((ImageTexture) LatestProgress).SetImage(Image.CreateFromData(LINES_OF_LON, LINES_OF_LAT, false, Image.Format.Rgb8, debugImage));

		ElevationGenerated = true;
	}


	public static void GenerateClimate()
	{
		GD.Print("Generating climate");

		byte[] debugImage = new byte[LINES_OF_LON * LINES_OF_LAT * 3];

		AirCurrentMap = new Vector2[LINES_OF_LON][];

		for (int x = 0; x < AirCurrentMap.Length; x++)
		{
			AirCurrentMap[x] = new Vector2[LINES_OF_LAT];
			/*for (int y = 0; y < AirCurrentMap[x].Length; y++)
			{
				if (y == 0 || y == LINES_OF_LAT - 1 || ElevationMap[(x + 1) % LINES_OF_LON][y] < ElevationMap[x][y] || ElevationMap[(x + 3) % LINES_OF_LON][y] < -0.1)
					AirCurrentMap[x][y] = Vector2.Right;
				else
				{
					float difference = 
						(ElevationMap[(x + 1) % LINES_OF_LON][y + 1] + ElevationMap[(x + 2) % LINES_OF_LON][y + 1]) -
						(ElevationMap[(x + 1) % LINES_OF_LON][y - 1] + ElevationMap[(x + 2) % LINES_OF_LON][y - 1]);
					difference = Mathf.Clamp(2f * difference, -0.99f, 0.99f);
					AirCurrentMap[x][y] = new Vector2(Mathf.Cos(difference), -Mathf.Sin(difference));
				}
			}*/
		}
		for (int x = LINES_OF_LON - 1; x > 0; x--)
		{
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				if (x != 359 && !AirCurrentMap[x][y].Equals(Vector2.Zero))
					continue;

				AirCurrentMap[x][y] = Vector2.Right;
				List<Vector2I> frontier = new List<Vector2I>();
				List<Vector2I> visited = new List<Vector2I>();
				frontier.Add(new Vector2I(x, y));

				while (frontier.Count > 0)
				{
					Vector2I pos = frontier[0];
					frontier.RemoveAt(0);
					visited.Add(pos);

					for (int x2 = Math.Max(0, pos.X - 1); x2 <= pos.X; x2++)
					{
						for (int y2 = Math.Max(0, pos.Y - 1); y2 <= MathF.Min(179, pos.Y + 1); y2++)
						{
							if (x2 == pos.X && y2 == pos.Y)
								continue;

							Vector2I newPos = new Vector2I(x2, y2);
							if (!AirCurrentMap[newPos.X][newPos.Y].Equals(Vector2.Right) && (ElevationMap[pos.X][pos.Y] <= 0 || ElevationMap[newPos.X][newPos.Y] <= 0.02 + ElevationMap[pos.X][pos.Y]))
							{
								AirCurrentMap[newPos.X][newPos.Y] = new Vector2(pos.X - newPos.X, pos.Y - newPos.Y);
								
								if (newPos.X > 0 && !frontier.Contains(newPos) && !visited.Contains(newPos))
									frontier.Add(newPos);
							}
						}
					}
				}
			}
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				AirCurrentMap[x][y] = (AirCurrentMap[x][y] + new Vector2(1, 0)).Normalized();
			}
		}

		for (int x = 0; x < LINES_OF_LON; x++)
		{
			for (int y = 0; y < LINES_OF_LAT; y++)
			{
				if (AirCurrentMap[x][y].IsEqualApprox(Vector2.Right))
				{
					debugImage[(x + y * LINES_OF_LON) * 3 + 2] = (byte) (255 * ElevationMap[x][y]);
				} else {
					if (AirCurrentMap[x][y].Y > 0)
						debugImage[(x + y * LINES_OF_LON) * 3 + 1] = (byte) (255 * AirCurrentMap[x][y].Y);
					else
						debugImage[(x + y * LINES_OF_LON) * 3 + 0] = (byte) (255 * -AirCurrentMap[x][y].Y);
				}

				if (ElevationMap[x][y] < 0) {
					debugImage[(x + y * LINES_OF_LON) * 3 + 2] = 64;
				}
			}
		}

		LatestProgress = new ImageTexture();
		((ImageTexture) LatestProgress).SetImage(Image.CreateFromData(LINES_OF_LON, LINES_OF_LAT, false, Image.Format.Rgb8, debugImage));


		ClimateGenerated = true;
	}


	public static int GetGlobalNoiseIndex(float lat, float lon)
	{
		return (int) (((lat + LINES_OF_LAT_HALF) * 4) + ((lon + LINES_OF_LON_HALF) * 4) * LINES_OF_LON_HALF * 4);
	}


	public static int[] GenerateHeightmap(Vector3 worldPos, int xSize, int ySize, bool includeWater)
	{
		return GenerateHeightmap(new Vector2(worldPos.X, worldPos.Z), xSize, ySize, includeWater);
	}


	public static int HeightAtPos(Vector2 pos, bool includeWater)
	{
		return HeightAtPos(pos.X, pos.Y, includeWater);
	}
	public static int HeightAtPos(float x, float y, bool includeWater)
	{
		int height = (int) (64 * noise.GetNoise2D(x, y));
		height += 64;

		if (includeWater && height < 0)
			height = 0;
		else if (height < -127)
			height = -127;
		
		return height;
	}


	public static int[] GenerateHeightmap(Vector2 worldPos, int xSize, int ySize, bool includeWater)
	{
		int[] heightmap = new int[xSize * ySize];

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				heightmap[x + y * xSize] = HeightAtPos(x + worldPos.X, y + worldPos.Y, includeWater);
			}
		}

		return heightmap;
	}
}