# Shadow Dash 2D

A fast-paced 2D endless runner built with Unity, focused on responsive movement and fair obstacle generation.

This project was built as a personal portfolio piece for Unity Game Developer internship applications.

## Overview

Shadow Dash 2D is a side-scrolling runner where the player survives as long as possible by jumping, double-jumping, and sliding through increasingly difficult obstacle patterns.

The core design goal is fairness: obstacles are only spawned when they are avoidable under current player physics and game speed.

## Core Features

- Player movement with jump, double jump, and slide
- Player feel helpers:
- jump buffer for more forgiving input timing
- slide buffer for responsive slide input
- Finite state machine for player animations and transitions
- Playability Guarantee system to avoid impossible obstacle patterns
- Weighted obstacle spawning by difficulty tier
- Dynamic difficulty over time via speed and spawn interval ramping
- Level progression: normal -> mid -> hard (time-based)
- Object pooling for obstacles and jump effects
- Parallax background with map transitions
- Main menu, pause menu, game over menu, and high score persistence
- Audio manager with pooled SFX sources and looping BGM

## Technical Highlights

### 1) Playability Guarantee (Fairness Algorithm)

Before spawning an obstacle, the game simulates jump trajectories within a reaction window and checks whether the obstacle can be avoided.

- Uses time-to-reach based on current world speed
- Simulates first jump and optional double jump timing
- Validates against obstacle top/bottom collider bounds
- Falls back gracefully if no currently playable obstacle is found

Main implementation:
- Assets/Scripts/Obstacles/PlayabilityChecker.cs
- Assets/Scripts/Obstacles/ObstacleSpawner.cs

### 2) Data-Driven Difficulty and Spawning

Obstacle types are ScriptableObject-driven and selected using weighted random logic per level tier.

- Tiered weights: normal, mid, hard
- Spawn interval ramps from easier to harder over time
- Obstacle unlock timings (Low, High, Spike, Floating)

Main implementation:
- Assets/Scripts/Core/LevelManager.cs
- Assets/Scripts/Obstacles/ObstacleSpawner.cs
- Assets/Scripts/Utils/WeightRandom.cs
- Assets/Scripts/Data/ObstacleBaseSO.cs

### 3) Performance-Oriented Runtime Systems

- Object pooling reduces Instantiate/Destroy overhead
- Shared managers for environment speed, score, audio, transitions
- Parallax rendering separated from gameplay logic

Main implementation:
- Assets/Scripts/Core/PoolManager.cs
- Assets/Scripts/Core/EnvironmentManager.cs
- Assets/Scripts/Core/ScoreManager.cs
- Assets/Scripts/Core/AudioManager.cs

## Project Structure

- Assets/Scripts/Core: Game flow, score, audio, pooling, input, level logic
- Assets/Scripts/Player: Controller, state machine, state data
- Assets/Scripts/Obstacles: Obstacle behavior, spawning, playability checks
- Assets/Scripts/Data: ScriptableObject definitions
- Assets/Scripts/Utils: Scene transition, parallax, weighted random helper, effects

## Controls

- Jump: Space (or Unity Input action mapped to Jump)
- Slide: key mapped to Slide input action

## Built With

- Unity (2D)
- C#
- TextMeshPro
- ScriptableObject-based configuration

## How to Run

1. Open the project in Unity.
2. Load the Main Menu scene.
3. Ensure Input actions include Jump and Slide.
4. Press Play in the Unity Editor.

## What I Focused On In This Project

- Clean separation between gameplay systems and data/configuration
- Building fair but challenging endless runner gameplay
- Implementing reusable runtime systems (pooling, managers, transitions)
- Writing internship-ready Unity C# architecture for review and extension

## Internship Portfolio Notes

If you are reviewing this project as a recruiter:

- The fairness/playability system is the key technical showcase.
- The project demonstrates gameplay programming, state management, and runtime optimization basics.
- Code is organized into focused modules for maintainability and iteration speed.
