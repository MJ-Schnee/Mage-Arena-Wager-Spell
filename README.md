# Wager Spell
*Take a chance, take a-take a chance-chance, take a chance, take a chance*

A spell with a chance to hit yourself instead of your target.
High risk, high reward.
Cast this spell and take a chance.

## Spell Information
* **Name**: "Wager"
* **Cooldown**: 60 seconds
* **Range**: 20 meters maximum
* **Damage**: 1000 damage
* **Cone of Vision**: 45°
* **Chance**: 50% to target yourself
* **Team Chest**: Can spawn in team chest
* **Visuals**: Small explosion on effected player
* **Sound**: Small explosion on effected player and jackpot when spell doesn't backfire

## Installation

1. **Prerequisites**:
   - BepInEx 5.4.21
   - ModSync
   - BlackMagicAPI

2. **Installation**:
- After downloading the mod, place the files in the following locations:

   ```
   MageArena/
   └── BepInEx/
       └── plugins/
           └── WagerSpell/
			   └── WagerSpell.dll
			   └── Sprites/
			       └── Wager_Main.png
			       └── Wager_Emission.png
			       └── Wager_Ui.png
			   └── Sounds/
			       └── Explode.wav
			       └── Jackpot.wav
			   └── AssetBundles/
			       └── explosion_prefab
   ```
## Configuration
Edit `BepInEx/config/com.YeahThatsMJ.WagerSpell.cfg` to adjust settings.

Compatible with [MageConfigurationAPI by D1GQ](https://thunderstore.io/c/mage-arena/p/D1GQ/MageConfigurationAPI/)!

The following settings can be adjusted:
* Cooldown
* Range
* Damage
* Chance
* Team chest spawning

## Changelog
**0.6.1**
* Utilize BlackMagicAPI for Asset Bundle loading

**0.6.0**
* Explosion and jackpot sound when spell hits another player

**0.5.0**
* Add explosion visual effect

**0.4.0**
* Add jackpot sound effect

**0.3.0**
* Add explosion sound effect

**0.2.0**
* Add sprites

**0.1.0**
* Implement basic version of spell

## Credits
* **Explode sound effect**: "Explosion 1.ogg" by filipex2000 - https://freesound.org/s/426439/
* **Jackpot sound effect**: "Playing and winning Slot Machine Jackpot - Sound Effect for editing" by SoundLibrary1 https://www.youtube.com/watch?v=qPwANcErhDU
* **Explode visual effect**: "Free Quick Effects Vol. 1" - Gabriel Aguiar Prod https://assetstore.unity.com/packages/vfx/particles/free-quick-effects-vol-1-304424
