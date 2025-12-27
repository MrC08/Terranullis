extends CharacterBody3D

const SPEED = 5.0
const JUMP_VELOCITY = 4.5

@onready var camera := $Camera3D

var flying := true
var flight_speed := SPEED

func _ready():
	camera.make_current()


func _physics_process(delta: float) -> void:
	if flying:
		if Input.is_action_pressed("crouch"):
			velocity.y = -flight_speed
		elif Input.is_action_pressed("jump"):
			velocity.y = flight_speed
		else:
			velocity.y = move_toward(velocity.y, 0, flight_speed)
	
	else:
		if not is_on_floor():
			velocity += get_gravity() * delta

		if Input.is_action_just_pressed("jump") and is_on_floor():
			velocity.y = JUMP_VELOCITY

	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		velocity.x = direction.x * (SPEED if !flying else flight_speed)
		velocity.z = direction.z * (SPEED if !flying else flight_speed)
	else:
		velocity.x = move_toward(velocity.x, 0, (SPEED if !flying else flight_speed))
		velocity.z = move_toward(velocity.z, 0, (SPEED if !flying else flight_speed))

	move_and_slide()


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		rotation.y -= event.screen_relative.x / 800
		camera.rotation.x -= event.screen_relative.y / 800


	elif event is InputEventMouseButton:
		if event.is_pressed():
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	
			if event.button_index == MOUSE_BUTTON_WHEEL_UP:
				flight_speed *= 1.1
			elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
				flight_speed *= 0.9


	elif event is InputEventKey:
		if event.keycode == KEY_ESCAPE:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
