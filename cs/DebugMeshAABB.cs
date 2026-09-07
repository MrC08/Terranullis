using Godot;
using System;

public partial class DebugMeshAABB : MeshInstance3D
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (((Chunk)GetParent().GetParent()).Position2D.X % 5 != 0)
			QueueFree();
		if (((Chunk)GetParent().GetParent()).Position2D.Y % 5 != 0)
			QueueFree();

		if (((Chunk)GetParent().GetParent()).needsCompilation)
			return;

		ImmediateMesh immediate = new();
		immediate.SurfaceBegin(Mesh.PrimitiveType.Lines);

		Aabb aabb = ((MeshInstance3D)GetParent()).GetAabb();

		var p = aabb.Position;
		var s = aabb.Size;

		Vector3[] v = [
			p,
			p + new Vector3(s.X, 0, 0),
			p + new Vector3(s.X, s.Y, 0),
			p + new Vector3(0, s.Y,  0),
			p + new Vector3(0, 0, s.Z),
			p + new Vector3(s.X, 0, s.Z),
			p + new Vector3(s.X, s.Y, s.Z),
			p + new Vector3(0, s.Y, s.Z)
		];

		// Bottom
		immediate.SurfaceAddVertex(v[0]);
		immediate.SurfaceAddVertex(v[1]);

		immediate.SurfaceAddVertex(v[1]);
		immediate.SurfaceAddVertex(v[2]);

		immediate.SurfaceAddVertex(v[2]);
		immediate.SurfaceAddVertex(v[3]);

		immediate.SurfaceAddVertex(v[3]);
		immediate.SurfaceAddVertex(v[0]);


		// Top
		immediate.SurfaceAddVertex(v[4]);
		immediate.SurfaceAddVertex(v[5]);

		immediate.SurfaceAddVertex(v[5]);
		immediate.SurfaceAddVertex(v[6]);

		immediate.SurfaceAddVertex(v[6]);
		immediate.SurfaceAddVertex(v[7]);

		immediate.SurfaceAddVertex(v[7]);
		immediate.SurfaceAddVertex(v[4]);


		// Sides
		immediate.SurfaceAddVertex(v[0]);
		immediate.SurfaceAddVertex(v[4]);

		immediate.SurfaceAddVertex(v[1]);
		immediate.SurfaceAddVertex(v[5]);

		immediate.SurfaceAddVertex(v[2]);
		immediate.SurfaceAddVertex(v[6]);

		immediate.SurfaceAddVertex(v[3]);
		immediate.SurfaceAddVertex(v[7]);

		immediate.SurfaceEnd();

		this.Mesh = immediate;
	}
}
