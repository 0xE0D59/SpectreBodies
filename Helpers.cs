using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpectreBodies
{
    public static class Helpers
    {
        public static Dictionary<string, string> MatchNamedCaptures(this Regex regex, string input)
        {
            var namedCaptureDictionary = new Dictionary<string, string>();
            GroupCollection groups = regex.Match(input).Groups;
            string[] groupNames = regex.GetGroupNames();
            foreach (string groupName in groupNames)
                if (groups[groupName].Captures.Count > 0)
                    namedCaptureDictionary.Add(groupName, groups[groupName].Value);
            return namedCaptureDictionary;
        }

        public static class FileContent
        {
            public const string SpectreBodyList =
                @"# This file contains the list of monster bodies to display for Raise Spectre spell
# To add new bodies:
# 1. Get next to the body you want to add
# 2. Open DevTree plugin
# 3. Click the button 'Debug around entities'
# 4. Expand the 'Entities' list at the bottom
# 5. Find the entity body you want to add
# 6. Copy the 'Metadata' property of that 
 
Metadata/Monsters/KaomWarrior/KaomWarrior7
Metadata/Monsters/WickerMan/WickerMan
Metadata/Monsters/Miner/MinerLantern
Metadata/Monsters/Miner/MinerLantern
Metadata/Monsters/BloodChieftain/MonkeyChiefBloodEnrage
Metadata/Monsters/Cannibal/CannibalMaleChampion";
        }
    }
}