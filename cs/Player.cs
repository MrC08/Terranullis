using System;
using Godot;

public partial class Player : CharacterBody3D
{
	const float ACCELERATION = 60f;
	const float SPEED = 5f;
	const float JUMP_VELOCITY = 7f;
	const float TERMINAL_VELOCITY = 50f;

	Camera3D camera;
	RayCast3D raycast;
	World world;

	bool flying = false;
	float flightSpeed = SPEED;


	public override void _Ready()
	{
		camera = (Camera3D) GetNode("Camera3D");
		raycast = (RayCast3D) GetNode("RayCast3D");
		world = (World) GetParent();
		
		camera.MakeCurrent();
	}


	public override void _PhysicsProcess(double delta)
	{
		float deltaf = (float) Math.Clamp(delta, 0, 1);

		Vector3 acceleration = Vector3.Zero;

		if (flying)
		{
			if (Input.IsActionPressed("crouch"))
			{
				acceleration = acceleration with {Y = -flightSpeed};
			} else if (Input.IsActionPressed("jump"))
			{
				acceleration = acceleration with {Y = flightSpeed};
			} else
			{
				acceleration = acceleration with {Y = 0f};
			}
		} else
		{
			if (!IsOnFloor())
			{
				acceleration = GetGravity() * deltaf;
			} else if (Input.IsActionJustPressed("jump"))
			{
				acceleration = acceleration with {Y = JUMP_VELOCITY};
			}
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (!direction.IsZeroApprox())
		{
			if (!flying)
				acceleration = (direction * ACCELERATION * deltaf) with {Y = acceleration.Y};
			else
				acceleration = (direction * flightSpeed) with {Y = acceleration.Y};
		}
		
		if (!flying) {
			acceleration += (-Velocity * 0.2f) with {Y = 0};

			Velocity += acceleration;

			if ((Velocity with {Y = 0f}).Length() > (flying ? flightSpeed : SPEED)) {
				float yVel = Velocity.Y;
				Velocity = ((Velocity with {Y = 0f}).Normalized() * (flying ? flightSpeed : SPEED)) with {Y = yVel};
			}
			if (Velocity.Y < -TERMINAL_VELOCITY)
				Velocity = Velocity with {Y = -TERMINAL_VELOCITY};
		} else
		{
			Velocity = acceleration;
		}

		if (Velocity.IsZeroApprox())
			Velocity = Vector3.Zero;

		MoveAndSlide();
		UpdateRaycast(false);
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
						world.SetBlock(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * -0.01f, 0);
						UpdateRaycast(true);
				} else if (buttonEvent.ButtonIndex == MouseButton.Right)
				{
					if (raycast.IsColliding())
						world.SetBlock(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * 0.01f, 2);
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