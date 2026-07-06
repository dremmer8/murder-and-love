VAR suspicion = 0       // =Sus level
VAR Mandy_affection = 0 // Affection value
VAR Fear = 0            // Fear value

// =============================================================================
//  PHASE 1 Getting in the laudromat
// =============================================================================

// After the cutscene, player walks toward Mandy and this conversation will be automatically triggered.
-> Mandy_story_phase_1
== Mandy_story_phase_1 ==
Mandy: Hey, isn't that Miss Lei! Doing laundry at this hour?

- (You_intro_choice)
* [1. Cant't sleep] 
    You: Uh, yeah. I just couldn't sleep.
    -> chose_sleepless
* [2. ... (Stay silent)] 
    You: ...
    ~ suspicion = suspicion + 1
    -> chose_silence
* [3. Continue] 
    You: Mrs. Wong, I have some clothes I urgently need for tomorrow. 
    ~ suspicion = suspicion + 1
    -> chose_lie

== chose_sleepless ==
Mandy: I’m the opposite, I can sleep right away. But my man is at his friend’s place playing mahjong, and Tian is sick, so I'm stuck covering the night shift at 3am...
-> Mandy_choice_1

== chose_silence ==
Mandy: Why aren't you saying anything? Is everything alright?
-> Mandy_choice_1_1

== chose_lie ==
Mandy: I see... Clothes can get dirty pretty fast. *Sigh*, I want to go home and sleep too, but my man is playing mahjong, and little Tien is sick, so I'm stuck covering the night shift...
-> Mandy_choice_1

== Mandy_choice_1_1 ==
- (You_choice_1_1)
* [1. Nothing.] 
    You: It's nothing.
    -> continue1
* [2. Tired.] 
    You: I'm just too tired. 
    -> continue1

== continue1 ==
Mandy: I see... *Sigh*, I want to go home and sleep too, but my man is playing mahjong, so I'm stuck covering the night shift...
-> Mandy_choice_1


== Mandy_choice_1 ==
// Camera zooms in: You notice a bruise on her arm.
- (mandy_loop_1)
+ {not chose_mahjong} [1. Ask about his husband] You: Mr. Wong is playing mahjong again? -> chose_mahjong
* [2. Sounds rough] You: I'm sorry, that sounds rough. -> chose_rough

== chose_mahjong ==
Mandy: Yeah. He had a few drinks tonight. Whenever he drinks, he goes to play mahjong.
    * [1. (Return to previous topic)] -> Mandy_choice_1

== chose_rough ==
Mandy: It's just how it is.
-> Mandy_conversation_beat2


== Mandy_conversation_beat2 ==
You: I want to wash these clothes.
Mandy: Just give the clothes to me, I'll wash them for you.

- (laundry_delivery_choice)
* [1. Being polite.] You: I'll do it myself—no need to trouble you. You must be tired.  -> refuse_help
* [2. Continue.] You: I want to wash them myself today.
    ~ suspicion = suspicion + 1
    -> refuse_help

== refuse_help ==
Mandy: Really? That works, too. Let's separate the colored and white clothes like usual?

* [(Hurriedly pull it back)]
    You: No, thanks—I’ll just tuck everything in together.
    ~ suspicion = suspicion + 1
    Mandy: Alright. That will be 13.80 in total.
    -> finish_phase_1


== finish_phase_1 ==
You: Okay.
// Player pays the money, Mandy gives the laundry token.
Mandy: You can use Machine 4. The laundry detergent is on the table.
//(You take the laundry token from Mandy's hand.)
-> END

// =============================================================================
//  PHASE 2 Asking about detergent
// =============================================================================

// Will be triggered if the player interacts with the detergent already

== Mandy_story_phase_2 ==
(You realize the heavy-duty laundry detergent is empty. You go to ask Mrs. Wong.)

You: Mrs. Wong, the heavy-duty laundry detergent over there is empty.
Mandy: Is that so? Is the regular detergent not enough?
You: Not really.

- (Mandy_choice_2)
* [1. Cat peed your bed (Lie)] You: Our cat peed on my boyfriend's clothes. Only heavy-duty detergent can get it out. 
    // If asked about cat
    Mandy: Your cat? I thought your boyfriend didn't allow you to keep a cat?
    -> cat_secondary_questions
* [2. Just want to.] You: I just want to get them a bit cleaner.
    ~ suspicion = suspicion + 1 
    -> continue_detergent_4 
    
== cat_secondary_questions ==
* [1. Convinced boyfriend. (Lie)] You: I convinced him because the kitty is so cute. It's currently in heat. (Lie)
    -> continue_detergent_2
* [2. Neighbor's cat.] You: Ah, I misspoke. It's the neighbor's cat. (Lie)
    ~ suspicion = suspicion + 1  
    -> continue_detergent_3
        
== continue_detergent_2 ==
Mandy: Alright then. I never expected someone like him to actually compromise. Unbelievable.
You: He has his gentle moments, too...
-> continue_detergent_4

== continue_detergent_3 ==
Mandy: It sneaked all the way into your home? What a wild cat.
-> continue_detergent_4

== continue_detergent_4 ==
Mandy: I see.

Mandy: The heavy-duty enzyme detergent is in the backroom. (Mandy picks up a ring of keys) —

* [Get it for her]
    You: Let me get it for you! Just sit down and rest.
    Mandy: Alright, that works.
    ~ Mandy_affection = Mandy_affection + 1
* [(Missed QTE)] Mandy: If you don't mind, go ahead and grab it for me.

- Mandy: It's on the shelf, the blue one. It should be labeled "Heavy Duty Enzyme Detergent".
You: Got it.
-> END
