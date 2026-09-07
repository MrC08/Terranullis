using Godot;

public partial class GenerateWorld : Sprite2D
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
			((Label) GetNode("../../Step")).Text = Generator.GetNextGenerationStep();
			Generator.StepGenerateWorld();
			Texture = Generator.LatestProgress;
			return;
		}

		GetTree().ChangeSceneToFile("res://scenes/main.tscn");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Keycode == Key.R)
			{
				Generator.Init(GD.Randi());
			}
		}
	}
}
