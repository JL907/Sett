## Latest Patch Notes `2.0.0`
* Added Pool Party Sett Skin
* Added Obsidian Dragon Skin
    * Skins will temporarly be set as new survivors.

---

## Old Patch Notes
`1.5.1`
* Fixed an issue where Haymaker's indicator was being positioned improperly
* Fixed an issue where Haymaker's indicator did not spawn at all

`1.5.0`
* Fixed an issue where Facebreaker was doing the backward onhit animation on enemies infront of sett
* Added minimum duration before detonate on next frame check on The Showstopper    
    * This should prevent The Showstopper from detonating instantly in some situations
* Sett's M1 has a new Left Punch animation while moving or in the air
* Sett's M1 has a new swing visual effect
* Sett's M1 has a new impact visual effect

`1.4.3`
* Fixed The Showstopper using incorrect coefficient value for target maximum health.
* Updated Plugin to use latest version of R2API

`1.4.2`
* Added SFX when selected on character select screen
* Removed Sett looping through two animations on character select screen
* Adjusted Sett's collision

`1.4.1`
* Fixed Standing Knuckle Down animation interrupt on movement
* Facebreaker now has an idle animation after cast while not moving
* Facebreaker on hit animations now have an idle animation on hit while not moving
* Haymaker now has a new animation if cast with full grit
* Haymaker startup duration is now 0.78 (down from 0.836)
* Haymaker now has an idle animation after cast while not moving
* Fixed Haymaker animation interrupt on movement
* The Showstopper impact effect is now less pronounced

`1.4.0`
* Fixed an issue where Sett's M1 was usable during skills
* Fixed an issue where Sett can cancel his dash into other skills
* Added extra check to detonate The Showstopper early if Sett hits terrain
* The Showstopper now bounces sett up 4 units (down from 5)
* The Showstopper now has 2 new animations and will pick randomly between three on cast.
* Facebreaker start up and duration now scale off attack speed
* Haymaker startup duration is 0.836 (up from 0.8)
* Fixed Haymaker using it's damage coefficient as it's proc coefficient. Proc coefficient is now 1 (down from 14)

`1.3.1`
* Fixed an issue where Sett's Grit had weird interaction with the item Transcendence.
* Fixed an issue where Facebreaker was using the wrong on hit animation
* Camera now zooms out during The Showstopper
* Adjusted Sett's M1 sound.

`1.3.0`
* Fixed an issue where Facebreaker wasn't displaying the on hit animation
* Added swing sound effect to Knuckle Down and reduced the frequency of grunt sound effect
* Lowered volume on hit sound effects.
* Sett's Movement Speed is now 8 M/S (up from 7 M/S)
* Facebreaker now properly displays correct radius in description.
* Facebreaker now applies a slow as well as a stun.
* Fixed issue where The Showstopper was canceling sprint.

`1.2.8`
* Bug Fix: Problem from 1.2.7 where Sett would only left punch.
* Bug Fix: Health regen was using the wrong value causing instant healing.
* Setts Base Health is now 180 (up from 168)
* Setts Gains 48 Health Per Level (up from 46)
* Setts Health Regen Per Level is 0.2 (up from 0)
* Setts Base Armor is now 20 (down from 33)
* Setts Armor Growth Per Level is now 0 (down from 3.3)
* Setts Base Damage is now 12 (down from 15)
* Setts Damage Growth Per Level is now 2.8 (down from 3.0)
* Knuckle Down Damage is now 280%/360% (Down from 350%/500%)
* Facebreaker Damage is now 400% (Down from 800%)
* The Showstopper's Bonus Damage is now 5% of Primary Target's Maximum Health (Down from 10%)

`1.2.7`
* Sett now gains 46 health per level (up from 23)
* Sett Configuration file now has options to change Setts base attributes as well as his skill's damage, cooldowns, etc.
* The Showstopper now pops Sett up 5 units from the ground (down from 10)

`1.2.6`
* Fixed an issue with The Showstopper impacting itself on enemy colliders such as Greater Wisps.
* Facebreaker now has 200 units of suction up from 100 units.

`1.2.5`
* Fixed an issue that allowed Sett to become Invincible

`1.2.4`
* The Showstopper slam radius has increased to 20 units up from 15 units.
* Sett now regenerates 0.25 (+0.25 every 4 levels) health per second every 5% missing health. Base health regen has changed down to 1 from 5.

`Patch 1.2.3`
* Fixed an issue where The Showstopper was doing 0 damage during multiplayer games
* Fixed an issue where The Showstopper would damage Sett during multiplayer games
* Fixed an issue where The Showstopper impact wouldn't pop sett up during multiplayer games
* Fixed an issue where The Haymaker was not showing visual effect during multiplayer games
* Fixed an issue where The Haymaker would update the position of its indicator incorrectly.
* Fixed an issue where Setts primary visual would display twice during its duration during multiplayer games
* Fixed an issue where the mod's file size was tripled.

`Patch 1.2.2`
* Fixed an issue where Haymaker was doing no damage to some bosses
* Fixed an issue where Facebreaker was not sucking in some enemies
* The Showstopper bounces Sett a bit higher on impact to prevent clipping.
* The Showstopper has a fixed height and no longer scales height with movement speed.
* The Showstopper's initial dash velocity is faster.
* Haymaker has 200% less base damage however the bonus Grit damage has increased to +300% from 50%.
* Haymaker now has an indicator (Indicator may sometimes not appear, this is a known issue.)
* Haymaker has a slightly tweaked visual effect.
* Haymaker hit box overlaps Sett a bit more.
* Grit now displays numerical value on the bar.

`Patch 1.2.1`
* Fixed an Issue with The Showstopper not bouncing Sett upon landing.

`Patch 1.2.0`
* The Showstopper now checks if sett is on stable ground before detonating. 
* The Showstopper detonates early if hits collision.
* The Showstopper detonates after a fixed amount of time if sett can't find any collision.
* Facebreaker now pulls all enemies around sett. 
* Facebreaker now has an indicator.
* Facebreaker updated visual effect.
* Facebreaker has an updated description.
* Haymaker updated visual effect.
* Haymaker now does TRUE damage.
* Haymaker should now hit enemies directly in front of Sett.
* Fixed an issue that caused The Showstopper to retain its hidden invincibility after the skill ended

`Patch 1.1.0`
* Fixed aiming with Knuckle Down and Facebreaker.
* Knuckle Down has Increased hit box on the sides.
* Facebreaker now has a wider angle and now originates from character position rather than camera origin.
* Sett has rag doll on death.
* Voice line when selected in character select.
* Grit now decays 4 seconds after not taking any damage.
* Footstep sounds and dust effect while running/sprinting.
* Updated Sett's color theme.

`Patch 1.0.0`
* Initial Release
