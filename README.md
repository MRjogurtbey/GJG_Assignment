This project is a high-performance Match-2 / Blast game prototype developed as a technical submission. The primary objective was to demonstrate advanced proficiency in Unity architecture, algorithmic efficiency, and mobile performance optimization.

Software Architecture
The project follows a decoupled architecture based on the Single Responsibility Principle (SRP):

  GridManager (The Orchestrator): Acts as a lean manager that coordinates gameplay states, delegating specific tasks to specialized modules to avoid the "Fat Manager" anti-pattern.
  MatchUtility (Logic Layer): A pure C# static class responsible for match detection using a highly optimized Breadth-First Search (BFS) algorithm.
  BoardRefiller (Action Layer): Manages the complex physics of gravity and hole-filling, utilizing a callback system to notify the manager when animations are complete.
  BlockPooler (Performance Layer): Implements an Object Pooling pattern with "Pre-warming" capabilities to eliminate runtime $Instantiate$ and $Destroy$ overhead.
  GridCameraController (Presentation Layer): Automatically calculates camera bounds based on grid dimensions ($M \times N$) and screen aspect ratio for a responsive UI.
  
Performance Highlights
  Three major optimization vectors were prioritized to ensure a smooth 60 FPS experience on mobile hardware:
  1. CPU Optimization: Dirty Flag Pattern
Instead of re-scanning the entire grid after every move, the system identifies and updates only the "Dirty Columns" affected by a blast. This reduces the computational complexity of visual updates by approximately 80-90%.
2. Memory Management: Zero-Allocation Logic
To minimize Garbage Collector (GC) pressure, MatchUtility utilizes static, reusable collections (HashSet, List, Queue). This ensures that matching logic does not trigger memory allocations during gameplay, preventing micro-stutters.
3. GPU & Rendering: Sprite Atlas & SRP Batching
All game assets are packed into a single Sprite Atlas. By maintaining a consistent material and texture, the project achieves maximum batching efficiency, reducing the Draw Call count from hundreds to nearly 1-2 calls per frame.

Tech Stack
  Unity 6 (URP)
  C#
  ScriptableObjects: Utilized for data-driven level configurations ($M, N$ dimensions and color sets).
  DOTween: Used for fluid, non-blocking visual feedback and animations.
