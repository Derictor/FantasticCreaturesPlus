using System;
using System.Collections.Generic;
using System.Linq;
using DBDef;
using Thea2.Common;
using Thea2.General;
using Thea2.Server;
using UnityEngine;

namespace GameScript
{
    public class FantasticCreaturesInheritance : ScriptBase
    {
        private const string TraitPrefix = "SKILL-FCP_HERITAGE_";
        private const string HybridPrefix = "SUBRACE-FCP_HALF_";
        private const string MixedPrefix = "SUBRACE-FCP_MIXED_";

        private static readonly string[] Origins =
        {
            "HUMAN", "ELF", "ORC", "DWARF", "GOBLIN", "SHADOWELF", "DEMON", "BEAST",
            "PIRATES", "GNOLL", "UNLIVING", "MYTHICAL"
        };

        private static readonly string[] Creatures =
        {
            "GRIFFIN", "KIRIN", "MERMAID", "PHOENIX", "UNICORN", "ENT", "BASILISK", "WOLF"
        };

        private static readonly Dictionary<string, string> VanillaHeritages = new Dictionary<string, string>
        {
            { "SKILL-RACE_ORCS", "ORC" }, { "SKILL-RACE_ELVES", "ELF" },
            { "SKILL-RACE_GOBLINS", "GOBLIN" }, { "SKILL-RACE_DWARVES", "DWARF" },
            { "SKILL-RACE_PIRATE_ORCS", "PIRATES" }
        };

        public static bool FRA_FCP_HasMixedParents(AdventureEventData data, LogicResultAction action, AdvNode node)
        {
            return EligiblePairs(data).Any();
        }

        public static void FMO_FCP_AddMixedChild(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            var pairs = EligiblePairs(data).ToList();
            if (pairs.Count == 0)
                return;

            var pair = pairs[UnityEngine.Random.Range(0, pairs.Count)];
            var heritages = new HashSet<string>(ReadHeritages(pair[0]));
            heritages.UnionWith(ReadHeritages(pair[1]));
            string creature = Creatures.FirstOrDefault(heritages.Contains);
            string origin = Origins.Take(8).FirstOrDefault(heritages.Contains);
            string child = creature != null
                ? HybridPrefix + (origin ?? "BEAST") + "_" + creature + "_CHILD"
                : MixedPrefix + (origin ?? "HUMAN") + "_CHILD";
            AddChild(data, child, pair[0], pair[1]);
        }

        public static void FMO_FCP_AddFirstGenerationChild(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            var mythic = CharactersInList(data, node, modifier.list).Where(IsAdult).ToList();
            var origin = CharactersInList(data, node, "adultOrigin").Where(IsAdult).ToList();
            if (mythic.Count == 0 || origin.Count == 0)
                return;

            AddChild(data, modifier.scriptCallWithTag.tag,
                mythic[UnityEngine.Random.Range(0, mythic.Count)],
                origin[UnityEngine.Random.Range(0, origin.Count)]);
        }

        public static bool FRA_FCP_HasUnregisteredHeritage(AdventureEventData data, LogicResultAction action, AdvNode node)
        {
            return UnregisteredHeritages(data).Any();
        }

        public static void FMO_FCP_RegisterHeritage(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            foreach (var character in UnregisteredHeritages(data).ToList())
                ApplyHeritages(character, ReadHeritages(character));
        }

        public static bool FRA_FCP_HasGrowingMixed(AdventureEventData data, LogicResultAction action, AdvNode node)
        {
            return GrowingMixed(data).Any();
        }

        public static void FMO_FCP_GrowMixed(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            foreach (Character character in GrowingMixed(data).ToList())
            {
                string id = character.subrace.Get().dbName;
                string next = id.EndsWith("_CHILD", StringComparison.Ordinal)
                    ? id.Substring(0, id.Length - 6) + "_YOUNG"
                    : id.Substring(0, id.Length - 6) + "_ADULT";
                Grow(data, character, next);
            }
        }

        public static void FMO_FCP_GrowLegacyHybrid(NOAdventureStatus status, AdventureEventData data, LogicModifier modifier, AdvNode node)
        {
            foreach (Character character in CharactersInList(data, node, modifier.list))
                Grow(data, character, modifier.scriptCallWithTag.tag);
        }

        private static IEnumerable<Character[]> EligiblePairs(AdventureEventData data)
        {
            var group = data.GetMainPlayerGroup();
            if (group == null || group.abstractGroup)
                yield break;

            var adults = group.characters.Select(reference => reference.Get())
                .Where(character => character != null && IsAdult(character))
                .Select(character => new { Character = character, Heritages = ReadHeritages(character) })
                .Where(parent => parent.Heritages.Count > 0).ToList();

            for (int i = 0; i < adults.Count; i++)
            {
                for (int j = i + 1; j < adults.Count; j++)
                {
                    var first = adults[i];
                    var second = adults[j];
                    if (first.Heritages.Count == 1 && second.Heritages.Count == 1)
                    {
                        string left = first.Heritages.First();
                        string right = second.Heritages.First();
                        if (left == right)
                            continue;

                        // The existing named encounters already cover pure FCP × a supported origin.
                        if ((Creatures.Contains(left) && Origins.Take(8).Contains(right)) ||
                            (Creatures.Contains(right) && Origins.Take(8).Contains(left)))
                            continue;
                    }

                    yield return new[] { first.Character, second.Character };
                }
            }
        }

        private static bool IsAdult(Character character)
        {
            string id = character.subrace.Get().dbName;
            bool mixed = id.StartsWith(HybridPrefix, StringComparison.Ordinal) || id.StartsWith(MixedPrefix, StringComparison.Ordinal);
            bool mixedJuvenile = mixed &&
                (id.EndsWith("_CHILD", StringComparison.Ordinal) || id.EndsWith("_YOUNG", StringComparison.Ordinal));

            return !mixedJuvenile && !character.attributes.Contains((Tag)TAG.CHILD, FInt.ONE) &&
                !character.attributes.Contains((Tag)TAG.GROWING_UP, FInt.ONE) &&
                !id.StartsWith("SUBRACE-FCP_CHILD_", StringComparison.Ordinal) &&
                !id.StartsWith("SUBRACE-FCP_YOUNG_", StringComparison.Ordinal);
        }

        private static IEnumerable<Character> UnregisteredHeritages(AdventureEventData data)
        {
            var group = data.GetMainPlayerGroup();
            if (group == null || group.abstractGroup)
                return Enumerable.Empty<Character>();

            return group.characters.Select(reference => reference.Get()).Where(character =>
                character != null && character.attributes.Contains((Tag)TAG.HALF_RACE, FInt.ONE) &&
                !StoredHeritages(character).Any() && ReadHeritages(character).Count > 1);
        }

        private static IEnumerable<Character> GrowingMixed(AdventureEventData data)
        {
            var group = data.GetMainPlayerGroup();
            if (group == null)
                return Enumerable.Empty<Character>();

            return group.characters.Select(reference => reference.Get()).Where(character =>
            {
                if (character == null || !character.attributes.Contains((Tag)TAG.GROWING_UP, FInt.ONE))
                    return false;

                string id = character.subrace.Get().dbName;
                return id.StartsWith(MixedPrefix, StringComparison.Ordinal) &&
                    (id.EndsWith("_CHILD", StringComparison.Ordinal) || id.EndsWith("_YOUNG", StringComparison.Ordinal));
            });
        }

        private static IEnumerable<Character> CharactersInList(AdventureEventData data, AdvNode node, string name)
        {
            var list = data.GetList(name, node);
            if (list == null)
                yield break;

            foreach (var key in list.GetList())
            {
                var character = list.GetItem(key).item as Character;
                if (character != null)
                    yield return character;
            }
        }

        private static IEnumerable<string> StoredHeritages(Character character)
        {
            if (character.learnedSkills == null)
                yield break;

            foreach (var instance in character.learnedSkills)
            {
                var skill = instance.source.Get();
                if (skill != null && skill.dbName.StartsWith(TraitPrefix, StringComparison.Ordinal))
                    yield return skill.dbName.Substring(TraitPrefix.Length);
            }
        }

        private static HashSet<string> ReadHeritages(Character character)
        {
            var result = new HashSet<string>(StoredHeritages(character));
            if (result.Count > 0)
                return result;

            var learned = character.learnedSkills;
            var subrace = character.subrace.Get();
            string id = subrace.dbName;
            if (id.StartsWith(HybridPrefix, StringComparison.Ordinal))
            {
                var parts = id.Substring(HybridPrefix.Length).Split('_');
                result.Add(parts[0]);
                result.Add(parts[1]);
                return result;
            }

            if (id.StartsWith("SUBRACE-FCP_", StringComparison.Ordinal))
            {
                string species = id.Substring(id.LastIndexOf('_') + 1);
                if (Creatures.Contains(species))
                    result.Add(species);
                return result;
            }

            if (id == "SUBRACE-ELF_SHADOW" || id == "SUBRACE-ELDER_ELF_SHADOW" ||
                id == "SUBRACE-LIGHT_SICK_ELF_SHADOW")
            {
                result.Add("SHADOWELF");
            }
            else
            {
                string race = subrace.race.dbName.Substring("RACE-".Length);
                if (Origins.Contains(race))
                    result.Add(race);
            }

            if (learned != null && character.attributes.Contains((Tag)TAG.HALF_RACE, FInt.ONE))
            {
                foreach (var instance in learned)
                {
                    var skill = instance.source.Get();
                    string heritage;
                    if (skill != null && VanillaHeritages.TryGetValue(skill.dbName, out heritage))
                        result.Add(heritage);
                }
            }
            return result;
        }

        private static void ApplyHeritages(Character character, HashSet<string> heritages)
        {
            if (heritages.Count < 2)
                return;

            foreach (string heritage in heritages)
            {
                var skill = Globals.GetInstanceFromDB<Skill>(TraitPrefix + heritage);
                if (skill == null)
                    continue;
                if (character.learnedSkills == null || !character.learnedSkills.Any(instance => instance.source.Get() == skill))
                    character.LearnSkill(Character.SkillCategory.Personal, skill, FInt.ONE);
            }
        }

        private static void AddChild(AdventureEventData data, string childId, Character first, Character second)
        {
            var group = data.GetMainPlayerGroup();
            var form = Globals.GetInstanceFromDB<Subrace>(childId);
            if (group == null || form == null)
                return;

            var firstHeritages = ReadHeritages(first);
            var secondHeritages = ReadHeritages(second);
            ApplyHeritages(first, firstHeritages);
            ApplyHeritages(second, secondHeritages);
            firstHeritages.UnionWith(secondHeritages);

            var child = Character.Instantiate(group, form, 1, false);
            ApplyHeritages(child, firstHeritages);
            var change = new AdventureEventDataChange();
            change.GatherReferenceInfo(child);
            change.count = 1;
            change.playerID = group.ownerID;
            data.AddUncollectedAdventureChange(change);
        }

        private static void Grow(AdventureEventData data, Character character, string nextId)
        {
            var form = Globals.GetInstanceFromDB<Subrace>(nextId);
            if (form == null)
                return;

            var heritages = ReadHeritages(character);
            character.attributes.SetBaseTo((Tag)TAG.GROWING_UP, FInt.ZERO);
            character.UpgradeSubraceTo(form, true, false);
            ApplyHeritages(character, heritages);
            var change = new AdventureEventDataChange();
            change.GatherReferenceInfo(character);
            change.count = 1;
            data.AddUncollectedAdventureChange(change);
        }
    }
}
