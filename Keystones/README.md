# Keystones

Adds a **Keystone** loadout slot to every survivor, vanilla or modded, with four League-style keystones. Originally part of [SettMod](https://thunderstore.io/package/lemonlust/SettMod/), now usable by everyone.

Keystones was created under Riot Games' "Legal Jibber Jabber" policy using assets owned by Riot Games. Riot Games does not endorse or sponsor this project.

## Keystones

| Keystone | Effect (default config) |
|---|---|
| **Conqueror** | Hits grant a stack, up to 12. Each stack gives +0.6 base damage (+0.045 every 4 levels). At max stacks, heal for 6% of damage dealt. |
| **Lethal Tempo** | Hits grant a stack, up to 6. Each stack gives +10% attack speed (60% at max). |
| **Phase Rush** | 3 hits on the same target within 4s give +30% movement speed (+5% every 4 levels) for 3s. Kills also trigger it. |
| **Electrocute** | 3 hits on the same target within 3s strike it with lightning for 600% damage (+75% every 4 levels). 5s cooldown per target. |

A hit is anything with a proc coefficient above 0, so damage-over-time ticks don't build stacks. Stacks start falling off after 4s (Conqueror) or 6s (Lethal Tempo) without landing a hit.

Every survivor defaults to **No Keystone**, so nothing changes until you pick one in the loadout screen.

## Config

`BepInEx/config/com.Lemonlust.Keystones.cfg`

- **General → Enable For All Survivors**: turn off to give the slot only to survivors whose mod opts in.
- **Survivors**: one toggle per survivor. Filled in after the first launch.
- One section per keystone with all its numbers. Descriptions in game update to match.

In multiplayer, everyone should use the same **Survivors** toggles. Turning a survivor off removes a loadout slot, and players whose configs disagree will see mismatched loadouts.

## For modders

Reference `Keystones.dll`, add a dependency, and call the API from your plugin's `Awake`:

```csharp
[BepInDependency(Keystones.KeystonePlugin.GUID)]
public class MyPlugin : BaseUnityPlugin
{
    void Awake()
    {
        // ...after your body prefab exists
        Keystones.KeystoneAPI.RegisterSurvivor(myBodyPrefab); // slot even if "all survivors" is off
        // or Keystones.KeystoneAPI.ExcludeSurvivor(myBodyPrefab);
    }
}
```

`KeystoneAPI.GetKeystone(body)` returns the keystone a body has selected, and the buff defs are exposed for anyone who wants to react to stacks.

## Changelog

**1.0.0**
- Split out of SettMod 4.6.3. Works on every survivor, adds a No Keystone option, config for every number, and a public API.
