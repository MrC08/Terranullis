using System;
using Godot;

public partial class Util : Node
{
	public static long smartPosOffsetX;
	public static long smartPosOffsetZ;

	public static int ChunkPosToChunkName(Vector3 v)
	{
		return GD.Hash(v.Floor());
	}
	
	public static int AbsPosToChunkName(Vector3 v)
	{
		return ChunkPosToChunkName(AbsPosToChunkPos(v));
	}
	
	public static int SmartPosToChunkName(Vector3 v)
	{
		return ChunkPosToChunkName(SmartPosToChunkPos(v));
	}
	
	public static int LODPosToChunkName(Vector3 v)
	{
		return ChunkPosToChunkName(LODPosToChunkPos(v));
	}

	public static Vector3 AbsPosToChunkPos(Vector3 v)
	{
		return new Vector3(v.X / Chunk.CHUNK_SIZE, v.Y / Chunk.CHUNK_VSIZE, v.Z / Chunk.CHUNK_SIZE).Floor();
	}

	public static Vector3 SmartPosToChunkPos(Vector3 v)
	{
		return AbsPosToChunkPos(SmartPosToAbsPos(v));
	}

	public static Vector3 AbsPosToLODPos(Vector3 v)
	{
		return new Vector3(v.X / LOD.LOD_SIZE, 0, v.Z / LOD.LOD_SIZE).Floor();
	}

	public static Vector3 LODPosToChunkPos(Vector3 v)
	{
		return SmartPosToChunkPos(LODPosToSmartPos(v));
	}

	public static Vector3 LODPosToSmartPos(Vector3 v)
	{
		return new Vector3(v.X * LOD.LOD_SIZE, 0, v.Z * LOD.LOD_SIZE).Floor();
	}

	public static Vector2 AbsPosToCoords(float x, float y)
	{
		return new Vector2(y / 180_000f, x / 90_000f);
	}

	public static Vector2 CoordsToAbsPos(float lat, float lon)
	{
		return new Vector2(lon * 180_000f, lat * 90_000f);
	}

	public static Vector3 SmartChunkPosToChunkPos(Vector3 smartChunkPos) {
		return smartChunkPos + new Vector3(smartPosOffsetX / Chunk.CHUNK_SIZE, 0, smartPosOffsetZ / Chunk.CHUNK_SIZE);
	}

	public static void UpdateSmartCoordinateOffset(World world)
	{
		Vector3 pos = world.player.GlobalPosition;

		if (pos.X > 1024 || pos.X < -1024)
		{
			long shiftX = (long) (pos.X - pos.X % 1024);
			smartPosOffsetX += shiftX;
			pos.X -= shiftX;
		}

		if (pos.Z > 1024 || pos.Z < -1024)
		{
			long shiftZ = (long) (pos.Z - pos.Z % 1024);
			smartPosOffsetZ += shiftZ;
			pos.Z -= shiftZ;
		}
		/*
		while (pos.X > 1024) {
			pos.X -= 1024;
			smartPosOffsetX += 1024;
		}
		while (pos.Z > 1024) {
			pos.Z -= 1024;
			smartPosOffsetZ += 1024;
		}
		while (pos.X < -1024) {
			pos.X += 1024;
			smartPosOffsetX -= 1024;
		}
		while (pos.Z < -1024) {
			pos.Z += 1024;
			smartPosOffsetZ -= 1024;
		}*/

		if (pos != world.player.GlobalPosition) {
			Vector3 offset = pos - world.player.GlobalPosition;

			foreach (Chunk chunk in world.chunkMap.Values) {
				chunk.Position += offset;
			}
			foreach (LOD lod in world.LODMap.Values) {
				lod.Position += offset;
			}
		}

		world.player.GlobalPosition = pos;
	}

	public static Vector3 AbsPosToSmartPos(Vector3 abs)
	{
		return abs with { X = abs.X - smartPosOffsetX, Z = abs.Z - smartPosOffsetZ };
	}

	public static Vector2 AbsPosToSmartPosXZ(Vector3 abs)
	{
		return new Vector2(abs.X - smartPosOffsetX, abs.Z - smartPosOffsetZ);
	}

	public static Vector3 SmartPosToAbsPos(Vector3 smart)
	{
		return smart with { X = smart.X + smartPosOffsetX, Z = smart.Z + smartPosOffsetZ };
	}

	public static Vector2 SmartPosToAbsPosXZ(Vector3 smart)
	{
		return new Vector2(smart.X + smartPosOffsetX, smart.Z + smartPosOffsetZ);
	}

	internal static Vector2 GetXZ(Vector3 vec3)
	{
		return new Vector2(vec3.X, vec3.Z);
	}
}
