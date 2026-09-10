using Godot;
using System;

[Tool]
public partial class SkyManager : WorldEnvironment
{
	Player player;
	DirectionalLight3D sun;

	[Export] Gradient zenithColorGradient;
	[Export] Gradient horizonColorGradient;
	[Export] Gradient nadirColorGradient;

	[Export] Color zenithDuskColor;
	[Export] Color horizonDuskColor;
	[Export] Color nadirDuskColor;

	[Export] Gradient nightGradient;
	[Export] Gradient nearFadeGradient;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = (Player) GetParent().GetNode("Player");
		sun = (DirectionalLight3D) GetNode("Sun");
	}


	public override void _Process(double delta)
	{
		Vector3 playerPos = Vector3.Zero;


		if (!Engine.IsEditorHint()) {
			playerPos = Util.SmartPosToAbsPos(player.GlobalPosition);

			sun.RotateZ((float) delta * 0.004363323f);
			//sun.RotateZ((float) delta * 0.3f);
			sun.Transform = sun.Transform.Orthonormalized();

			sun.GlobalPosition = playerPos;
		} else {
			sun = (DirectionalLight3D) GetNode("Sun");
		}

		Color zenith = zenithColorGradient.Sample(playerPos.Y / 256);
		Color horizon = horizonColorGradient.Sample(playerPos.Y / 256);
		Color nadir = nadirColorGradient.Sample(playerPos.Y / 256);

		float solarAltitude = sun.GlobalTransform.Basis.Z.Y * -90f; // In degrees
		float sunsetFactor = (float) Math.Pow(1 - Math.Abs(Math.Min(solarAltitude + 2.5, 89)) / 90, 3.0) / 2f;

		sun.LightEnergy = Math.Clamp(-solarAltitude * 0.125f, 0, 1);
		sun.ShadowEnabled = sun.LightEnergy > 0;

		if (solarAltitude > 0) {
			float nightFactor = MathF.Min(1, solarAltitude / 360f);
			zenith *= nightGradient.Sample(nightFactor);
			horizon *= nightGradient.Sample(nightFactor);
			nadir *= nightGradient.Sample(nightFactor);

			sunsetFactor = (float) Math.Pow(sunsetFactor, Math.Max(1, solarAltitude / 20f));
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

		RenderingServer.GlobalShaderParameterSet("sky_zenith_color", zenith);
		RenderingServer.GlobalShaderParameterSet("sky_horizon_color", horizon);
		RenderingServer.GlobalShaderParameterSet("sky_nadir_color", nadir);
		RenderingServer.GlobalShaderParameterSet("sun_dir", sun.GlobalTransform.Basis.Z.Normalized());
		RenderingServer.GlobalShaderParameterSet("near_fade_color", nearFadeGradient.Sample(Math.Min(1f, Math.Max(0f, (solarAltitude + 15f) / 60f))));
	}
}
