namespace uttest
{
    public class ComboboxItem
    {
        public string BattleID { get; set; }
        public string Monster { get; set; }

        public ComboboxItem(string battleID, string monster)
        {
            BattleID = battleID;
            Monster = monster;
        }

        public override string ToString()
        {
            return BattleID + "\t" + Monster;
        }
    }
}
