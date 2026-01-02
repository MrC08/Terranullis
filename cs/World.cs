using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class World : Node3D
{
	public readonly Vector3 CHUNK_SCALAR = new Vector3(16, 128, 16);

	readonly PackedScene chunkScene = (PackedScene) ResourceLoader.Load("res://scenes/chunk.tscn");
	readonly PackedScene LODScene = (PackedScene) ResourceLoader.Load("res://scenes/lod.tscn");
	public Node3D chunkManager;
	public Node3D LODManager;
	CharacterBody3D player;
	ChunkCompiler chunkCompiler;
	[Export] Noise[] noiseArray;

	public Dictionary<int, Chunk> chunkMap;
	public Dictionary<int, LOD> LODMap;

	public int primaryRenderDistance = 16;
	public int primaryLoadDistance = 20;
	public int primaryRenderLODDistance = 24;

	private int tick = 0;
	private double chunksCompiled = 0;
	private double avgChunkCompilationTime = 0;
	private bool chunkActivityLastFrame = true;

	public override void _Ready()
	{
		chunkManager = (Node3D) GetNode("ChunkManager");
		LODManager = (Node3D) GetNode("LODManager");
		player = (CharacterBody3D) GetNode("Player");

		chunkCompiler = new ChunkCompiler();

		chunkMap = new();
		LODMap = new();

		Generator.Init();
		Generator.noiseArray = noiseArray;
	}


	public override void _Process(double delta)
	{
		tick++;

		bool chunkActivityThisFrame = false;

		int x = (int) (player.Position.X / Chunk.CHUNK_SIZE) - primaryLoadDistance + (tick % (primaryLoadDistance * 2));
		for (int y = -1; y <= 1; y++)
		{
			for (int z = (int) (player.Position.Z / Chunk.CHUNK_SIZE - primaryLoadDistance); z < (int) (player.Position.Z / Chunk.CHUNK_SIZE + primaryLoadDistance); z++)
			{
				Vector3 pos = new Vector3(x, y, z);

				if (new Vector2(pos.X * Chunk.CHUNK_SIZE, pos.Z * Chunk.CHUNK_SIZE).DistanceSquaredTo(new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z)) >= Math.Pow(primaryLoadDistance * Chunk.CHUNK_SIZE, 2))
					continue;

				int hash = Util.ChunkPosToChunkName(pos);

				if (!chunkMap.ContainsKey(hash))
				{
					CreateChunk(pos);

					chunkActivityThisFrame = true;
				}
			}
		}

		chunkCompiler.wait();

		if (WorldPosToChunk(player.GlobalPosition) == null)
		{
			CreateChunk((player.GlobalPosition / CHUNK_SCALAR).Floor());
			chunkActivityThisFrame = true;
		}
		if (!WorldPosToChunk(player.GlobalPosition).generated)
			WorldPosToChunk(player.GlobalPosition).Generate();
		if (WorldPosToChunk(player.GlobalPosition).needsCompilation)
			WorldPosToChunk(player.GlobalPosition).Compile();

		List<ICompilable> chunksToGenerate = new List<ICompilable>();
		List<ICompilable> chunksToCompile = new List<ICompilable>();

		int[] hashes = chunkMap.Keys.ToArray();
		foreach (int hash in hashes)
		{
			Chunk c = chunkMap[hash];
			if (new Vector2(c.GlobalPosition.X, c.GlobalPosition.Z).DistanceSquaredTo(new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z)) > Math.Pow((primaryRenderDistance + 1) * 16, 2))
			{
				c.Visible = false;
				if (new Vector2(c.GlobalPosition.X, c.GlobalPosition.Z).DistanceSquaredTo(new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z)) > Math.Pow(primaryLoadDistance * 16, 2))
				{
					c.QueueFree();
					chunkMap.Remove(c.hash);
				} else if (!c.generated)
				{
					chunksToGenerate.Add(c);
					chunkActivityThisFrame = true;
				}
			} else {
				c.Visible = true;

				if (!c.generated)
				{
					chunksToGenerate.Add(c);
					chunkActivityThisFrame = true;
				} else if (c.needsCompilation)
				{
					chunksToCompile.Add(c);
					chunkActivityThisFrame = true;
				}
			}
		}

		if (chunkActivityThisFrame)
			chunkCompiler.run(chunksToGenerate, chunksToCompile);

		if (!chunkActivityThisFrame && !chunkActivityLastFrame)
		{
			x = (int) (player.Position.X / LOD.LOD_SIZE) - primaryRenderLODDistance + (tick % (primaryRenderLODDistance * 2));
			for (int z = (int) (player.Position.Z / LOD.LOD_SIZE - primaryRenderLODDistance); z < (int) (player.Position.Z / LOD.LOD_SIZE + primaryRenderLODDistance); z++)
			{
				Vector3 pos = new Vector3(x, 0, z);

				if (new Vector2(pos.X * LOD.LOD_SIZE, pos.Z * LOD.LOD_SIZE).DistanceSquaredTo(new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z)) >= Math.Pow(primaryRenderLODDistance * LOD.LOD_SIZE, 2))
					continue;

				int hash = Util.LODPosToChunkName(pos);

				if (!LODMap.ContainsKey(hash))
				{
					LOD l = (LOD) LODScene.Instantiate();
					LODManager.AddChild(l);
					LODMap.Add(hash, l);

					l.GlobalPosition = pos * new Vector3(LOD.LOD_SIZE, 0, LOD.LOD_SIZE);
					l.Init(this);
				}
			}

			List<ICompilable> LODsToCompile = new List<ICompilable>();

			hashes = LODMap.Keys.ToArray();
			for (int i = 0; i < 256; i++)
			{
				int hash = hashes[(i + 256 * tick) % hashes.Length];

				LOD l = LODMap[hash];
				if (new Vector2(l.GlobalPosition.X, l.GlobalPosition.Z).DistanceSquaredTo(new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z)) > Math.Pow((1 + primaryRenderLODDistance) * LOD.LOD_SIZE, 2))
				{
					l.QueueFree();
					LODMap.Remove(l.hash);
				} else {
					l.Visible = true;

					if (l.needsCompilation)
					{
						LODsToCompile.Add(l);
					}
				}

				if (LODsToCompile.Count > 20)
					break;
			}

			chunkCompiler.run(new List<ICompilable>(), LODsToCompile);
		}

		chunkActivityLastFrame = chunkActivityThisFrame;
	}


	public long GetBlock(int x, int y, int z, long fallback)
	{
		if (y > 255 || y < -128)
			return fallback;

		int hash = Util.WorldPosToChunkName(new Vector3(x, y, z));

		if (!chunkMap.ContainsKey(hash))
			return fallback;

		Chunk chunk = chunkMap[hash];

		if (!chunk.generated)
			return fallback;

		//GD.Print(x, " ", y, " ", z);

		x %= Chunk.CHUNK_SIZE;
		y %= Chunk.CHUNK_VSIZE;
		z %= Chunk.CHUNK_SIZE;

		if (x < 0)
			x += Chunk.CHUNK_SIZE;
		if (y < 0)
			y += Chunk.CHUNK_VSIZE;
		if (z < 0)
			z += Chunk.CHUNK_SIZE;

		//GD.Print(x, " ", y, " ", z);
		//GD.Print(" ");

		return chunk.GetBlock(x, y, z);
	}

	public Chunk tryToGetChunkFromBlockCoords(int x, int y, int z)
	{
		if (y > 255 || y < -128)
			return null;

		int hash = Util.WorldPosToChunkName(new Vector3(x, y, z));

		if (!chunkMap.ContainsKey(hash))
			return null;

		Chunk chunk = chunkMap[hash];

		if (!chunk.generated)
			return null;
		
		return chunk;
	}

	public void markChunkAsCompilationNeeded(Chunk chunk)
	{
		if (chunk != null)
			chunk.needsCompilation = true;
	}

	public bool SetBlock(int gx, int gy, int gz, long block)
	{
		Chunk chunk = tryToGetChunkFromBlockCoords(gx, gy, gz);
		if (chunk == null)
			return false;

		int x = gx % Chunk.CHUNK_SIZE;
		int y = gy % Chunk.CHUNK_VSIZE;
		int z = gz % Chunk.CHUNK_SIZE;

		if (gx < 0)
			x += Chunk.CHUNK_SIZE;
		if (gy < 0)
			y += Chunk.CHUNK_VSIZE;
		if (gz < 0)
			z += Chunk.CHUNK_SIZE;

		chunk.SetBlock(x, y, z, block);
		chunk.needsCompilation = true;

		if (x == 0)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx - 1, gy, gz));
		if (x == Chunk.CHUNK_SIZE - 1)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx + 1, gy, gz));
		if (y == 0)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx, gy - 1, gz));
		if (y == Chunk.CHUNK_VSIZE - 1)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx, gy + 1, gz));
		if (z == 0)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx, gy, gz - 1));
		if (z == Chunk.CHUNK_SIZE - 1)
			markChunkAsCompilationNeeded(tryToGetChunkFromBlockCoords(gx, gy, gz + 1));


		return true;
	}

	public bool SetBlock(Vector3 v, long block)
	{
		return SetBlock((int) Math.Floor(v.X), (int) Math.Floor(v.Y), (int) Math.Floor(v.Z), block);
	}

	public Chunk WorldPosToChunk(Vector3 v)
	{
		int hash = Util.WorldPosToChunkName(v);

		if (chunkMap.ContainsKey(hash))
		{
			return chunkMap[hash];
		}
		return null;
	}


	public void CreateChunk(Vector3 chunkPos)
	{
		int hash = Util.ChunkPosToChunkName(chunkPos);

		Chunk c = (Chunk) chunkScene.Instantiate();
		chunkManager.AddChild(c);
		chunkMap.Add(hash, c);

		c.GlobalPosition = chunkPos * CHUNK_SCALAR;
		c.Init(this);
	}
}