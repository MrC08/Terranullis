using System.Collections.Generic;
using Godot;

public struct Face
{
	public enum Facing
	{
		UP, DOWN, LEFT, RIGHT, BACK, FORWARD
	}

	public Vector3 position;

	public int texture;

	public Facing facing;

	public void Add(List<Vector3> vertices, List<Vector3> normals, List<Vector2> tex, List<int> indices, int index)
	{
		if (facing == Facing.LEFT)
		{
			vertices.Add(position);
			vertices.Add(position + Vector3.Back);
			vertices.Add(position + Vector3.Up + Vector3.Back);
			vertices.Add(position + Vector3.Up);
		
			indices.Add(index);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 2);

			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));

			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
		} else if (facing == Facing.RIGHT)
		{
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z));
			
			indices.Add(index);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 2);
			indices.Add(index + 3);

			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
		} else if (facing == Facing.DOWN)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z));
			vertices.Add(new Vector3(position.X, position.Y, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z + 1));

			float texture = 3;
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));

			normals.Add(Vector3.Down);
			normals.Add(Vector3.Down);
			normals.Add(Vector3.Down);
			normals.Add(Vector3.Down);

			indices.Add(index);
			indices.Add(index + 2);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 2);
		} else if (facing == Facing.UP)
		{
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z + 1));

			float texture = 2;
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));

			normals.Add(Vector3.Up);
			normals.Add(Vector3.Up);
			normals.Add(Vector3.Up);
			normals.Add(Vector3.Up);

			indices.Add(index);
			indices.Add(index + 1);
			indices.Add(index + 2);
			indices.Add(index + 3);
			indices.Add(index + 2);
			indices.Add(index + 1);
		} else if (facing == Facing.FORWARD)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z));

			float texture = 1;
			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
			
			normals.Add(Vector3.Forward);
			normals.Add(Vector3.Forward);
			normals.Add(Vector3.Forward);
			normals.Add(Vector3.Forward);
			
			indices.Add(index);
			indices.Add(index + 1);
			indices.Add(index + 2);
			indices.Add(index + 3);
			indices.Add(index + 2);
			indices.Add(index + 1);
		} else
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z + 1));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z + 1));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1, position.Z + 1));

			float texture = 1;
			tex.Add(new Vector2(0, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, (texture + 1) / Chunk.TEX_SIZE));
			tex.Add(new Vector2(0, texture / Chunk.TEX_SIZE));
			tex.Add(new Vector2(1, texture / Chunk.TEX_SIZE));
			
			normals.Add(Vector3.Back);
			normals.Add(Vector3.Back);
			normals.Add(Vector3.Back);
			normals.Add(Vector3.Back);
			
			indices.Add(index + 2);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 1);
			indices.Add(index);
			indices.Add(index + 2);
		}
	}

	public void Set(Vector3 position, int texture, Facing facing)
	{
		this.position = position;
		this.texture = texture;
		this.facing = facing;
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
			f.facing == facing &&
			f.position.Equals(position);
	}

	public override int GetHashCode()
	{
		throw new System.NotImplementedException();
	}
}