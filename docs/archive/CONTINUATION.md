# Fantastic Creatures Plus - Continuation Notes

Last updated: 2026-11-12 11:32 (-03:00)

## Project

Continue the existing mod in:

`Thea2_Data/StreamingAssets/Mods/FantasticCreaturesPlus` within the game installation.

Do not create a replacement mod. Keep the mod enabled and preserve its working `info.txt` format. The user wants English names and IDs, with selected in-game descriptions and event text localized to Brazilian Portuguese.

## Runtime status

After fixing missing required prototype fields and restoring the three temporarily removed manifest entries, the 2026-11-12 11:31 run completed database initialization and reached the main menu. The log contains `Enter FSM State Thea2.Client.FSMMainMenu` and the database hash, with no `NullReferenceException`. The original `Missing: Language(CHARACTER_NAMES ln:8) :Any` warning still appears, so it is not the cause of the 3/11 startup stall. The earlier trial adding `Language="Any"` to every FCP name collection remains reverted.

The affected records were missing required data: growth skills lacked `SkillSubskill`, awakening skills lacked `BaseEssence`, groups lacked `EventsLoyalty`, and juvenile subraces declared empty zero-chance cargo without the required `ItemCargo`. These fixes were applied together, so no single field has been isolated as the sole trigger; the complete mod now passes database initialization.

## Changes already made

- Removed the manifest reference to a redundant root-level `DATABASE_SKILLS_1_NATURAL.xml`.
- Moved the redundant root-level database file out of the mod folder to a session backup; it is not part of the mod.
- Confirmed all FCP skill entries from the redundant file are already present in the registered `DB\Abilities.xml` and `DB\Traits.xml`.
- Removed `name="1_Natural"` from the roots of `DB\Abilities.xml` and `DB\Traits.xml`. Their roots now match known-good examples: `<SKILLS_1_NATURAL>`.
- Removed XML declarations from `DB\Abilities.xml` and `Events.xml`, following the earlier successful fix for this game's mod XML loader. Keep the XML declaration in `info.txt`; it is the mod manifest and has a different format.
- Corrected 16 image paths in `info.txt` for child portraits and child event backgrounds.
- Updated the story module `lastEventIndex` to 16, matching its highest adventure ID.
- Added PT-BR descriptions for child races, groups, and settlements.
- Rebuilt child adventures 9-16 from the valid adult encounter graph, with child-specific images, names, and subrace targets. All eight now have 12 recognized nodes and complete Fight, Tame, Recruit, and Flee routes.
- Added eight hidden named weapon recipes, eight private item tags, eight guaranteed item skills/packs, and constrained all 16 weapon cargo profiles to the matching item.
- Changed adult encounter recruitment to grant the creature's default cargo, so recruited adults receive their named weapon.
- Added PT-BR text blocks for child adventures 9-16.
- Renamed the FCP localization library from the shared `LOC_LIBRARY-DES_EN` to the unique `LOC_LIBRARY-EN_FCP`. This prevents the 121-entry FCP library from colliding with the 4,610-entry base-game/Yuri English library loaded before it.
- Replaced invalid or placeholder icon IDs on FCP abilities and weapon skills with icon IDs verified in the installed base-game database.
- Moved FCP race and child abilities/traits out of the global `Fallback` skill group into species-specific `FCP_*` groups. Their species skill packs still explicitly guarantee the intended skills, while vanilla characters should no longer randomly receive them.
- Added separate PT-BR descriptions for the eight weapon abilities; item and skill names remain in English as requested.
- The `27` counter shown in the screenshot belonged to Dawidmir's vanilla child-growth skill, not the Phoenix Hatchling or Griffin Chick. The new FCP countdowns are separate skills, added for both juvenile stages.
- Implemented the agreed three stages for all eight creatures: Filhote -> Jovem -> Adulto. Each stage advances after a 25-45 turn countdown; `WActA_GrowUp` marks the character and per-species events upgrade the subrace without resetting level.
- Added eight Young subraces with midpoint attributes, one species-specific awakening trait, and a second countdown to adulthood. Their portraits temporarily reuse the existing adult portraits; child portraits remain unchanged.
- The adult subrace and character-name labels now use canonical species names (Griffin, Kirin, Mermaid, Phoenix, Unicorn, Ent, Basilisk, Wolf).
- Growth events use the existing species encounter images and include PT-BR text. No new portrait or event image files were needed.
- Fixed the 3/11 database initialization failure by replacing unsupported `TraitModifiers` / `TraitModifier` fields in `DB\Abilities.xml` with valid game `SkillSubskill` entries. Removed those fields from skills that already had a valid subskill.
- A diagnostic trial added `Language="Any"` to all 24 FCP character-name collections; it still stopped at 3/11 and generated a warning for every such collection. The trial attributes were removed again.
- Restored all 16 database entries in `info.txt` after the isolation tests.
- Added `SUBSKILL-CHILD_GROW_UP` to all 16 child/young growth countdown skills, matching the vanilla `SKILL-HUM_CHILD_GROW_UP` pattern.
- Added gray `BaseEssence` with amount 0 to all eight young awakening skills.
- Added `EventsLoyalty` to all 40 FCP groups, using each group's existing `EventsRoaming` collection.
- Removed 16 empty `DefaultCargo Chance="0"` elements from child/young subraces; those elements were optional, while their nested `ItemCargo` was required.

## Static validation

- `info.txt` parses and lists 16 database XML files.
- All 16 referenced database XML files exist and parse.
- `Events.xml` parses as the story module and contains 16 adventures, with `lastEventIndex=16`.
- All 16 adventures use the recognized lowercase `nodes` schema, with 12 nodes each and valid output targets.
- The eight named weapon recipes resolve to custom item tags and guaranteed skills; all 16 cargo profiles select exactly one matching item with zero trash chance.
- All 16 event blocks are present in the PT-BR localization file.
- No `.xml` file remaining in the mod contains an XML declaration.
- All 120 image names are unique and every `Path`/`Path2` resolves to an existing file.
- All referenced `DES_FCP_*` description keys, including the eight weapon descriptions, have translations.
- `DATABASE_DES_LOCALIZATION_FCP.xml` parses with its unique `LOC_LIBRARY-EN_FCP` identifier; sample built-in trait keys remain present in both the base and Yuri localization files.
- All changed skill icon IDs exist in the installed base-game database, all eight weapon ability-description keys resolve, and all 37 FCP ability/trait grants resolve.
- No FCP ability or trait remains in the global `Fallback` group.
- The eight child countdowns, eight young countdowns, and eight young awakening abilities resolve through the registered skill packs and descriptions.
- All eight growth events have unique IDs 17-24, valid three-node routes, and transformations targeting the matching child, young, and adult subraces.
- All 24 creature subraces have valid races, skill packs, character-name mappings, and registered graphic models; all declared image paths resolve.
- Cross-database `FCP` references checked so far resolve.
- All 16 mod database XML files parse after the skill fix; no mod DB XML retains `TraitModifiers` / `TraitModifier`; all 57 `SkillSubskill` references in `Abilities.xml` resolve to subskills in the installed game database, including the 16 child-growth markers.
- All 16 registered database files exist and parse; targeted checks confirm the 16 growth skills, eight awakening skills, 40 groups, and juvenile cargo definitions now satisfy the missing-field findings.
- Runtime verified: the game passed 3/11, completed database initialization, and reached the main menu with the full manifest enabled.
- The mod's in-game localization behavior after the unique library change is still unverified.

## Audit findings

- Resolved: child adventures had uppercase wrapper elements that the module did not recognize, plus incomplete result routes. They now follow the validated lowercase node format and adult encounter flow.
- Resolved: weapon cargo profiles now require unique custom item tags. Each hidden recipe defines a fixed English item name and a guaranteed themed skill; adult recruits receive their default item cargo.
- Resolved statically: FCP's partial localization library no longer uses the shared base-game/Yuri library identifier that could hide built-in `DES_...` strings.
- Resolved statically: invalid FCP skill icon IDs and explicit `IconPlaceholder` weapon-skill icons were replaced with valid game icons.
- Resolved statically: species-only FCP skills are no longer in the global fallback pool; confirm in-game that vanilla units such as Rat no longer inherit them.
- Resolved: the 3/11 database initialization failure. Required skill, group, and cargo fields were corrected, and a fresh run reached the main menu with all 16 database files registered.
- Remaining: verify FCP localization, weapon behavior, and the three-stage creature growth/skill retention in gameplay. The base-game `CHARACTER_NAMES` language warning remains non-fatal.
- Remaining: reopen the game and confirm that built-in card text and FCP descriptions both resolve; verify weapon ability icons/descriptions and that recipes still render and equip correctly.
- Follow-up: check the `CHARACTER_NAMES` warning after the database loads; investigate the earlier child-model warnings or `ClientGroup.Move` exceptions only if they recur.
- The three-stage design is now implemented statically; actual turn-by-turn transformation and skill retention still require in-game testing.

## Next action

1. In-game, confirm FCP descriptions and weapon skills display correctly.
2. Test the child -> young -> adult countdowns and confirm each transformation preserves the intended skills and equipment.

## Key files

- `info.txt` - active mod manifest.
- `DB\Abilities.xml` and `DB\Traits.xml` - custom natural skills and traits.
- `Events.xml` - story module.
- `DB\WeaponRecipes.xml`, `WeaponTags.xml`, `WeaponSkills.xml`, `WeaponSkillPacks.xml`, and `Weapons.xml` - named weapons, guaranteed item skills, and cargo profiles.
- `Localization\Events_PTBR.txt` - PT-BR text for all 16 encounter events.
- `DB\DATABASE_DES_NAMES_FCP.xml` - custom character names; the log's `CHARACTER_NAMES` warning may need follow-up.
- `DB\DATABASE_DES_LOCALIZATION_FCP.xml` - PT-BR database descriptions.
- `<Thea 2 user data folder>/output_log.txt` - Unity startup log.
- `Thea2_Data/StreamingAssets/GameLog.txt` within the game installation - game log.
