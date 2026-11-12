using DBDef;
using Thea2.Common;
using Thea2.General;
using Thea2.Server;

namespace GameScript
{
    public class EncounterRewards : ScriptBase
    {
        public static void FMO_FCP_RecruitWithCargo(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            Recruit(data, modifier, true);
        }

        public static void FMO_FCP_Recruit(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            Recruit(data, modifier, false);
        }

        public static void FMO_FCP_RewardPet(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            var group = data.GetMainPlayerGroup();
            var cargo = Globals.GetInstanceFromDB<ItemCargo>(modifier.scriptCallWithTag.tag);
            if (group == null || group.abstractGroup || cargo == null)
                return;

            var change = ItemBase.InstantaiteFromToGroup(cargo, group, 1f, 1);
            if (change == null)
                return;

            change.playerID = group.ownerID;
            data.AddUncollectedAdventureChange(change);
        }

        private static void Recruit(AdventureEventData data, LogicModifier modifier, bool addCargo)
        {
            var group = data.GetMainPlayerGroup();
            var form = Globals.GetInstanceFromDB<Subrace>(modifier.scriptCallWithTag.tag);
            if (group == null || group.abstractGroup || form == null)
                return;

            var character = Character.Instantiate(group, form, 1, addCargo);
            var change = new AdventureEventDataChange();
            change.GatherReferenceInfo(character);
            change.count = 1;
            change.playerID = group.ownerID;
            data.AddUncollectedAdventureChange(change);
        }
    }
}
