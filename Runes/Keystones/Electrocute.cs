using BepInEx.Configuration;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Runes
{
    /// <summary>
    /// Stacks build on the attacker, not the target: every third hit within the window, on any enemies,
    /// calls down lightning on the enemy struck. If that hit was a kill, the bolt jumps to the nearest enemy.
    /// </summary>
    internal sealed class Electrocute : Keystone
    {
        private const int HitsToStrike = 3;

        internal static ConfigEntry<float> window;
        internal static ConfigEntry<float> damage;
        internal static ConfigEntry<float> damagePerLevelStep;
        internal static ConfigEntry<float> retargetRange;

        internal static BuffDef buff;

        public override KeystoneType Type => KeystoneType.Electrocute;
        public override string Id => "ELECTROCUTE";
        public override string DisplayName => "Electrocute";
        public override string IconName => "Electrocute_rune";

        internal override void BindConfig(ConfigFile config)
        {
            const string section = "Electrocute";
            window = config.Bind(section, "Stack Window", 3f, "Seconds each Electrocute stack lasts. Three stacks at once call down lightning.");
            damage = config.Bind(section, "Damage Coefficient", 2f, "Lightning damage as a multiple of your damage stat.");
            damagePerLevelStep = config.Bind(section, "Damage Coefficient Per 4 Levels", 0.75f, "");
            retargetRange = config.Bind(section, "Retarget Range", 30f, "If the third hit kills its target, the lightning strikes the nearest enemy within this many meters instead.");
        }

        internal override void CreateBuffs()
        {
            // A self buff so the player can see their stacks; it moves with them from enemy to enemy.
            buff = KeystoneContent.AddBuff("KeystoneElectrocuteBuff", icon, true, false);
        }

        internal override string Description()
        {
            return Hit + " against enemies grant <color=#c9aa71>1</color> stack. Reaching <color=#ffffff>3</color> stacks within " +
                Num(window.Value) + " seconds, on any enemies, strikes the enemy you hit with lightning for <color=#f68835>" + Pct(damage.Value) + "</color> " +
                "<color=#d62d20>(+" + Pct(damagePerLevelStep.Value) + " every 4 levels)</color> damage. " +
                "If that hit was a kill, the lightning strikes the nearest enemy instead.";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            // The lightning itself must not build stacks, or it would feed the next strike.
            if (damageReport.damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit)) return;

            CharacterBody body = handler.body;
            if (body.GetBuffCount(buff) + 1 >= HitsToStrike)
            {
                body.ClearTimedBuffs(buff.buffIndex);
                Fire(body, damageReport.victimBody);
            }
            else
            {
                // Each stack has its own timer, so only hits from the last few seconds count.
                body.AddTimedBuff(buff, window.Value);
            }
        }

        private static void Fire(CharacterBody attacker, CharacterBody victim)
        {
            HurtBox target = null;
            if (victim && victim.healthComponent && victim.healthComponent.alive) target = victim.mainHurtBox;
            if (!target) target = FindNearestEnemy(attacker, victim ? victim.corePosition : attacker.corePosition, victim);
            if (!target) return;

            float levelSteps = Mathf.Floor(attacker.level / 4f);
            float coefficient = damage.Value + levelSteps * damagePerLevelStep.Value;

            // Tagged as a lightning strike so it can't feed Electrocute or retrigger Charged Perforator.
            ProcChainMask procChainMask = default(ProcChainMask);
            procChainMask.AddProc(ProcType.LightningStrikeOnHit);

            OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
            {
                attacker = attacker.gameObject,
                damageColorIndex = DamageColorIndex.Item,
                damageValue = attacker.damage * coefficient,
                isCrit = Util.CheckRoll(attacker.crit, attacker.master),
                procChainMask = procChainMask,
                procCoefficient = 1f,
                target = target
            });
        }

        private static HurtBox FindNearestEnemy(CharacterBody attacker, Vector3 origin, CharacterBody exclude)
        {
            if (!attacker.teamComponent) return null;

            BullseyeSearch search = new BullseyeSearch
            {
                searchOrigin = origin,
                maxDistanceFilter = retargetRange.Value,
                teamMaskFilter = TeamMask.GetEnemyTeams(attacker.teamComponent.teamIndex),
                sortMode = BullseyeSearch.SortMode.Distance,
                filterByLoS = false
            };
            search.RefreshCandidates();
            if (exclude) search.FilterOutGameObject(exclude.gameObject);

            foreach (HurtBox hurtBox in search.GetResults())
            {
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive) return hurtBox;
            }
            return null;
        }
    }
}
