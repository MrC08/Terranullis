using Godot;
using System;

public partial class DebugMenu : Control
{
	Player player;

	Label pos;
	Label rot;
	Label vel;
	Label fps;
	Label compTime;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = (Player) GetParent().GetParent().GetNode("World/Player");

		pos = (Label) GetNode("Position");
		rot = (Label) GetNode("Rotation");
		vel = (Label) GetNode("Velocity");
		fps = (Label) GetNode("FPS");
		compTime = (Label) GetNode("CompTime");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		pos.Text = "Pos: " + ((player.GlobalPosition * 100).Round() / 100).ToString();
		rot.Text = "Rot: " + ((((Camera3D) player.GetNode("Camera3D")).GlobalRotationDegrees * 100).Round() / 100).ToString();
		vel.Text = "Vel: " + ((player.Velocity * 100).Round() / 100).ToString();
		fps.Text = Math.Round(Engine.GetFramesPerSecond()).ToString() + " fps";
		compTime.Text = "Cmp. time: " + Math.Round(Chunk.AverageTime, 2).ToString() + "ms";
	}
}
