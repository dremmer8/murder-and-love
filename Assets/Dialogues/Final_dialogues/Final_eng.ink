// Ending cutscenes (GameManager): 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding
EXTERNAL PlayEndingCutscene(cinematicIndex)

// Unhide a scene item mid-dialogue (DialogueItemUnhide). Ids: first_laundry_coin, backroom_key, second_laundry_coin, police_lights.
// Coin/key also flash Mandy hand props (Token_1Prop / KeyProp / Token_2Prop) for the give-item anim duration.
EXTERNAL UnhideItem(itemId)

// Give away a basket item mid-dialogue (DialogueItemGiveAway → BasketCollector.GiveBack).
// Change pay: change_coin_1 .. change_coin_4
EXTERNAL GiveAwayItem(itemId)

// Swap baked lighting scenarios (BakedLightingController). 1 = blackout, 0 = lights on.
EXTERNAL SetBlackout(blackout)

// Animator triggers (DialogueAnimationTargets): Mandy doRelax/doIdle/doTalk/doGiveItem; Lau doSitDrink/doPager/doStandLoop.
EXTERNAL TriggerAnimation(targetId, animationName)

// Dialogue cutscene camera (CutsceneDialogueCameraManager). Holds 10–25s then returns to player.
EXTERNAL ChangeCamera(cameraId)

// Pan player FPV toward a look target (Lau1, Lau2, Mandy1, Mandy2) and restore afterward.
EXTERNAL LookAtTarget(targetId, duration)
EXTERNAL RestoreLook(duration)

// Play a SoundLibrary / FMOD one-shot by key (InkStoryCommands → SoundManager).
EXTERNAL PlayAudioClip(soundKey)

//story phase number
VAR story_phase = 1


// Unity (GlobalVariableOperator) syncs and stores this across dialogues.
VAR game_progression = 0

//story variables
VAR mahjong_mentioned = false
VAR lie_about_period = false
VAR proposal_admit = false
VAR boyfriend_needs_clothes = false
VAR cant_sleep = false
VAR kitchen_knife = false
VAR has_detergent = false
VAR lied_about_cat = false
VAR lau_cat_pee = false
VAR black_out_happened = false

VAR did_insult = false
VAR told_lie_sick = false
VAR told_lie_busy = false

VAR lied_about_wine = false
VAR lied_about_hand = false
VAR Cop_knows_period = false
VAR coin_machine_attempt = 0
VAR knows_backroom = false

{story_phase:
    - 1: -> intro
    
    - 2: ->Mandy_story_phase_1
    
    - 3:-> LAU_story_phase_1
        
    - 4:  -> Thought_about_not_leaving_clothes
    
    - 5:-> Thought_about_empty_detergent
    
    - 6:-> Mandy_story_phase_2
    
    - 7: -> LAU_story_phase_2
    
    - 8:-> Inner_voice_backroom_phase_1
    
    - 9: -> Boyfriend_pager_phase_1
    
    - 10:-> Boyfriend_pager_phase_1
    
    - 11: -> Thought_about_how_detergent_looks
    
    - 12:-> Thought_about_got_right_detergent
    
    - 13: -> Thought_washing_clothes_1
    
    - 14: -> Thought_about_need_another_washer
    
    - 15: -> Mandy_story_phase_3
    
    - 16: ->Interaction_with_coin_machine
    
    - 17: ->Boyfriend_pager_phase_2
    
    - 18: ->Mandy_story_phase_4
    
    - 19: ->LAU_story_phase_3
    
    - 20: ->Thought_washing_clothes_2
    
    - 21: -> Chaos_blackout
    
    - 22: ->Inner_voice_phase_2
    
    - 23: ->Boyfriend_pager_phase_3
    
    - 24: ->How_to_turn_on_circuit_box
    
    - 25: ->Attempt_leaving_backroom
    
    - 26: ->Mandy_smoking_scene_1
    
    - 27: -> Mandy_smoking_scene_2
    
    - 28: ->Mandy_smoking_scene_3
    
    - 29: ->Boyfriend_pager_ending
    
    - 30: ->Lau_confess_ending
    - 31: ->Boyfriend_ending_dialogue_final

}


// =============================================================================
//  PHASE 1 intro on black screen
// =============================================================================
== intro ==
~ game_progression = 1
Lam Tong City. Another damp midnight. After an exhausting day, we were getting ready for bed.
-> intro_intruder

= intro_intruder
But that peace was suddenly shattered by a <>
~ ChangeCamera("Player")
* loan shark
* gangster
* robber
-<>.

// change page
He tore through our home, destroyed what we’d built, searching for anything of value. My boyfriend said there was only one way out. He turned to me and asked for the 
* kitchen knife.
* box cutter.
-

// change page
Silence returned to the night. Blood seeped deep into the carpet. One issue resolved. An even worse one arose. Covered in blood was
~ ChangeCamera("Player")
* his shirt.
* our bed sheet.
* my favorite dress.
-

// change page
He held me with his bloodied hands, telling me that as long as the blood was washed off our clothes, everything would be all right.
And so here I am, at 3 a.m., trying to wash away the crime we committed.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 2 Trigger zone in front of drunk man (only once triggered)
// =============================================================================

== LAU_story_phase_1 ==
~ game_progression = 3
{ LAU_story_phase_1 > 1: -> repeat_visit }

// After talking to Mandy, then walking past Lau for the first time.
Drunk Man: A pretty lady at this hour? Are you looking for me? # vo:p_2_l_1
- (Lau_choice_1)
    ~ ChangeCamera("Player")
    * [Greeting] 
        You: Good Evening. # vo:p_2_l_2
    * [Continue] 
        You: I’m just here for the laundry. # vo:p_2_l_3
    * [Insult] 
        You: Fuck off. # vo:p_2_l_4
        ~ did_insult = true
        Drunk Man: Woah, take it easy, young lady. I was just joking around. # vo:p_2_l_5
       -
       -> flattery -> clothes_question

= flattery
Drunk Man: You’re lucky to have me here, you know. # vo:p_2_l_6
Drunk Man: No one would dare to harass a beautiful young lady like yourself in front of a police officer!! # vo:p_2_l_7
Drunk Cop: No villains can slip through my fingers. # vo:p_2_l_8
~did_insult = true
~ ChangeCamera("l3")
Thoughts: Shit. I thought he’s just an average drunk... # vo:p_2_l_9
~ ChangeCamera("Player")
* [...] 
You: ... # vo:p_2_l_10
Drunk Cop: ... # vo:p_2_l_11 -> clothes_question
* [You don’t look like a cop]
You: Sorry, but you don’t look like a police officer. # vo:p_2_l_12
Drunk Cop: What, are cops not allowed to do their laundry at night, # vo:p_2_l_13
Drunk Cop: after investigating a bloody crime scene? # vo:p_2_l_14
** [Nevermind]
You: Never mind what I said. # vo:p_2_l_15
-> clothes_question

= clothes_question
{ boyfriend_needs_clothes:
    Drunk Cop: So? Your boyfriend wants you to wash clothes in the middle of the night? # vo:p_2_l_16
- else:
    Drunk Cop: Why are you washing clothes in the middle of the night? # vo:p_2_l_17
}

-(questions_clothes)
* {not boyfriend_needs_clothes} [Can’t sleep] 
You: I can’t sleep. # vo:p_2_l_18
Drunk Cop: But why would you come to a laundromat at 3am? # vo:p_2_l_19
Drunk Cop: You’re young and beautiful, and you have a partner, # vo:p_2_l_20
Drunk Cop: which is the complete opposite of me. # vo:p_2_l_21
Drunk Cop: You’re not here to wash the smell of your ex-wife from your clothes, like I am, haha. # vo:p_2_l_22
        * * [Sorry to hear that] 
            You: I’m sorry. # vo:p_2_l_23
            Drunk Cop: I just can’t sleep being reminded of her. # vo:p_2_l_24
            -> need_to_answer
        * * [...] 
            -> need_to_answer
* [Lie] 
        You: My boyfriend needs these clothes tomorrow for work, Officer. # vo:p_2_l_25
        -> boyfriend_excuse
        
* { not boyfriend_needs_clothes} [For my boyfriend]
You: I need to wash some clothes for my boyfriend, Officer. # vo:p_2_l_26
-> boyfriend_excuse

* [None of your business.] 
        You: It’s none of your business. # vo:p_2_l_27
        { did_insult:
        Drunk Cop: Ooh, pretty lady has some secrets. # vo:p_2_l_28
-> need_to_answer
        - else:
        Drunk Man: Woah, take it easy, young lady. # vo:p_2_l_29
        -> flattery
        }

= need_to_answer
{ need_to_answer > 1:
    Drunk Cop: But don’t throw curveballs. I’m familiar with that. # vo:p_2_l_30
    Drunk Cop: Just answer the question. # vo:p_2_l_31
- else:
    Drunk Cop: But you still need to answer my question. # vo:p_2_l_32
}
Drunk Cop: Why are you washing clothes in the middle of the night? # vo:p_2_l_33
-> questions_clothes

= boyfriend_excuse
Drunk Cop: But your boyfriend couldn’t be bothered to accompany you at this hour? # vo:p_2_l_34
    ~ ChangeCamera("Player")
    * [He’s sick (Lie)] 
        You: He’s sick and he needs his clothes for work tomorrow. # vo:p_2_l_35
        ~ told_lie_sick = true
        Drunk Cop: Sick but still goes to work? Huh, that’s how I lost my wife. # vo:p_2_l_36
    * [He’s busy tonight and I have nothing to do.] 
        You: He’s busy tonight, and since I have nothing to do at home anyway, # vo:p_2_l_37
        You: I might as well help him with some chores. # vo:p_2_l_38
        ~ told_lie_busy = true
        Drunk Cop: Busy at this hour, huh? # vo:p_2_l_39
        Drunk Cop: Are you sure he’s not up to no good? # vo:p_2_l_40
- 
~ ChangeCamera("l2")
* [Can’t stop him from work.]
You: ...He’s a hardworking guy. # vo:p_2_l_41
* [Excuse]
You: ...He’s a hardworking guy. # vo:p_2_l_42
-
~ ChangeCamera("Player")
Drunk Cop: Fair. Money is important. # vo:p_2_l_43
Drunk Cop: Your boyfriend is lucky to have someone like you. # vo:p_2_l_44
-> ending

= ending
Drunk Cop: Go on and wash your clothes then, young lady. # vo:p_2_l_45
~ ChangeCamera("Player")
-> END

= repeat_visit
~ game_progression = 3
Drunk Cop: All good? # vo:p_2_l_46
    ~ ChangeCamera("Player")
    + [Nothing.]
        You: .. # vo:p_2_l_47
    + [Ask which machine]
        You: Which machine did I put my clothes in again? # vo:p_2_l_48
        Drunk Cop: Hmmm... Number four? # vo:p_2_l_49
    + [Ask about where to find detergent]
        You: Where can I find detergent? # vo:p_2_l_50
        Drunk Cop: It’s on the table behind me. # vo:p_2_l_51
-
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 2 getting into the laundromat
// =============================================================================
== Mandy_story_phase_1 ==
~ game_progression = 2
{ Mandy_story_phase_1 > 1: -> Mandy_phase_1_repeat }
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
Mrs Wong: Hey! # vo:p_3_l_1
Mrs Wong: Isn’t that Miss Lee! Doing laundry at this hour? # vo:p_3_l_2

- (You_intro_choice)
* [Can’t sleep] 
    You: Uh, yeah. I just couldn’t sleep. # vo:p_3_l_3
    ~ cant_sleep = true
    Mrs Wong: Poor girl. # vo:p_3_l_4
* [... (Stay silent)] 
    You: ... # vo:p_3_l_5
    Mrs Wong: Why aren’t you saying anything? # vo:p_3_l_6
* [I really need to wash these.] 
    You: I have some clothes I really need for tomorrow. # vo:p_3_l_7
    Mrs Wong: What’s the hurry? # vo:p_3_l_8
- 
-> MrsWong_ask_alright

  = MrsWong_ask_alright 
Mrs Wong: Is everything alright? # vo:p_3_l_9
~ ChangeCamera("m4")
Thoughts: Mrs Wong is always so kind to me... I used to tell her so many things. # vo:p_3_l_10
Thoughts: But now I have blood on my hands... # vo:p_3_l_11
Thoughts: I can only pretend that everything is normal. # vo:p_3_l_12
~ ChangeCamera("Player")
* [Say you’re fine. (Lie)]
You: Yeah, I’m fine. And you? # vo:p_3_l_13
Mrs Wong: Oh, I’m really tired. # vo:p_3_l_14
* {not cant_sleep} [Just tired.]
You: Sorry, just had a long day. # vo:p_3_l_15
Mrs Wong: Same. # vo:p_3_l_16
* {cant_sleep} [Make an excuse why you can’t sleep]
You: It’s just too hot to fall asleep in this weather. # vo:p_3_l_17
Mrs Wong: Fair. I’m the opposite. # vo:p_3_l_18
* [Need to wash clothes for Jason]
You: Just need to wash these for my boyfriend. # vo:p_3_l_19
~ boyfriend_needs_clothes = true
Mrs Wong: What’s the hurry? It’s 3 am. # vo:p_3_l_20 -> explain_hurry
-
Mrs Wong: I’d fall asleep as soon as my head hit the pillow, if I don’t need to be here. # vo:p_3_l_21
-> Vi_ask_about_MrsWong_phase_1

=explain_hurry
~ ChangeCamera("Player")
* [Lie]
You: My boyfriend needs to wear these tomorrow at work. # vo:p_3_l_22
* [He just asked me to.]
You: Well, my boyfriend just told me to wash them because he needs them. # vo:p_3_l_23
-
Mrs Wong: I see. # vo:p_3_l_24
Mrs Wong: And my husband is playing Mahjong somewhere again, leaving me to run this place overnight. # vo:p_3_l_25
~ mahjong_mentioned = true
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: It’s a wonder what we women put up with. # vo:p_3_l_26
-> Vivian_question_loop

= Vi_ask_about_MrsWong_phase_1
- (Vivian_question_loop)

~ ChangeCamera("Player")
* {mahjong_mentioned == false} [Is everything alright?]
    You: Is everything alright? # vo:p_3_l_27
    Mrs Wong: You could say so. # vo:p_3_l_28
    Mrs Wong: My husband is off playing Mahjong again, someone has to run this place. # vo:p_3_l_29
    -> Vivian_question_loop

* [Couldn’t your son help?]
You: Couldn’t your son help? # vo:p_3_l_30
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: He’s sick. # vo:p_3_l_31
Mrs Wong: I asked him to stay home and rest. # vo:p_3_l_32
   -> Vi_ask_about_MrsWong_phase_1 
+ [Sounds rough. (Continue)] -> That_sounds_rough
=That_sounds_rough
    You: I’m sorry, Mrs Wong. That sounds rough. # vo:p_3_l_33
    Mrs Wong: I’m used to it by now. # vo:p_3_l_34
    -> MrsWong_phase_1_proposol

= MrsWong_phase_1_proposol
Mrs Wong: Okay, enough about me. # vo:p_3_l_35
Mrs Wong: I haven’t seen your boyfriend in a while, how are you guys doing? # vo:p_3_l_36
~ ChangeCamera("b4")
Thoughts: A week ago, he proposed to me, and I said yes. # vo:p_3_l_37
Thoughts: That was the happiest day of my life. # vo:p_3_l_38
Thoughts: Why does this terrible thing need to happen to us... # vo:p_3_l_39
~ ChangeCamera("Player")
* [Tell her about the proposal]
~ proposal_admit = true
You: Jason just proposed to me last week... # vo:p_3_l_40
Mrs Wong: Woah, congratulations, Miss Lee! Wait, did you say yes? # vo:p_3_l_41
Mrs Wong: You seem more concerned than happy. # vo:p_3_l_42
-> proposal_admitted
* [Don’t bring it up]
You: Jason is quite busy these days with his job. We are doing quite fine. # vo:p_3_l_43
Mrs Wong: I see. # vo:p_3_l_44
-> MrsWong_phase_1_laundry_coin

= proposal_admitted
~ ChangeCamera("Player")
Thoughts: If this hadn’t happened, I’d probably be in Jason’s arms right now, dreaming about our wedding... # vo:p_3_l_45
* [Force a smile and say you’re happy]
You: Of course, I’ve been waiting for his proposal for months! # vo:p_3_l_46
* [Excuse of looking concerned]
You: I said yes. Sorry, just too many things happened... # vo:p_3_l_47
-
Mrs Wong: I understand. You’ll look so beautiful in your wedding dress. # vo:p_3_l_48
~ ChangeCamera("Player")
** [Thank her]
You: Haha, you’re too kind. # vo:p_3_l_49
Mrs Wong: How wonderful. # vo:p_3_l_50
--
-> MrsWong_phase_1_laundry_coin

= MrsWong_phase_1_laundry_coin
~ ChangeCamera("Player")
Mrs Wong: Give me your clothes and I’ll toss them in for you. # vo:p_3_l_51
~ ChangeCamera("m5")
Thoughts: No, she can’t touch the clothes, it has blood all over... # vo:p_3_l_52
~ ChangeCamera("Player")
* [I will do it myself.]
You: I’ll do it myself—no need to trouble you. You must be tired. # vo:p_3_l_53
Mrs Wong: Okay. Let me at least help you separate the colors from the whites— # vo:p_3_l_54

- (laundry_delivery_choice)
~ ChangeCamera("Player")
* [Refuse] 
You: No, thanks—I’ll just tuck everything in one big load. # vo:p_3_l_55
Mrs Wong: That would ruin your clothes, Miss Lee. # vo:p_3_l_56
  ** [It will be fine.]
  You: It will be fine. # vo:p_3_l_57
  ** [I don’t mind.]
  You: Nah, I don’t mind. # vo:p_3_l_58
* [Make an excuse to stop her]
You: I’m short on cash, so I’ll just wash one load. # vo:p_3_l_59
-
    -> give_money

= give_money
Mrs Wong: Sure. That comes to 80 cents in total. # vo:p_3_l_60
~ ChangeCamera("Player")
* [Give the money]
You: Here. # vo:p_3_l_61
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("first_laundry_coin")
Mrs Wong: Here you go. Machine Nr. 4. It’s the one on your left. # vo:p_3_l_62
 * * [Thank you.]
 You: Thank you, Mrs Wong. # vo:p_3_l_63
 * * [...]
 You: ... # vo:p_3_l_67
-
    ~ ChangeCamera("Player")
    -> END
    
= Mandy_phase_1_repeat
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [Ask which washer]
You: Which washer was it again? # vo:p_3_l_65
Mrs Wong: Machine Nr. 4. It’s the one on your left. # vo:p_3_l_66
~ ChangeCamera("Player")
-> END
+ [Nothing]
You: Nothing. I got this. # vo:p_3_l_67
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 3 Asking LAU about detergent
// =============================================================================

// Will be triggered if the player is asking for the detergent



// =============================================================================
//  PHASE 4 after interacting with the washer nr. 4
// =============================================================================

== Thought_about_not_leaving_clothes ==
~ game_progression = 4
~ ChangeCamera("b2")
Thoughts: This is the correct washer. # vo:p_4_l_1
Thoughts: I saw some detergent on the table behind the chairs. # vo:p_4_l_2
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 5 after interacting with the detergent
// =============================================================================
== Thought_about_empty_detergent ==
~ game_progression = 5
~ ChangeCamera("b1")
Thoughts: Shit, they’re out of heavy-duty detergent. # vo:p_5_l_1
Thoughts: I need it to get the bloodstains out... # vo:p_5_l_2
Thoughts: I have to ask around to get some. # vo:p_5_l_3
~ ChangeCamera("Player")
-> END


// =============================================================================
//  PHASE 6 Press E in front of Mandy, after detergent check happened 
// =============================================================================

== Mandy_story_phase_2 ==
~ game_progression = 6
{ knows_backroom:-> ask_mandy_questions}
~ TriggerAnimation("Mandy", "doRelax")

~ ChangeCamera("Player")

* [Ask about detergent]
You: Mrs Wong, there’s no heavy-duty laundry detergent left. # vo:p_6_l_1
Mrs Wong: Is that so? I remember I put a lot of regular detergent there, isn’t that strong enough? # vo:p_6_l_2
~ ChangeCamera("m4")
Thoughts: I don’t want to lie to Mrs Wong, but how can I explain... # vo:p_6_l_3
  ~ ChangeCamera("Player")
  ** [Cat peed on the clothes. (Lie)]
  ~ lied_about_cat = true
  You: It’s a bit awkward situation. My cat peed on the sheets. # vo:p_6_l_4
  You: We have to wash these sheets with heavy-duty laundry detergent. # vo:p_6_l_5
  Mrs Wong: Your cat? I thought your boyfriend didn’t allow you to keep a cat? # vo:p_6_l_6
  -> cat_secondary_questions
  ** [I got my period. (Lie)]
  ~ lie_about_period = true
  You: You know? I’m in the time of the month. I have to wash these sheets... # vo:p_6_l_7
  You: Only heavy-duty detergent can get it out. # vo:p_6_l_8
  ~ TriggerAnimation("Mandy", "doTalk")
  Mrs Wong: Oh, I understand. It’s awful that we women have to go through this every month. # vo:p_6_l_9
 -> get_detergent_in_backroom
  ** [Dodge the question] 
         You: I think heavy-duty detergent gets clothes cleaner. # vo:p_6_l_10
         Mrs Wong: Okay. # vo:p_6_l_11
-> get_detergent_in_backroom

    
= cat_secondary_questions
~ ChangeCamera("b4")
Thoughts: Oh God, I completely forgot I’d ever said that to her... # vo:p_6_l_12
Thoughts: A few months ago, Jason got mad at me because I suggested we get a kitten. # vo:p_6_l_13
Thoughts: He said it was a waste of money. # vo:p_6_l_14
Thoughts: How can I cover up my lie now... # vo:p_6_l_15
~ ChangeCamera("Player")
* [Convinced boyfriend. (Lie)] 
You: I convinced him because the kitty is so cute. # vo:p_6_l_16
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: Alright then. I didn’t expect someone as stubborn as your boyfriend to actually give in. # vo:p_6_l_17
 ~ ChangeCamera("Player")
 ** [Defend Jason]
You: He is actually very gentle to me. He just doesn’t like cats that much. # vo:p_6_l_18
Mrs Wong: If you say so. # vo:p_6_l_19
* [I missspoke. (Lie)]
You: Ah, I misspoke. It’s the neighbor’s cat. # vo:p_6_l_20
Mrs Wong: It sneaked all the way into your room? What a wild cat. # vo:p_6_l_21
You: Yeah, pretty wild. # vo:p_6_l_22
-
-> get_detergent_in_backroom

= get_detergent_in_backroom
~ ChangeCamera("b5")
Mrs Wong: The heavy-duty detergents are in the backroom. # vo:p_6_l_23
Mrs Wong: I’m too tired to move... Can you get it yourself? # vo:p_6_l_24
~ ChangeCamera("Player")
* [Sure.]
You: Yes sure, no worries. # vo:p_6_l_25
Mrs Wong: Thank you. I will rest here then. # vo:p_6_l_26
- 
-> Ending_mandy_story_phase_2

= Ending_mandy_story_phase_2
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("backroom_key")
Mrs Wong: Here is the key to the backroom. It’s near Washer Nr. 9. # vo:p_6_l_27
Mrs Wong: The detergent you want is called Enzyme Laundry Detergent, the blue one on the shelf. # vo:p_6_l_28
You: Thank you, Mrs Wong. # vo:p_6_l_29
~ knows_backroom = true

~ ChangeCamera("Player")
-> END

= ask_mandy_questions
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [Ask where the backroom is]
You: Where is the backroom? # vo:p_6_l_30
Mrs Wong: The backroom is at the corner, near the washer Nr. 9. # vo:p_6_l_31 -> ask_mandy_questions
+ [Ask how the heavy-duty detergent looks like]
You: Which one is the heavy-duty detergent again? # vo:p_6_l_32
Mrs Wong: It’s called Enzyme Laundry Detergent, the blue one on the shelf. # vo:p_6_l_33
-> ask_mandy_questions
+ [Nothing.] 
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 7 Asking LAU about detergent
// =============================================================================

// Press E in front of Lau, after detergent check happened 

== LAU_story_phase_2 ==
~ game_progression = 7
{LAU_story_phase_2 == 1: -> first_time | -> repeat_visit }

    = repeat_visit
    {has_detergent:
        Drunk Cop: Can’t get enough of me, huh? # vo:p_7_l_1
    - else:
    Drunk Cop: I told you you can get detergent at the desk # vo:p_7_l_2
    }
    ~ ChangeCamera("Player")
    -> END

    = first_time
Drunk Cop: What does a beauty like you want with me? # vo:p_7_l_3
        ~ ChangeCamera("Player")
        * [Just detergent.]
        You: Nothing. Just some heavy-duty detergents. # vo:p_7_l_4
        * [Asking politely for detergent]
        You: Sorry to bother you. Do you have some heavy-duty detergent by chance? # vo:p_7_l_5
        * [End the conversation] 
        You: Nothing. # vo:p_7_l_6
            ~ ChangeCamera("Player")
            -> END
    
    - 
    ~ ChangeCamera("l2")
    Drunk Cop: Hm, what could I get in return? # vo:p_7_l_7
    Drunk Cop: ...How about a little kiss? # vo:p_7_l_8
    ~ ChangeCamera("Player")
    -> LAU_story_phase_2_continue_1


    = LAU_story_phase_2_continue_1
        ~ ChangeCamera("Player")
        * [No.] 
        You: No. Do you have the detergent or not? # vo:p_7_l_9
        Drunk Cop: Why do you need heavy-duty detergent anyways? # vo:p_7_l_10
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [I have a boyfriend.] 
        You: ...I have a boyfriend. # vo:p_7_l_11
            Drunk Cop: Wow, someone is deeply in love. # vo:p_7_l_12
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [Disgust] 
        You: Ew, shouldn’t you be with your wife and kids? # vo:p_7_l_13
            Drunk Cop: Ha, good question. They don’t talk to me anymore. # vo:p_7_l_14
            * * [Being sarcastic] 
            You: I can see why. # vo:p_7_l_15
                Drunk Cop: Woah, that was harsh. # vo:p_7_l_16
            * * [Why?] 
            You: Why? # vo:p_7_l_17
                Drunk Cop: I don’t know... # vo:p_7_l_18
            * * [Apologize] 
            You: I’m sorry, I didn’t mean to... # vo:p_7_l_19
                Drunk Cop: It’s okay. # vo:p_7_l_20
            - - 
            ~ ChangeCamera("l4")
            Drunk Cop: I didn’t even see it coming. # vo:p_7_l_21
            Drunk Cop: I worked so hard day and night for her and the kid, # vo:p_7_l_22
            Drunk Cop: but it was still not enough? # vo:p_7_l_23
            ~ ChangeCamera("Player")
            Drunk Cop: Anyways, I’m single now, so where is my kiss? # vo:p_7_l_24
            -> LAU_story_phase_2_continue_1

    = reason_for_detergent
        Drunk Cop: You can find detergent on the table there. # vo:p_7_l_25
        ~ ChangeCamera("Player")
        * [No heavy-duty detergent]
        You: They don’t have heavy-duty detergent anymore. # vo:p_7_l_26
        Drunk Cop: Why do you need heavy-duty detergent anyways? # vo:p_7_l_27
        ~ ChangeCamera("Player")
        ** [It makes clothes cleaner (Excuse)] 
         You: I think heavy-duty detergent gets clothes cleaner and makes them smell nicer. # vo:p_7_l_28
         Drunk Cop: Really... What a strange habit. # vo:p_7_l_29
* [Lie] You: My cat peed on the sheets. It’s stinky as hell. # vo:p_7_l_30
            Drunk Cop: Really? I can’t smell it. # vo:p_7_l_31
            ~ lau_cat_pee = true
            ~ ChangeCamera("l3")
            ** [Bluff]
            You: If you really want to take a whiff of my cat’s pee, go ahead. # vo:p_7_l_32
            Drunk Cop: Haha, no need. I believe you. # vo:p_7_l_33
            ** [There’s something wrong with your nose.]
            You: What’s wrong with your nose? # vo:p_7_l_34
            Drunk Cop: What? I have the best nose of the whole precinct. # vo:p_7_l_35
        -
        ~ ChangeCamera("Player")
        Drunk Cop: Anyways, I don’t have the special detergent for you. # vo:p_7_l_36
        Drunk Cop: You should go ask Mrs Wong. # vo:p_7_l_37
    ~ ChangeCamera("Player")
    -> END
    
    
// =============================================================================
//  PHASE 8 Triggered when you enter the backroom, and during that you can’t go out. 
// =============================================================================
== Inner_voice_backroom_phase_1 ==
{ black_out_happened: 
    -> Inner_voice_phase_2 
}
~ game_progression = 8
~ PlayAudioClip("musicAccent_2")
{ Inner_voice_backroom_phase_1:
    - 1: 
    ~ ChangeCamera("b1")
    Thoughts: The look in Mrs Wong’s eyes seemed to hold a hint of pity. # vo:p_8_l_1
    Thoughts: The police officer grinned, as if he’d known all along. # vo:p_8_l_2
    Thoughts: Did they all know, but were just toying with me? # vo:p_8_l_3
    Thoughts: Had they seen the bloodstains on those clothes? # vo:p_8_l_4
    ~ ChangeCamera("Player")
    - 2: 
    ~ ChangeCamera("b1")
    Thoughts: I’ve yet to cry. Yet to be sorrowful or to mourn. # vo:p_8_l_5
    Thoughts: With each step further I become less of a human. How long do I need to hide? # vo:p_8_l_6
    Thoughts: Do I need to carry this secret with me for the rest of our life? # vo:p_8_l_7
    ~ ChangeCamera("Player")
    - 3:
    ~ ChangeCamera("b1")
    Thoughts: This won’t bring a dead man back. Regardless of how much detergent goes in there. # vo:p_8_l_8
    Thoughts: Saying it does not bring him back. Then nothing will give me peace. # vo:p_8_l_9
    Thoughts: And when I’m done with this, a murder awaits me at home. # vo:p_8_l_10
    ~ ChangeCamera("Player")
    - 4:
    ~ ChangeCamera("b2")
    Thoughts: This won’t bring him back. Regardless of how much detergent goes in there. # vo:p_8_l_11
    Thoughts: Saying it does not bring him back. # vo:p_8_l_12
    Thoughts: But at least then the crime would exist somewhere outside my own skull. Somewhere  where it won’t grow. # vo:p_8_l_13
    Thoughts: Does Jason think the same? # vo:p_8_l_14
    ~ ChangeCamera("Player")
    - else: 
    ~ ChangeCamera("b2")
    Thoughts: For how long can I keep up this facade, these lies? # vo:p_8_l_15
    ~ ChangeCamera("Player")
}

~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 9 Directly after the innervoice, pager beeps as interruption.
// =============================================================================
== Boyfriend_pager_phase_1 ==
~ game_progression = 9
J: Has anyone seen you? # vo:p_10_l_1
J: Play it cool, bb. # vo:p_10_l_2
J: Don’t forget — use heavy-duty detergent. # vo:p_10_l_3
J: Packed up. Driving to the harbor. # vo:p_10_l_4
J: Be quick with cleaning up!!!! # vo:p_10_l_5
J: TTYL. # vo:p_10_l_6
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 10 After pager phase for around 5 sec, if player didn’t find the detergent yet.
// =============================================================================
== Thought_about_how_detergent_looks ==
~ game_progression = 11
~ ChangeCamera("b1")
Thoughts: Mrs Wong said that the heavy-duty detergent is blue and should be somewhere on the shelf. # vo:p_11_l_1
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 11 After player got the detergent.
// =============================================================================
== Thought_about_got_right_detergent ==
~ game_progression = 12
~ PlayAudioClip("musicAccent_1")
~ has_detergent = true
~ ChangeCamera("b2")
Thoughts: That’s the correct detergent. I need to put these into washer Nr. 4 as soon as possible. # vo:p_12_l_1
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 12 During the first washing mini game
// =============================================================================
== Thought_washing_clothes_1 ==
~ game_progression = 13
~ ChangeCamera("b4")
Thoughts: Jason was wearing this shirt when we met. # vo:p_13_l_1
Thoughts: On a rainy day, I accidentally slipped and my groceries fell to the ground. # vo:p_13_l_2
Thoughts: He was the one who helped me pick them up. We were together two weeks later. # vo:p_13_l_3
Thoughts: I thought we would continue living this ordinary life together, in love. # vo:p_13_l_4
Thoughts: Who could have thought... # vo:p_13_l_5
Thoughts: I wonder if we could ever forget what happened after we washed these. # vo:p_13_l_6
Thoughts: Could we really put these shirts on and pretend they were never stained with blood? # vo:p_13_l_7
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 13 after the first washing mini game, commenting that she needs another round
// =============================================================================
== Thought_about_need_another_washer ==
~ game_progression = 14
~ ChangeCamera("b2")
Thoughts: Shit. Still many clothes left. # vo:p_14_l_1
Thoughts: I should have thought that one round is not enough. # vo:p_14_l_2
Thoughts: I need to get another laundry coin. # vo:p_14_l_3
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 14 Press E in front of mandy, After the first washing mini game
// =============================================================================
== Mandy_story_phase_3 ==
~ game_progression = 15
{ Mandy_story_phase_3:
    - 1: -> Ask_for_laundry_coin_Mandy
    - else:
    ~ TriggerAnimation("Mandy", "doRelax")
    Mrs Wong: You get your change now? # vo:p_15_l_1
    ~ ChangeCamera("Player")
    + [Where is the coin change machine?]
    You: Not yet. Where is the coin change machine again? # vo:p_15_l_2
    Mrs Wong: It’s the one in the corner. # vo:p_15_l_3
    You: Thanks. # vo:p_15_l_4
    ~ ChangeCamera("Player")
    -> END
}
    
= Ask_for_laundry_coin_Mandy
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
* [Ask for another laundry coin]
You: Mrs Wong, can I buy another laundry coin? # vo:p_15_l_5
Mrs Wong: Another one? Did all your clothes fall into a pit or something? # vo:p_15_l_6
   ~ ChangeCamera("Player")
   ** [Summer sweating excuse]
   You: You know, sweating in the summer and stuff. I can’t stand smelly clothes. # vo:p_15_l_7
   Mrs Wong: Hmm... I remember you were here just two days ago. # vo:p_15_l_8
   You: Well, clothes get dirty pretty fast in summer. # vo:p_15_l_9
   -> Mandy_phase_3_fair
    ** {lied_about_cat} [Cat peed everywhere. (Lie)]
    You: I told you, the cat peed on our clothes, so we have to wash all the bedding and stuff, because it stinks so much... # vo:p_15_l_10
    Mrs Wong: Naughty cat. What’s the name? # vo:p_15_l_11 -> Cat_name_question
    ** {lie_about_period} [Period got everywhere]
     You: Well... # vo:p_15_l_12
     You: My flow is especially heavy today... # vo:p_15_l_13
     You: And I accidentally put my clothes on the bloodstained sheets. # vo:p_15_l_14
     -> Mandy_phase_3_fair
~ ChangeCamera("Player")
-> END

= Mandy_phase_3_fair
Mrs Wong: Why didn’t your boyfriend come and let you rest? # vo:p_15_l_15
~ ChangeCamera("m4")
Thoughts: He’s currently dealing with the body... # vo:p_15_l_16
~ ChangeCamera("Player")
* [I volunteered (Lie)]
You: It was me who offered to help him. He is too busy with work. # vo:p_15_l_17
* [No time]
You: He doesn’t have time. # vo:p_15_l_18
-
Mrs Wong: Hah, such a typical excuse. # vo:p_15_l_19
~ ChangeCamera("Player")
** [Defend Jason]
You: He works very hard to earn money though. # vo:p_15_l_20
--
Mrs Wong: Hmm. I have to warn you, when a man stops taking care of household, # vo:p_15_l_21
Mrs Wong: it’s normally a sign that he will start neglecting your feelings. # vo:p_15_l_22
Thoughts: It’s true that Jason always asks me to do housework... # vo:p_15_l_23
Thoughts: But he works a lot to make money. # vo:p_15_l_24
Thoughts: And his marriage proposal was so romantic... # vo:p_15_l_25
~ ChangeCamera("Player")
** [Jason is different.]
You: Don’t worry, my boyfriend is different. # vo:p_15_l_26
{ proposal_admit:
    Mrs Wong: I don’t mean to scare you so soon after you just accepted his proposal. # vo:p_15_l_27
- else:
    Mrs Wong: I’m not saying your boyfriend is just like my useless husband. # vo:p_15_l_28
}
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: But many of them always start out romantic, and then... # vo:p_15_l_29
Mrs Wong: My husband used to buy me jewelry and take me to the docks for stargazing every other day. # vo:p_15_l_30
Mrs Wong: Now, he won’t even talk to me, unless he wants food or needs me to cover his shifts. # vo:p_15_l_31
Mrs Wong: And you can see Mr. Lau there, being drunk at 3am... # vo:p_15_l_32
Mrs Wong: I have to say, some men really are useless... # vo:p_15_l_33
Drunk Cop: You’re speaking a bit too loudly, aren’t you? # vo:p_15_l_34
   ~ LookAtTarget("Lau1", 0.75)
   *** [We were talking about you]
   You: We were talking about you. # vo:p_15_l_35
   ~ ChangeCamera("l1")
   Drunk Cop: Me? I’m not useless, I’m the best cop in the whole precinct. # vo:p_15_l_36
   *** [Not talking about you]
    ~ ChangeCamera("l1")
   You: We were not talking about you. # vo:p_15_l_37
   Drunk Cop: ... # vo:p_15_l_38
   ---
   ~ RestoreLook(0.75)
   Mrs Wong: Haha. # vo:p_15_l_39

->Mandy_phase_3_ending

= Cat_name_question
~ ChangeCamera("Player")
Thoughts: Now I just have to come up with a name for this imaginary cat... # vo:p_15_l_40
* [Jason.]
You: He’s Jason. # vo:p_15_l_41
Mrs Wong: Interesting choice to name your cat after your boyfriend, haha. # vo:p_15_l_42
  ~ ChangeCamera("Player")
  ** [We thought it’s funny.]
  You: ... We thought it’s funny. # vo:p_15_l_43 ->Mandy_phase_3_fair
  ** [Haha]
  You: ... haha # vo:p_15_l_44
  ->Mandy_phase_3_ending
  
* [Miao Miao.]
You: Miao Miao. # vo:p_15_l_45
* [Caesar.]
You: Caesar. # vo:p_15_l_46
-
Mrs Wong: Cute. # vo:p_15_l_47
->Mandy_phase_3_ending

= Mandy_phase_3_ending
~ ChangeCamera("Player")
Thoughts: I should wash them quickly, Jason will get mad if I’m too slow... # vo:p_15_l_48
* [Ask Mrs Wong for paying]
You: The second washer would cost 80 cents, too, right? # vo:p_15_l_49
Mrs Wong: Yes. # vo:p_15_l_50
Thoughts: Damn, I’d completely forgotten. # vo:p_15_l_51
Thoughts: Blood had splattered on our money when it happened. # vo:p_15_l_52
~ ChangeCamera("Player")
** [Wait, I need to get some change first.]
You: Uhm, give me one second. I have to get some change. # vo:p_15_l_53
Mrs Wong: Are you sure? I have change here. # vo:p_15_l_54
~ ChangeCamera("b6")
Thoughts: Mrs Wong can’t see the blood on the bill. # vo:p_15_l_55
Thoughts: I need to use the coin change machine. # vo:p_15_l_56
   ~ ChangeCamera("Player")
   *** [I got this.]
   You: No worries, I got this. # vo:p_15_l_57
   *** [Make an excuse]
   You: It’s okay, I need some change for payphones anyways. # vo:p_15_l_58
   -
   Mrs Wong: Sure. The coin change machine is in the corner. # vo:p_15_l_59
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 15 In front of coin machine, after press (E Interact)
// =============================================================================
== Thought_about_coin_machine_1 ==
~ ChangeCamera("b3")
Thoughts: I just need to put it in...
~ ChangeCamera("Player")
-> Boyfriend_pager_phase_2

// =============================================================================
//  PHASE 16 Fired by CoinMachineOperator after each bill attempt.
//  coin_machine_attempt is persisted via GlobalVariableOperator (visit counts reset per Story).
// =============================================================================
== Interaction_with_coin_machine ==
~ coin_machine_attempt = coin_machine_attempt + 1
~ game_progression = 16
{ coin_machine_attempt:
    - 1: 
    ~ ChangeCamera("b2")
    Thoughts: Shit, this machine always has this problem. But trying again normally helps. # vo:p_16_l_1
    ~ ChangeCamera("Player")
    - 2:
    ~ ChangeCamera("b2")
    Thoughts: Again? It needs to work... # vo:p_16_l_2
    ~ ChangeCamera("Player")
    - else:
    // Win: unlock Collect 5 coins (TaskManager storyPhase 17).
    ~ game_progression = 17
    ~ ChangeCamera("b3")
    Thoughts: Finally... I should collect these coins to buy another laundry token. # vo:p_16_l_3
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 17 Pager interruption (progression 17 is set on coin-machine win above).
// =============================================================================    
== Boyfriend_pager_phase_2 ==
//pager beeps and vibrates
J: A cop stopped me. # vo:p_17_l_1
J: I lied about the trunk. # vo:p_17_l_2
J: Got away with a speeding ticket. # vo:p_17_l_3
J: Hands shaking so bad I can barely drive. # vo:p_17_l_4
J: Finally at the harbor. # vo:p_17_l_5
J: HURRY UP WITH WASHING VIVIAN! # vo:p_17_l_6
J: We got each other. # vo:p_17_l_7
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 18 Press E in front of Mandy, after get coins
// =============================================================================
== Mandy_story_phase_4 ==
~ game_progression = 18
~ TriggerAnimation("Mandy", "doRelax")
Mrs Wong: You get your change now? # vo:p_18_l_1
~ ChangeCamera("Player")
* [Yes. (Pay)]
~ GiveAwayItem("change_coin_1")
~ GiveAwayItem("change_coin_2")
~ GiveAwayItem("change_coin_3")
~ GiveAwayItem("change_coin_4")
You: Yes, here. # vo:p_18_l_2
Mrs Wong: Miss Lee, wait. # vo:p_18_l_3
Mrs Wong: What happened with your arm? # vo:p_18_l_4
~ ChangeCamera("Player")
Thoughts: I almost thought there was still blood on my hand. # vo:p_18_l_5
Thoughts: It was just a bruise. # vo:p_18_l_6
Thoughts: Jason had gripped my arm tightly when he asked me to get the knife. # vo:p_18_l_7
Thoughts: I hadn’t even noticed that you could see it at all. # vo:p_18_l_8
** [Lie]
You: Oh, I was too clumsy, I accidentally bumped into the corner of the table. # vo:p_18_l_9
Mrs Wong: Really? I thought these come from... You know. # vo:p_18_l_10
** [Dodge]
You: Oh, you don’t really need to know, Mrs Wong. # vo:p_18_l_11
-
Mrs Wong: I know where these come from. # vo:p_18_l_12
Mrs Wong: Whenever my husband comes home drunk, I have to put up with that, too. # vo:p_18_l_13
Thoughts: Jason had a few drinks tonight before it happened. # vo:p_18_l_14
Thoughts: With alcohol he becomes different, sometimes. # vo:p_18_l_15
Thoughts: He can get... pretty rough. # vo:p_18_l_16
Thoughts: But that’s mostly because of the stress from his job. # vo:p_18_l_17
Thoughts: Aside from these drunk episodes, he’s the sweetest person I’ve ever met. # vo:p_18_l_18
** [Defend Jason]
You: No Jason is not like that, it happened by accident. # vo:p_18_l_19
** [Feel sorry for her]
You: I understand...I feel so sorry for you. # vo:p_18_l_20
--
Mrs Wong: If you have any trouble, please let me know, okay? # vo:p_18_l_21
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("second_laundry_coin")
Mrs Wong: Here you are. Washer Nr. 9. # vo:p_18_l_22
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 19 Triggered automatically, as player drops cloth walking towards the other washer.
// =============================================================================
== LAU_story_phase_3 ==
~ game_progression = 19
~ PlayAudioClip("musicAccent_4")
Drunk Cop: There’s an awfully red stain on your clothes. # vo:p_19_l_1
-> LAU_story_phase_3_continue_1

    = LAU_story_phase_3_continue_1
        ~ ChangeCamera("Player")
        * [Lie about an accident] 
        You: Well, my boyfriend accidentally broke a bottle and cut his hand. # vo:p_19_l_2
        ~ lied_about_hand = true
        Drunk Cop: Uh, that must’ve hurt. # vo:p_19_l_3
        Drunk Cop: Why is there blood all over the chest area of this shirt? # vo:p_19_l_4
        ~ ChangeCamera("Player")
        ** [Try to cover the lie up]
        You: We tried to wrap his hand around his shirt to stop the bleeding. # vo:p_19_l_5
        Drunk Cop: Wrap his hand around his shirt? Hahaha # vo:p_19_l_6
        Drunk Cop: I’m starting to wonder if you’re actually more drunk than I am. # vo:p_19_l_7
        ~ ChangeCamera("l3")
        Thoughts: Stupid slip-up...I hope he doesn’t notice that my hands are shaking. # vo:p_19_l_8
        ~ ChangeCamera("Player")
        Drunk Cop: I hope your boyfriend is doing alright. # vo:p_19_l_9
        ~ ChangeCamera("Player")
        *** [He is.]
        You: Yes, he is, thanks. That’s why he’s home. # vo:p_19_l_10
        *** [Deflect]
        You: You’re the nosiest cop I’ve ever seen. # vo:p_19_l_11
        Drunk Cop: Or the most observant. # vo:p_19_l_12
        ---
        -> LAU_story_phase_3_continue_2
        
        ~ ChangeCamera("Player")
        * [Lie about spilled wine.] 
        You: Well, my boyfriend spilled red wine all over the place. # vo:p_19_l_13
        ~ lied_about_wine = true
        Drunk Cop: Really... What kind of wine is that? It’s as red as blood. # vo:p_19_l_14
        ~ ChangeCamera("l4")
        Thoughts: Damn, I know nothing about wine. # vo:p_19_l_15
        ~ ChangeCamera("Player")
            * * [I don’t remember.] 
            You: I don’t remember. # vo:p_19_l_16
            You: He got this wine from the wine shop on the next street over. # vo:p_19_l_17
            * * [Make something up]
            You: la... Toise. Something like it. # vo:p_19_l_18
Drunk Cop: Interesting, never heard of that before. I thought I knew everything about wine. # vo:p_19_l_19
            ~ ChangeCamera("Player")
            *** [It’s rare.]
            You: It’s rare, my boyfriend got it as a gift from a friend abroad. # vo:p_19_l_20
            --
            Drunk Cop: How strange... # vo:p_19_l_21
            Drunk Cop: A fun fact about wine... it dries to a purple-ish color. # vo:p_19_l_22
            Drunk Cop: Blood, on the other hand, turns dark, rust-red. # vo:p_19_l_23
            -> LAU_story_phase_3_ending
            
        ~ ChangeCamera("Player")
        * {lie_about_period} [Lie about period.] 
        ~Cop_knows_period = true
        You: I’m... having the time of the month. # vo:p_19_l_24
        Drunk Cop: Oh, I see. # vo:p_19_l_25
        Drunk Cop: But why is there blood all over the chest area of this shirt? # vo:p_19_l_26
         ~ ChangeCamera("Player")
         ** [Try to explain]
         You: I accidentally put it in the wrong spot on the bedsheet. # vo:p_19_l_27
         Drunk Cop: Okay... # vo:p_19_l_28
         -> LAU_story_phase_3_ending 
        * [None of your business.] 
        You: Just mind your own business. # vo:p_19_l_29
        Drunk Cop: I’m a cop and I patrol this area: # vo:p_19_l_30
        Drunk Cop: Of course, I have to take care of other people’s business. # vo:p_19_l_31
        ~ ChangeCamera("Player")
        Thoughts: I thought I could get away with this... # vo:p_19_l_32
        Thoughts: Now I have to come up with an excuse. # vo:p_19_l_33
        -> LAU_story_phase_3_continue_1
    ~ ChangeCamera("Player")
    -> END
    
    = LAU_story_phase_3_continue_2
    {told_lie_sick:
        -> sick_reply_phase_3
    - else:
        {told_lie_busy:
            -> busy_reply_phase_3
        - else:
            -> LAU_story_phase_3_ending
        }
    }

    
    = sick_reply_phase_3
    Drunk Cop: But I thought he stayed at home because he’s sick? # vo:p_19_l_34
    ~ ChangeCamera("Player")
    * [I said that]
    You: That as well. # vo:p_19_l_35
    Drunk Cop: Cut his hand and got sick! Maybe next time he'll trip and break a leg! # vo:p_19_l_36
    ~ ChangeCamera("Player")
    ** [He’s sleeping.]
        You: He already fell asleep. # vo:p_19_l_37
    ** [I want to do some housework.]
        You: He’s taking some rest, so I’d just do some housework before this here closes. # vo:p_19_l_38
    - 
    Drunk Cop: Okay, I thought the constant pager beeping would be from your poor boyfriend. # vo:p_19_l_39
    ** [It was from my mom.]
        You: It was from my mom. # vo:p_19_l_40
        -> LAU_story_phase_3_ending
    ** [No, from a friend.]
        You: No, it’s my best friend texting me. # vo:p_19_l_41
        You: She recently bought a pager and has been using it all the time. # vo:p_19_l_42
        -> LAU_story_phase_3_ending
    
    = busy_reply_phase_3
    Drunk Cop: But I thought he stayed at home because he’s busy? # vo:p_19_l_43
    ~ ChangeCamera("Player")
    * [Agree]
        You: Yes, he had some stuff to do for his work. # vo:p_19_l_44
        -> LAU_story_phase_3_ending
    * [Busy with sleeping.]
        You: Busy with sleeping. # vo:p_19_l_45
        -> LAU_story_phase_3_ending


    = LAU_story_phase_3_ending 
    Drunk Cop: This red stain reminds me of today's crime scene. # vo:p_19_l_46
    Drunk Cop: A middle-aged man stabbed his wife to death. # vo:p_19_l_47
    Drunk Cop: He refused to plead guilty, so we had no choice but to put him in jail. # vo:p_19_l_48
    Drunk Cop: Her shirt also has this red stain... # vo:p_19_l_49
    ~ ChangeCamera("l1")
    * {lied_about_wine} [Bluff]
    You: There’s no need to overthink it. # vo:p_19_l_50
    You: The bottle of wine my boyfriend spilled might just have been made differently. # vo:p_19_l_51
    * {lied_about_wine} [...]
    * {lied_about_hand} [Tell him that he was overthinking]
    You: Why do you need to overthink so much? # vo:p_19_l_52
    You: Isn’t cutting your hand accidentally quite normal? # vo:p_19_l_53
    * {lied_about_hand} [Show that you’re scared of the crime]
    You: ...Sounds scary. # vo:p_19_l_54
    * {Cop_knows_period} [Tell him that he was overthinking]
    You: There’s no need to overthink it. # vo:p_19_l_55
    -
    ~ ChangeCamera("Player")
    Drunk Cop: All right then... I hope you’re being honest. # vo:p_19_l_56
    Drunk Cop: You do know what happens if you lie to a police officer, don’t you? # vo:p_19_l_57
    * [Silence]
    You: ... # vo:p_19_l_58
    Drunk Cop: Haha. I was just joking around. Go wash your clothes. # vo:p_19_l_59
    -> END
    
    * [I won’t lie.]
    You: Of course I won’t lie to you. # vo:p_19_l_60
    Drunk Cop: Sweet girl. # vo:p_19_l_61
    Drunk Cop: But if you’re hiding anything, you’d better tell me soon. # vo:p_19_l_62
    Drunk Cop: Go wash your clothes first. # vo:p_19_l_63
    ~ ChangeCamera("Player")
    -> END
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 20 During the second washing mini game, x lines in total, so maybe each action a line automatically
// =============================================================================
== Thought_washing_clothes_2 ==
~ game_progression = 20
~ ChangeCamera("b1")
Thoughts: Before that person came in, Jason had a few drinks. # vo:p_20_l_1
Thoughts: He was wearing this. # vo:p_20_l_2
Thoughts: Why did we.. why did Jason do that. # vo:p_20_l_3
~ ChangeCamera("Player")
Thoughts: I can’t forget the man’s eyes when Jason stabbed... # vo:p_20_l_4
~ ChangeCamera("b3")
Thoughts: I can’t forget the smell of blood when Jason held me after it happened. # vo:p_20_l_5
Thoughts: How long do I need to hide? # vo:p_20_l_6
Thoughts: I'm tired of keeping these lies... # vo:p_20_l_7
Thoughts: But how could I possibly abandon the one I love so deeply, # vo:p_20_l_8
Thoughts: the one who protected me and even killed someone to keep me safe? # vo:p_20_l_9
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 21 after the second washing clothes mini game, as the lights turned off.
// =============================================================================
== Chaos_blackout ==
~ game_progression = 21
~ black_out_happened = true
~ SetBlackout(1)
Drunk Cop: Damn, what the hell? # vo:p_21_l_1
Mrs Wong: Not again... # vo:p_21_l_2
Mrs Wong: Miss Lee, do me a favour, would you please check the circuit box in the back room? # vo:p_21_l_3
~ ChangeCamera("Player")
* [I will do it.]
You: I can do it. I’m standing next to it. # vo:p_21_l_4
Mrs Wong: Thank you, Miss Lee. # vo:p_21_l_5
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 22 after enter the backroom
// =============================================================================
== Inner_voice_phase_2 ==
~ game_progression = 22
{ Inner_voice_phase_2:
-1: 
~ ChangeCamera("b3")
Thoughts: Why did this happen... # vo:p_22_l_1
Thoughts: Is it my destiny that I can never wash away our crime? # vo:p_22_l_2
Thoughts: Should I just give up? But I got nowhere to hide... # vo:p_22_l_3
Thoughts: Should I go back? But I’ll always be living in the shadow of this murder. # vo:p_22_l_4
Thoughts: Should I can ask Mrs. Wong for help? But Jason will hate me... # vo:p_22_l_5
Thoughts: ...or I betray him and report his murder? # vo:p_22_l_6
~ ChangeCamera("Player")

    - 2: 
    ~ ChangeCamera("b3")
    Thoughts: The cop grins like he already knows. # vo:p_22_l_7
    Thoughts: Knows and toys with me, maybe he does, and maybe that’s a relief I won’t admit to. # vo:p_22_l_8
    Thoughts: To be caught would at least mean the end of this act. # vo:p_22_l_9
    ~ ChangeCamera("Player")
    - else:
    ~ ChangeCamera("b1")
    Thoughts: I don’t care about karma anymore. # vo:p_22_l_10
    Thoughts: I wish someone could be by my side... # vo:p_22_l_11
    Thoughts: Should I give up? But I got nowhere to hide... # vo:p_22_l_12
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END


    
// =============================================================================
//  PHASE 23 After the inner voice phase
// =============================================================================
== Boyfriend_pager_phase_3 ==
~ game_progression = 23
J: It’s done. # vo:p_23_l_1
J: Rocks in the body bag. # vo:p_23_l_2
J: Tossed the whole thing in the ocean. # vo:p_23_l_3
J: Why are you so slow? # vo:p_23_l_4
J: Hurry up!!! # vo:p_23_l_5
J: You don’t want our lives ruined, right? # vo:p_23_l_6
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 24 after boyfriend pager ( now it’s automatically switched in ink) (player can’t leave the backroom without light switching back on)
// =============================================================================
== How_to_turn_on_circuit_box ==
~ game_progression = 24
    ~ ChangeCamera("b4")
    Thoughts: I should turn on the power. # vo:p_24_l_1
    Thoughts: The circuit box should be on the wall. # vo:p_24_l_2
    Thoughts: I guess I need to turn on the biggest switch there? # vo:p_24_l_3
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 25 If the player attempts to leave the backroom while the power blackout is still there
// =============================================================================
== Attempt_leaving_backroom ==
~ game_progression = 25
    ~ ChangeCamera("b1")
    Thoughts: I can’t leave yet. I need to turn on the power. # vo:p_25_l_1
    Thoughts: The circuit box should be on the wall. # vo:p_25_l_2
    Thoughts: I guess I need to turn on the biggest switch there? # vo:p_25_l_3
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 26 After player comes out of the backroom, mandy stands in front of the door and automatically start the conversation
// =============================================================================
== Mandy_smoking_scene_1 ==
~ game_progression = 26
~ TriggerAnimation("Mandy", "doRelax")
Mrs Wong: Is everything okay back there? You’ve been gone quite a while, Miss Lee. # vo:p_26_l_1
* [Make an excuse]
You: Sorry, I couldn’t find the correct switch for the lights. # vo:p_26_l_2
* [Sorry.]
You: I’m really sorry. # vo:p_26_l_3
-
Mrs Wong: No need to apologize. Want a cigarette? # vo:p_26_l_4

- (smoke_choice)
   * [Thanks]
   You: No, thank you. # vo:p_26_l_5
   -
   Mrs Wong: Mind if we talk for a second? Just between us women. # vo:p_26_l_6
      ** [What’s it about?]
      You: Sure. What’s it about? # vo:p_26_l_7
      ** [Not sure.]
      You: I’m not sure... # vo:p_26_l_8
      Mrs Wong: I’m just worried about you. # vo:p_26_l_9
      - -
      Mrs Wong: Someone’s been paging you all the time, right? Is that your boyfriend? # vo:p_26_l_10
         *** [Yes.]
         You: Yes, he just worried because it’s late. # vo:p_26_l_11
         *** [Deny]
         You: No... # vo:p_26_l_12
         - - -
         ~ TriggerAnimation("Mandy", "doTalk")
         Mrs Wong: You can be completely honest with me, Miss Lee. # vo:p_26_l_13
         Mrs Wong: Every time you come out from the backroom like a ghost, and... # vo:p_26_l_14
         Mrs Wong: There’s more red in your clothes than on your face. # vo:p_26_l_15
         -> Mandy_smoking_scene_2


// =============================================================================
//  PHASE 27 If the player not admit at first, but come to her again by pressing E, and can choose to admit again
// =============================================================================
== Mandy_smoking_scene_2 ==
~ game_progression = 27
~ TriggerAnimation("Mandy", "doRelax")
+ [Tell her about the murder]
You: Mrs Wong, I’ve done something bad... # vo:p_27_l_1
-> Admit_to_Mandy

+ (dont_tell) [Don’t tell her]
  {dont_tell:
  - 1:
  You: Mrs Wong, nothing happened. # vo:p_27_l_2
  Mrs Wong: Then I must have been overthinking it. # vo:p_27_l_3
  Mrs Wong: I’m going to have a smoke here for a bit. # vo:p_27_l_4
  Mrs Wong: If you want to come talk to me later, feel free to stop by again. # vo:p_27_l_5
  - else: 
  You: Nothing. # vo:p_27_l_6
  Mrs Wong: Okay. # vo:p_27_l_7
  Mrs Wong: I’m going to have a smoke here for a bit. # vo:p_27_l_8
  Mrs Wong: If you want to come talk to me later, feel free to stop by again. # vo:p_27_l_9
  }
-> END

= Admit_to_Mandy
Mrs Wong: Calm down and take a deep breath. I’m here. # vo:p_27_l_10
* [Try to calm down and explain]
You: Someone broke in... # vo:p_27_l_11
You: Jason... he... everything happened so fast. # vo:p_27_l_12
You: he told me if the blood was washed away, everything would go back to normal... # vo:p_27_l_13
Mrs Wong: My goodness, Miss Lee... # vo:p_27_l_14
Mrs Wong: So all those lies... You were just trying to survive, right? # vo:p_27_l_15
** [Apologise]
You: I’m sorry, I was panicking. # vo:p_27_l_16
Mrs Wong: (Sigh) # vo:p_27_l_17
Mrs Wong: I should have known he was that kind of guy. # vo:p_27_l_18
*** [Jason is different]
You: But he loves me! He was protecting me, Mrs Wong... # vo:p_27_l_19
Mrs Wong: Did he do that for you, or for his own safety? # vo:p_27_l_20
Mrs Wong: But what are you going to do? # vo:p_27_l_21
Mrs Wong: They’ll find out sooner or later. # vo:p_27_l_22
**** [I don’t know.]
You: I don’t know, Mrs Wong... # vo:p_27_l_23
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: ...My cousin lives in Tou San. # vo:p_27_l_24
Mrs Wong: You can go there, but you’ll have to leave everything behind - # vo:p_27_l_25
Mrs Wong: Including your boyfriend - and try to make a living there. # vo:p_27_l_26
Mrs Wong: I can’t guarantee it’s safe there, or that no one will find you. # vo:p_27_l_27
Mrs Wong: But it’s better than continuing to hide here. # vo:p_27_l_28
Mrs Wong: Think about it, Miss Lee. Take your time. # vo:p_27_l_29
-> Mandy_smoking_scene_3

// =============================================================================
//  PHASE 28 If the player admitted, but not accept her help yet.
// =============================================================================
== Mandy_smoking_scene_3 ==
~ TriggerAnimation("Mandy", "doRelax")
- (final_choices_mandy)
* [What is Tou San like?]
You: What is Tou San like? # vo:p_28_l_1
Mrs Wong: My cousin said it’s a county town, not as bustling as here. # vo:p_28_l_2
Mrs Wong: If you put in some effort, you can still find a good way to make a living there. # vo:p_28_l_3 -> final_choices_mandy
+ [Accept help and escape (Ending)]-> Mandy_escape_ending
+ [Take time to think]
You: I don’t know... I need to think. # vo:p_28_l_4
Mrs Wong: Take your time. I’ll stay here for a while. # vo:p_28_l_5
Mrs Wong: you can come back to me whenever you’re ready. # vo:p_28_l_6
~ game_progression = 28
-> END

= Mandy_escape_ending
~ PlayEndingCutscene(1)
Thoughts: It’s time to live on my own. # vo:p_28_l_7
Thoughts: I don’t need to listen to Jason anymore, # vo:p_28_l_8
Thoughts: and I don’t need to obey him all the time. # vo:p_28_l_9
Thoughts: I have to escape this life. And escape him. # vo:p_28_l_10
You: So, how can I go to Tou San? # vo:p_28_l_1
Mrs Wong: There’s a ferry to go there every morning at 8:00. # vo:p_28_l_12
Mrs Wong: You need to head to the harbor now. # vo:p_28_l_13
Mrs Wong: So leave this man, start a new life. # vo:p_28_l_14
You: Thank you, Mrs Wong... # vo:p_28_l_15
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: You can just call me Mandy. # vo:p_28_l_16
Mandy: I’ll send a message to my cousin Cindy to pick you up. # vo:p_28_l_17
Mandy: Also, Vivian... be independent. # vo:p_28_l_18
Mandy: That’s the most precious thing a woman can have. # vo:p_28_l_19
Mandy: Don’t rely on any men for your life and happiness. # vo:p_28_l_20
You: I promise you, Mandy. # vo:p_28_l_21
Mandy: Goodbye, Vivian. # vo:p_28_l_22
You: Goodbye, Mandy. # vo:p_28_l_23
-
~ game_progression = 28
-> END

// =============================================================================
//  PHASE 29 After you talk to mandy, you will recieve a message from your boyfriend.
// =============================================================================
== Boyfriend_pager_ending ==
~ game_progression = 29
J: Just arrived home. # vo:p_29_l_1
J: Write me as soon as you’re coming home. # vo:p_29_l_2
~ ChangeCamera("Player")
-> END


// =============================================================================
//  PHASE 30 After mandy talks to you, if player goes to Lau and press E.
// =============================================================================
== Lau_confess_ending ==
Drunk Cop: What’s that face for, sweet heart? You look like you just saw a ghost. # vo:p_30_l_1
+ [Nothing.]
You: I’m just tired. # vo:p_30_l_2
-> END
* [Report boyfriend’s murder]
You: I want to report a murder. # vo:p_30_l_3
Police Officer: Say that again, miss. What happened? # vo:p_30_l_4
~ TriggerAnimation("Lau", "doPager")
~ PlayEndingCutscene(2)
-> murder_confess
= murder_confess
You: Someone broke in and threatened us. # vo:p_30_l_5
You: And my boyfriend... My boyfriend wanted to solve it with a knife. # vo:p_30_l_6
You: It all happened so fast... # vo:p_30_l_7
Police Officer: Were you a part of this? # vo:p_30_l_8
  * [I gave it to him.]
  You: He asked me to get a knife for him when the intruder broke in. # vo:p_30_l_9
  You: And then asked me to wash these clothes. # vo:p_30_l_10
  * [No, I’m not part of this.]
  You: I didn’t touch the knife. He asked me to wash those clothes, # vo:p_30_l_11
  You: I didn’t know what to do so I came here. # vo:p_30_l_12
  -
  Police Officer: Where is the suspect right now? # vo:p_30_l_13
  -(answer_suspect)
  *** [Say address]
  You: He’s at our apartment. 4th Floor, Block 3, 32 Lin Faa Street. # vo:p_30_l_14
  *** [...]
  Police Officer: Miss, please cooperate. # vo:p_30_l_15 -> answer_suspect
  ---
  Police Officer: What’s your name? # vo:p_30_l_16
  **** [Say your name]
  You: Vivian Lee. # vo:p_30_l_17
  Police Officer: And his name? # vo:p_30_l_18
  ***** [Say his name]
  You: Jason Ho. # vo:p_30_l_19
  
  ~ UnhideItem("police_lights")
  Police Officer: ...Miss, you did the right thing. # vo:p_30_l_20
  Police Officer: I knew something was fishy, but I’m glad you were the first to tell me. # vo:p_30_l_21
  Police Officer: Otherwise, the crime of harboring a murderer is very serious. # vo:p_30_l_22
  Police Officer: It took courage to confess and report a crime of your lover. # vo:p_30_l_23
  Police Officer: Now, please come with me. # vo:p_30_l_24
  ~ game_progression = 30
  -> END

// =============================================================================
//  PHASE 31 Standard dialogue after choosing to complete the mission on the pager. Maybe shown after the black screen?
// =============================================================================
== Boyfriend_ending_dialogue_final ==
~ game_progression = 31
You: I’m done. # vo:p_31_l_1
Jason: I just got home, too. # vo:p_31_l_2
Jason: I love you. # vo:p_31_l_3
You: I love you, too. # vo:p_31_l_4
Jason: It will pass. # vo:p_31_l_5
Jason: We still have tomorrow. # vo:p_31_l_6
Jason: We’ve got us. # vo:p_31_l_7
You: We’ve got us. # vo:p_31_l_8
Jason: Now, try to forget about it. # vo:p_31_l_9
Jason: Come into my arm, sweetheart. # vo:p_31_l_10
-> END
