# 2D Action RPG ⚔️

## 📖 Overview
A 2D Action Role-Playing Game developed from scratch using Unity and C#. This personal project is built to demonstrate core game development mechanics and strong Software Engineering principles, specifically focusing on Object-Oriented Programming (OOP), clean code architecture, and performance optimization.

**[🎮 Click here to play the WebGL version on Itch.io] (Insert your Itch.io link here)**
*(Or insert a YouTube link to your Gameplay Demo)*

## ✨ Technical Highlights
* **Robust OOP Architecture:** Designed a scalable `Entity` base class that encapsulates core logic (health management, physics, collision detection, and damage feedback). This class is efficiently inherited by `Player`, `Enemy`, and `ObjectToProtect` classes, reducing code duplication and enhancing maintainability.
* **Dynamic Enemy Spawning System:** Developed an `Enemy_Respawner` system with dynamic cooldown reduction and safety caps. It manages object instantiation efficiently while preventing memory overload and gameplay imbalance.
* **Physics & Combat Mechanics:** Utilized Unity's `Physics2D` (Raycast for ground detection, OverlapCircle for attack hitboxes) to create precise and responsive combat interactions.
* **State-Driven Animations:** Integrated smooth transitions using Unity Animator and decoupled logic using Animation Events (`Entity_AnimationEvents`).

## 🎮 Controls
* **A / D** or **Left / Right Arrows:** Move
* **Space:** Jump
* **Left Mouse Click:** Attack

## 🛠️ Technologies Used
* **Game Engine:** Unity (2D Core)
* **Programming Language:** C#
* **Version Control:** Git & GitHub

## 📂 Project Structure Snapshot
* `Scripts/`: Contains all C# scripts, neatly categorized (e.g., Core, Player, Enemies, UI).
* `Prefabs/`: Reusable GameObjects like Enemies and Environment props.
* `Animations/`: Animator controllers and animation clips.

## 🚀 How to Run Locally
1. Clone this repository: 
   ```bash
   git clone [https://github.com/your-username/your-repo-name.git](https://github.com/your-username/your-repo-name.git)