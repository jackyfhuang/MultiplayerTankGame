# Multiplayer Tank Game

A Unity 2D tank shooting game built for multiplayer functionality.

## Features

- **Tank Controller**: Move and rotate your tank using WASD or Arrow keys
- **Bullet Shooting**: Press Spacebar to shoot bullets
- **Collision Detection**: Bullets ignore the tank that shot them and have a grace period to prevent immediate self-collision

## Controls

- **WASD** or **Arrow Keys**: Move forward/backward and rotate the tank
- **Spacebar**: Shoot bullets

## Project Structure

- `Assets/Bullet.cs` - Bullet behavior script with collision handling
- `Assets/TankController.cs` - Tank movement and shooting controller
- `Assets/PlayerTank.prefab` - Tank prefab
- `Assets/Bullet.prefab` - Bullet prefab
- `Assets/Scenes/SampleScene.unity` - Main game scene

## Setup

1. Open the project in Unity (tested with Unity 6000.3.11f1)
2. Open the `SampleScene` scene
3. Ensure the PlayerTank prefab is in the scene with:
   - Bullet prefab assigned to the `bulletPrefab` field
   - FirePoint transform assigned to the `firePoint` field
4. Press Play to start the game

## Requirements

- Unity 2024.3 or later
- Universal Render Pipeline (URP)
- Input System package

## License

This project is for educational purposes.
