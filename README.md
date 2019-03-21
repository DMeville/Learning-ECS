Trying to learn how to use pure ECS in Unity. Many of the tutorials on youtube and documentation are out of date so I've been stumbling through it. Keeping a record as I work through learning.

# Using
- Unity 2019.1.0b5
- Entities preview.29 - 0.0.12
- Hybrid Renderer preview.9 - 0.0.1
- Burst preview.6 - 1.0.0

# 01-SpawnAndRender
Sets up bootstrap that creates a bunch of entities, and renders them.

# 02-Archetypes
Same thing, but uses archetypes when instantiating entities

# 03-BasicSystem
Setting up our first basic system that moves our objects in one direction at a constant speed. Also adds the MovementSpeed component for containing the data needed.

# 04-BasicSystem2
Same thing, but adding rotation to our system. Testing if it's better to have one system for each rotation and translation, or if it's better to do them both at the same time. 
