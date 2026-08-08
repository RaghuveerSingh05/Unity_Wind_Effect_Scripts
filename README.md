# Unity Wind Physics System

A dynamic wind simulation system for Unity with real-time UI controls, tree sway, falling leaves, and physics interactions.

---

## 📋 Overview

This project demonstrates a realistic outdoor environment with dynamic wind effects. It includes:

- **Custom wind controller** with direction, strength, and speed control
- **Runtime UI** for adjusting wind properties in real-time
- **Tree sway** using transform-based rotation (reliable in builds)
- **Falling leaves** with particle system and wind influence
- **Physics object** (ball) reacting to wind forces
- **Toby Foliage Engine** integration for grass and tree shaders

---

## 🚀 Features

### Wind Controller (DynamicWindController.cs)
- Wind direction control (X and Z axis)
- Wind strength control (0-3)
- Wind speed control (0.1-5)
- Random wind variation with adjustable intervals
- Force applied to rigidbody objects and particles
- Runtime UI updates without scene restart

### UI System (WindSettingsUI.cs)
- Press **E** to open/close settings panel
- Sliders for: Strength, Speed, Direction X, Direction Z
- Toggle for Random Wind
- Real-time value display
- Mouse cursor management

<img width="1488" height="826" alt="Screenshot 2026-08-07 172426" src="https://github.com/user-attachments/assets/7cdc8e5d-ef04-4910-ae67-c451107c1ed0" />

### Tree Leaf System (TreeLeafController.cs)
- Custom oval leaf meshes
- Wind direction affects leaf drift
- Wind strength affects fall speed
- Leaves rotate while falling
- Activation threshold prevents movement when wind is low
- Custom spawn points per tree

<img width="1492" height="831" alt="Screenshot 2026-08-07 173007" src="https://github.com/user-attachments/assets/ffa15b8e-114c-4ac9-814e-c64ed2029b91" />

### Physics Ball (WindBall.cs)
- Rigidbody with configurable mass and drag
- Wind force applied in FixedUpdate
- Activation threshold for minimal movement
- Realistic gravity and friction

---

## 🛠️ Scripts

| Script | Description |
|--------|-------------|
| `DynamicWindController.cs` | Main wind system with singleton pattern |
| `WindSettingsUI.cs` | Runtime UI controls with Input System |
| `TreeLeafController.cs` | Falling leaves particle system with wind |
| `WindBall.cs` | Physics ball reacting to wind |
| `ManualTreeWind.cs` | Transform-based tree sway (build reliable) |

---



## 📦 Requirements

- Unity 6
- Unity Input System Package
- Toby Foliage Engine (for grass/tree shaders)
- European Forests - Realistic Trees asset (optional)

---

## 🎮 Controls

| Key | Action |
|-----|--------|
| **E** | Open/Close wind settings panel |
| **ESC** | Close wind settings panel |
| **Sliders** | Adjust wind properties in real-time |



