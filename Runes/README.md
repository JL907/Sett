# Runes

Adds a **Keystone** loadout slot to every survivor, vanilla or modded, with six League-style keystones. Originally part of [SettMod](https://thunderstore.io/package/lemonlust/SettMod/), now usable by everyone.

Runes was created under Riot Games' "Legal Jibber Jabber" policy using assets owned by Riot Games. Riot Games does not endorse or sponsor this project.

## Keystones

| Keystone | Effect (default config) |
|---|---|
| **Conqueror** | Hits grant a stack, up to 12. Each stack gives +0.6 base damage (+0.045 every 4 levels). At max stacks, heal for 6% of damage dealt. |
| **Lethal Tempo** | Hits grant a stack, up to 6. Each stack gives +10% attack speed (60% at max). |
| **Stormraider's Surge** | Dealing 25% of an enemy's max health within 3s gives +48% movement speed and 50% slow resistance for 4s. Cooldown goes from 20s at level 1 down to 10s at level 18. Replaces Phase Rush, and takes over its loadout pick. |
| **Electrocute** | 3 hits on the same target within 3s strike it with lightning for 600% damage (+75% every 4 levels). 5s cooldown per target. |
| **Grasp of the Undying** | Every 4s, your next hit deals bonus damage equal to 5% of your max health, heals you for 2% of your max health, and permanently gives +1 max health for the rest of the run. |
| **Glacial Augment** | Stunning, freezing, shocking or rooting an enemy (Stun Grenade counts) sends 3 glacial rays from it towards you and nearby allies. The frozen zones last 3s plus the stun's duration; enemies inside are slowed 20% and deal 15% less damage. 25s cooldown. |

A hit is anything with a proc coefficient above 0, so damage-over-time ticks don't build stacks. Stormraider's Surge is the exception: all damage counts toward its threshold. Stacks start falling off after 4s (Conqueror) or 6s (Lethal Tempo) without landing a hit.

Every survivor defaults to **No Keystone**, so nothing changes until you pick one in the loadout screen.

## Config

`BepInEx/config/com.Lemonlust.Runes.cfg`

- **General → Enable For All Survivors**: turn off to give the slot only to survivors whose mod opts in.
- **Survivors**: one toggle per survivor. Filled in after the first launch.
- One section per keystone with all its numbers. Descriptions in game update to match.

In multiplayer, everyone should use the same **Survivors** toggles. Turning a survivor off removes a loadout slot, and players whose configs disagree will see mismatched loadouts.

## For modders

Reference `Runes.dll`, add a dependency, and call the API from your plugin's `Awake`:

```csharp
[BepInDependency(Runes.KeystonePlugin.GUID)]
public class MyPlugin : BaseUnityPlugin
{
    void Awake()
    {
        // ...after your body prefab exists
        Runes.KeystoneAPI.RegisterSurvivor(myBodyPrefab); // slot even if "all survivors" is off
        // or Runes.KeystoneAPI.ExcludeSurvivor(myBodyPrefab);
    }
}
```

`KeystoneAPI.GetKeystone(body)` returns the keystone a body has selected, and the buff defs are exposed for anyone who wants to react to stacks.

## Changelog

**1.0.0**
- Split out of SettMod 4.6.3. Works on every survivor, adds a No Keystone option, config for every number, and a public API.
