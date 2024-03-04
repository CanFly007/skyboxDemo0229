# Skybox Demo
## Main Features
- Real-Time Skybox Switching
- PBR with metallic-roughness workflow
- Near-Real-Time Image-Based Lighting Computation
- Capture the color and direction of the brightest area

## Setup
### Windows building
For visual studio 2019:
```bash
cmake -S . -B build -G "Visual Studio 16 2019" -A x64
```

### Mac building
For Xcode:
```bash
brew install cmake assimp glm glfw freetype
cmake -S . -B build -G Xcode
```
## Example
![Example Image](https://github.com/CanFly007/skyboxDemo0229/blob/master/resources/textures/output.png)

![Example Image](https://github.com/CanFly007/skyboxDemo0229/blob/master/resources/textures/output2.png)
