using Godot;
using System;
using System.Collections.Generic;

public partial class LOD : Node3D, ICompilable
{
	public const int LOD_SIZE = 64;
	public const int LOD_SIZE1 = LOD_SIZE + 1;
	public const int LOD_SIZE_SQ = LOD_SIZE * LOD_SIZE;
	public const float TEX_SIZE = 16f;

	MeshInstance3D meshInstance;
	World world;

	public bool needsCompilation;
	public bool generated = false;
	public int hash;

	public override void _Ready()
	{
		meshInstance = (MeshInstance3D) GetNode("MeshInstance3D");

		needsCompilation = true;
	}

	public void Init(World world)
	{
		this.world = world;

		meshInstance.Mesh = new ArrayMesh();

		hash = Util.WorldPosToChunkName(GlobalPosition);
		Name = hash.ToString();
	}

	public void ThreadedCompile()
	{
		GodotThread.SetThreadSafetyChecksEnabled(false);
		Compile();
	}


	public double Compile()
	{
		double t = Time.GetTicksUsec();

		meshInstance.Mesh = new ArrayMesh();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> tex = new List<Vector2>();
		List<int> indices = new List<int>();
		
		int index = 0;

		int[] noiseMap = Generator.GenerateHeightmap(GlobalPosition, LOD_SIZE + 1, LOD_SIZE + 1, false);

		for (int x = 0; x < LOD_SIZE; x++) {
			for (int z = 0; z < LOD_SIZE; z++) {
				vertices.Add(new Vector3(x, noiseMap[x + z * LOD_SIZE1], z));
				vertices.Add(new Vector3(x + 1, noiseMap[(x + 1) + z * LOD_SIZE1], z));
				vertices.Add(new Vector3(x, noiseMap[x + (z + 1) * LOD_SIZE1], z + 1));
				vertices.Add(new Vector3(x + 1, noiseMap[(x + 1) + (z + 1) * LOD_SIZE1], z + 1));

				float texture = 2;//noiseMap[x + z * LOD_SIZE1] > 0 ? 2 : 4;
				tex.Add(new Vector2(0, texture / TEX_SIZE));
				tex.Add(new Vector2(1, texture / TEX_SIZE));
				tex.Add(new Vector2(0, (texture + 1) / TEX_SIZE));
				tex.Add(new Vector2(1, (texture + 1) / TEX_SIZE));

				Vector3 a = new Vector3(x + 1, noiseMap[(x + 1) + z * LOD_SIZE1], z) - new Vector3(x, noiseMap[x + z * LOD_SIZE1], z);
				Vector3 b = new Vector3(x, noiseMap[x + (z + 1) * LOD_SIZE1], z + 1) - new Vector3(x, noiseMap[x + z * LOD_SIZE1], z);
				Vector3 normal = -new Vector3(
					(a.Y * b.Z) - (a.Z * b.Y),
					(a.Z * b.X) - (a.X * b.Z),
					(a.X * b.Y) - (a.Y * b.X)
				);

				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				indices.Add(index);
				indices.Add(index + 1);
				indices.Add(index + 2);
				indices.Add(index + 3);
				indices.Add(index + 2);
				indices.Add(index + 1);
				
				index += 4;
			}
		}
		
		arrays[(int) Mesh.ArrayType.Vertex] = Variant.From(vertices.ToArray());
		arrays[(int) Mesh.ArrayType.Normal] = Variant.From(normals.ToArray());
		arrays[(int) Mesh.ArrayType.TexUV] = Variant.From(tex.ToArray());
		arrays[(int) Mesh.ArrayType.Index] = Variant.From(indices.ToArray());
		
		((ArrayMesh) meshInstance.Mesh).AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		needsCompilation = false;

		double end_t = Time.GetTicksUsec();
		
		//GD.Print("LOD Took msec: ", (end_t - t) * 0.001);
		return (end_t - t) * 0.001;
	}
}