using Godot;

public class BlockData
{
	public long[][] rawData;
	public long[] staticData;

	public BlockData()
	{
		rawData = new long[Chunk.CHUNK_VSIZE / 16][];
		staticData = new long[Chunk.CHUNK_VSIZE / 16];

		for (int i = 0; i < staticData.Length; i++)
		{
			staticData[i] = 0;
		}
	}

	public long Get(int x, int y, int z)
	{
		if (staticData[y / 16] == -1)
		{
			return rawData[y / 16][x * Chunk.CHUNK_SIZE_SQ + y % 16 * Chunk.CHUNK_SIZE + z];
		}
		return staticData[y / 16];
	}

	public void Set(int x, int y, int z, long block)
	{
		if (staticData[y / 16] == -1)
		{
			rawData[y / 16][x * Chunk.CHUNK_SIZE_SQ + y % 16 * Chunk.CHUNK_SIZE + z] = block;
		} else
		{
			rawData[y / 16] = new long[Chunk.CHUNK_SIZE_SQ * Chunk.CHUNK_SIZE];
			for (int i = 0; i < Chunk.CHUNK_SIZE_SQ * Chunk.CHUNK_SIZE; i++)
				rawData[y / 16][i] = staticData[y / 16];

			rawData[y / 16][x * Chunk.CHUNK_SIZE_SQ + y % 16 * Chunk.CHUNK_SIZE + z] = block;
			staticData[y / 16] = -1;
		}
	}

	public void Recalculate()
	{
		for (int slice = 0; slice < staticData.Length; slice++)
		{
			if (staticData[slice] != -1)
				continue;
			
			bool same = rawData[slice] != null;
			if (same)
			{
				for (int i = 1; i < rawData[slice].Length; i++)
				{
					if (rawData[slice][i] != rawData[slice][0])
					{
						same = false;
						break;
					}
				}	

				if (same)
				{
					staticData[slice] = rawData[slice][0];
					rawData[slice] = null;
				}
			}
		}
	}
}