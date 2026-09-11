using System;
using Godot;

public partial class Player : Node3D
{
	const float ACCELERATION = 60f;
	const float RUN_SPEED = 5.8f;
	const float WALK_SPEED = 3f;
	const float JUMP_VELOCITY = 7f;
	const float TERMINAL_VELOCITY = 50f;

	// Matches the capsule this replaced: radius 0.4, height 1.9, centered 1.0 above the player origin.
	const float HALF_WIDTH = 0.4f;
	const float FEET_OFFSET = 0.05f;
	const float HEIGHT = 1.9f;

	Camera3D camera;
	RayCast3D raycast;
	World world;
	MeshInstance3D ultraFarLOD;

	public Vector3 Velocity;
	bool onFloor = false;
	Vector3 gravity;
	float lastJumpPressedTime = float.NegativeInfinity;

	bool flying = false;
	float flightSpeed = RUN_SPEED;


	public override void _Ready()
	{
		camera = (Camera3D) GetNode("Camera3D");
		raycast = (RayCast3D) GetNode("RayCast3D");
		world = (World) GetParent();
		ultraFarLOD = (MeshInstance3D) GetNode("UltraFarLOD");
		
		camera.MakeCurrent();

		((ShaderMaterial) ultraFarLOD.MaterialOverride).SetShaderParameter("heightmap", Generator.ShaderReadyElevationMap);

		Vector3 gravityDir = (Vector3) ProjectSettings.GetSetting("physics/3d/default_gravity_vector", Vector3.Down);
		float gravityMag = (float) ProjectSettings.GetSetting("physics/3d/default_gravity", 9.8f);
		gravity = gravityDir * gravityMag;
	}


	public override void _Process(double delta)
	{
		float deltaf = (float)Math.Clamp(delta, 0, 1);

		Vector3 acceleration = Vector3.Zero;

		if (Input.IsActionJustPressed("jump")) {
			lastJumpPressedTime = Time.GetTicksMsec();
		}

		if (flying)
		{
			if (Input.IsActionPressed("crouch"))
			{
				acceleration = acceleration with { Y = -flightSpeed };
			}
			else if (Input.IsActionPressed("jump"))
			{
				acceleration = acceleration with { Y = flightSpeed };
			}
			else
			{
				acceleration = acceleration with { Y = 0f };
			}
		}
		else
		{
			if (!onFloor)
			{
				acceleration = gravity * deltaf;
			}
			else if (Time.GetTicksMsec() - lastJumpPressedTime < 100f)
			{
				acceleration = acceleration with { Y = JUMP_VELOCITY };
				lastJumpPressedTime = float.NegativeInfinity;
			}
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (!direction.IsZeroApprox())
		{
			if (!flying)
			{
				if ((Velocity with { Y = 0f }).Length() < WALK_SPEED)
				{
					acceleration = (direction * ACCELERATION * deltaf) with { Y = acceleration.Y };
				}
			}
			else
			{
				acceleration = (direction * flightSpeed) with { Y = acceleration.Y };
			}
		}

		if (!flying)
		{
			Velocity += acceleration;

			if (Velocity.Y < -TERMINAL_VELOCITY)
				Velocity = Velocity with { Y = -TERMINAL_VELOCITY };
		}
		else
		{
			Velocity = acceleration;
		}

		Velocity -= (Velocity with { Y = 0f}) * deltaf * 5f;

		if (Velocity.IsZeroApprox())
			Velocity = Vector3.Zero;

		Vector3 entityMin = Position + new Vector3(-HALF_WIDTH, FEET_OFFSET, -HALF_WIDTH);
		Vector3 entityMax = Position + new Vector3(HALF_WIDTH, FEET_OFFSET + HEIGHT, HALF_WIDTH);

		CollisionResult result = world.CheckWorldSmartPosAABB(entityMin, entityMax, Velocity, deltaf);

		Position = Util.AbsPosToSmartPos(result.AbsPosition) + new Vector3(HALF_WIDTH, -FEET_OFFSET, HALF_WIDTH);
		Velocity = result.Velocity;
		onFloor = result.OnFloor;

		UpdateRaycast(false);

		ultraFarLOD.GlobalPosition = ((GlobalPosition / 32f).Floor() * 32f) with {Y = 0f};
		ultraFarLOD.GlobalRotation = Vector3.Zero;
		((ShaderMaterial) ultraFarLOD.MaterialOverride).SetShaderParameter("offset", new Vector2(Util.smartPosOffsetX, Util.smartPosOffsetZ));
		((ShaderMaterial) ultraFarLOD.MaterialOverride).SetShaderParameter("near_limit", (world.primaryRenderLODDistance - 6) * LOD.LOD_SIZE);
		((ShaderMaterial) ultraFarLOD.MaterialOverride).SetShaderParameter("far_color", new Vector3(0.3f, 0.5f, 0.7f)); // TODO: Make this change with the day/night cycle
	}


	public void UpdateRaycast(bool forceUpdate)
	{
		raycast.TargetPosition = camera.Transform.Basis.Y * 4f;
		if (forceUpdate)
			raycast.ForceRaycastUpdate();
		Vector3 pos = (raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * -0.01f).Floor() + new Vector3(0.5f, 0.5f, 0.5f);

		((Node3D) raycast.GetNode("Node3D")).GlobalPosition = pos;
		((Node3D) raycast.GetNode("Node3D")).Visible = raycast.IsColliding();
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Rotation -= new Vector3(0, motionEvent.ScreenRelative.X / 800, 0);
				camera.Rotation -= new Vector3(motionEvent.ScreenRelative.Y / 800, 0, 0);
			}
		} else if (@event is InputEventMouseButton buttonEvent)
		{
			if (buttonEvent.IsPressed())
			{
				if (Input.MouseMode != Input.MouseModeEnum.Captured)
				{
					Input.MouseMode = Input.MouseModeEnum.Captured;
				} else if (buttonEvent.ButtonIndex == MouseButton.WheelUp)
				{
					flightSpeed *= 1.1f;
				} else if (buttonEvent.ButtonIndex == MouseButton.WheelDown)
				{
					flightSpeed *= 0.9f;
				} else if (buttonEvent.ButtonIndex == MouseButton.Left)
				{
					if (raycast.IsColliding())
						world.SetBlock(Util.SmartPosToAbsPos(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * -0.01f), 0);
						UpdateRaycast(true);
				} else if (buttonEvent.ButtonIndex == MouseButton.Right)
				{
					if (raycast.IsColliding())
						world.SetBlock(Util.SmartPosToAbsPos(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * 0.01f), 2);
						UpdateRaycast(true);
				}
			}
		} else if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			} else if (Input.IsActionJustPressed("F11"))
			{
				DisplayServer.WindowSetMode(
					DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ?
					DisplayServer.WindowMode.Windowed :
					DisplayServer.WindowMode.Fullscreen);
			} else if (Input.IsActionJustPressed("tab"))
			{
				flying = !flying;
			}
		}
	}
}