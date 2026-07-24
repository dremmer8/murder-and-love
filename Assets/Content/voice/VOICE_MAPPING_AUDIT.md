# Voice ↔ Ink Mapping Audit

Generated against Assets/Dialogues/Final_dialogues/Final_eng.ink.

## Naming convention

| Token | Meaning |
|-------|---------|
| p_N | Ink story_phase N |
| l_N | Nth speaker line in that phase knot (all speakers counted in order) |
| Folder MC/ | Vivian — Ink speaker You: |
| Folder Mandy/ | Mandy — Ink speaker Mrs Wong: (rarely Mandy:) |

**Line indexing rule used:** every Speaker: text line inside the phase knot increments l, including Thoughts:, choice-branch lines, and mutually exclusive options. Commands (~), stitches (=), and comments are skipped.

## Status legend

| Status | Meaning |
|--------|---------|
| OK | VO line numbers + authors match Ink for that phase |
| PARTIAL | Most lines match; some missing or extra |
| MISMATCH | Wrong phase, wrong authors, and/or shifted indices |
| NO VO | No MC/Mandy wavs for this phase (may be intentional) |

## Summary

| Phase | Knot | Ink lines | MC VO | Mandy VO | Status |
|------:|------|----------:|------:|---------:|--------|
| 2 | Mandy_story_phase_1 | 71 | 15 | 0 | MISMATCH |
| 3 | LAU_story_phase_1 | 53 | 24 | 32 | MISMATCH |
| 4 | Thought_about_not_leaving_clothes | 2 | 0 | 0 | NO VO |
| 5 | Thought_about_empty_detergent | 3 | 0 | 0 | NO VO |
| 6 | Mandy_story_phase_2 | 33 | 11 | 13 | PARTIAL |
| 7 | LAU_story_phase_2 | 33 | 0 | 0 | NO VO |
| 8 | Inner_voice_backroom_phase_1 | 15 | 4 | 0 | OK |
| 9 | Boyfriend_pager_phase_1 | 6 | 0 | 0 | NO VO |
| 10 | Boyfriend_pager_phase_1 | 6 | 0 | 0 | NO VO |
| 11 | Thought_about_how_detergent_looks | 1 | 0 | 0 | NO VO |
| 12 | Thought_about_got_right_detergent | 1 | 0 | 0 | NO VO |
| 13 | Thought_washing_clothes_1 | 7 | 8 | 0 | PARTIAL |
| 14 | Thought_about_need_another_washer | 3 | 0 | 0 | NO VO |
| 15 | Mandy_story_phase_3 | 59 | 24 | 22 | OK |
| 16 | Interaction_with_coin_machine | 3 | 0 | 0 | NO VO |
| 17 | Boyfriend_pager_phase_2 | 7 | 0 | 0 | NO VO |
| 18 | Mandy_story_phase_4 | 23 | 5 | 8 | PARTIAL |
| 19 | LAU_story_phase_3 | 60 | 26 | 0 | PARTIAL |
| 20 | Thought_washing_clothes_2 | 11 | 11 | 0 | OK |
| 21 | Chaos_blackout | 4 | 1 | 3 | PARTIAL |
| 22 | Inner_voice_phase_2 | 12 | 0 | 0 | NO VO |
| 23 | Boyfriend_pager_phase_3 | 6 | 0 | 0 | NO VO |
| 24 | How_to_turn_on_circuit_box | 3 | 0 | 0 | NO VO |
| 25 | Attempt_leaving_backroom | 3 | 0 | 0 | NO VO |
| 26 | Mandy_smoking_scene_1 | 15 | 7 | 8 | OK |
| 27 | Mandy_smoking_scene_2 | 29 | 8 | 19 | PARTIAL |
| 28 | Mandy_smoking_scene_3 | 23 | 6 | 13 | OK |
| 29 | Boyfriend_pager_ending | 2 | 0 | 0 | NO VO |
| 30 | Lau_confess_ending | 24 | 11 | 0 | PARTIAL |
| 31 | Boyfriend_ending_dialogue_final | 10 | 1 | 0 | PARTIAL |

---

## Phase 2 — Mandy_story_phase_1

**Status:** MISMATCH

- Ink speaker lines: **71** (You/MC: 29, Mrs Wong/Mandy: 34, Thoughts: 8, Other: 0)
- Voice files: MC 15, Mandy 0

> **Note:** First Mandy conversation. Mandy VO is filed under p_3 (see Phase 3 / cross-check section below), not p_2.

### Checklist

- [ ] **MC missing** voice for lines: [5, 7, 13, 17, 19, 22, 30, 32, 45, 49, 51, 53, 57, 59, 61, 62, 63, 65, 67, 68, 69, 71]
- [ ] **MC extra** voice (not a You line): [2, 4, 18, 25, 26, 37, 38]
- [ ] MC files that sit on **Thoughts** lines: [12] (confirm intentional)
- [ ] **Mandy missing** voice for lines: [1, 2, 4, 6, 8, 9, 14, 16, 18, 20, 21, 24, 25, 26, 28, 29, 31, 33, 34, 36, 37, 38, 43, 44, 46, 50, 52, 54, 55, 58, 60, 64, 66, 70]
- [ ] **Author swap:** MC folder has Mrs Wong/Mandy lines [2, 4, 18, 25, 26, 37, 38]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Mrs Wong | Mandy | — | MISSING | Hey! |
| 2 | Mrs Wong | Mandy | MC | WRONG FOLDER | Isn’t that Miss Lee! Doing laundry at this hour? |
| 3 | You | MC | MC | OK | Uh, yeah. I just couldn’t sleep. |
| 4 | Mrs Wong | Mandy | MC | WRONG FOLDER | Poor girl. |
| 5 | You | MC | — | MISSING | ... |
| 6 | Mrs Wong | Mandy | — | MISSING | Why aren’t you saying anything? |
| 7 | You | MC | — | MISSING | I have some clothes I really need for tomorrow. |
| 8 | Mrs Wong | Mandy | — | MISSING | What’s the hurry? |
| 9 | Mrs Wong | Mandy | — | MISSING | Is everything alright? |
| 10 | Thoughts | Thoughts | — | no VO | Mrs Wong is always so kind to me... I used to tell her so many things. |
| 11 | Thoughts | Thoughts | — | no VO | But now I have blood on my hands... |
| 12 | Thoughts | Thoughts | MC | MC (thought?) | I can only pretend that everything is normal. |
| 13 | You | MC | — | MISSING | Yeah, I’m fine. And you? |
| 14 | Mrs Wong | Mandy | — | MISSING | Oh, I’m really tired. |
| 15 | You | MC | MC | OK | Sorry, just had a long day. |
| 16 | Mrs Wong | Mandy | — | MISSING | Same. |
| 17 | You | MC | — | MISSING | It’s just too hot to fall asleep in this weather. |
| 18 | Mrs Wong | Mandy | MC | WRONG FOLDER | Fair. I’m the opposite. |
| 19 | You | MC | — | MISSING | Just need to wash these for my boyfriend. |
| 20 | Mrs Wong | Mandy | — | MISSING | What’s the hurry? It’s 3 am. |
| 21 | Mrs Wong | Mandy | — | MISSING | I’d fall asleep as soon as my head hit the pillow, if I don’t need ... |
| 22 | You | MC | — | MISSING | My boyfriend needs to wear these tomorrow at work. |
| 23 | You | MC | MC | OK | Well, my boyfriend just told me to wash them because he needs them. |
| 24 | Mrs Wong | Mandy | — | MISSING | I see. |
| 25 | Mrs Wong | Mandy | MC | WRONG FOLDER | And my husband is playing Mahjong somewhere again, leaving me to ru... |
| 26 | Mrs Wong | Mandy | MC | WRONG FOLDER | It’s a wonder what we women put up with. |
| 27 | You | MC | MC | OK | Is everything alright? |
| 28 | Mrs Wong | Mandy | — | MISSING | You could say so. |
| 29 | Mrs Wong | Mandy | — | MISSING | My husband is off playing Mahjong again, someone has to run this pl... |
| 30 | You | MC | — | MISSING | I’m sorry, Mrs Wong. That sounds rough. |
| 31 | Mrs Wong | Mandy | — | MISSING | I’m used to it by now. |
| 32 | You | MC | — | MISSING | Phew. Couldn’t your son help? |
| 33 | Mrs Wong | Mandy | — | MISSING | He’s sick. |
| 34 | Mrs Wong | Mandy | — | MISSING | I asked him to stay home and rest. |
| 35 | You | MC | MC | OK | I’m sorry, Mrs Wong. That sounds rough. |
| 36 | Mrs Wong | Mandy | — | MISSING | I’m used to it by now. |
| 37 | Mrs Wong | Mandy | MC | WRONG FOLDER | Okay, enough about me. |
| 38 | Mrs Wong | Mandy | MC | WRONG FOLDER | I haven’t seen your boyfriend in a while, how are you guys doing? |
| 39 | Thoughts | Thoughts | — | no VO | A week ago, he proposed to me, and I said yes. |
| 40 | Thoughts | Thoughts | — | no VO | That was the happiest day of my life. |
| 41 | Thoughts | Thoughts | — | no VO | Why does this terrible thing need to happen to us... |
| 42 | You | MC | MC | OK | Jason just proposed to me last week... |
| 43 | Mrs Wong | Mandy | — | MISSING | Woah, congratulations, Miss Lee! Wait, did you say yes? |
| 44 | Mrs Wong | Mandy | — | MISSING | You seem more concerned than happy. |
| 45 | You | MC | — | MISSING | Jason is quite busy these days with his job. We are doing quite fine. |
| 46 | Mrs Wong | Mandy | — | MISSING | I see. |
| 47 | Thoughts | Thoughts | — | no VO | If this hadn’t happened, I’d probably be in Jason’s arms right now,... |
| 48 | You | MC | MC | OK | Of course, I’ve been waiting for his proposal for months! |
| 49 | You | MC | — | MISSING | I said yes. Sorry, just too many things happened... |
| 50 | Mrs Wong | Mandy | — | MISSING | I see. You’ll look so beautiful in your wedding dress. |
| 51 | You | MC | — | MISSING | Haha, you’re too kind. |
| 52 | Mrs Wong | Mandy | — | MISSING | How wonderful. |
| 53 | You | MC | — | MISSING | ... |
| 54 | Mrs Wong | Mandy | — | MISSING | ... |
| 55 | Mrs Wong | Mandy | — | MISSING | Give me your clothes and I’ll toss them in for you. |
| 56 | Thoughts | Thoughts | — | no VO | No, she can’t touch the clothes, it has blood all over... |
| 57 | You | MC | — | MISSING | I’ll do it myself—no need to trouble you. You must be tired. |
| 58 | Mrs Wong | Mandy | — | MISSING | Okay. Let me at least help you separate the colors from the whites— |
| 59 | You | MC | — | MISSING | No, thanks—I’ll just tuck everything in one big load. |
| 60 | Mrs Wong | Mandy | — | MISSING | That would ruin your clothes, Miss Lee. |
| 61 | You | MC | — | MISSING | It will be fine. |
| 62 | You | MC | — | MISSING | Nah, I don’t mind. |
| 63 | You | MC | — | MISSING | I’m short on cash, so I’ll just wash one load. |
| 64 | Mrs Wong | Mandy | — | MISSING | Sure. That comes to 80 cents in total. |
| 65 | You | MC | — | MISSING | Here. |
| 66 | Mrs Wong | Mandy | — | MISSING | Here you go. Machine Nr. 4. It’s the one on your left. |
| 67 | You | MC | — | MISSING | Thank you, Mrs Wong. |
| 68 | You | MC | — | MISSING | ... |
| 69 | You | MC | — | MISSING | Which washer was it again? |
| 70 | Mrs Wong | Mandy | — | MISSING | Machine Nr. 4. It’s the one on your left. |
| 71 | You | MC | — | MISSING | Nothing. I got this. |

---

## Phase 3 — LAU_story_phase_1

**Status:** MISMATCH

- Ink speaker lines: **53** (You/MC: 20, Mrs Wong/Mandy: 0, Thoughts: 1, Other: 32)
- Voice files: MC 24, Mandy 32

> **Note:** Ink routes this phase to **Lau**, but Mandy + many MC wavs under p_3 actually align with **Phase 2** Mandy_story_phase_1 (see cross-check).

### Checklist

- [ ] **MC missing** voice for lines: [2, 4, 10, 12, 18, 25, 28, 29, 37, 39, 44, 50, 52]
- [ ] **MC extra** voice (not a You line): [7, 13, 17, 19, 22, 30, 33, 46, 47, 53, 55, 57, 58, 59, 61, 63, 64]
- [ ] **Mandy extra** voice (not a Mrs Wong/Mandy line): [1, 2, 4, 6, 8, 9, 14, 16, 18, 20, 21, 24, 25, 26, 28, 29, 31, 32, 34, 35, 36, 41, 42, 44, 48, 50, 51, 54, 56, 60, 62, 65]
- [ ] **Author swap:** Mandy folder has You lines [2, 4, 18, 25, 28, 29, 44, 50]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Drunk Man | Other | Mandy | UNEXPECTED VO | A pretty lady at this hour? Are you looking for me? |
| 2 | You | MC | Mandy | WRONG FOLDER | Good Evening. |
| 3 | You | MC | MC | OK | I’m just here for the laundry. |
| 4 | You | MC | Mandy | WRONG FOLDER | Fuck off. |
| 5 | Drunk Man | Other | — | no VO (other) | Woah, take it easy, young lady. I was just joking around. |
| 6 | Drunk Man | Other | Mandy | UNEXPECTED VO | You’re lucky to have me here, you know. |
| 7 | Drunk Man | Other | MC | UNEXPECTED VO | No one would dare to harass a beautiful young lady like yourself in... |
| 8 | Drunk Cop | Other | Mandy | UNEXPECTED VO | No villains can slip through my fingers. |
| 9 | Thoughts | Thoughts | Mandy | WRONG (Mandy) | Shit. I thought he’s just an average drunk... |
| 10 | You | MC | — | MISSING | ... |
| 11 | Drunk Cop | Other | — | no VO (other) | ... |
| 12 | You | MC | — | MISSING | Sorry, but you don’t look like a police officer. |
| 13 | Drunk Cop | Other | MC | UNEXPECTED VO | What, are cops not allowed to do their laundry at night, |
| 14 | Drunk Cop | Other | Mandy | UNEXPECTED VO | after investigating a bloody crime scene? |
| 15 | You | MC | MC | OK | Never mind what I said. |
| 16 | Drunk Cop | Other | Mandy | UNEXPECTED VO | So? Your boyfriend wants you to wash clothes in the middle of the n... |
| 17 | Drunk Cop | Other | MC | UNEXPECTED VO | Why are you washing clothes in the middle of the night? |
| 18 | You | MC | Mandy | WRONG FOLDER | I can’t sleep. |
| 19 | Drunk Cop | Other | MC | UNEXPECTED VO | But why would you come to a laundromat at 3am? |
| 20 | Drunk Cop | Other | Mandy | UNEXPECTED VO | You’re young and beautiful, and you have a partner, |
| 21 | Drunk Cop | Other | Mandy | UNEXPECTED VO | which is the complete opposite of me. |
| 22 | Drunk Cop | Other | MC | UNEXPECTED VO | You’re not here to wash the smell of your ex-wife from your clothes... |
| 23 | You | MC | MC | OK | I’m sorry. |
| 24 | Drunk Cop | Other | Mandy | UNEXPECTED VO | I just can’t sleep being reminded of her. |
| 25 | You | MC | Mandy | WRONG FOLDER | ... |
| 26 | Drunk Cop | Other | Mandy | UNEXPECTED VO | ... |
| 27 | You | MC | MC | OK | My boyfriend needs these clothes tomorrow for work, Officer. |
| 28 | You | MC | Mandy | WRONG FOLDER | I need to wash some clothes for my boyfriend, Officer. |
| 29 | You | MC | Mandy | WRONG FOLDER | It’s none of your business. |
| 30 | Drunk Cop | Other | MC | UNEXPECTED VO | Ooh, pretty lady has some secrets. |
| 31 | Drunk Man | Other | Mandy | UNEXPECTED VO | Woah, take it easy, young lady. |
| 32 | Drunk Cop | Other | Mandy | UNEXPECTED VO | But don’t throw curveballs. I’m familiar with that. |
| 33 | Drunk Cop | Other | MC | UNEXPECTED VO | Just answer the question. |
| 34 | Drunk Cop | Other | Mandy | UNEXPECTED VO | But you still need to answer my question. |
| 35 | Drunk Cop | Other | Mandy | UNEXPECTED VO | Why are you washing clothes in the middle of the night? |
| 36 | Drunk Cop | Other | Mandy | UNEXPECTED VO | But your boyfriend couldn’t be bothered to accompany you at this hour? |
| 37 | You | MC | — | MISSING | He’s sick and he needs his clothes for work tomorrow. |
| 38 | Drunk Cop | Other | — | no VO (other) | Sick but still goes to work? Huh, that’s how I lost my wife. |
| 39 | You | MC | — | MISSING | He’s busy tonight, and since I have nothing to do at home anyway, |
| 40 | You | MC | MC | OK | I might as well help him with some chores. |
| 41 | Drunk Cop | Other | Mandy | UNEXPECTED VO | Busy at this hour, huh? |
| 42 | Drunk Cop | Other | Mandy | UNEXPECTED VO | Are you sure he’s not up to no good? |
| 43 | You | MC | MC | OK | Well, I can’t stop him from working. |
| 44 | You | MC | Mandy | WRONG FOLDER | ...He’s a hardworking guy. |
| 45 | Drunk Cop | Other | — | no VO (other) | Fair. Money is important. |
| 46 | Drunk Cop | Other | MC | UNEXPECTED VO | Your boyfriend is lucky to have someone like you. |
| 47 | Drunk Cop | Other | MC | UNEXPECTED VO | Go on and wash your clothes then, young lady. |
| 48 | Drunk Cop | Other | Mandy | UNEXPECTED VO | All good? |
| 49 | You | MC | MC | OK | .. |
| 50 | You | MC | Mandy | WRONG FOLDER | Which machine did I put my clothes in again? |
| 51 | Drunk Cop | Other | Mandy | UNEXPECTED VO | Hmmm... Number four? |
| 52 | You | MC | — | MISSING | Where can I find detergent? |
| 53 | Drunk Cop | Other | MC | UNEXPECTED VO | It’s on the table behind me. |

**Orphan VO line numbers** (no Ink speaker line at this index): [54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65]
- l_54: Mandy/p_3/p_3_l_54.wav
- l_55: MC/p_3/p_3_l_55.wav
- l_56: Mandy/p_3/p_3_l_56.wav
- l_57: MC/p_3/p_3_l_57.wav
- l_58: MC/p_3/p_3_l_58.wav
- l_59: MC/p_3/p_3_l_59.wav
- l_60: Mandy/p_3/p_3_l_60.wav
- l_61: MC/p_3/p_3_l_61.wav
- l_62: Mandy/p_3/p_3_l_62.wav
- l_63: MC/p_3/p_3_l_63.wav
- l_64: MC/p_3/p_3_l_64.wav
- l_65: Mandy/p_3/p_3_l_65.wav

---

## Phase 6 — Mandy_story_phase_2

**Status:** PARTIAL

- Ink speaker lines: **33** (You/MC: 14, Mrs Wong/Mandy: 14, Thoughts: 5, Other: 0)
- Voice files: MC 11, Mandy 13

### Checklist

- [ ] **MC missing** voice for lines: [29, 30, 32]
- [ ] **Mandy missing** voice for lines: [31]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | You | MC | MC | OK | Mrs Wong, there’s no heavy-duty laundry detergent left. |
| 2 | Mrs Wong | Mandy | Mandy | OK | Is that so? I remember I put a lot of regular detergent there, isn’... |
| 3 | Thoughts | Thoughts | — | no VO | I don’t want to lie to Mrs Wong, but how can I explain... |
| 4 | You | MC | MC | OK | It’s a bit awkward situation. My cat peed on the sheets. |
| 5 | You | MC | MC | OK | We have to wash these sheets with heavy-duty laundry detergent. |
| 6 | Mrs Wong | Mandy | Mandy | OK | Your cat? I thought your boyfriend didn’t allow you to keep a cat? |
| 7 | You | MC | MC | OK | You know? I’m in the time of the month. I have to wash these sheets... |
| 8 | You | MC | MC | OK | Only heavy-duty detergent can get it out. |
| 9 | Mrs Wong | Mandy | Mandy | OK | Oh, I understand. It’s awful that we women have to go through this ... |
| 10 | You | MC | MC | OK | I think heavy-duty detergent gets clothes cleaner. |
| 11 | Mrs Wong | Mandy | Mandy | OK | Okay. |
| 12 | Thoughts | Thoughts | — | no VO | Oh God, I completely forgot I’d ever said that to her... |
| 13 | Thoughts | Thoughts | — | no VO | A few months ago, Jason got mad at me because I suggested we get a ... |
| 14 | Thoughts | Thoughts | — | no VO | He said it was a waste of money. |
| 15 | Thoughts | Thoughts | — | no VO | How can I cover up my lie now... |
| 16 | You | MC | MC | OK | I convinced him because the kitty is so cute. |
| 17 | Mrs Wong | Mandy | Mandy | OK | Alright then. I didn’t expect someone as stubborn as your boyfriend... |
| 18 | You | MC | MC | OK | He is actually very gentle to me. He just doesn’t like cats that much. |
| 19 | Mrs Wong | Mandy | Mandy | OK | If you say so. |
| 20 | You | MC | MC | OK | Ah, I misspoke. It’s the neighbor’s cat. |
| 21 | Mrs Wong | Mandy | Mandy | OK | It sneaked all the way into your room? What a wild cat. |
| 22 | You | MC | MC | OK | Yeah, pretty wild. |
| 23 | Mrs Wong | Mandy | Mandy | OK | The heavy-duty detergents are in the backroom. |
| 24 | Mrs Wong | Mandy | Mandy | OK | I’m too tired to move... Can you get it yourself? |
| 25 | You | MC | MC | OK | Yes sure, no worries. |
| 26 | Mrs Wong | Mandy | Mandy | OK | Thank you. I will rest here then. |
| 27 | Mrs Wong | Mandy | Mandy | OK | Here is the key to the backroom. It’s near Washer Nr. 9. |
| 28 | Mrs Wong | Mandy | Mandy | OK | The detergent you want is called Enzyme Laundry Detergent, the blue... |
| 29 | You | MC | — | MISSING | Thank you, Mrs Wong. |
| 30 | You | MC | — | MISSING | Where is the backroom? |
| 31 | Mrs Wong | Mandy | — | MISSING | The backroom is at the corner, near the washer Nr. 9. |
| 32 | You | MC | — | MISSING | Which one is the heavy-duty detergent again? |
| 33 | Mrs Wong | Mandy | Mandy | OK | It’s called Enzyme Laundry Detergent, the blue one on the shelf. |

---

## Phase 7 — LAU_story_phase_2

**Status:** NO VO

- Ink speaker lines: **33** (You/MC: 9, Mrs Wong/Mandy: 0, Thoughts: 0, Other: 24)
- Voice files: MC 0, Mandy 0

_No MC/Mandy voice files for this phase._

## Phase 8 — Inner_voice_backroom_phase_1

**Status:** OK

- Ink speaker lines: **15** (You/MC: 0, Mrs Wong/Mandy: 0, Thoughts: 15, Other: 0)
- Voice files: MC 4, Mandy 0

### Checklist

- [ ] MC: voices Thoughts [1, 2, 3, 4]; missing Thoughts [5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
- [ ] MC files that sit on **Thoughts** lines: [1, 2, 3, 4] (confirm intentional)
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Thoughts | Thoughts | MC | MC (thought?) | The look in Mrs Wong’s eyes seemed to hold a hint of pity. |
| 2 | Thoughts | Thoughts | MC | MC (thought?) | The police officer grinned, as if he’d known all along. |
| 3 | Thoughts | Thoughts | MC | MC (thought?) | Did they all know, but were just toying with me? |
| 4 | Thoughts | Thoughts | MC | MC (thought?) | Had they seen the bloodstains on those clothes? |
| 5 | Thoughts | Thoughts | — | no VO | I’ve yet to cry. Yet to be sorrowful or to mourn. |
| 6 | Thoughts | Thoughts | — | no VO | With each step further I become less of a human. How long do I need... |
| 7 | Thoughts | Thoughts | — | no VO | Do I need to carry this secret with me for the rest of our life? |
| 8 | Thoughts | Thoughts | — | no VO | This won’t bring a dead man back. Regardless of how much detergent ... |
| 9 | Thoughts | Thoughts | — | no VO | Saying it does not bring him back. Then nothing will give me peace. |
| 10 | Thoughts | Thoughts | — | no VO | And when I’m done with this, a murder awaits me at home. |
| 11 | Thoughts | Thoughts | — | no VO | This won’t bring him back. Regardless of how much detergent goes in... |
| 12 | Thoughts | Thoughts | — | no VO | Saying it does not bring him back. |
| 13 | Thoughts | Thoughts | — | no VO | But at least then the crime would exist somewhere outside my own sk... |
| 14 | Thoughts | Thoughts | — | no VO | Does Jason think the same? |
| 15 | Thoughts | Thoughts | — | no VO | For how long can I keep up this facade, these lies? |

---

## Phase 13 — Thought_washing_clothes_1

**Status:** PARTIAL

- Ink speaker lines: **7** (You/MC: 0, Mrs Wong/Mandy: 0, Thoughts: 7, Other: 0)
- Voice files: MC 8, Mandy 0

### Checklist

- [ ] **MC extra** voice (not a You line): [8]
- [ ] MC files that sit on **Thoughts** lines: [1, 2, 3, 4, 5, 6, 7] (confirm intentional)
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Thoughts | Thoughts | MC | MC (thought?) | Jason was wearing this shirt when we met. |
| 2 | Thoughts | Thoughts | MC | MC (thought?) | On a rainy day, I accidentally slipped and my groceries fell to the... |
| 3 | Thoughts | Thoughts | MC | MC (thought?) | He was the one who helped me pick them up. We were together two wee... |
| 4 | Thoughts | Thoughts | MC | MC (thought?) | I thought we would continue living this ordinary life together, in ... |
| 5 | Thoughts | Thoughts | MC | MC (thought?) | Who could have thought... |
| 6 | Thoughts | Thoughts | MC | MC (thought?) | I wonder if we could ever forget what happened after we washed these. |
| 7 | Thoughts | Thoughts | MC | MC (thought?) | Could we really put these shirts on and pretend they were never sta... |

**Orphan VO line numbers** (no Ink speaker line at this index): [8]
- l_8: MC/p_13/p_13_l_8.wav

---

## Phase 15 — Mandy_story_phase_3

**Status:** OK

- Ink speaker lines: **59** (You/MC: 24, Mrs Wong/Mandy: 22, Thoughts: 10, Other: 3)
- Voice files: MC 24, Mandy 22

### Checklist

- [x] MC line numbers match
- [x] Mandy line numbers match Mrs Wong/Mandy lines

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Mrs Wong | Mandy | Mandy | OK | You get your change now? |
| 2 | You | MC | MC | OK | Not yet. Where is the coin change machine again? |
| 3 | Mrs Wong | Mandy | Mandy | OK | It’s the one in the corner. |
| 4 | You | MC | MC | OK | Thanks. |
| 5 | You | MC | MC | OK | Mrs Wong, can I buy another laundry coin? |
| 6 | Mrs Wong | Mandy | Mandy | OK | Another one? Did all your clothes fall into a pit or something? |
| 7 | You | MC | MC | OK | You know, sweating in the summer and stuff. I can’t stand smelly cl... |
| 8 | Mrs Wong | Mandy | Mandy | OK | Hmm... I remember you were here just two days ago. |
| 9 | You | MC | MC | OK | Well, clothes get dirty pretty fast in summer. |
| 10 | You | MC | MC | OK | I told you, the cat peed on our clothes, so we have to wash all the... |
| 11 | Mrs Wong | Mandy | Mandy | OK | Naughty cat. What’s the name? |
| 12 | You | MC | MC | OK | Well... |
| 13 | You | MC | MC | OK | My flow is especially heavy today... |
| 14 | You | MC | MC | OK | And I accidentally put my clothes on the bloodstained sheets. |
| 15 | Mrs Wong | Mandy | Mandy | OK | Why didn’t your boyfriend come and let you rest? |
| 16 | Thoughts | Thoughts | — | no VO | He’s currently dealing with the body... |
| 17 | You | MC | MC | OK | It was me who offered to help him. He is too busy with work. |
| 18 | You | MC | MC | OK | He doesn’t have time. |
| 19 | Mrs Wong | Mandy | Mandy | OK | Hah, such a typical excuse. |
| 20 | You | MC | MC | OK | He works very hard to earn money though. |
| 21 | Mrs Wong | Mandy | Mandy | OK | Hmm. I have to warn you, when a man stops getting involved in house... |
| 22 | Mrs Wong | Mandy | Mandy | OK | it’s normally a sign that he will start neglecting your feelings. |
| 23 | Thoughts | Thoughts | — | no VO | It’s true that Jason always asks me to do housework... |
| 24 | Thoughts | Thoughts | — | no VO | But he works a lot to make money. |
| 25 | Thoughts | Thoughts | — | no VO | And his marriage proposal was so romantic... |
| 26 | You | MC | MC | OK | Don’t worry, my boyfriend is different. |
| 27 | Mrs Wong | Mandy | Mandy | OK | I don’t mean to scare you so soon after you just accepted his propo... |
| 28 | Mrs Wong | Mandy | Mandy | OK | I’m not saying your boyfriend is just like my useless husband. |
| 29 | Mrs Wong | Mandy | Mandy | OK | But many of them always start out romantic, and then... |
| 30 | Mrs Wong | Mandy | Mandy | OK | My husband used to buy me jewelry and take me to the docks for star... |
| 31 | Mrs Wong | Mandy | Mandy | OK | Now, he won’t even talk to me, unless he wants food or needs me to ... |
| 32 | Mrs Wong | Mandy | Mandy | OK | And you can see Mr. Lau there, being drunk at 3am... |
| 33 | Mrs Wong | Mandy | Mandy | OK | I have to say, some men really are useless... |
| 34 | Drunk Cop | Other | — | no VO (other) | You’re speaking a bit too loudly, aren’t you? |
| 35 | You | MC | MC | OK | We were talking about you. |
| 36 | Drunk Cop | Other | — | no VO (other) | I? I’m not useless, I’m the best cop in the whole precinct. |
| 37 | You | MC | MC | OK | We were not talking about you. |
| 38 | Drunk Cop | Other | — | no VO (other) | ... |
| 39 | Mrs Wong | Mandy | Mandy | OK | Haha. |
| 40 | Thoughts | Thoughts | — | no VO | Now I just have to come up with a name for this imaginary cat... |
| 41 | You | MC | MC | OK | He’s Jason. |
| 42 | Mrs Wong | Mandy | Mandy | OK | Interesting choice to name your cat after your boyfriend, haha. |
| 43 | You | MC | MC | OK | ... We thought it’s funny. |
| 44 | You | MC | MC | OK | ... haha |
| 45 | You | MC | MC | OK | Miao Miao. |
| 46 | You | MC | MC | OK | Caesar. |
| 47 | Mrs Wong | Mandy | Mandy | OK | Cute. |
| 48 | Thoughts | Thoughts | — | no VO | I should wash them quickly, Jason will get mad if I’m too slow... |
| 49 | You | MC | MC | OK | The second washer would cost 80 cents, too, right? |
| 50 | Mrs Wong | Mandy | Mandy | OK | Yes. |
| 51 | Thoughts | Thoughts | — | no VO | (Damn, I’d completely forgotten. |
| 52 | Thoughts | Thoughts | — | no VO | (Blood had splattered on our money when it happened. |
| 53 | You | MC | MC | OK | Uhm, give me one second. I have to get some change. |
| 54 | Mrs Wong | Mandy | Mandy | OK | Are you sure? I have change here. |
| 55 | Thoughts | Thoughts | — | no VO | (Mrs Wong can’t see the blood on the bill. |
| 56 | Thoughts | Thoughts | — | no VO | (I need to use the coin change machine. |
| 57 | You | MC | MC | OK | No worries, I got this. |
| 58 | You | MC | MC | OK | It’s okay, I need some change for payphones anyways. |
| 59 | Mrs Wong | Mandy | Mandy | OK | Sure. The coin change machine is in the corner. |

---

## Phase 18 — Mandy_story_phase_4

**Status:** PARTIAL

- Ink speaker lines: **23** (You/MC: 5, Mrs Wong/Mandy: 9, Thoughts: 9, Other: 0)
- Voice files: MC 5, Mandy 8

### Checklist

- [ ] **MC missing** voice for lines: [10, 12, 21]
- [ ] **MC extra** voice (not a You line): [11]
- [ ] MC files that sit on **Thoughts** lines: [9, 19] (confirm intentional)
- [ ] **Mandy missing** voice for lines: [5, 11, 14, 23]
- [ ] **Mandy extra** voice (not a Mrs Wong/Mandy line): [10, 12, 21]
- [ ] **Author swap:** Mandy folder has You lines [10, 12, 21]
- [ ] **Author swap:** MC folder has Mrs Wong/Mandy lines [11]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Mrs Wong | Mandy | Mandy | OK | You get your change now? |
| 2 | You | MC | MC | OK | Yes, here. |
| 3 | Mrs Wong | Mandy | Mandy | OK | Here you are. Washer Nr. 9. |
| 4 | Mrs Wong | Mandy | Mandy | OK | Miss Lee, wait. |
| 5 | Mrs Wong | Mandy | — | MISSING | What happened with your arm? |
| 6 | Thoughts | Thoughts | — | no VO | I almost thought there was still blood on my hand. |
| 7 | Thoughts | Thoughts | — | no VO | It was just a bruise. |
| 8 | Thoughts | Thoughts | — | no VO | Jason had gripped my arm tightly when he asked me to get the knife. |
| 9 | Thoughts | Thoughts | MC | MC (thought?) | I hadn’t even noticed that you could see it at all. |
| 10 | You | MC | Mandy | WRONG FOLDER | Oh, I was too clumsy, I accidentally bumped into the corner of the ... |
| 11 | Mrs Wong | Mandy | MC | WRONG FOLDER | Really? I thought these come from... You know. |
| 12 | You | MC | Mandy | WRONG FOLDER | Oh, you don’t really need to know, Mrs Wong. |
| 13 | Mrs Wong | Mandy | Mandy | OK | I know where these come from. |
| 14 | Mrs Wong | Mandy | — | MISSING | Whenever my husband comes home drunk, I have to put up with that, too. |
| 15 | Thoughts | Thoughts | — | no VO | Jason had a few drinks tonight before it happened. |
| 16 | Thoughts | Thoughts | — | no VO | With alcohol he becomes different, sometimes. |
| 17 | Thoughts | Thoughts | — | no VO | He can get... pretty rough. |
| 18 | Thoughts | Thoughts | — | no VO | But that’s mostly because of the stress from his job. |
| 19 | Thoughts | Thoughts | MC | MC (thought?) | Aside from these drunk episodes, he’s the sweetest person I’ve ever... |
| 20 | You | MC | MC | OK | No Jason is not like that, it happened by accident. |
| 21 | You | MC | Mandy | WRONG FOLDER | I understand...I feel so sorry for you. |
| 22 | Mrs Wong | Mandy | Mandy | OK | If you have any trouble, please let me know, okay? |
| 23 | Mrs Wong | Mandy | — | MISSING | Here you are. Washer Nr. 9. |

---

## Phase 19 — LAU_story_phase_3

**Status:** PARTIAL

- Ink speaker lines: **60** (You/MC: 27, Mrs Wong/Mandy: 0, Thoughts: 4, Other: 29)
- Voice files: MC 26, Mandy 0

### Checklist

- [ ] **MC missing** voice for lines: [23, 26, 33, 36, 39, 47, 48, 49, 55, 57]
- [ ] **MC extra** voice (not a You line): [24, 27, 29, 40, 44, 45, 53, 54, 60]
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Drunk Cop | Other | — | no VO (other) | There’s an awfully red stain on your clothes. |
| 2 | You | MC | MC | OK | Well, my boyfriend accidentally broke a bottle and cut his hand. |
| 3 | Drunk Cop | Other | — | no VO (other) | Uh, that must’ve hurt. |
| 4 | Drunk Cop | Other | — | no VO (other) | Why is there blood all over the chest area of this shirt? |
| 5 | You | MC | MC | OK | We tried to wrap his hand around his shirt to stop the bleeding. |
| 6 | Drunk Cop | Other | — | no VO (other) | Wrap his hand around his shirt? Hahaha |
| 7 | Drunk Cop | Other | — | no VO (other) | I’m starting to wonder if you’re actually more drunk than I am. |
| 8 | Thoughts | Thoughts | — | no VO | Stupid misspoke...I hope he doesn’t notice that my hands are shaking. |
| 9 | Drunk Cop | Other | — | no VO (other) | I hope your boyfriend is doing alright. |
| 10 | You | MC | MC | OK | Yes, he is, thanks. That’s why he’s home. |
| 11 | You | MC | MC | OK | You’re the nosiest cop I’ve ever seen. |
| 12 | Drunk Cop | Other | — | no VO (other) | Or the most observant. |
| 13 | You | MC | MC | OK | Well, my boyfriend spilled red wine all over the place. |
| 14 | Drunk Cop | Other | — | no VO (other) | Really... What kind of wine is that? It’s as red as blood. |
| 15 | Thoughts | Thoughts | — | no VO | Damn, I know nothing about wine. |
| 16 | You | MC | MC | OK | I don’t remember. |
| 17 | You | MC | MC | OK | He got this wine from the wine shop on the next street over. |
| 18 | You | MC | MC | OK | la... Toise. Something like it. |
| 19 | Drunk Cop | Other | — | no VO (other) | Interesting, never heard of that before. I thought I knew everythin... |
| 20 | You | MC | MC | OK | It’s rare, my boyfriend got it as a gift from a friend abroad. |
| 21 | Drunk Cop | Other | — | no VO (other) | How strange... |
| 22 | Drunk Cop | Other | — | no VO (other) | Blood, on the other hand, turns dark, rust-red. |
| 23 | You | MC | — | MISSING | I’m... having the time of the month. |
| 24 | Drunk Cop | Other | MC | UNEXPECTED VO | Oh, I see. |
| 25 | Drunk Cop | Other | — | no VO (other) | But why is there blood all over the chest area of this shirt? |
| 26 | You | MC | — | MISSING | I accidentally put it in the wrong spot on the bedsheet. |
| 27 | Drunk Cop | Other | MC | UNEXPECTED VO | Okay... |
| 28 | Drunk Cop | Other | — | no VO (other) | I’m a cop and I patrol this area: |
| 29 | Drunk Cop | Other | MC | UNEXPECTED VO | Of course, I have to take care of other people’s business. |
| 30 | Thoughts | Thoughts | — | no VO | I thought I could get away with this... |
| 31 | Thoughts | Thoughts | — | no VO | Now I have to come up with an excuse. |
| 32 | Drunk Cop | Other | — | no VO (other) | But I thought he stayed at home because he’s sick? |
| 33 | You | MC | — | MISSING | That as well. |
| 34 | Drunk Cop | Other | — | no VO (other) | Cut his hand and got sick, that’s almost unrealistically tragic. Ma... |
| 35 | You | MC | MC | OK | He already fell asleep. |
| 36 | You | MC | — | MISSING | He’s taking some rest, so I’d just do some housework before this he... |
| 37 | You | MC | MC | OK | It was from my mom. |
| 38 | You | MC | MC | OK | No, it’s my best friend texting me. |
| 39 | You | MC | — | MISSING | She recently bought a pager and has been using it all the time. |
| 40 | Drunk Cop | Other | MC | UNEXPECTED VO | But I thought he stayed at home because he’s busy? |
| 41 | You | MC | MC | OK | Yes, he had some stuff to do for his work. |
| 42 | You | MC | MC | OK | Busy with sleeping. |
| 43 | Drunk Cop | Other | — | no VO (other) | This red stain reminds me of the crime scene I witnessed today. |
| 44 | Drunk Cop | Other | MC | UNEXPECTED VO | A middle-aged man stabbed his wife to death. |
| 45 | Drunk Cop | Other | MC | UNEXPECTED VO | He refused to plead guilty, so we had no choice but to put him in j... |
| 46 | Drunk Cop | Other | — | no VO (other) | Her shirt also has this red stain... |
| 47 | You | MC | — | MISSING | There’s no need to overthink it. |
| 48 | You | MC | — | MISSING | The bottle of wine my boyfriend spilled might just have been made d... |
| 49 | You | MC | — | MISSING | Why do you need to overthink so much? |
| 50 | You | MC | MC | OK | Isn’t cutting your hand accidentally quite normal? |
| 51 | You | MC | MC | OK | ...Sounds scary. |
| 52 | You | MC | MC | OK | There’s no need to overthink it. |
| 53 | Drunk Cop | Other | MC | UNEXPECTED VO | All right then... I hope you’re being honest. |
| 54 | Drunk Cop | Other | MC | UNEXPECTED VO | You do know what happens if you lie to a police officer, don’t you? |
| 55 | You | MC | — | MISSING | ... |
| 56 | Drunk Cop | Other | — | no VO (other) | Haha. I was just joking around. Go wash your clothes. |
| 57 | You | MC | — | MISSING | Of course I won’t lie to you. |
| 58 | Drunk Cop | Other | — | no VO (other) | Sweet girl. |
| 59 | Drunk Cop | Other | — | no VO (other) | But if you’re hiding anything, you’d better tell me soon. |
| 60 | Drunk Cop | Other | MC | UNEXPECTED VO | Go wash your clothes first. |

---

## Phase 20 — Thought_washing_clothes_2

**Status:** OK

- Ink speaker lines: **11** (You/MC: 0, Mrs Wong/Mandy: 0, Thoughts: 11, Other: 0)
- Voice files: MC 11, Mandy 0

### Checklist

- [x] MC line numbers match
- [ ] MC files that sit on **Thoughts** lines: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] (confirm intentional)
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Thoughts | Thoughts | MC | MC (thought?) | Before that person came in, Jason was holding me tightly. |
| 2 | Thoughts | Thoughts | MC | MC (thought?) | He was wearing this. It was so warm and I felt so safe in his arm. |
| 3 | Thoughts | Thoughts | MC | MC (thought?) | Why did we.. why did Jason do that. |
| 4 | Thoughts | Thoughts | MC | MC (thought?) | I can’t forget the dead man’s open eyes... |
| 5 | Thoughts | Thoughts | MC | MC (thought?) | I can’t forget the smell of blood when Jason held me after it happe... |
| 6 | Thoughts | Thoughts | MC | MC (thought?) | He rocked me back and forth, as if I were trapped in a cradle... |
| 7 | Thoughts | Thoughts | MC | MC (thought?) | How long do I need to hide? |
| 8 | Thoughts | Thoughts | MC | MC (thought?) | Maybe it’s okay to give up... |
| 9 | Thoughts | Thoughts | MC | MC (thought?) | I suddenly felt a strange sense of relief. |
| 10 | Thoughts | Thoughts | MC | MC (thought?) | But how could I possibly abandon the one I love so deeply, |
| 11 | Thoughts | Thoughts | MC | MC (thought?) | the one who protected me and even killed someone to keep me safe? |

---

## Phase 21 — Chaos_blackout

**Status:** PARTIAL

- Ink speaker lines: **4** (You/MC: 1, Mrs Wong/Mandy: 2, Thoughts: 0, Other: 1)
- Voice files: MC 1, Mandy 3

### Checklist

- [ ] **MC missing** voice for lines: [3]
- [ ] **MC extra** voice (not a You line): [4]
- [ ] **Mandy missing** voice for lines: [4]
- [ ] **Mandy extra** voice (not a Mrs Wong/Mandy line): [3, 5]
- [ ] **Author swap:** Mandy folder has You lines [3]
- [ ] **Author swap:** MC folder has Mrs Wong/Mandy lines [4]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Drunk Cop | Other | — | no VO (other) | Ah, what the hell is this? |
| 2 | Mrs Wong | Mandy | Mandy | OK | Not again... I need to check the circuit box in the back room... |
| 3 | You | MC | Mandy | WRONG FOLDER | I can do it. I’m standing next to it. |
| 4 | Mrs Wong | Mandy | MC | WRONG FOLDER | Thank you, Miss Lee. |

**Orphan VO line numbers** (no Ink speaker line at this index): [5]
- l_5: Mandy/p_21/p_21_l_5.wav

---

## Phase 26 — Mandy_smoking_scene_1

**Status:** OK

- Ink speaker lines: **15** (You/MC: 7, Mrs Wong/Mandy: 8, Thoughts: 0, Other: 0)
- Voice files: MC 7, Mandy 8

### Checklist

- [x] MC line numbers match
- [x] Mandy line numbers match Mrs Wong/Mandy lines

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Mrs Wong | Mandy | Mandy | OK | Is everything okay back there? You’ve been gone quite a while, Miss... |
| 2 | You | MC | MC | OK | Sorry, I couldn’t find the correct switch for the lights. |
| 3 | You | MC | MC | OK | I’m really sorry. |
| 4 | Mrs Wong | Mandy | Mandy | OK | No need to apologize. Want a cigarette? |
| 5 | You | MC | MC | OK | No, thank you. |
| 6 | Mrs Wong | Mandy | Mandy | OK | Mind if we talk for a second? Just between us women. |
| 7 | You | MC | MC | OK | Sure. What’s it about? |
| 8 | You | MC | MC | OK | I’m not sure... |
| 9 | Mrs Wong | Mandy | Mandy | OK | I’m just worried about you. |
| 10 | Mrs Wong | Mandy | Mandy | OK | Someone’s been paging you all the time, right? Is that your boyfriend? |
| 11 | You | MC | MC | OK | Yes, he just worried because it’s late. |
| 12 | You | MC | MC | OK | No... |
| 13 | Mrs Wong | Mandy | Mandy | OK | You can be completely honest with me, Miss Lee. |
| 14 | Mrs Wong | Mandy | Mandy | OK | Every time you come out from the backroom like a ghost, and... |
| 15 | Mrs Wong | Mandy | Mandy | OK | There’s more red in your clothes than on your face. |

---

## Phase 27 — Mandy_smoking_scene_2

**Status:** PARTIAL

- Ink speaker lines: **29** (You/MC: 9, Mrs Wong/Mandy: 20, Thoughts: 0, Other: 0)
- Voice files: MC 8, Mandy 19

### Checklist

- [ ] **MC missing** voice for lines: [6]
- [ ] **Mandy missing** voice for lines: [10]

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | You | MC | MC | OK | Mrs Wong, I’ve done something bad... |
| 2 | You | MC | MC | OK | Mrs Wong, nothing happened. |
| 3 | Mrs Wong | Mandy | Mandy | OK | Then I must have been overthinking it. |
| 4 | Mrs Wong | Mandy | Mandy | OK | I’m going to have a smoke here for a bit. |
| 5 | Mrs Wong | Mandy | Mandy | OK | If you want to come talk to me later, feel free to stop by again. |
| 6 | You | MC | — | MISSING | Nothing. |
| 7 | Mrs Wong | Mandy | Mandy | OK | Okay. |
| 8 | Mrs Wong | Mandy | Mandy | OK | I’m going to have a smoke here for a bit. |
| 9 | Mrs Wong | Mandy | Mandy | OK | If you want to come talk to me later, feel free to stop by again. |
| 10 | Mrs Wong | Mandy | — | MISSING | Calm down and take a deep breath. I’m here. |
| 11 | You | MC | MC | OK | Someone broke in... |
| 12 | You | MC | MC | OK | Jason... he... everything happened so fast. |
| 13 | You | MC | MC | OK | he told me if the blood was washed away, everything would go back t... |
| 14 | Mrs Wong | Mandy | Mandy | OK | My goodness, Miss Lee... |
| 15 | Mrs Wong | Mandy | Mandy | OK | So all those lies... You were just trying to survive, right? |
| 16 | You | MC | MC | OK | I’m sorry, I was panicking. |
| 17 | Mrs Wong | Mandy | Mandy | OK | (Sigh) |
| 18 | Mrs Wong | Mandy | Mandy | OK | I should have known he was that kind of guy. |
| 19 | You | MC | MC | OK | But he loves me! He was protecting me, Mrs Wong... |
| 20 | Mrs Wong | Mandy | Mandy | OK | Did he do that for you, or for his own safety? |
| 21 | Mrs Wong | Mandy | Mandy | OK | But what are you going to do? |
| 22 | Mrs Wong | Mandy | Mandy | OK | They’ll find out sooner or later. |
| 23 | You | MC | MC | OK | I don’t know, Mrs Wong... |
| 24 | Mrs Wong | Mandy | Mandy | OK | ...My cousin lives in Tou San. |
| 25 | Mrs Wong | Mandy | Mandy | OK | You can go there, but you’ll have to leave everything behind - |
| 26 | Mrs Wong | Mandy | Mandy | OK | Including your boyfriend - and try to make a living there. |
| 27 | Mrs Wong | Mandy | Mandy | OK | I can’t guarantee it’s safe there, or that no one will find you. |
| 28 | Mrs Wong | Mandy | Mandy | OK | But it’s better than continuing to hide here. |
| 29 | Mrs Wong | Mandy | Mandy | OK | Think about it, Miss Lee. Take your time. |

---

## Phase 28 — Mandy_smoking_scene_3

**Status:** OK

- Ink speaker lines: **23** (You/MC: 6, Mrs Wong/Mandy: 13, Thoughts: 4, Other: 0)
- Voice files: MC 6, Mandy 13

### Checklist

- [x] MC line numbers match
- [x] Mandy line numbers match Mrs Wong/Mandy lines

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | You | MC | MC | OK | What is Tou San like? |
| 2 | Mrs Wong | Mandy | Mandy | OK | My cousin said it’s a county town, not as bustling as here. |
| 3 | Mrs Wong | Mandy | Mandy | OK | If you put in some effort, you can still find a good way to make a ... |
| 4 | You | MC | MC | OK | I don’t know... I need to think. |
| 5 | Mrs Wong | Mandy | Mandy | OK | Take your time. I’ll stay here for a while. |
| 6 | Mrs Wong | Mandy | Mandy | OK | you can come back to me whenever you’re ready. |
| 7 | Thoughts | Thoughts | — | no VO | It’s time to live on my own. |
| 8 | Thoughts | Thoughts | — | no VO | I don’t need to listen to Jason anymore, |
| 9 | Thoughts | Thoughts | — | no VO | and I don’t need to obey him all the time. |
| 10 | Thoughts | Thoughts | — | no VO | I have to escape this life. And escape him. |
| 11 | You | MC | MC | OK | So, how can I go to Tou San? |
| 12 | Mrs Wong | Mandy | Mandy | OK | There’s a ferry to go there every morning at 8:00. |
| 13 | Mrs Wong | Mandy | Mandy | OK | You need to head to the harbor now. |
| 14 | Mrs Wong | Mandy | Mandy | OK | So leave this man, start a new life. |
| 15 | You | MC | MC | OK | Thank you, Mrs Wong... |
| 16 | Mrs Wong | Mandy | Mandy | OK | You can just call me Mandy. |
| 17 | Mandy | Mandy | Mandy | OK | I’ll send a message to my cousin Cindy to pick you up. |
| 18 | Mandy | Mandy | Mandy | OK | Also, Vivian... be independent. |
| 19 | Mandy | Mandy | Mandy | OK | That’s the most precious thing a woman can have. |
| 20 | Mandy | Mandy | Mandy | OK | Don’t rely on any men for your life and happiness. |
| 21 | You | MC | MC | OK | I promise you, Mandy. |
| 22 | Mandy | Mandy | Mandy | OK | Goodbye, Vivian. |
| 23 | You | MC | MC | OK | Goodbye, Mandy. |

---

## Phase 30 — Lau_confess_ending

**Status:** PARTIAL

- Ink speaker lines: **24** (You/MC: 12, Mrs Wong/Mandy: 0, Thoughts: 0, Other: 12)
- Voice files: MC 11, Mandy 0

### Checklist

- [ ] **MC missing** voice for lines: [14]
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | Drunk Cop | Other | — | no VO (other) | What’s that face for, sweet heart? You look like you just saw a ghost. |
| 2 | You | MC | MC | OK | I’m just tired. |
| 3 | You | MC | MC | OK | I want to report a murder. |
| 4 | Police Officer | Other | — | no VO (other) | Say that again, miss. What happened? |
| 5 | You | MC | MC | OK | Someone broke in and threatened us. |
| 6 | You | MC | MC | OK | And my boyfriend... My boyfriend wanted to solve it with a knife. |
| 7 | You | MC | MC | OK | It all happened so fast... |
| 8 | Police Officer | Other | — | no VO (other) | Were you a part of this? |
| 9 | You | MC | MC | OK | He asked me to get a knife for him when the intruder broke in. |
| 10 | You | MC | MC | OK | And then asked me to wash these clothes. |
| 11 | You | MC | MC | OK | I didn’t touch the knife. He asked me to wash those clothes, |
| 12 | You | MC | MC | OK | I didn’t know what to do so I came here. |
| 13 | Police Officer | Other | — | no VO (other) | Where is the suspect right now? |
| 14 | You | MC | — | MISSING | He’s at our apartment. 4th Floor, Block 3, 32 Lin Faa Street. |
| 15 | Police Officer | Other | — | no VO (other) | Miss, please cooperate. |
| 16 | Police Officer | Other | — | no VO (other) | What’s your name? |
| 17 | You | MC | MC | OK | Vivian Lee. |
| 18 | Police Officer | Other | — | no VO (other) | And his name? |
| 19 | You | MC | MC | OK | Jason Ho. |
| 20 | Police Officer | Other | — | no VO (other) | ...Miss, you did the right thing. |
| 21 | Police Officer | Other | — | no VO (other) | I knew something was fishy, but I’m glad you were the first to tell... |
| 22 | Police Officer | Other | — | no VO (other) | Otherwise, the crime of harboring a murderer is very serious. |
| 23 | Police Officer | Other | — | no VO (other) | It took courage to confess and report a crime of your lover. |
| 24 | Police Officer | Other | — | no VO (other) | Now, please come with me. |

---

## Phase 31 — Boyfriend_ending_dialogue_final

**Status:** PARTIAL

- Ink speaker lines: **10** (You/MC: 3, Mrs Wong/Mandy: 0, Thoughts: 0, Other: 7)
- Voice files: MC 1, Mandy 0

### Checklist

- [ ] **MC missing** voice for lines: [4, 8]
- [x] Mandy N/A (no Mrs Wong/Mandy lines and no Mandy VO)

### Line-by-line

| l | Ink speaker | Role | VO folder | Verdict | Text (preview) |
|--:|-------------|------|-----------|---------|----------------|
| 1 | You | MC | MC | OK | I’m back. |
| 2 | Jason | Other | — | no VO (other) | I just got home, too. |
| 3 | Jason | Other | — | no VO (other) | I love you. |
| 4 | You | MC | — | MISSING | I love you, too. |
| 5 | Jason | Other | — | no VO (other) | It will pass. |
| 6 | Jason | Other | — | no VO (other) | We still have tomorrow. |
| 7 | Jason | Other | — | no VO (other) | We’ve got us. |
| 8 | You | MC | — | MISSING | We’ve got us. |
| 9 | Jason | Other | — | no VO (other) | Now, try to forget about it. |
| 10 | Jason | Other | — | no VO (other) | Come into my arm, sweetheart. |

---

## Cross-check: p_3 voice files vs Phase 2 Ink (Mandy_story_phase_1)

Hypothesis: Mandy’s first-scene VO was exported as p_3 instead of p_2.

| l | Ink (phase 2) | Role | p_3 VO | Verdict | Text |
|--:|---------------|------|----------|---------|------|
| 1 | Mrs Wong | Mandy | Mandy | OK | Hey! |
| 2 | Mrs Wong | Mandy | Mandy | OK | Isn’t that Miss Lee! Doing laundry at this hour? |
| 3 | You | MC | MC | OK | Uh, yeah. I just couldn’t sleep. |
| 4 | Mrs Wong | Mandy | Mandy | OK | Poor girl. |
| 5 | You | MC | — | MISSING in p_3 | ... |
| 6 | Mrs Wong | Mandy | Mandy | OK | Why aren’t you saying anything? |
| 7 | You | MC | MC | OK | I have some clothes I really need for tomorrow. |
| 8 | Mrs Wong | Mandy | Mandy | OK | What’s the hurry? |
| 9 | Mrs Wong | Mandy | Mandy | OK | Is everything alright? |
| 10 | Thoughts | Thoughts | — | MISSING in p_3 | Mrs Wong is always so kind to me... I used to tell her so... |
| 11 | Thoughts | Thoughts | — | MISSING in p_3 | But now I have blood on my hands... |
| 12 | Thoughts | Thoughts | — | MISSING in p_3 | I can only pretend that everything is normal. |
| 13 | You | MC | MC | OK | Yeah, I’m fine. And you? |
| 14 | Mrs Wong | Mandy | Mandy | OK | Oh, I’m really tired. |
| 15 | You | MC | MC | OK | Sorry, just had a long day. |
| 16 | Mrs Wong | Mandy | Mandy | OK | Same. |
| 17 | You | MC | MC | OK | It’s just too hot to fall asleep in this weather. |
| 18 | Mrs Wong | Mandy | Mandy | OK | Fair. I’m the opposite. |
| 19 | You | MC | MC | OK | Just need to wash these for my boyfriend. |
| 20 | Mrs Wong | Mandy | Mandy | OK | What’s the hurry? It’s 3 am. |
| 21 | Mrs Wong | Mandy | Mandy | OK | I’d fall asleep as soon as my head hit the pillow, if I d... |
| 22 | You | MC | MC | OK | My boyfriend needs to wear these tomorrow at work. |
| 23 | You | MC | MC | OK | Well, my boyfriend just told me to wash them because he n... |
| 24 | Mrs Wong | Mandy | Mandy | OK | I see. |
| 25 | Mrs Wong | Mandy | Mandy | OK | And my husband is playing Mahjong somewhere again, leavin... |
| 26 | Mrs Wong | Mandy | Mandy | OK | It’s a wonder what we women put up with. |
| 27 | You | MC | MC | OK | Is everything alright? |
| 28 | Mrs Wong | Mandy | Mandy | OK | You could say so. |
| 29 | Mrs Wong | Mandy | Mandy | OK | My husband is off playing Mahjong again, someone has to r... |
| 30 | You | MC | MC | OK | I’m sorry, Mrs Wong. That sounds rough. |
| 31 | Mrs Wong | Mandy | Mandy | OK | I’m used to it by now. |
| 32 | You | MC | Mandy | WRONG AUTHOR | Phew. Couldn’t your son help? |
| 33 | Mrs Wong | Mandy | MC | WRONG AUTHOR | He’s sick. |
| 34 | Mrs Wong | Mandy | Mandy | OK | I asked him to stay home and rest. |
| 35 | You | MC | Mandy | WRONG AUTHOR | I’m sorry, Mrs Wong. That sounds rough. |
| 36 | Mrs Wong | Mandy | Mandy | OK | I’m used to it by now. |
| 37 | Mrs Wong | Mandy | — | MISSING in p_3 | Okay, enough about me. |
| 38 | Mrs Wong | Mandy | — | MISSING in p_3 | I haven’t seen your boyfriend in a while, how are you guy... |
| 39 | Thoughts | Thoughts | — | MISSING in p_3 | A week ago, he proposed to me, and I said yes. |
| 40 | Thoughts | Thoughts | MC | MC (thought?) | That was the happiest day of my life. |
| 41 | Thoughts | Thoughts | Mandy | WRONG AUTHOR | Why does this terrible thing need to happen to us... |
| 42 | You | MC | Mandy | WRONG AUTHOR | Jason just proposed to me last week... |
| 43 | Mrs Wong | Mandy | MC | WRONG AUTHOR | Woah, congratulations, Miss Lee! Wait, did you say yes? |
| 44 | Mrs Wong | Mandy | Mandy | OK | You seem more concerned than happy. |
| 45 | You | MC | — | MISSING in p_3 | Jason is quite busy these days with his job. We are doing... |
| 46 | Mrs Wong | Mandy | MC | WRONG AUTHOR | I see. |
| 47 | Thoughts | Thoughts | MC | MC (thought?) | If this hadn’t happened, I’d probably be in Jason’s arms ... |
| 48 | You | MC | Mandy | WRONG AUTHOR | Of course, I’ve been waiting for his proposal for months! |
| 49 | You | MC | MC | OK | I said yes. Sorry, just too many things happened... |
| 50 | Mrs Wong | Mandy | Mandy | OK | I see. You’ll look so beautiful in your wedding dress. |
| 51 | You | MC | Mandy | WRONG AUTHOR | Haha, you’re too kind. |
| 52 | Mrs Wong | Mandy | — | MISSING in p_3 | How wonderful. |
| 53 | You | MC | MC | OK | ... |
| 54 | Mrs Wong | Mandy | Mandy | OK | ... |
| 55 | Mrs Wong | Mandy | MC | WRONG AUTHOR | Give me your clothes and I’ll toss them in for you. |
| 56 | Thoughts | Thoughts | Mandy | WRONG AUTHOR | No, she can’t touch the clothes, it has blood all over... |
| 57 | You | MC | MC | OK | I’ll do it myself—no need to trouble you. You must be tired. |
| 58 | Mrs Wong | Mandy | MC | WRONG AUTHOR | Okay. Let me at least help you separate the colors from t... |
| 59 | You | MC | MC | OK | No, thanks—I’ll just tuck everything in one big load. |
| 60 | Mrs Wong | Mandy | Mandy | OK | That would ruin your clothes, Miss Lee. |
| 61 | You | MC | MC | OK | It will be fine. |
| 62 | You | MC | Mandy | WRONG AUTHOR | Nah, I don’t mind. |
| 63 | You | MC | MC | OK | I’m short on cash, so I’ll just wash one load. |
| 64 | Mrs Wong | Mandy | MC | WRONG AUTHOR | Sure. That comes to 80 cents in total. |
| 65 | You | MC | Mandy | WRONG AUTHOR | Here. |
| 66 | Mrs Wong | Mandy | — | MISSING in p_3 | Here you go. Machine Nr. 4. It’s the one on your left. |
| 67 | You | MC | — | MISSING in p_3 | Thank you, Mrs Wong. |
| 68 | You | MC | — | MISSING in p_3 | ... |
| 69 | You | MC | — | MISSING in p_3 | Which washer was it again? |
| 70 | Mrs Wong | Mandy | — | MISSING in p_3 | Machine Nr. 4. It’s the one on your left. |
| 71 | You | MC | — | MISSING in p_3 | Nothing. I got this. |

**Score if treating p_3 as Phase 2:** OK-ish 41, wrong author 15, missing 15 (of 71 lines).

### Suggested fix for Phase 2 / 3

1. Move correctly authored p_3 wavs that belong to Mandy scene 1 → p_2 (same l_N).
2. Fix wrong-author pairs (swap MC ↔ Mandy folders) where Verdict is WRONG AUTHOR.
3. Record or remove still-MISSING lines.
4. Clear bogus p_3 Mandy files; record real Lau-phase VO under p_3 if needed (speakers: Drunk Man / Drunk Cop / You).

---

## Suggested rename / fix actions (quick)

| Case | Action |
|------|--------|
| Phase 2 / 3 | Re-home Mandy scene 1 VO from p_3 → p_2; fix author swaps; fill gaps |
| Phase 6 | Add MC l_29,30,32, Mandy l_31 |
| Phase 8 | Decide if remaining Thoughts l_5–15 need VO |
| Phase 13 | Remove or justify extra MC l_8 |
| Phase 15 | None — OK |
| Phase 18 | Re-index / re-assign swapped MC↔Mandy lines |
| Phase 19 | Re-check Lau dialogue line list vs MC VO |
| Phase 20 | None — OK |
| Phase 21 | Fix swapped l_3/l_4; remove orphan Mandy l_5 |
| Phase 26 | None — OK |
| Phase 27 | Add MC l_6, Mandy l_10 |
| Phase 28 | None — OK (Thoughts intentionally unvoiced) |
| Phase 30 | Add MC l_14 |
| Phase 31 | Add MC l_4, l_8 |
