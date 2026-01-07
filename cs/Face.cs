using System.Runtime.CompilerServices;
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

	[MethodImpl(MethodImplOptions.AggressiveOptimization)] // No, fr tho
	public bool Add(MeshSpan<Vector3> vertices, MeshSpan<Vector3> normals, MeshSpan<float> tex, MeshSpan<int> indices, int index)
	{
		if (facing == Facing.WEST)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X, position.Y, position.Z),
				new Vector3(position.X, position.Y, position.Z + continuation),
				new Vector3(position.X, position.Y + 1, position.Z + continuation),
				new Vector3(position.X, position.Y + 1, position.Z));
		
			indices.AddHextuplet(index,
				index + 3,
				index + 1,
				index + 1,
				index + 3,
				index + 2);

			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

			normals.AddQuadruplet(Vector3.Left);

			continuation = 1;
			return true;
		} else if (facing == Facing.EAST)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X + 1f, position.Y, position.Z),
				new Vector3(position.X + 1f, position.Y, position.Z + continuation),
				new Vector3(position.X + 1f, position.Y + 1, position.Z + continuation),
				new Vector3(position.X + 1f, position.Y + 1, position.Z));
			
			indices.AddHextuplet(index,
				index + 1,
				index + 3,
				index + 1,
				index + 2,
				index + 3);

			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
			normals.AddQuadruplet(Vector3.Right);

			continuation = 1;
			return true;
		} else if (facing == Facing.DOWN)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X, position.Y, position.Z),
				new Vector3(position.X + 1, position.Y, position.Z),
				new Vector3(position.X, position.Y, position.Z + continuation),
				new Vector3(position.X + 1, position.Y, position.Z + continuation));

			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

			normals.AddQuadruplet(Vector3.Down);

			indices.AddHextuplet(index,
				index + 2,
				index + 1,
				index + 3,
				index + 1,
				index + 2);

			continuation = 1;
			return true;
		} else if (facing == Facing.UP)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X, position.Y + 1f, position.Z),
				new Vector3(position.X + 1, position.Y + 1f, position.Z),
				new Vector3(position.X, position.Y + 1f, position.Z + continuation),
				new Vector3(position.X + 1, position.Y + 1f, position.Z + continuation));

			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);

			normals.AddQuadruplet(Vector3.Up);

			indices.AddHextuplet(index,
				index + 1,
				index + 2,
				index + 3,
				index + 2,
				index + 1);

			continuation = 1;
			return true;
		} else if (facing == Facing.NORTH)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X, position.Y, position.Z),
				new Vector3(position.X + continuation, position.Y, position.Z),
				new Vector3(position.X, position.Y + 1, position.Z),
				new Vector3(position.X + continuation, position.Y + 1, position.Z));

			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
			normals.AddQuadruplet(Vector3.Forward);
			
			indices.AddHextuplet(index,
				index + 1,
				index + 2,
				index + 3,
				index + 2,
				index + 1);

			continuation = 1;
			return true;
		} else if (facing == Facing.SOUTH)
		{
			vertices.AddQuadruplet(
				new Vector3(position.X, position.Y, position.Z + 1f),
				new Vector3(position.X + continuation, position.Y, position.Z + 1f),
				new Vector3(position.X, position.Y + 1, position.Z + 1f),
				new Vector3(position.X + continuation, position.Y + 1, position.Z + 1f));

			tex.Add(0.0001f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.9999f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.0001f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			tex.Add(0.9999f); tex.Add((texture + 0.0001f) / Chunk.TEX_SIZE); tex.Add(1f / continuation); tex.Add(0);
			
			normals.AddQuadruplet(Vector3.Back);
			
			indices.AddHextuplet(index + 2,
				index + 3,
				index + 1,
				index + 1,
				index,
				index + 2);

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