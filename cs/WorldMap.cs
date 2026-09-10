using Godot;

public partial class WorldMap : TextureRect
{
	CharacterBody3D player;
	TextureRect marker;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		player = (CharacterBody3D) GetNode("../../World/Player");
		marker = (TextureRect) GetNode("Marker");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Texture = Generator.LatestProgress;

		Vector2 markerPos = Util.SmartPosToAbsPosXZ(player.GlobalPosition);
		markerPos /= 500f;

		markerPos.X += 180f;
		markerPos.Y += 90f;

		markerPos *= 2f;

		marker.Position = markerPos;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Keycode == Key.M && keyEvent.Pressed)
			{
				Visible = !Visible;
				if (Visible)
					Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		} else if (@event is InputEventMouseButton mouseEvent)
		{
			if (Visible && mouseEvent.Pressed)
			{
				Vector3 pos = new Vector3(GetLocalMousePosition().X, 530, GetLocalMousePosition().Y) / 2;

				/*GD.Print(
					pos.Z - 90, "° lat, ", pos.X - 180, "° lon ",
					" Tem:", Generator.TemperatureMap[(int)pos.X][(int)pos.Z],
					" Hmd:", Generator.HumidityMap[(int)pos.X][(int)pos.Z],
					" Ele:", Generator.ElevationMap[(int)pos.X][(int)pos.Z],
					" Bio:", Generator.BiomeMap[(int)pos.X][(int)pos.Z]
				);*/

				pos.X -= 180;
				pos.Z -= 90;

				pos.X *= 500f;
				pos.Z *= 500f;

				Visible = !Visible;

				player.GlobalPosition = Util.AbsPosToSmartPos(pos);
			}
		}
	}
}