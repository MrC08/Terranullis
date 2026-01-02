using System.Collections.Generic;
using Godot;

public struct Face
{
	public enum Facing
	{
		UP, DOWN, LEFT, RIGHT, BACK, FORWARD, NULL
	}

	private const float e0 = -0.0002f;
	private const float e1 = 1.0002f;

	public Vector3I position;
	public int texture;
	public Facing facing;
	public int continuation;

	public Face()
	{
		position = Vector3I.MinValue;
		texture = -1;
		facing = Facing.NULL;
		continuation = 1;
	}

	public bool Add(List<Vector3> vertices, List<Vector3> normals, List<float> tex, List<int> indices, int index)
	{
		if (facing == Facing.LEFT)
		{
			vertices.Add(new Vector3(position.X, position.Y + e0, position.Z + e0));
			vertices.Add(new Vector3(position.X, position.Y + e0, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X, position.Y + e1, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X, position.Y + e1, position.Z + e0));
		
			indices.Add(index);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 2);

			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);

			continuation = 1;
			return true;
		} else if (facing == Facing.RIGHT)
		{
			vertices.Add(new Vector3(position.X + 1f, position.Y + e0, position.Z + e0));
			vertices.Add(new Vector3(position.X + 1f, position.Y + e0, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X + 1f, position.Y + e1, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X + 1f, position.Y + e1, position.Z + e0));
			
			indices.Add(index);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 2);
			indices.Add(index + 3);

			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);

			continuation = 1;
			return true;
		} else if (facing == Facing.DOWN)
		{
			vertices.Add(new Vector3(position.X + e0, position.Y, position.Z + e0));
			vertices.Add(new Vector3(position.X + e1, position.Y, position.Z + e0));
			vertices.Add(new Vector3(position.X + e0, position.Y, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X + e1, position.Y, position.Z + continuation * e1));

			float texture = 3;
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

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

			continuation = 1;
			return true;
		} else if (facing == Facing.UP)
		{
			vertices.Add(new Vector3(position.X + e0, position.Y + 1f, position.Z + e0));
			vertices.Add(new Vector3(position.X + e1, position.Y + 1f, position.Z + e0));
			vertices.Add(new Vector3(position.X + e0, position.Y + 1f, position.Z + continuation * e1));
			vertices.Add(new Vector3(position.X + e1, position.Y + 1f, position.Z + continuation * e1));

			float texture = 2;
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

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

			continuation = 1;
			return true;
		} else if (facing == Facing.FORWARD)
		{
			vertices.Add(new Vector3(position.X + e0, position.Y + e0, position.Z));
			vertices.Add(new Vector3(position.X + continuation * e1, position.Y + e0, position.Z));
			vertices.Add(new Vector3(position.X + e0, position.Y + e1, position.Z));
			vertices.Add(new Vector3(position.X + continuation * e1, position.Y + e1, position.Z));

			float texture = 1;
			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
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

			continuation = 1;
			return true;
		} else if (facing == Facing.BACK)
		{
			vertices.Add(new Vector3(position.X + e0, position.Y + e0, position.Z + 1f));
			vertices.Add(new Vector3(position.X + continuation * e1, position.Y + e0, position.Z + 1f));
			vertices.Add(new Vector3(position.X + e0, position.Y + e1, position.Z + 1f));
			vertices.Add(new Vector3(position.X + continuation * e1, position.Y + e1, position.Z + 1f));

			float texture = 1;
			tex.Add(0.0001f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 1) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add(texture / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
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

			continuation = 1;
			return true;
		}

		return false;
	}

	public void Set(Vector3I position, int texture, Facing facing)
	{
		this.position = position;
		this.texture = texture;
		this.facing = facing;
	}

	public void CopyFrom(Face other)
	{
		position = other.position;
		texture = other.texture;
		facing = other.facing;
		continuation = other.continuation;
	}

	public bool Continues(Face other)
	{
		if (other.texture != texture || other.facing != facing)
			return false;
		
		if (facing == Facing.BACK || facing == Facing.FORWARD)
			return (
				other.position.X == position.X + continuation &&
				other.position.Y == position.Y &&
				other.position.Z == position.Z
			);

		return (
			other.position.X == position.X &&
			other.position.Y == position.Y &&
			other.position.Z == position.Z + continuation
		);
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