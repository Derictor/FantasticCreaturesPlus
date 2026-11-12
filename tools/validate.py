"""Validate mod data, assets, and event wiring without starting Thea 2."""

from collections import Counter
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SPECIES = "GRIFFIN KIRIN MERMAID PHOENIX UNICORN ENT BASILISK WOLF".split()
ORIGINS = "HUMAN ELF ORC DWARF GOBLIN SHADOWELF DEMON BEAST".split()
STAGES = ("CHILD", "YOUNG", "ADULT")


def xml(path):
    return ET.parse(ROOT / path).getroot()


def unique(records, key):
    records = list(records)
    result = {key(record): record for record in records}
    assert len(result) == len(records), "Duplicate identifiers"
    return result


def calls(event, name):
    return [call for call in event.iter("scriptCallWithTag") if call.get("scriptName") == name]


def main():
    manifest = xml("info.txt")
    databases = {file.text: xml(file.text) for file in manifest.findall("DB")}
    images = unique(manifest.findall("IMAGE"), lambda image: image.get("Name"))
    for image in images.values():
        for attribute in ("Path", "Path2"):
            path = image.get(attribute)
            if path:
                assert (ROOT / path).is_file(), path
    for species in SPECIES:
        for stage, prefix in (("Child", "Child"), ("Young", "Young"), ("Apex", "")):
            portrait = f"FCP{prefix}{species.title()}"
            alias = f"FCP{species.title()}Map{stage}"
            assert images[portrait].get("Model") == alias
            fallback = images[alias].get("Model")
            assert fallback and fallback not in images, alias
    assert (ROOT / manifest.get("Graphic")).is_file()
    for locale in manifest.findall("LOCALIZATION"):
        assert (ROOT / locale.text).is_file()
    script_text = "\n".join((ROOT / file.text).read_text(encoding="utf-8") for file in manifest.findall("SCRIPT"))
    story = xml(manifest.findtext("STORY_MODULE"))
    events = unique(story.findall("Adventure"), lambda event: int(event.get("uniqueID")))
    for parent in (story, manifest):
        assert int(parent.get("lastEventIndex")) == max(events), "Stale lastEventIndex"
    for event in events.values():
        nodes = unique(event.findall("nodes"), lambda node: node.get("ID"))
        assert int(event.get("nodeIdMax")) == max(map(int, nodes)), event.get("name")
        for node_id, node in nodes.items():
            image = node.get("image")
            if image:
                assert images[image].get("EventImage") == "true", image
            for output in node.findall("outputs"):
                assert output.get("ownerID") == node_id
                assert output.get("targetID") in nodes, event.get("name")

    mod_records = {record.tag for db in databases.values() for record in db}
    for db in databases.values():
        for record in db.iter():
            for value in record.attrib.values():
                if value.startswith(("SUBRACE-FCP_", "RACE-FCP", "GROUP-FCP_", "EVENTC-FCP_", "SKILL_PACK-FCP_", "SKILL-FCP_", "SUBSKILL-FCP_")):
                    assert value in mod_records, (record.tag, value)
    for event in events.values():
        for call in event.iter("scriptCallWithTag"):
            script_name = call.get("scriptName", "")
            if script_name.startswith(("FRA_FCP_", "FMO_FCP_")):
                assert f" {script_name}(" in script_text, script_name
            tag = call.get("tag", "")
            if tag.startswith("SUBRACE-FCP_"):
                assert tag in mod_records, (event.get("name"), tag)
    for collection in databases["DB/DATABASE_EVENT_COLLECTIONS_FCP.xml"]:
        for event in collection.findall("Event"):
            assert event.get("ModuleName") == story.get("name")
            assert int(event.get("EventID")) in events

    forms = unique(list(databases["DB/Creatures.xml"]) + list(databases["DB/Lineages/Subraces.xml"]), lambda form: form.tag)
    for form in forms.values():
        for sex in ("Male", "Female"):
            portrait = form.find("Graphic" + sex).get("Value")
            if not (form.tag.startswith("SUBRACE-FCP_MIXED_") and form.tag.endswith("_ADULT")):
                assert images[portrait].get("Model"), form.tag
        assert form.find("DescriptionInfo").get("IconName") in images or form.tag.startswith("SUBRACE-FCP_MIXED_")

    def verify_growth(event, child, young, adult):
        for form, is_child in ((child, True), (young, True), (adult, False)):
            marker = forms[form].find("SecondSubraceAttributes[@Tag='TAG-CHILD']")
            assert (marker is not None) == is_child, form
        destinations = Counter(call.get("tag") for call in calls(event, "FMO_FCP_GrowCreature"))
        assert calls(event, "FwT_ThisPlayerThisGroupCharacters"), event.get("name")
        assert not calls(event, "FwT_AllPlayersAllGroupsCharacters"), event.get("name")
        assert destinations == Counter([young, adult]), event.get("name")
        portraits = {images[forms[stage].find("GraphicMale").get("Value")].get("Path") for stage in (child, young, adult)}
        assert len(portraits) == 3, event.get("name")

    for index, species in enumerate(SPECIES):
        adult = "SUBRACE-FCP_" + species
        child = "SUBRACE-FCP_CHILD_" + species
        young = "SUBRACE-FCP_YOUNG_" + species
        assert Counter(n.get("tag") for n in calls(events[1 + index], "FMO_FCP_RecruitWithCargo")) == Counter({adult: 2})
        assert len(calls(events[1 + index], "FMO_FCP_RewardPet")) == 1
        assert Counter(n.get("tag") for n in calls(events[9 + index], "FMO_FCP_Recruit")) == Counter({child: 2})
        assert Counter(n.get("tag") for n in calls(events[25 + index], "FMO_FCP_Recruit")) == Counter({young: 2})
        verify_growth(events[17 + index], child, young, adult)
        assert [n.get("tag") for n in calls(events[33 + index], "FMO_FCP_Recruit")] == [child]
        for offset, origin in enumerate(ORIGINS):
            pair = index * len(ORIGINS) + offset
            key = f"SUBRACE-FCP_HALF_{origin}_{species}_"
            birth = events[41 + pair]
            assert [n.get("tag") for n in calls(birth, "FMO_FCP_AddFirstGenerationChild")] == [key + "CHILD"]
            filters = calls(birth, "FwT_ThisPlayerThisGroupCharacters")
            assert {adult, "TAG-CHILD"} <= {n.get("tag") for n in filters}
            assert not any("GENDER" in n.get("tag", "") for n in filters)
            verify_growth(events[105 + pair], *(key + stage for stage in STAGES))

    heritage = databases["DB/Lineages/HeritageSubskills.xml"]
    skills = databases["DB/Lineages/HeritageSkills.xml"]
    profiles = {sub.tag[len("SUBSKILL-FCP_HERITAGE_"):]: sub for sub in heritage}
    assert len(profiles) == len(ORIGINS) + len(SPECIES) + 4
    for name, subskill in profiles.items():
        skill = skills.find("SKILL-FCP_HERITAGE_" + name)
        assert skill.find("SkillSubskill").get("Value") == subskill.tag
        assert skill.find("SkillGroupFilter").get("Value") == "FCP_HEREDITARY"
        for level in subskill.findall("SubskillAttributesOnLevel"):
            modifiers = [float(a.get("AttributeFloatValue")) for a in level.findall("SubskillAttribute")]
            assert any(amount > 0 for amount in modifiers) and any(amount < 0 for amount in modifiers), name
    for event_id, modifier in [(169, "FMO_FCP_AddMixedChild"), (170, "FMO_FCP_GrowMixed"), (171, "FMO_FCP_RegisterHeritage")]:
        assert len(calls(events[event_id], modifier)) == 1
    for origin in ORIGINS:
        for stage in STAGES:
            form = forms[f"SUBRACE-FCP_MIXED_{origin}_{stage}"]
            assert (form.find("SecondSubraceAttributes[@Tag='TAG-CHILD']") is not None) == (stage != "ADULT")

    print(f"{len(databases)} databases, {len(images)} image records, {len(forms)} forms, {len(events)} events validated.")


if __name__ == "__main__":
    main()
