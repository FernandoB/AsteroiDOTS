# AsteroiDOTS

Asteroids like game developed in Unity DOTS

### Unity version
  - 2020.2.7f1
  
### ECS packages
  - com.unity.entities: 0.17.0-preview.42
  - com.unity.rendering.hybrid: 0.11.0-preview.44
  - com.unity.physics: 0.6.0-preview.3

### Notes

  - Play the scene called **MainScene** to start the game
  - Although the game runs fine in any aspect ratio, for the best experience play in a 36:25 aspect ratio.
  - Disable "Low Resolution Aspect Ratios"
  <img width="202" alt="Screenshot 2023-04-19 at 14 54 12" src="https://user-images.githubusercontent.com/1079323/233160853-dd40490d-a493-4825-afe3-b131cf066d00.png">

### Controls

  - W, A, D to control the player's movement
  - Space to fire the current weapon
  - H for hyperspace

### Project

  - The source code is located in the Source folder and is divided into three subfolders: 
    - Game
    - Components
    - Systems
  - The Game folder is the GameObject part of the game. There is the MainGame class that handles the GameStart and GameEnd states, the UI, and visual and sound fx.
  - Components and Systems folders are the ECS part. Each one has the following subfolders which in turn contain the code for each relevant part of the game:
    - Player
    - Weapon
    - Asteroids
    - AlienShip
    - Powerups
    - Score
    - GameState
    - FX 


