using Godot;


public static class Generator
{
	public static Noise[] noiseArray;

	public static void Init()
	{
	}


	public static int[] GenerateHeightmap(Vector3 worldPos, int xSize, int ySize, bool includeWater)
	{
		return GenerateHeightmap(new Vector2(worldPos.X, worldPos.Z), xSize, ySize, includeWater);
	}


	public static int[] GenerateHeightmap(Vector2 worldPos, int xSize, int ySize, bool includeWater)
	{
		int[] heightmap = new int[xSize * ySize];

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				heightmap[x + y * xSize] += (int) (64 * noiseArray[2].GetNoise2D(x + worldPos.X, y + worldPos.Y));
				heightmap[x + y * xSize] += (int) (192 * noiseArray[3].GetNoise2D(x + worldPos.X, y + worldPos.Y));
				heightmap[x + y * xSize] += 64;

				if (includeWater && heightmap[x + y * xSize] < 0)
					heightmap[x + y * xSize] = 0;
				else if (heightmap[x + y * xSize] < -127)
					heightmap[x + y * xSize] = -127;
			}
		}

		return heightmap;
	}
}