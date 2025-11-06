# Unity 60 FPS Optimization Guide

This guide provides comprehensive instructions for optimizing your Unity project to achieve consistent 60 FPS performance.

## Part 1: Unity Editor Settings (Manual Configuration)

### 1. Project Settings - Player

**General Settings:**
- Color Space: Linear (better for rendering pipeline)
- Auto Graphics API: Unchecked (manual control)
- Graphics APIs: Vulkan/Metal for mobile, DirectX 11/12 for desktop

**Frame Rate Settings:**
- Target Frame Rate: 60 (set via script, not in settings)
- VSync Count: Don't Sync (0) - crucial for 60 FPS control

**Mobile-Specific:**
- Default Orientation: Auto Rotation (Landscape Left/Right only)
- Rendering: 
  - Graphics Jobs: Enabled
  - Lightmap Encoding: Low Quality
  - HDR Cubemap Encoding: Low Quality

### 2. Project Settings - Quality

Use the automated Project FPS Optimizer script, or manually configure:

**Very Low (Emergency Mode):**
- Pixel Light Count: 0
- Shadows: Disable
- Texture Quality: Quarter Res (Mipmap Limit: 2)
- Anti Aliasing: Disabled
- V Sync Count: Don't Sync

**Low (Performance Mode):**
- Pixel Light Count: 0
- Shadows: Hard Shadows Only (if needed)
- Shadow Resolution: Low
- Shadow Distance: 15
- Texture Quality: Half Res (Mipmap Limit: 1)
- LOD Bias: 0.5
- Maximum LOD Level: 1

**Medium (Balanced Mode):**
- Pixel Light Count: 1
- Shadows: Hard Shadows Only
- Shadow Resolution: Low
- Shadow Distance: 25
- Texture Quality: Full Res
- LOD Bias: 0.7
- Maximum LOD Level: 0

### 3. Project Settings - Physics

**Timing:**
- Fixed Timestep: 0.02 (50 Hz) or 0.0167 (60 Hz max)
- Maximum Allowed Timestep: 0.033 (30 FPS minimum)

**Solver:**
- Default Solver Iterations: 4 (reduced from 6)
- Default Solver Velocity Iterations: 1
- Bounce Threshold: 2
- Sleep Threshold: 0.005

### 4. Project Settings - Time

**Frame Rate Management:**
- Maximum Delta Time: 0.033 (prevents spiral of death)
- Maximum Particle Delta Time: 0.033

### 5. Universal Render Pipeline (URP) Asset

**General:**
- Render Scale: 0.85-0.9 for mobile, 1.0 for desktop
- Upscaling Filter: FidelityFX Super Resolution 1.0
- HDR: Disabled for mobile

**Lighting:**
- Main Light: Pixel (if shadows needed) or Vertex
- Cast Shadows: Enabled only if necessary
- Shadow Resolution: 256 (mobile) / 512 (desktop)
- Additional Lights: Disabled or Per Vertex
- Additional Light Shadows: Disabled

**Shadows:**
- Max Distance: 25 (mobile) / 40 (desktop)
- Cascade Count: 1 (mobile) / 2 (desktop)
- Soft Shadows: Disabled

**Advanced:**
- SRP Batcher: Enabled
- Dynamic Batching: Disabled
- Mixed Lighting: Disabled
- Debug Level: Disabled

## Part 2: Code Implementation

### 1. Add Performance Manager Scripts

Add these scripts to your project in the following order:

1. **ConsistentFPSManager.cs** - Main performance management
2. **RuntimeQualityLock.cs** - Prevents runtime quality changes
3. **ProjectFPSOptimizer.cs** - Editor tool for project-wide optimization

### 2. Scene Setup

**In your main/startup scene:**

1. Create empty GameObject named "PerformanceManager"
2. Add `ConsistentFPSManager` component
3. Configure settings:
   - Target FPS: 60
   - Enable Adaptive Quality: true
   - Max Quality Level: 2 (Medium) for mobile, 3 for desktop
   - Show FPS Display: true (for testing)

4. Add `RuntimeQualityLock` component
5. Configure settings:
   - Lock Quality On Start: true
   - Locked Frame Rate: 60
   - Enable Emergency Mode: true

### 3. Prefab Creation

1. Make the PerformanceManager a prefab
2. Set to DontDestroyOnLoad
3. Add to all scenes or use auto-initialization

## Part 3: Content Optimization

### 1. Texture Optimization

**Texture Import Settings:**
- Max Size: 1024 (mobile), 2048 (desktop) maximum
- Compression: ASTC 6x6 (mobile), BC7 (desktop)
- Generate Mip Maps: Enabled
- Filter Mode: Bilinear
- Aniso Level: 1

**Texture Streaming:**
- Enable Texture Streaming in Quality Settings
- Memory Budget: 256MB (mobile), 512MB (desktop)

### 2. Mesh Optimization

**Model Import Settings:**
- Read/Write Enabled: Disabled
- Optimize Mesh: Enabled
- Generate Colliders: Only when needed
- Normals: Import (don't calculate)
- Tangents: Import or Calculate Mikktspace

**LOD Groups:**
- Use LOD groups for complex models
- LOD 0: 100% (0-30m)
- LOD 1: 50% (30-60m)
- LOD 2: 25% (60m+)

### 3. Audio Optimization

**Audio Import Settings:**
- Load Type: Compressed In Memory (small files) / Streaming (large files)
- Compression Format: Vorbis (mobile), PCM (desktop)
- Quality: 70% for mobile
- Sample Rate Setting: Preserve Sample Rate

### 4. Animation Optimization

**Animation Settings:**
- Anim. Compression: Keyframe Reduction
- Rotation Error: 0.5
- Position Error: 0.5
- Scale Error: 0.5
- Remove redundant keyframes

## Part 4: Runtime Optimization Strategies

### 1. Object Pooling

Implement object pooling for:
- Projectiles
- UI elements
- Particle effects
- Frequently spawned objects

### 2. Culling Optimization

**Camera Settings:**
- Far Clip Plane: As small as possible
- Near Clip Plane: 0.1 minimum
- Culling Mask: Only render necessary layers

**Occlusion Culling:**
- Bake occlusion data for static geometry
- Enable on cameras: `camera.useOcclusionCulling = true`

### 3. Batching Optimization

**Static Batching:**
- Mark static objects as "Static"
- Group small objects with same material

**GPU Instancing:**
- Use Graphics.DrawMeshInstanced for repeated objects
- Enable GPU Instancing on materials

### 4. Lighting Optimization

**Lighting Strategy:**
- Baked lighting for static objects
- Single directional light (sun/moon)
- Avoid real-time point lights
- Use Light Probes for dynamic objects

## Part 5: Platform-Specific Optimizations

### Mobile (iOS/Android)

**Additional Settings:**
- Install Location: Auto (Android)
- Internet Access: Not Required
- Write Access: Internal Only
- Graphics Jobs: Enabled
- Arm64: Required

**URP Mobile Renderer:**
- Depth Texture: Disabled
- Opaque Texture: Disabled
- Render Scale: 0.85
- MSAA: Disabled

### Desktop (Windows/Mac/Linux)

**Settings:**
- Fullscreen Mode: Exclusive Fullscreen
- Default Screen Width/Height: 1920x1080
- Resizable Window: Enabled
- Graphics Jobs: Enabled

**URP Desktop Renderer:**
- Render Scale: 1.0
- MSAA: 2x maximum
- HDR: Can be enabled if needed

## Part 6: Monitoring and Testing

### 1. Performance Profilers

**Unity Profiler:**
- CPU Usage: <16.67ms for 60 FPS
- GPU: Monitor frame time
- Memory: Keep below 1GB on mobile
- Rendering: Batches <100, Draw Calls <100

**Built-in Monitoring:**
- Use ConsistentFPSManager's display
- Monitor for emergency mode activation
- Check quality level changes

### 2. Test Scenarios

**Performance Tests:**
- Worst-case scenarios (many objects)
- Level transitions
- UI-heavy scenes
- Extended play sessions (memory leaks)

**Device Testing:**
- Minimum spec devices
- Battery drain tests
- Thermal throttling scenarios

## Part 7: Troubleshooting Common Issues

### Frame Rate Drops

1. Check CPU usage in Profiler
2. Verify texture memory usage
3. Look for garbage collection spikes
4. Monitor draw calls and batches

### Quality Inconsistency

1. Ensure RuntimeQualityLock is active
2. Check for external quality changes
3. Verify URP asset configuration
4. Monitor emergency mode triggers

### Memory Issues

1. Enable texture streaming
2. Implement aggressive garbage collection
3. Use object pooling
4. Monitor for memory leaks

## Automated Setup

Run the Project FPS Optimizer from **Tools > Performance > 60 FPS Project Optimizer** to automatically apply most of these settings.