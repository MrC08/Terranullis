using System.Runtime.InteropServices;
using Godot;


public static class Generator
{
	public static Noise[] noiseArray;
	public static ImageTexture globalNoiseTex;

	public static void Init()
	{
		// Parrallels lat * 4 * parrellels lon * 4 for 500x500 block resolution, about ~16 chunk resolution
		float[] globalNoise = new float[180 * 4 * 360 * 4];

		for (float lat = -90.0f; lat < 90.0; lat += 0.25f)
		{
			for (float lon = -180.0f; lon < 180.0; lon += 0.25f)
			{
				Vector2 pos = Util.CoordsToWorldPos(lat, lon);
				int height = HeightAtPos(pos, true);
				globalNoise[GetGlobalNoiseIndex(lat, lon)] = height;
			}
		}

		Image img = Image.CreateFromData(360 * 4, 180 * 4, false, Image.Format.Rf, MemoryMarshal.AsBytes<float>(globalNoise));
		globalNoiseTex = ImageTexture.CreateFromImage(img);
	}

	public static int GetGlobalNoiseIndex(float lat, float lon)
	{
		return (int) (((lat + 90) * 4) + ((lon + 180) * 4) * 180 * 4);
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
		int height = (int) (64 * noiseArray[2].GetNoise2D(x, y));
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