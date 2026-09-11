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
	public Node3D player;
	ChunkCompiler chunkCompiler;
	[Export] Noise[] noiseArray;

	public Dictionary<int, Chunk> chunkMap;
	public Dictionary<int, LOD> LODMap;

	public int primaryRenderDistance = 16;
	public int primaryLoadDistance = 20;
	public int primaryRenderLODDistance = 32;

	private int tick = 0;
	private double chunksCompiled = 0;
	private double avgChunkCompilationTime = 0;
	private bool chunkActivityLastFrame = true;

	public override void _Ready()
	{
		chunkManager = (Node3D) GetNode("ChunkManager");
		LODManager = (Node3D) GetNode("LODManager");
		player = (Node3D) GetNode("Player");

		chunkCompiler = new ChunkCompiler();

		chunkMap = new();
		LODMap = new();

		Generator.Init();

		MeshInstance3D raymarchQuad = (MeshInstance3D) player.GetNode("Camera3D/MeshInstance3D");
		((ShaderMaterial) raymarchQuad.MaterialOverride).SetShaderParameter("global_noise", Generator.globalNoiseTex);
		//raymarchQuad.Visible = true;
	}


	public override void _Process(double delta)
	{
		tick++;
		ulong currentTimeUsec = Time.GetTicksUsec();

		bool chunkActivityThisFrame = false;

		Vector3 playerSmartPos = player.Position;
		Vector2 playerSmartPosXZ = Util.GetXZ(playerSmartPos);

		int x = (int) (playerSmartPos.X / Chunk.CHUNK_SIZE) - primaryLoadDistance + (tick % (primaryLoadDistance * 2));
		for (int y = -1; y <= 1; y++)
		{
			for (int z = (int) (playerSmartPos.Z / Chunk.CHUNK_SIZE - primaryLoadDistance); z < (int) (playerSmartPos.Z / Chunk.CHUNK_SIZE + primaryLoadDistance); z++)
			{
				Vector3 pos = new Vector3(x, y, z);

				if (new Vector2(pos.X * Chunk.CHUNK_SIZE, pos.Z * Chunk.CHUNK_SIZE).DistanceSquaredTo(playerSmartPosXZ) >= Math.Pow(primaryLoadDistance * Chunk.CHUNK_SIZE, 2))
					continue;

				int hash = Util.ChunkPosToChunkName(Util.SmartChunkPosToChunkPos(pos));

				if (!chunkMap.ContainsKey(hash))
				{
					CreateChunk(Util.SmartChunkPosToChunkPos(pos));

					chunkActivityThisFrame = true;
				}
			}
		}

		chunkCompiler.wait();

		Util.UpdateSmartCoordinateOffset(this);
		playerSmartPos = player.Position;
		playerSmartPosXZ = Util.GetXZ(playerSmartPos);

		for (x = (int) (playerSmartPos.X / Chunk.CHUNK_SIZE) - 1; x <= (int) (playerSmartPos.X / Chunk.CHUNK_SIZE) + 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = (int) (playerSmartPos.Z / Chunk.CHUNK_SIZE) - 1; z <= (int) (playerSmartPos.Z / Chunk.CHUNK_SIZE) + 1; z++) {
					Vector3 pos = Util.SmartChunkPosToChunkPos(new Vector3(x, y, z));
					int hash = Util.ChunkPosToChunkName(pos);
					if (!chunkMap.ContainsKey(hash))
					{
						CreateChunk(pos);
					}
					if (!chunkMap[hash].generated)
						chunkMap[hash].Generate();
					if (chunkMap[hash].needsCompilation)
						chunkMap[hash].Compile(true);
				}
			}
		}

		List<ICompilable> chunksToGenerate = new List<ICompilable>();
		List<ICompilable> chunksToCompile = new List<ICompilable>();

		int[] hashes = chunkMap.Keys.ToArray();
		foreach (int hash in hashes)
		{
			Chunk c = chunkMap[hash];
			float dist_sq = Util.GetXZ(c.Position).DistanceSquaredTo(playerSmartPosXZ);

			if (dist_sq > Math.Pow((primaryRenderDistance + 1) * 16, 2))
			{
				c.Visible = false;
				if (dist_sq > Math.Pow(primaryLoadDistance * 16, 2))
				{
					c.QueueFree();
					chunkMap.Remove(c.hash);
				} else if (!c.generated)
				{
					chunksToGenerate.Insert((int) Math.Min(dist_sq / 8, chunksToGenerate.Count), c);
					chunkActivityThisFrame = true;
				}
			} else {
				c.Visible = !c.visuallyEmpty;

				if (!c.generated)
				{
					if (dist_sq < 16)
						c.Generate();
					else
						chunksToGenerate.Insert((int) Math.Min(dist_sq / 8, chunksToGenerate.Count), c);

					chunkActivityThisFrame = true;
				} else if (c.needsCompilation)
				{
					if (dist_sq < 16)
						c.Compile();
					else
						chunksToCompile.Insert((int) Math.Min(dist_sq / 8, chunksToCompile.Count), c);

					chunkActivityThisFrame = true;
				} else if (c.compiledWithIncompleteSurroundings && c.lastCompiledTime + 5_000_000 <= currentTimeUsec) {
					c.needsCompilation = true;
				}
			}
		}

		if (chunkActivityThisFrame)
			chunkCompiler.run(chunksToGenerate, chunksToCompile);

		if (!chunkActivityThisFrame && !chunkActivityLastFrame)
		{
			x = (int) (playerSmartPos.X / LOD.LOD_SIZE) - primaryRenderLODDistance + (tick % (primaryRenderLODDistance * 2));
			for (int z = (int) (playerSmartPos.Z / LOD.LOD_SIZE - primaryRenderLODDistance); z < (int) (playerSmartPos.Z / LOD.LOD_SIZE + primaryRenderLODDistance); z++)
			{
				Vector3 pos = new Vector3(x, 0, z);

				if (new Vector2(pos.X * LOD.LOD_SIZE, pos.Z * LOD.LOD_SIZE).DistanceSquaredTo(playerSmartPosXZ) >= Math.Pow(primaryRenderLODDistance * LOD.LOD_SIZE, 2))
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
				if (new Vector2(l.GlobalPosition.X, l.GlobalPosition.Z).DistanceSquaredTo(playerSmartPosXZ) > Math.Pow((1 + primaryRenderLODDistance) * LOD.LOD_SIZE, 2))
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


	public ulong GetBlock(int x, int y, int z, ulong fallback)
	{
		if (y < -128)
			return 1;

		if (y > 255)
			return fallback;

		int hash = Util.AbsPosToChunkName(new Vector3(x, y, z));

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

	public Chunk TryToGetChunkFromBlockCoords(int x, int y, int z)
	{
		if (y > 255 || y < -128)
			return null;

		int hash = Util.AbsPosToChunkName(new Vector3(x, y, z));

		if (!chunkMap.ContainsKey(hash))
			return null;

		Chunk chunk = chunkMap[hash];

		if (!chunk.generated)
			return null;
		
		return chunk;
	}

	public void MarkChunkAsCompilationNeeded(Chunk chunk)
	{
		if (chunk != null)
			chunk.needsCompilation = true;
	}

	public bool SetBlock(int gx, int gy, int gz, ulong block)
	{
		Chunk chunk = TryToGetChunkFromBlockCoords(gx, gy, gz);
		if (chunk == null)
			return false;

		int x = gx % Chunk.CHUNK_SIZE;
		int y = gy % Chunk.CHUNK_VSIZE;
		int z = gz % Chunk.CHUNK_SIZE;

		if (x < 0)
			x += Chunk.CHUNK_SIZE;
		if (y < 0)
			y += Chunk.CHUNK_VSIZE;
		if (z < 0)
			z += Chunk.CHUNK_SIZE;

		chunk.SetBlock(x, y, z, block);
		chunk.needsCompilation = true;

		if (x == 0)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx - 1, gy, gz));
		if (x == Chunk.CHUNK_SIZE - 1)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx + 1, gy, gz));
		if (y == 0)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx, gy - 1, gz));
		if (y == Chunk.CHUNK_VSIZE - 1)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx, gy + 1, gz));
		if (z == 0)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx, gy, gz - 1));
		if (z == Chunk.CHUNK_SIZE - 1)
			MarkChunkAsCompilationNeeded(TryToGetChunkFromBlockCoords(gx, gy, gz + 1));


		return true;
	}

	public bool SetBlock(Vector3 v, ulong block)
	{
		return SetBlock((int) Math.Floor(v.X), (int) Math.Floor(v.Y), (int) Math.Floor(v.Z), block);
	}

	public Chunk WorldPosToChunk(Vector3 v)
	{
		int hash = Util.AbsPosToChunkName(v);

		if (chunkMap.ContainsKey(hash))
		{
			return chunkMap[hash];
		}
		return null;
	}


	public Chunk CreateChunk(Vector3 chunkPos)
	{
		int hash = Util.ChunkPosToChunkName(chunkPos);

		Chunk c = (Chunk) chunkScene.Instantiate();
		chunkManager.AddChild(c);
		chunkMap.Add(hash, c);

		c.GlobalPosition = Util.AbsPosToSmartPos(chunkPos * CHUNK_SCALAR);
		c.Init(this);

		return c;
	}


	public bool CheckForCollisionAtPoint(Vector3 pos) {
		return CheckForCollisionAtPoint((int)pos.X, (int)pos.Y, (int)pos.Z);
	}

	public bool CheckForCollisionAtPoint(int x, int y, int z) {
		ulong block = GetBlock(x, y, z, 0);
		return BlockTable.Get(block).IsCollidable;
	}

	const float COLLISION_EPSILON = 0.001f;

	public CollisionResult CheckWorldSmartPosAABB(Vector3 entityMin, Vector3 entityMax, Vector3 velocity, float delta) {
		return CheckWorldAbsPosAABB(
			Util.SmartPosToAbsPos(entityMin),
			Util.SmartPosToAbsPos(entityMax),
			velocity,
			delta
		);
	}

	public CollisionResult CheckWorldAbsPosAABB(Vector3 entityMin, Vector3 entityMax, Vector3 velocity, float delta) {
		Vector3 move = velocity * delta;

		move.Y = ResolveAxisMovement(entityMin, entityMax, 1, move.Y);
		entityMin.Y += move.Y;
		entityMax.Y += move.Y;

		move.X = ResolveAxisMovement(entityMin, entityMax, 0, move.X);
		entityMin.X += move.X;
		entityMax.X += move.X;

		move.Z = ResolveAxisMovement(entityMin, entityMax, 2, move.Z);
		entityMin.Z += move.Z;
		entityMax.Z += move.Z;

		// A tiny persistent downward probe keeps OnFloor true while resting with
		// zero vertical velocity, not just while actively falling into the ground.
		bool onFloor = velocity.Y <= 0f && (move.Y > velocity.Y * delta
			|| ResolveAxisMovement(entityMin, entityMax, 1, -COLLISION_EPSILON) > -COLLISION_EPSILON);

		Vector3 resultVelocity = velocity;
		if (move.X != velocity.X * delta) resultVelocity.X = 0f;
		if (move.Y != velocity.Y * delta) resultVelocity.Y = 0f;
		if (move.Z != velocity.Z * delta) resultVelocity.Z = 0f;

		return new CollisionResult {
			AbsPosition = entityMin,
			Velocity = resultVelocity,
			OnFloor = onFloor
		};
	}

	// Clamps `delta` (a displacement along `axis`, where 0=X, 1=Y, 2=Z) to the
	// nearest solid block the [min, max] AABB would otherwise sweep into,
	// checking only blocks the box already overlaps on the other two axes.
	private float ResolveAxisMovement(Vector3 min, Vector3 max, int axis, float delta) {
		if (delta == 0f)
			return 0f;

		int a1 = (axis + 1) % 3;
		int a2 = (axis + 2) % 3;

		int min1 = Mathf.FloorToInt(min[a1]);
		int max1 = Mathf.CeilToInt(max[a1]) - 1;
		int min2 = Mathf.FloorToInt(min[a2]);
		int max2 = Mathf.CeilToInt(max[a2]) - 1;

		float leadingFace = delta > 0f ? max[axis] : min[axis];
		float targetFace = leadingFace + delta;

		int cellFrom = Mathf.FloorToInt(delta > 0f ? leadingFace : targetFace);
		int cellTo = Mathf.FloorToInt(delta > 0f ? targetFace : leadingFace);

		for (int c = cellFrom; c <= cellTo; c++) {
			for (int c1 = min1; c1 <= max1; c1++) {
				for (int c2 = min2; c2 <= max2; c2++) {
					int bx, by, bz;
					switch (axis) {
						case 0: bx = c; by = c1; bz = c2; break;
						case 1: bx = c2; by = c; bz = c1; break;
						default: bx = c1; by = c2; bz = c; break;
					}

					if (!CheckForCollisionAtPoint(bx, by, bz))
						continue;

					float blockFace = delta > 0f ? c : c + 1f;
					float allowed = blockFace - leadingFace;

					if (delta > 0f)
						delta = Mathf.Min(delta, Mathf.Max(0f, allowed - COLLISION_EPSILON));
					else
						delta = Mathf.Max(delta, Mathf.Min(0f, allowed + COLLISION_EPSILON));
				}
			}
		}

		return delta;
	}
}