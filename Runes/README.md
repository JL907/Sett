# Runes
### Runes was created under Riot Games' "Legal Jibber Jabber" policy using assets owned by Riot Games.  Riot Games does not endorse or sponsor this project.

<p align="center">
  <h2>My League Mods</h2>
</p>

<p align="center">
  <a href="https://thunderstore.io/package/lemonlust/SettMod/"><img width="128" height="128" src="https://user-images.githubusercontent.com/7343912/167464106-0d8f197a-8f71-4446-8853-57dddc300b5b.png"></a>
  <a href="https://thunderstore.io/package/lemonlust/KatarinaMod/"><img width="128" height="128" src="https://user-images.githubusercontent.com/7343912/167464178-cdc05ba4-1a93-434b-8ef6-3cfb7de6c1f5.png"></a>
  <a href="https://thunderstore.io/package/lemonlust/JinxMod/"><img width="128" height="128" src="https://user-images.githubusercontent.com/7343912/167464222-08ee52e6-bf9e-4117-9bd8-70636d63b03b.png"></a>
  <a href="https://thunderstore.io/package/lemonlust/LuxMod/"><img width="128" height="128" src="https://github.com/user-attachments/assets/0c4a06d2-00eb-4cf7-ae7c-3bc38a7fefee"></a>
</p>

## Introduction
Runes adds a **Keystone** loadout slot to every survivor, vanilla or modded, with five League of Legends keystones to pick from. It started as part of [SettMod](https://thunderstore.io/package/lemonlust/SettMod/) and is now its own mod, so every survivor can use it.

Every survivor defaults to **No Keystone**, so nothing changes until you pick one in the loadout screen.

## Requirements
* Every player in a multiplayer lobby needs the mod, on the same version.
* Everyone should also use the same **Survivors** toggles in the config (see below).

## Patch Notes
See the [Changelog](https://thunderstore.io/c/riskofrain2/p/lemonlust/Runes/CHANGELOG).

`Bug reports can be posted in discord` https://discord.gg/aYuS9Wsxxx

## Discord
<a href="https://discord.gg/aYuS9Wsxxx" target="_blank">
  <img width="40%" border="0" align="center"  src="https://user-images.githubusercontent.com/7343912/134153480-837a1d90-18de-46cc-a58f-64920c58b7f9.png"/>
</a>

## Ko-Fi
<a href="https://ko-fi.com/lemonlust" target="_blank">
  <img width="40%" border="0" align="center"  src="https://uploads-ssl.webflow.com/5c14e387dab576fe667689cf/5cbed8a4cf61eceb26012821_SupportMe_red.png"/>
</a>

## Keystones

| | Keystone | Effect (default config) |
|---|---|---|
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/Conqueror_rune.png"> | **Conqueror** | Hits grant a stack, up to 12. Each stack gives +0.6 base damage (+0.045 every 4 levels). At max stacks, heal for 6% of damage dealt. |
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/Lethal_Tempo_rune.png"> | **Lethal Tempo** | Hits grant a stack, up to 6. Each stack gives +10% attack speed (60% at max). |
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/Stormraiders_Surge_rune.png"> | **Stormraider's Surge** | Dealing 25% of an enemy's max health within 3s gives +48% movement speed and 50% slow resistance for 4s. No cooldown: it can trigger again as soon as it ends. Replaces Phase Rush, and takes over its loadout pick. |
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/Electrocute_rune.png"> | **Electrocute** | Every 3 hits within 3s, on any enemies, strike the enemy you hit with lightning for 200% damage (+75% every 4 levels). Stacks stay with you, so kills don't waste them; if the third hit is a kill, the lightning jumps to the nearest enemy. 4s cooldown, during which hits don't build stacks. |
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/Grasp_of_the_Undying_rune.png"> | **Grasp of the Undying** | Every 4s, your next hit deals bonus damage equal to 5% of your max health, heals you for 2% of your max health, and permanently gives +1 max health for the rest of the run. |
| <img width="48" src="https://raw.githubusercontent.com/JL907/Sett/main/Runes/Icons/None_rune.png"> | **No Keystone** | The default. No effect. |

* A hit is anything with a proc coefficient above 0, so damage-over-time ticks don't build stacks. Stormraider's Surge is the exception: all damage counts toward its threshold.
* Stacks start falling off after 4s (Conqueror) or 6s (Lethal Tempo) without landing a hit.

## Config
`BepInEx/config/com.Lemonlust.Runes.cfg`

* **General → Enable For All Survivors**: turn off to give the slot only to survivors whose mod opts in.
* **Survivors**: one toggle per survivor, filled in after the first launch. Turning a survivor off removes its loadout slot, so in multiplayer players whose configs disagree will see mismatched loadouts.
* One section per keystone with all its numbers. The descriptions in game update to match.

## For Modders
Reference `Runes.dll`, add a dependency, and call the API from your plugin's `Awake` once your body prefab exists:

```csharp
[BepInDependency(Runes.KeystonePlugin.GUID)]
public class MyPlugin : BaseUnityPlugin
{
    void Awake()
    {
        Runes.KeystoneAPI.RegisterSurvivor(myBodyPrefab);
    }
}
```

* `KeystoneAPI.RegisterSurvivor(bodyPrefab)` gives a survivor the slot even when **Enable For All Survivors** is off.
* `KeystoneAPI.ExcludeSurvivor(bodyPrefab)` keeps a survivor from getting the slot.
* `KeystoneAPI.GetKeystone(body)` returns the keystone a body has selected, and the buff defs are exposed for anyone who wants to react to stacks.

## Credits
* Originally made for [SettMod](https://thunderstore.io/package/lemonlust/SettMod/)
* All the homies at the Risk of Rain 2 Modding Discord
* League of Legends
