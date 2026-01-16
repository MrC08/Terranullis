using System.Linq.Expressions;
using Godot;

public partial class DebugGenerateWorld : Sprite2D
{
	Image img;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Generator.Init();
		Texture = Generator.LatestProgress;

		img = Image.CreateEmpty(360, 180, false, Image.Format.Rgb8);
		img.Fill(new Color(0, 0, 0));
		((Sprite2D) GetNode("AirOverlay")).Texture = ImageTexture.CreateFromImage(img);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!Generator.WorldGenerated) {
			Generator.StepGenerateWorld();
			Texture = Generator.LatestProgress;
			return;
		}

		Sprite2D test = (Sprite2D) GetNode("test");

		for (int i = 0; i < 1000; i++)
		{
			test.Position += Generator.AirCurrentMap[(int) test.Position.X + 180][(int) test.Position.Y + 90];

			if (test.Position.X >= 180 || test.Position.Y <= -90 || test.Position.Y >= 90)
				test.Position = new Vector2(-180, (GD.Randf() - 0.5f) * 180f);

			img.SetPixelv(
				new Vector2I((int) test.Position.X + 180, (int) test.Position.Y + 90),
				img.GetPixelv(
					new Vector2I((int) test.Position.X + 180, (int) test.Position.Y + 90))
				+ new Color(0.01f, 0.01f, 0.01f));
		}
		((ImageTexture) ((Sprite2D) GetNode("AirOverlay")).Texture).Update(img);

		Line2D cursor = (Line2D) GetNode("Cursor");
		cursor.GlobalPosition = GetGlobalMousePosition();
		Vector2I pos = new Vector2I((int) cursor.GlobalPosition.X / 2 + 180, (int) cursor.GlobalPosition.Y / 2 + 90);

		if (pos.X > 0 && pos.Y > 0 && pos.X < 360 && pos.Y < 180)
		{
			cursor.Visible = true;
			cursor.SetPointPosition(1, Generator.AirCurrentMap[pos.X][pos.Y] * 20);
		} else
			cursor.Visible = false;
	}
}
