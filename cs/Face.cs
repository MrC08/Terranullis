using System.Collections.Generic;
using Godot;

public struct Face
{
	public enum Facing
	{
		UP, DOWN, WEST, EAST, SOUTH, NORTH, NULL
	}

	public Vector3I position;
	public ushort texture;
	public Facing facing;
	public int continuation;

	public Face()
	{
		position = Vector3I.MinValue;
		texture = ushort.MaxValue;
		facing = Facing.NULL;
		continuation = 1;
	}

	public bool Add(List<Vector3> vertices, List<Vector3> normals, List<float> tex, List<int> indices, int index)
	{
		if (facing == Facing.WEST)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
			vertices.Add(new Vector3(position.X, position.Y, position.Z + continuation));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z + continuation));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z));
		
			indices.Add(index);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 2);

			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);
			normals.Add(Vector3.Left);

			continuation = 1;
			return true;
		} else if (facing == Facing.EAST)
		{
			vertices.Add(new Vector3(position.X + 1f, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + 1f, position.Y, position.Z + continuation));
			vertices.Add(new Vector3(position.X + 1f, position.Y + 1, position.Z + continuation));
			vertices.Add(new Vector3(position.X + 1f, position.Y + 1, position.Z));
			
			indices.Add(index);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 2);
			indices.Add(index + 3);

			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);
			normals.Add(Vector3.Right);

			continuation = 1;
			return true;
		} else if (facing == Facing.DOWN)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z));
			vertices.Add(new Vector3(position.X, position.Y, position.Z + continuation));
			vertices.Add(new Vector3(position.X + 1, position.Y, position.Z + continuation));

			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

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
			vertices.Add(new Vector3(position.X, position.Y + 1f, position.Z));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1f, position.Z));
			vertices.Add(new Vector3(position.X, position.Y + 1f, position.Z + continuation));
			vertices.Add(new Vector3(position.X + 1, position.Y + 1f, position.Z + continuation));

			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

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
		} else if (facing == Facing.NORTH)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
			vertices.Add(new Vector3(position.X + continuation, position.Y, position.Z));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z));
			vertices.Add(new Vector3(position.X + continuation, position.Y + 1, position.Z));

			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
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
		} else if (facing == Facing.SOUTH)
		{
			vertices.Add(new Vector3(position.X, position.Y, position.Z + 1f));
			vertices.Add(new Vector3(position.X + continuation, position.Y, position.Z + 1f));
			vertices.Add(new Vector3(position.X, position.Y + 1, position.Z + 1f));
			vertices.Add(new Vector3(position.X + continuation, position.Y + 1, position.Z + 1f));

			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
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

	public void Set(Vector3I position, ushort texture, Facing facing)
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
		
		if (facing == Facing.SOUTH || facing == Facing.NORTH)
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