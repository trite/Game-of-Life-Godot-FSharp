extends TextureRect

@export var shader_material: ShaderMaterial;

var first_pass := true;

var done := false;

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
#  self.texture = $"..".get_texture()
	# pass # Replace with function body.
#	self.material = load("res://ShaderWorld2.tres");
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
#	if first_pass:
#		first_pass = false;
#	elif !done:
#		self.material = shader_material;
#		done = true;
		

		
