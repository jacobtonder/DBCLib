namespace DBCLib.Test.Structures
{
    public class CharTitlesEntry
    {
        public uint Id;                             // 0
        public uint ConditionId;                    // 1 This is never used by the client. Still looks like pointing somewhere. Serverside?
        public LocalizedString NameMale;            // 2-18
        public LocalizedString NameFemale;          // 19-35
        public uint TitleMaskId;                    // 36 Used ingame in the drop down menu.
        
    }
}
