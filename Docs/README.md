\# Stealth Platformer (Unity 6, 2D)

MVP: player stealth meters + StaticGuard.



\## Run

1\. Unity 6.x

2\. Open `Assets/\_Project/Scenes/SampleScene.unity`

3\. Play



\## Controls

Move: A/D or ←/→ • Jump: Space • Crouch: C



\## Project layout

Assets/

&nbsp; \_Project/

&nbsp;   Scenes/

&nbsp;     SampleScene.unity

&nbsp;     Sandbox\_Visibility.unity

&nbsp;     Sandbox\_Noise.unity

&nbsp;   Scripts/                      # already created

&nbsp;     Data/

&nbsp;     Environment/

&nbsp;     Player/

&nbsp;     UI/

&nbsp;   Data/                         # ScriptableObjects (versioned)

&nbsp;     NoiseProfile.asset

&nbsp;     VisibilitySettings.asset

&nbsp;     Audio/

&nbsp;       FootstepSurfaceMap.asset

&nbsp;     Gameplay/

&nbsp;       InputActions.inputactions

&nbsp;   Prefabs/

&nbsp;     Player/

&nbsp;       Player.prefab

&nbsp;       HUD.prefab

&nbsp;     Environment/

&nbsp;       Torch.prefab

&nbsp;       DarkPatch.prefab

&nbsp;     UI/

&nbsp;       Canvas\_Main.prefab

&nbsp;       NoiseBar.prefab

&nbsp;       VisibilityBar.prefab

&nbsp;   Art/

&nbsp;     Sprites/

&nbsp;       Player/

&nbsp;         player\_idle.png

&nbsp;         player\_run\_strip.png

&nbsp;       Environment/

&nbsp;         torch.png

&nbsp;         dark\_patch.png

&nbsp;         tileset\_ground.png

&nbsp;       UI/

&nbsp;         bar\_bg.png

&nbsp;         bar\_fill\_noise.png

&nbsp;         bar\_fill\_visibility.png

&nbsp;     Animations/

&nbsp;       Player/

&nbsp;         Player.controller

&nbsp;         player\_idle.anim

&nbsp;         player\_run.anim

&nbsp;     Materials/

&nbsp;       Sprites-Lit.mat

&nbsp;       Sprites-Unlit.mat

&nbsp;     Fonts/

&nbsp;       Inter-Regular.ttf

&nbsp;   Audio/

&nbsp;     SFX/

&nbsp;       footsteps\_grass\_01.wav

&nbsp;       footsteps\_grass\_02.wav

&nbsp;       jump.wav

&nbsp;       land\_soft.wav

&nbsp;       land\_hard.wav

&nbsp;       ui\_tick.wav

&nbsp;     Music/

&nbsp;       prototype\_loop.wav

&nbsp;     Mixers/

&nbsp;       Master.mixer

&nbsp;         SFX (group)

&nbsp;         Music (group)

&nbsp;   Physics/

&nbsp;     Materials/

&nbsp;       pm\_default.physicMaterial2D

&nbsp;       pm\_slippery.physicMaterial2D

&nbsp;   UI/

&nbsp;     Layouts/

&nbsp;       HudLayout.asset            # UI Toolkit or saved prefab variants

&nbsp;     Styles/

&nbsp;       ui\_styles.uss              # if using UI Toolkit

&nbsp;   Addressables/                  # optional; prefer over Resources/

&nbsp;     Groups.asset

&nbsp;   Settings/

&nbsp;     GraphicsSettings.asset

&nbsp;     Input/

&nbsp;       InputSystem.settings

&nbsp;     Quality/

&nbsp;       QualitySettings.asset

&nbsp;   Editor/

&nbsp;     BuildProfiles/

&nbsp;       Windows64.buildprofile

&nbsp;       Android.buildprofile

&nbsp;   Docs/

&nbsp;     README.md

&nbsp;     CHANGELOG.md

&nbsp; ThirdParty/                      # external libs or art packs



\## Roadmap

\- v0.1.0: MVP (player meters, StaticGuard)

\- v0.2.0: Patroller, search state

\- v0.3.0: Items (noise gadget), Android input



