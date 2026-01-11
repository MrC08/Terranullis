using Godot;

public partial class Util : Node
{
	public static int ChunkPosToChunkName(Vector3 v)
	{
		return GD.Hash(v.Floor());
	}
	
	public static int WorldPosToChunkName(Vector3 v)
	{
		return ChunkPosToChunkName(WorldPosToChunkPos(v));
	}
	
	public static int LODPosToChunkName(Vector3 v)
	{
		return ChunkPosToChunkName(LODPosToChunkPos(v));
	}

	public static Vector3 WorldPosToChunkPos(Vector3 v)
	{
		return new Vector3(v.X / Chunk.CHUNK_SIZE, v.Y / Chunk.CHUNK_VSIZE, v.Z / Chunk.CHUNK_SIZE).Floor();
	}

	public static Vector3 WorldPosToLODPos(Vector3 v)
	{
		return new Vector3(v.X / LOD.LOD_SIZE, 0, v.Z / LOD.LOD_SIZE).Floor();
	}

	public static Vector3 LODPosToChunkPos(Vector3 v)
	{
		return WorldPosToChunkPos(LODPosToWorldPos(v));
	}

	public static Vector3 LODPosToWorldPos(Vector3 v)
	{
		return new Vector3(v.X * LOD.LOD_SIZE, 0, v.Z * LOD.LOD_SIZE).Floor();
	}

	public static Vector2 WorldPosToCoords(float x, float y)
	{
		return new Vector2(y / 180_000f, x / 90_000f);
	}

	public static Vector2 CoordsToWorldPos(float lat, float lon)
	{
		return new Vector2(lon * 180_000f, lat * 90_000f);
	}
}
