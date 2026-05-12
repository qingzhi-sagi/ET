namespace ET
{
    public static class SpellEditorConstants
    {
        public const string MenuPath = "ET/Spell/Spell Editor";
        public const string WindowTitle = "Spell Editor";
        public const string BtAssetRoot = "Packages/cn.etetet.statesync/Assets/BT";
        public const int BuffIdOffset = 100000;
        public const int SpellGroupSize = 10;

        public static bool IsMainSpell(int id)
        {
            return id % SpellGroupSize == 0;
        }

        public static bool IsSubSpell(int id)
        {
            return id % SpellGroupSize != 0;
        }

        public static int DefaultBuffId(int spellId)
        {
            return spellId + BuffIdOffset;
        }

        public static int MainSpellBase(int spellId)
        {
            return spellId / SpellGroupSize * SpellGroupSize;
        }
    }
}
