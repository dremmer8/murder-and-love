import re
from pathlib import Path

ink_path = Path(__file__).with_name("Final_eng.ink")
text = ink_path.read_text(encoding="utf-8")
raw_lines = text.replace("\r\n", "\n").replace("\r", "\n").split("\n")

switch = re.search(r"\{story_phase:(.*?)\}", text, re.S)
phases = {
    int(m.group(1)): m.group(2)
    for m in re.finditer(r"-\s*(\d+)\s*:\s*->\s*(\w+)", switch.group(1))
}

KNOT_VOICE = {
    "LAU_story_phase_1": 2,
    "Mandy_story_phase_1": 3,
    "Mandy_story_phase_2": 6,
    "LAU_story_phase_2": 7,
    "Inner_voice_backroom_phase_1": 8,
    "Mandy_story_phase_3": 15,
    "Mandy_story_phase_4": 18,
    "LAU_story_phase_3": 19,
    "Chaos_blackout": 21,
    "Mandy_smoking_scene_1": 26,
    "Mandy_smoking_scene_2": 27,
    "Mandy_smoking_scene_3": 28,
    "Lau_confess_ending": 30,
    "Boyfriend_ending_dialogue_final": 31,
    "Thought_washing_clothes_1": 13,
    "Thought_washing_clothes_2": 20,
}


def story_to_voice(story_phase: int) -> int:
    if story_phase == 2:
        return 3
    if story_phase == 3:
        return 2
    return story_phase


knot_to_voice = {}
for sp, knot in phases.items():
    knot_to_voice[knot] = KNOT_VOICE.get(knot, story_to_voice(sp))
for k, v in KNOT_VOICE.items():
    knot_to_voice[k] = v

speaker_re = re.compile(
    r"^(?:\*+|\+)?\s*(?:\[[^\]]*\]\s*)?([A-Za-z][A-Za-z0-9 ]*?):\s*(.*)$"
)
vo_tag_re = re.compile(r"\s*#\s*vo:p_\d+_l_\d+\b", re.I)
knot_header_re = re.compile(r"^==\s+(\w+)\s*==\s*$")
# Trailing divert on same line (optional spaces)
divert_re = re.compile(r"^(?P<body>.*?)(?P<div>\s*->\s*\S+)\s*$")

out = []
current_knot = None
line_idx = 0
tagged = 0
with_divert = 0
skipped_no_phase = 0

for line in raw_lines:
    stripped = line.strip()

    m_knot = knot_header_re.match(stripped)
    if m_knot:
        current_knot = m_knot.group(1)
        line_idx = 0
        out.append(line)
        continue

    if (
        current_knot is None
        or not stripped
        or stripped.startswith("//")
        or stripped.startswith("~")
        or stripped.startswith("=")
    ):
        out.append(line)
        continue

    if not speaker_re.match(stripped):
        out.append(line)
        continue

    voice_phase = knot_to_voice.get(current_knot)
    if voice_phase is None:
        skipped_no_phase += 1
        out.append(vo_tag_re.sub("", line).rstrip())
        continue

    line_idx += 1
    tag = f"# vo:p_{voice_phase}_l_{line_idx}"
    leading = line[: len(line) - len(line.lstrip(" \t"))]
    content = vo_tag_re.sub("", line.strip()).rstrip()

    # Ink requires tags BEFORE divert on the same line:
    #   Text # vo:p_N_l_M -> target
    # not Text -> target # vo:...
    dm = divert_re.match(content)
    if dm and "->" in content:
        body = dm.group("body").rstrip()
        divert = dm.group("div")
        # Avoid treating `->` inside choice labels; speaker lines only.
        content = f"{body} {tag} -> {divert.lstrip().lstrip('->').strip()}"
        with_divert += 1
    else:
        content = f"{content} {tag}"

    out.append(leading + content)
    tagged += 1

new_text = "\n".join(out)
if text.endswith("\n") and not new_text.endswith("\n"):
    new_text += "\n"

ink_path.write_text(new_text, encoding="utf-8")
print(f"tagged={tagged} with_divert={with_divert} skipped_no_phase={skipped_no_phase}")
print(f"lines={len(out)} (source={len(raw_lines)})")

# show divert samples
shown = 0
for sample in out:
    if "# vo:" in sample and "->" in sample:
        print(" ", sample.strip()[:160])
        shown += 1
        if shown >= 8:
            break
