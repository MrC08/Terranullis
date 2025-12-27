using System.Collections.Generic;
using Godot;

public struct Face
{
	public Vector3 position1;
	public Vector3 position2;
	public Vector3 position3;
	public Vector3 position4;

	public int texture;

	public int index1;
	public int index2;
	public int index3;
	public int index4;
	public int index5;
	public int index6;

	public Vector3 normal;

	public void add(List<Vector3> vertices, List<Vector3> normals, List<Vector2> tex, List<int> indices, int index)
	{
		vertices.Add(position1);
		vertices.Add(position2);
		vertices.Add(position3);
		vertices.Add(position4);

		tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
		tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
		tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
		tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
		
		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);
		
		indices.Add(index1);
		indices.Add(index2);
		indices.Add(index3);
		indices.Add(index4);
		indices.Add(index5);
		indices.Add(index6);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		
		Face f = (Face) obj;
		return
			f.texture == texture &&
			f.index1 == index1 &&
			f.index2 == index2 &&
			f.index3 == index3 &&
			f.index4 == index4 &&
			f.index5 == index5 &&
			f.index6 == index6 &&
			f.normal.Equals(normal) &&
			f.position1.Equals(position1) &&
			f.position2.Equals(position2) &&
			f.position3.Equals(position3) &&
			f.position4.Equals(position4);
	}

	public override int GetHashCode()
	{
		throw new System.NotImplementedException();
	}
}