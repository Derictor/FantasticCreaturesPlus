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
    assert (ROOT / manifest.get("Graphic")).is_file()
    for locale in manifest.findall("LOCALIZATION"):
        assert (ROOT / locale.text).is_file()
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
            portrait = images[form.find("Graphic" + sex).get("Value")]
            assert portrait.get("Model"), form.tag
        assert form.find("DescriptionInfo").get("IconName") in images

    def verify_growth(event, child, young, adult):
        for form, is_child in ((child, True), (young, True), (adult, False)):
            marker = forms[form].find("SecondSubraceAttributes[@Tag='TAG-CHILD']")
            assert (marker is not None) == is_child, form
        destinations = Counter(call.get("tag") for call in calls(event, "FMO_UpgradeToSubraceNoLevelReset"))
        assert destinations == Counter([young, adult]), event.get("name")
        portraits = {images[forms[stage].find("GraphicMale").get("Value")].get("Path") for stage in (child, young, adult)}
        assert len(portraits) == 3, event.get("name")

    for index, species in enumerate(SPECIES):
        adult = "SUBRACE-FCP_" + species
        child = "SUBRACE-FCP_CHILD_" + species
        young = "SUBRACE-FCP_YOUNG_" + species
        assert Counter(n.get("tag") for n in calls(events[1 + index], "FMO_AddCharacter")) == Counter({adult: 2})
        assert len(calls(events[1 + index], "FMO_AddItems")) == 1
        assert Counter(n.get("tag") for n in calls(events[9 + index], "FMO_AddCharacterNoDefaultCargo")) == Counter({child: 2})
        assert Counter(n.get("tag") for n in calls(events[25 + index], "FMO_AddCharacterNoDefaultCargo")) == Counter({young: 2})
        verify_growth(events[17 + index], child, young, adult)
        for offset, origin in enumerate(ORIGINS):
            pair = index * len(ORIGINS) + offset
            key = f"SUBRACE-FCP_HALF_{origin}_{species}_"
            birth = events[41 + pair]
            assert [n.get("tag") for n in calls(birth, "FMO_AddCharacterNoDefaultCargo")] == [key + "CHILD"]
            filters = calls(birth, "FwT_ThisPlayerThisGroupCharacters")
            assert {adult, "TAG-CHILD"} <= {n.get("tag") for n in filters}
            assert not any("GENDER" in n.get("tag", "") for n in filters)
            verify_growth(events[105 + pair], *(key + stage for stage in STAGES))

    print(f"{len(databases)} databases, {len(images)} image records, {len(forms)} forms, {len(events)} events validated.")


if __name__ == "__main__":
    main()
