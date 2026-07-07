VAR story_phase = 1
VAR Mandy_affection = 0 // Affection value

// =============================================================================
//  PHASE 1 Getting in the laudromat
// =============================================================================

{story_phase:
    - 1: 
        ->Mandy_story_phase_1
    - 2:   
        ->Mandy_story_phase_2
}

== Mandy_story_phase_1 ==

// If the player goes to Mandy again after talked to her once, before the second phase condition was met (checks detergent)
{ Mandy_story_phase_1 > 1: 
    Mandy: All good?
    + [Nothing.]
        You: Nothing.
        -> END
    + [Ask which machine]
        You: Sorry, which washing machine should I go?
        Mandy: Washing machine Nr. 4.
        -> END
}
// After the intro cutscene, player walks toward Mandy and this conversation will be automatically triggered. 
# camera: intro
# animate_mandy: M_stand_idle_greeting_1
Mandy: Hey!
# vo: mandy_P1_1_1
Mandy: isn't that Miss Lee! Doing laundry at this hour?
# vo: mandy_P1_1_1.2

- (You_intro_choice)
* [Cant't sleep] 
    You: Uh, yeah. I just couldn't sleep.
    # vo: VI_1_sleepless
    -> chose_sleepless
* [... (Stay silent)] 
    You: ...
    -> chose_silence
* [Continue] 
    You: Mrs. Wong, I have some clothes I urgently need for tomorrow. 
      # vo: VI_1_urgent
    -> chose_urgent

== chose_sleepless ==
Mandy: Poor girl.
# animate_mandy: M_stand_talk_agree_1
    ~ Mandy_affection = Mandy_affection + 1
    -> chose_urgent
== chose_silence ==
Mandy: ...
-> chose_urgent

== chose_urgent ==
Mandy: Give me your clothes and I'll wash them for you. Here is your coin.
# animate_mandy: M_stand_give_item_1
-> END

// =============================================================================
//  PHASE 2 Asking about detergent
// =============================================================================

// Will be triggered if the player interacts with the detergent already, and player needs to press E to talk to Mandy

== Mandy_story_phase_2 ==
(You realize the heavy-duty laundry detergent is empty. You go to ask Mrs. Wong.)

You: Mrs. Wong, the heavy-duty laundry detergent over there is empty.
Mandy: Is that so? Is the regular detergent not enough?
    ~ Mandy_affection = Mandy_affection + 1
You: Not really.

- (Mandy_choice_2)
* [1. Cat peed your bed (Lie)] You: Our cat peed on my boyfriend's clothes. Only heavy-duty detergent can get it out. 
    Mandy: Your cat? I thought your boyfriend didn't allow you to keep a cat?
    -> END
* [2. Just want to.] You: I just want to get them a bit cleaner.
    -> END
