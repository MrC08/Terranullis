using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Chunk : Node3D, ICompilable
{
	public const int CHUNK_SIZE = 16;
	public const int CHUNK_SIZE_SQ = CHUNK_SIZE * CHUNK_SIZE;
	public const int CHUNK_VSIZE = 128;
	public const float TEX_SIZE = 16f;

	public static ulong AmountCompiled;
	public static double AverageTime;

	MeshInstance3D meshInstance;
	CollisionShape3D collisionShape;
	World world;

	public BlockData blockData;
	public bool needsCompilation;
	public bool generated = false;
	public Vector2 Position2D;

	public int hash;

	public override void _Ready()
	{
		meshInstance = (MeshInstance3D) GetNode("MeshInstance3D");
		collisionShape = (CollisionShape3D) GetNode("RigidBody3D/CollisionShape3D");

		needsCompilation = true;
	}

	public void Init(World world)
	{
		this.world = world;

		meshInstance.Mesh = new ArrayMesh();

		blockData = new BlockData();

		hash = Util.WorldPosToChunkName(GlobalPosition);
		Name = hash.ToString();

		Position2D = new Vector2(GlobalPosition.X, GlobalPosition.Z);
	}

	public ulong GetBlock(int x, int y, int z, ulong fallback)
	{
		if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_VSIZE || z < 0 || z >= CHUNK_SIZE)
			return world.GetBlock((int) GlobalPosition.X + x, (int) GlobalPosition.Y + y, (int) GlobalPosition.Z + z, fallback);
		
		return blockData.Get(x, y, z);
	}

	public ulong GetBlock(int x, int y, int z)
	{
		return GetBlock(x, y, z, 0);
	}


	private BlockData cachedBlockdataXP;
	private BlockData cachedBlockdataXN;
	private BlockData cachedBlockdataZP;
	private BlockData cachedBlockdataZN;
	private ulong GetBlockDuringGeneration(int x, int y, int z)
	{
		if (y < 0 || y >= CHUNK_VSIZE)
		{
			return GetBlock(x, y, z, 0);
		} else if (x < 0 && cachedBlockdataXN != null)
		{
			return cachedBlockdataXN.Get(x + CHUNK_SIZE, y, z);
		} else if (z < 0 && cachedBlockdataZN != null)
		{
			return cachedBlockdataZN.Get(x, y, z + CHUNK_SIZE);
		} else if (x >= CHUNK_SIZE && cachedBlockdataXP != null)
		{
			return cachedBlockdataXP.Get(x - CHUNK_SIZE, y, z);
		} else if (z >= CHUNK_SIZE && cachedBlockdataZP != null)
		{
			return cachedBlockdataZP.Get(x, y, z - CHUNK_SIZE);
		}
		
		if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_VSIZE || z < 0 || z >= CHUNK_SIZE)
			return world.GetBlock((int) GlobalPosition.X + x, (int) GlobalPosition.Y + y, (int) GlobalPosition.Z + z, 0);
		
		return blockData.Get(x, y, z);
	}


	public void SetBlock(int x, int y, int z, ulong block)
	{
		blockData.Set(x, y, z, block);
	}


	public bool IsBlockTransparent(int x, int y, int z)
	{
		return GetBlock(x, y, z) == 0;
	}

	public bool IsBlockTransparentDuringGeneration(int x, int y, int z, BlockType block)
	{
		BlockType otherBlock = BlockTable.Get(GetBlockDuringGeneration(x, y, z));
		if (otherBlock.IsInvisible)
			return true;
		if (otherBlock.IsTransparent)
			return !block.Equals(otherBlock);
		return false;
	}

	public void ThreadedGenerate()
	{
		GodotThread.SetThreadSafetyChecksEnabled(false);
		Generate();
	}

	public void Generate()
	{
		int[] heightmap = Generator.GenerateHeightmap(GlobalPosition, CHUNK_SIZE, CHUNK_SIZE, false);

		for (int x = 0; x < CHUNK_SIZE; x++)
		{
			for (int z = 0; z < CHUNK_SIZE; z++)
			{
				int height = heightmap[x + z * CHUNK_SIZE];

				int topGenerate = Math.Min(height - (int) GlobalPosition.Y, CHUNK_VSIZE - 1);

				for (int y = topGenerate; y >= 0; y--)
				{
					if (y == topGenerate && topGenerate == height - (int) GlobalPosition.Y)
					{
						SetBlock(x, y, z, 1);
					} else {
						SetBlock(x, y, z, 2);
					}
				}

				/*if (GlobalPosition.Y <= -1)
				{
					for (int y = CHUNK_VSIZE - 1; y > topGenerate; y--)
					{
						SetBlock(x, y, z, 4);
					}
				}*/
			}		
		}

		blockData.Recalculate();

		generated = true;
	}

	
	public void ThreadedCompile()
	{
		GodotThread.SetThreadSafetyChecksEnabled(false);
		Compile();
	}

	public double Compile() { return Compile(true); }

	public double Compile(bool forceCompile)
	{
		double t = Time.GetTicksUsec();

		meshInstance.Mesh = new ArrayMesh();
		collisionShape.Shape = new ConcavePolygonShape3D();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		
		MeshSpan<Vector3> vertices = new MeshSpan<Vector3>();
		MeshSpan<Vector3> normals = new MeshSpan<Vector3>();
		MeshSpan<float> tex = new MeshSpan<float>();
		MeshSpan<int> indices = new MeshSpan<int>();
		
		int index = 0;

		Chunk chunk = world.WorldPosToChunk(GlobalPosition + new Vector3(16, 0, 0));
		if (chunk != null)
			cachedBlockdataXP = chunk.blockData;
		else if (!forceCompile)
			return -1;
		chunk = world.WorldPosToChunk(GlobalPosition + new Vector3(-16, 0, 0));
		if (chunk != null)
			cachedBlockdataXN = chunk.blockData;
		else if (!forceCompile)
			return -1;
		chunk = world.WorldPosToChunk(GlobalPosition + new Vector3(0, 0, 16));
		if (chunk != null)
			cachedBlockdataZP = chunk.blockData;
		else if (!forceCompile)
			return -1;
		chunk = world.WorldPosToChunk(GlobalPosition + new Vector3(0, 0, -16));
		if (chunk != null)
			cachedBlockdataZN = chunk.blockData;
		else if (!forceCompile)
			return -1;
		
		Face xp = new Face();
		Face xn = new Face();
		Face yp = new Face();
		Face yn = new Face();
		Face zp = new Face();
		Face zn = new Face();
		Face tempFace = new Face();

		for (int yChunk = 0; yChunk < CHUNK_VSIZE / 16; yChunk++) {
			if (blockData.staticData[yChunk] != ulong.MaxValue)
			{
				if (BlockTable.Get(blockData.staticData[yChunk]).IsInvisible)
					continue;
			}

			for (int y = yChunk * 16; y < yChunk * 16 + 16; y++) {
				for (int x = 0; x < CHUNK_SIZE; x++) {
					for (int z = 0; z < CHUNK_SIZE; z++) {
						if (blockData.staticData[yChunk] != ulong.MaxValue && !(
							x == 0 || x == CHUNK_SIZE - 1 || y % 16 == 0 || y % 16 == CHUNK_SIZE - 1 || z == 0 || z == CHUNK_SIZE - 1
						))
							continue;

						BlockType block = BlockTable.Get(GetBlock(x, y, z, 0));

						if (!block.IsInvisible) {
							if (IsBlockTransparentDuringGeneration(x - 1, y, z, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureWest, Face.Facing.WEST);
								if (!xp.Continues(tempFace))
								{
									if (xp.Add(vertices, normals, tex, indices, index))
										index += 4;
									xp.CopyFrom(tempFace);
								} else {
									xp.continuation++;
								}
							}
							if (IsBlockTransparentDuringGeneration(x + 1, y, z, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureEast, Face.Facing.EAST);
								if (!xn.Continues(tempFace))
								{
									if (xn.Add(vertices, normals, tex, indices, index))
										index += 4;
									xn.CopyFrom(tempFace);
								} else {
									xn.continuation++;
								}
							}
							if (IsBlockTransparentDuringGeneration(x, y - 1, z, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureBottom, Face.Facing.DOWN);
								if (!yn.Continues(tempFace))
								{
									if (yn.Add(vertices, normals, tex, indices, index))
										index += 4;
									yn.CopyFrom(tempFace);
								} else {
									yn.continuation++;
								}
							}
							if (IsBlockTransparentDuringGeneration(x, y + 1, z, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureTop, Face.Facing.UP);
								if (!yp.Continues(tempFace))
								{
									if (yp.Add(vertices, normals, tex, indices, index))
										index += 4;
									yp.CopyFrom(tempFace);
								} else {
									yp.continuation++;
								}
							}
							if (IsBlockTransparentDuringGeneration(x, y, z - 1, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureNorth, Face.Facing.NORTH);
								if (!zn.Continues(tempFace))
								{
									if (zn.Add(vertices, normals, tex, indices, index))
										index += 4;
									zn.CopyFrom(tempFace);
								} else {
									zn.continuation++;
								}
							}
							if (IsBlockTransparentDuringGeneration(x, y, z + 1, block)) {
								tempFace.Set(new Vector3I(x, y, z), block.TextureSouth, Face.Facing.SOUTH);
								if (!zp.Continues(tempFace))
								{
									if (zp.Add(vertices, normals, tex, indices, index))
										index += 4;
									zp.CopyFrom(tempFace);
								} else {
									zp.continuation++;
								}
							}
						}
					}
				}
			}
		}

		if (xp.Add(vertices, normals, tex, indices, index))
			index += 4;
		if (xn.Add(vertices, normals, tex, indices, index))
			index += 4;
		if (yp.Add(vertices, normals, tex, indices, index))
			index += 4;
		if (yn.Add(vertices, normals, tex, indices, index))
			index += 4;
		if (zp.Add(vertices, normals, tex, indices, index))
			index += 4;
		if (zn.Add(vertices, normals, tex, indices, index))
			index += 4;
		
		if (index != 0) {
			arrays[(int) Mesh.ArrayType.Vertex] = Variant.CreateFrom(vertices.GetSpan());
			arrays[(int) Mesh.ArrayType.Normal] = Variant.CreateFrom(normals.GetSpan());
			arrays[(int) Mesh.ArrayType.Weights] = Variant.CreateFrom(tex.GetSpan());
			arrays[(int) Mesh.ArrayType.Index] = Variant.CreateFrom(indices.GetSpan());
			
			((ArrayMesh) meshInstance.Mesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
			
			List<Vector3> rawFaces = new List<Vector3>();
			foreach (int i in indices.GetSpan()) {
				rawFaces.Add(vertices.GetSpan()[i]);
			}
			
			((ConcavePolygonShape3D) collisionShape.Shape).SetFaces(rawFaces.ToArray());
		
			ArrayMesh newMesh = new ArrayMesh();
			newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
			new Callable(this, nameof(SetNewMesh)).CallDeferred(newMesh);
		}

		needsCompilation = false;

		double end_t = Time.GetTicksUsec();
		AmountCompiled++;
		AverageTime *= ((double) AmountCompiled - 1) / AmountCompiled;
		AverageTime += (end_t - t) * 0.001 / AmountCompiled;
		
		//GD.Print("Took msec: ", (end_t - t) * 0.001);
		return (end_t - t) * 0.001;
	}

	public void SetNewMesh(ArrayMesh newMesh)
	{
		meshInstance.Mesh = newMesh;
	}
}