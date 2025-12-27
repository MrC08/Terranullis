using Godot;


public static class Generator
{
	public static Noise[] noiseArray;

	public static void Init()
	{
	}


	public static int[] GenerateHeightmap(Vector3 worldPos, int xSize, int ySize)
	{
		return GenerateHeightmap(new Vector2(worldPos.X, worldPos.Z), xSize, ySize);
	}


	public static int[] GenerateHeightmap(Vector2 worldPos, int xSize, int ySize)
	{
		int[] heightmap = new int[xSize * ySize];

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				heightmap[x + y * xSize] = (int) (4 * noiseArray[0].GetNoise2D(x + worldPos.X, y + worldPos.Y));
				heightmap[x + y * xSize] = (int) (16 * noiseArray[1].GetNoise2D(x + worldPos.X, y + worldPos.Y));
				heightmap[x + y * xSize] = (int) (32 * noiseArray[2].GetNoise2D(x + worldPos.X, y + worldPos.Y));
				heightmap[x + y * xSize] = (int) (64 * noiseArray[3].GetNoise2D(x + worldPos.X, y + worldPos.Y));
			}
		}

		return heightmap;
	}
}