using Godot;
using System;

public partial class DebugMenu : Control
{
	Player player;

	Label pos;
	Label rot;
	Label vel;
	Label fps;
	Label fpsAvg;
	Label compTime;

	double averageFPS;
	ulong averageFPScount;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = (Player) GetParent().GetParent().GetNode("World/Player");

		pos = (Label) GetNode("TopLeft/Position");
		rot = (Label) GetNode("TopLeft/Rotation");
		vel = (Label) GetNode("TopLeft/Velocity");
		fps = (Label) GetNode("TopRight/FPS/Counter");
		fpsAvg = (Label) GetNode("TopRight/AverageFPS/Counter");
		compTime = (Label) GetNode("CompTime");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		pos.Text = "Pos: " + ((player.GlobalPosition * 100).Round() / 100).ToString();
		rot.Text = "Rot: " + ((((Camera3D) player.GetNode("Camera3D")).GlobalRotationDegrees * 100).Round() / 100).ToString();
		vel.Text = "Vel: " + ((player.Velocity * 100).Round() / 100).ToString();
		compTime.Text = "Cmp. time: " + Math.Round(Chunk.AverageTime, 2).ToString() + "ms";

		double fpsTime = Engine.GetFramesPerSecond();
		averageFPScount++;
		averageFPS *= ((double) averageFPScount - 1) / averageFPScount;
		averageFPS += fpsTime / averageFPScount;

		fps.Text = Math.Round(fpsTime).ToString();
		fpsAvg.Text = Math.Round(averageFPS).ToString();
	}


	public void ResetAverageFPS()
	{
		averageFPScount = 1;
		((Timer) GetNode("ResetAverageFPS")).Start();
	}
}
