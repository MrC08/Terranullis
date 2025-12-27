using Godot;
using System;

[Tool]
public partial class SkyManager : WorldEnvironment
{
	CharacterBody3D player;
	DirectionalLight3D sun;

	[Export] Gradient zenithColorGradient;
	[Export] Gradient horizonColorGradient;
	[Export] Gradient nadirColorGradient;

	[Export] Color zenithDuskColor;
	[Export] Color horizonDuskColor;
	[Export] Color nadirDuskColor;

	[Export] Gradient nightGradient;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = (CharacterBody3D) GetParent().GetNode("Player");
		sun = (DirectionalLight3D) GetNode("Sun");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 playerPos = Vector3.Zero;


		if (!Engine.IsEditorHint()) {
			playerPos = player.GlobalPosition;

			//sun.RotateX((float) delta / 10);
			sun.Rotation = sun.Rotation + new Vector3((float) delta * 0.004363323f * 10f, 0, 0);
		}

		if (sun.Rotation.X > Math.PI)
			sun.Rotation -= new Vector3(2 * MathF.PI, 0, 0);

		Color zenith = zenithColorGradient.Sample(playerPos.Y / 256);
		Color horizon = horizonColorGradient.Sample(playerPos.Y / 256);
		Color nadir = nadirColorGradient.Sample(playerPos.Y / 256);

		float solarAltitude = MathF.Abs(sun.RotationDegrees.X) > 90 ? (Math.Sign(sun.RotationDegrees.X) * 180) - sun.RotationDegrees.X : sun.RotationDegrees.X;
		float sunsetFactor = (float) Math.Pow(1 - Math.Abs(Math.Min(solarAltitude + 2.5, 89)) / 90, 2.75) / 1.85f;


		if (solarAltitude > 0) {
			sun.LightEnergy = Math.Max(0, 1 - solarAltitude / 5);
			((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).SunAngleMax = Math.Max(0, 25 - solarAltitude * 18);

			float nightFactor = MathF.Min(1, solarAltitude / 360f);
			zenith *= nightGradient.Sample(nightFactor);
			horizon *= nightGradient.Sample(nightFactor);
			nadir *= nightGradient.Sample(nightFactor);

			sunsetFactor = (float) Math.Pow(sunsetFactor, Math.Max(1, solarAltitude / 20f));
		} else {
			sun.LightEnergy = 1;
			((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).SunAngleMax = 25;
		}

		zenith = zenith.Lerp(zenithDuskColor, sunsetFactor);
		horizon = horizon.Lerp(horizonDuskColor, sunsetFactor);
		nadir = nadir.Lerp(nadirDuskColor, sunsetFactor);

		if (playerPos.Y > 512)
		{
			zenith = zenith.Darkened((playerPos.Y - 512f) / 2400f);
			horizon = horizon.Darkened((playerPos.Y - 512f) / 2400f);
			nadir = nadir.Darkened((playerPos.Y - 512f) / 2400f);
		}

		((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).SkyTopColor = zenith;
		
		((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).SkyHorizonColor = horizon;
		((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).GroundHorizonColor = horizon;
		
		((ProceduralSkyMaterial) Environment.Sky.SkyMaterial).GroundBottomColor = nadir;
	}
}
