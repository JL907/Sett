using System.Collections.Generic;

namespace SettMod.Modules.LeagueSkins
{
    internal static class LeagueSkinCatalog
    {
        private static readonly string[] EmoteProps = { "Joke", "Wolverine" };

        private static LeagueSkinDefinition Skin(int id, string displayName, string defaultTexture, Dictionary<string, string> partTextures, params string[] hiddenParts)
        {
            string number = id.ToString("00");
            List<string> hidden = new List<string>(EmoteProps);
            hidden.AddRange(hiddenParts);
            return new LeagueSkinDefinition
            {
                folder = "Skin" + number,
                meshFile = "sett_skin" + number + ".skn",
                skeletonFile = "sett_skin" + number + ".skl",
                nameToken = "SETT_SKIN" + number + "_NAME",
                displayName = displayName,
                iconTexture = "settloadscreen_" + number + ".tex",
                defaultTexture = defaultTexture,
                partTextures = partTextures ?? new Dictionary<string, string>(),
                hiddenParts = hidden.ToArray()
            };
        }

        internal static readonly LeagueSkinDefinition[] definitions =
        {
            Skin(38, "Spirit Blossom", "sett_skin38_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Hair_Beads_Wisps", "sett_skin38_beadshairwisps_tx_cm.tex" },
                    { "Hand_Beads", "sett_skin38_beadshairwisps_tx_cm.tex" },
                    { "Wisps_Base", "sett_skin38_beadshairwisps_tx_cm.tex" }
                },
                "Arms_Kanmei", "Arms_Akana", "Wisps_Ult", "VFX_Badger", "Barbell", "Badger_Akana", "Badger_Kanmei"),

            Skin(45, "Soul Fighter", "sett_skin45_tx_cm.tex", null,
                "TV", "FX_Hand", "FX_Claws", "FX1", "FX2"),

            Skin(56, "HEARTSTEEL", "sett_skin56_tx_cm.tex", null,
                "Cat", "Robot"),

            Skin(66, "Radiant Serpent", "sett_skin66_base_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "SnakeBase", "sett_skin66_snake_tx_cm.tex" }
                },
                "Dark", "SnakeDark", "Gold", "Emblem", "SnakeW", "Envelopes"),

            Skin(67, "Spirit Blossom Springs", "sett_skin67_tx_cm.tex", null,
                "Steam", "Cloud"),

            Skin(76, "HEARTSTEEL: Live My Life", "sett_skin76_tx_cm.tex", null,
                "Recall_Shirt", "Recall_Props", "Hairw", "Recall_Body", "Recall_Net", "Tear")
        };
    }
}
