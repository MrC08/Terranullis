using System;
using Godot;

public partial class Player : CharacterBody3D
{
	const float SPEED = 5f;
	const float JUMP_VELOCITY = 7f;

	Camera3D camera;
	RayCast3D raycast;
	World world;

	bool flying = true;
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

		if (flying)
		{
			if (Input.IsActionPressed("crouch"))
			{
				Velocity = new Vector3(Velocity.X, -flightSpeed, Velocity.Z);
			} else if (Input.IsActionPressed("jump"))
			{
				Velocity = new Vector3(Velocity.X, flightSpeed, Velocity.Z);
			} else
			{
				//Velocity = new Vector3(Velocity.X, Mathf.MoveToward(Velocity.Y, 0, flightSpeed), Velocity.Z);
				Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
			}
		} else
		{
			if (!IsOnFloor())
			{
				Velocity += GetGravity() * deltaf;
			} else if (Input.IsActionJustPressed("jump"))
			{
				Velocity = new Vector3(Velocity.X, JUMP_VELOCITY, Velocity.Z);
			}
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (!direction.IsZeroApprox())
		{
			Velocity = new Vector3(
				direction.X * (flying ? flightSpeed : SPEED),
				Velocity.Y,
				direction.Z * (flying ? flightSpeed : SPEED)
			);
		} else
		{
			Velocity = new Vector3(
				Velocity.X * 0.5f * deltaf,
				Velocity.Y,
				Velocity.Z * 0.5f * deltaf
			);
		}

		MoveAndSlide();
		raycast.TargetPosition = camera.Transform.Basis.Y * 4f;
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
					world.SetBlock(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * -0.01f, 0);
				} else if (buttonEvent.ButtonIndex == MouseButton.Right)
				{
					world.SetBlock(raycast.GetCollisionPoint() + raycast.GetCollisionNormal() * 0.01f, 1);
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
			}
		}
	}
}