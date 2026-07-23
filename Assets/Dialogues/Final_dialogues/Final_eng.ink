// Ending cutscenes (GameManager): 1 = escapeEnding, 2 = confessionEnding, 3 = CompletionEnding
EXTERNAL PlayEndingCutscene(cinematicIndex)

// Unhide a scene item mid-dialogue (DialogueItemUnhide). Ids: first_laundry_coin, backroom_key, second_laundry_coin
EXTERNAL UnhideItem(itemId)

// Give away a basket item mid-dialogue (DialogueItemGiveAway → BasketCollector.GiveBack).
// Change pay: change_coin_1 .. change_coin_4
EXTERNAL GiveAwayItem(itemId)

// Swap baked lighting scenarios (BakedLightingController). 1 = blackout, 0 = lights on.
EXTERNAL SetBlackout(blackout)

// Mandy animator triggers: doRelax, doIdle, doTalk, doGiveItem (DialogueAnimationTargets).
EXTERNAL TriggerAnimation(targetId, animationName)

// Dialogue cutscene camera (CutsceneDialogueCameraManager). Holds 10–25s then returns to player.
EXTERNAL ChangeCamera(cameraId)

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
VAR gun_chosen = false
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
VAR Sus = 0

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
On a humid midnight in Lam Tong City. We were just about to go to sleep after a tiring day.
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
    ~ kitchen_knife = true
* gun.
    ~ gun_chosen = true
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
Drunk Man: A pretty lady at this hour? Are you looking for me? 
- (Lau_choice_1)
    ~ ChangeCamera("Player")
    * [Greeting] 
        You: Good Evening.
    * [Continue] 
        You: I’m just here for the laundry.
    * [Insult] 
        You: Fuck off.
        ~ did_insult = true
        Drunk Man: Woah, take it easy, young lady. I was just joking around.
       -
       -> flattery -> clothes_question

= flattery
Drunk Man: You’re lucky to have me here, you know. 
Drunk Man: I’m a cop — no one would dare to harass a beautiful young lady like yourself in front of a police officer!
Drunk Cop: No villains can slip through my fingers.
~ ChangeCamera("b1")
Thoughts: Shit. I thought he’s just an average drunk...
~ ChangeCamera("Player")
* [...] 
You: ...
Drunk Cop: ... -> clothes_question
* [You don’t look like a cop]
You: Sorry, but you don’t look like a police officer.
Drunk Cop: What, are cops not allowed to do their laundry at night, 
Drunk Cop: after investigating a bloody crime scene?
~ ChangeCamera("Player")
** [Nevermind]
You: Never mind what I said.
-> clothes_question

= clothes_question
{ boyfriend_needs_clothes:
    Drunk Cop: Sorry for eavesdropping... 
    Drunk Cop: Why would your boyfriend need you to wash clothes in the middle of the night?
- else:
    Drunk Cop: Why are you washing clothes in the middle of the night?
}

-(questions_clothes)
~ ChangeCamera("Player")
* {not boyfriend_needs_clothes} [Can’t sleep] 
You: I can’t sleep.
Drunk Cop: But why would you come to a laundromat at 3am?
Drunk Cop: You’re young and beautiful, and you have a partner, 
Drunk Cop: which is the complete opposite of me. 
Drunk Cop: You’re not here to wash the smell of your ex-wife from your clothes, like I am, haha.
        * * [Sorry to hear that] 
            You: I’m sorry.
            Drunk Cop: I just can’t sleep being reminded of her. 
            -> need_to_answer
        * * [...] 
            You: ...
            Drunk Cop: ... 
            -> need_to_answer
* [Lie] 
        You: My boyfriend needs these clothes tomorrow for work, Officer.
        -> boyfriend_excuse
        
* { not boyfriend_needs_clothes} [For my boyfriend]
You: I need to wash some clothes for my boyfriend, Officer.
-> boyfriend_excuse

* [None of your business.] 
        You: It’s none of your business.
        { did_insult:
        Drunk Cop: Ooh, pretty lady has some secrets.
-> need_to_answer
        - else:
        Drunk Man: Woah, take it easy, young lady. 
        -> flattery
        }

= need_to_answer
{ need_to_answer > 1:
    Drunk Cop: But don’t throw curveballs. I’m familiar with that.
    Drunk Cop: Just answer the question.
- else:
    Drunk Cop: But you still need to answer my question.
}
Drunk Cop: Why are you washing clothes in the middle of the night?
-> questions_clothes

= boyfriend_excuse
Drunk Cop: But your boyfriend couldn’t be bothered to accompany you at this hour?
    ~ ChangeCamera("Player")
    * [He’s sick (Lie)] 
        You: He’s sick and he needs his clothes for work tomorrow.
        ~ told_lie_sick = true
        Drunk Cop: Sick but still goes to work? Huh, that’s how I lost my wife.
    * [He’s busy tonight and I have nothing to do.] 
        You: He’s busy tonight, and since I have nothing to do at home anyway, 
        You: I might as well help him with some chores.
        ~ told_lie_busy = true
        Drunk Cop: Busy at this hour, huh? 
        Drunk Cop: Are you sure he’s not up to no good?
- 
~ ChangeCamera("Player")
* [Can’t stop him.]
You: Well, I can’t stop him from working.
* [Excuse]
You: ...He’s a hardworking guy.
-
Drunk Cop: Fair. Money is important.
Drunk Cop: Your boyfriend is lucky to have someone like you to wash his clothes.
-> ending

= ending
Drunk Cop: Go on and wash your clothes then, young lady.
~ ChangeCamera("Player")
-> END

= repeat_visit
Drunk Cop: All good?
    ~ ChangeCamera("Player")
    + [Nothing.]
        You: ..
    + [Ask which machine]
        You: Which machine did I put my clothes in again?
        Drunk Cop: Hmmm... Number four?
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
Mrs Wong: Hey!
Mrs Wong: Isn’t that Miss Lee! Doing laundry at this hour?

- (You_intro_choice)
~ ChangeCamera("Player")
* [Can’t sleep] 
    You: Uh, yeah. I just couldn’t sleep.
    ~ cant_sleep = true
    Mrs Wong: Poor girl.
* [... (Stay silent)] 
    You: ...
    Mrs Wong: Why aren’t you saying anything? 
* [I really need to wash these.] 
    You: I have some clothes I really need for tomorrow.
    Mrs Wong: What’s the hurry? 
- 
-> MrsWong_ask_alright

  = MrsWong_ask_alright 
Mrs Wong: Is everything alright?
~ ChangeCamera("b1")
Thoughts: Mrs Wong is always so kind to me... I used to tell her so many things.
Thoughts: But now I have blood on my hands...
Thoughts: I can only pretend that everything is normal.
~ ChangeCamera("Player")
* [Say you’re fine. (Lie)]
You: Yeah, I’m fine. And you?
Mrs Wong: Oh, I’m really tired.
* {not cant_sleep} [Just tired.]
You: Sorry, just had a long day.
Mrs Wong: Same.
* {cant_sleep} [Make an excuse why you can’t sleep]
You: It’s just too hot to fall asleep in this weather. 
Mrs Wong: Fair. I’m the opposite.
* [Need to wash clothes for Jason]
You: Just need to wash these for my boyfriend.
~ boyfriend_needs_clothes = true
Mrs Wong: What’s the hurry? It’s 3 am. -> explain_hurry
-
Mrs Wong: I’d fall asleep as soon as my head hit the pillow, if I don’t need to be here.
-> Vi_ask_about_MrsWong_phase_1

=explain_hurry
~ ChangeCamera("Player")
* [Lie]
You: My boyfriend needs to wear these tomorrow at work.
* [He just asked me to.]
You: Well, my boyfriend just told me to wash them because he needs them.
-
Mrs Wong: I see. 
Mrs Wong: And my husband is playing Mahjong somewhere again, leaving me to run this place overnight. 
~ mahjong_mentioned = true
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: It’s a wonder what we women put up with. 
-> Vivian_question_loop

= Vi_ask_about_MrsWong_phase_1
- (Vivian_question_loop)

~ ChangeCamera("Player")
* {mahjong_mentioned == false} [Is everything alright?]
    You: Is everything alright?
    Mrs Wong: You could say so.
    Mrs Wong: My husband is off playing Mahjong again, someone has to run this place.
    -> Vivian_question_loop

* [Couldn’t your son help?]
    -> MrsWong_son_situation 

+ [Sounds rough. (Continue)]
    You: I’m sorry, Mrs Wong. That sounds rough.
    Mrs Wong: I’m used to it by now.
    -> MrsWong_phase_1_proposol

= MrsWong_son_situation
You: Phew. Couldn’t your son help?
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: He’s sick.
Mrs Wong: I asked him to stay home and rest.
   -> Vi_ask_about_MrsWong_phase_1 

= That_sounds_rough
You: I’m sorry, Mrs Wong. That sounds rough.
Mrs Wong: I’m used to it by now.
-> MrsWong_phase_1_proposol

= MrsWong_phase_1_proposol
Mrs Wong: Okay, enough about me.
Mrs Wong: I haven’t seen your boyfriend in a while, how are you guys doing?
~ ChangeCamera("b3")
Thoughts: A week ago, he proposed to me, and I said yes. 
Thoughts: That was the happiest day of my life.
Thoughts: Why does this terrible thing need to happen to us...
~ ChangeCamera("Player")
* [Tell her about the proposal]
~ proposal_admit = true
You: Jason just proposed to me last week...
Mrs Wong: Woah, congratulations, Miss Lee! Wait, did you say yes? 
Mrs Wong: You seem more concerned than happy.
-> proposal_admitted
* [Don’t bring it up]
You: Jason is quite busy these days with his job. We are doing quite fine.
Mrs Wong: I see.
-> MrsWong_phase_1_laundry_coin

= proposal_admitted
~ ChangeCamera("b2")
Thoughts: If this hadn’t happened, I’d probably be in Jason’s arms right now, dreaming about our wedding...
~ ChangeCamera("Player")
* [Force a smile and say you’re happy]
You: Of course, I’ve been waiting for his proposal for months!
* [Excuse of looking concerned]
You: I said yes. Sorry, just too many things happened...
-
Mrs Wong: I see. You’ll look so beautiful in your wedding dress.
~ ChangeCamera("Player")
** [Thank her]
You: Haha, you’re too kind.
Mrs Wong: How wonderful.
** [...]
You: ...
Mrs Wong: ...
--
-> MrsWong_phase_1_laundry_coin

= MrsWong_phase_1_laundry_coin
Mrs Wong: Give me your clothes and I’ll toss them in for you.
~ ChangeCamera("b2")
Thoughts: No, she can’t touch the clothes, it has blood all over...
~ ChangeCamera("Player")
* [I will do it myself.]
You: I’ll do it myself—no need to trouble you. You must be tired.
Mrs Wong: Okay. Let me at least help you separate the colors from the whites—

- (laundry_delivery_choice)
~ ChangeCamera("Player")
* [Refuse] 
You: No, thanks—I’ll just tuck everything in one big load.
Mrs Wong: That would ruin your clothes, Miss Lee.
  ~ ChangeCamera("Player")
  ** [It will be fine.]
  You: It will be fine.
  ** [I don’t mind.]
  You: Nah, I don’t mind.
* [Make an excuse to stop her]
You: I’m short on cash, so I’ll just wash one load.
-
    -> give_money

= give_money
Mrs Wong: Sure. That comes to 80 cents in total.
~ ChangeCamera("Player")
* [Give the money]
You: Here.
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("first_laundry_coin")
Mrs Wong: Here you go. Machine Nr. 4. It’s the one on your left.
 * * [Thank you.]
 You: Thank you, Mrs Wong. 
 * * [...]
 You: ...
-
    ~ ChangeCamera("Player")
    -> END
    
= Mandy_phase_1_repeat
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [Ask which washer]
You: Which washer was it again?
Mrs Wong: Machine Nr. 4. It’s the one on your left.
~ ChangeCamera("Player")
-> END
+ [Nothing]
You: Nothing. I got this.
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
Thoughts: I shouldn’t leave the clothes alone here. 
Thoughts: I saw some detergent on the table behind the chairs.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 5 after interacting with the detergent
// =============================================================================
== Thought_about_empty_detergent ==
~ game_progression = 5
~ ChangeCamera("b1")
Thoughts: Shit, they’re out of heavy-duty detergent.
Thoughts: I need it to get the bloodstains out... 
Thoughts: I have to ask around to get some.
~ ChangeCamera("Player")
-> END


// =============================================================================
//  PHASE 6 Press E in front of Mandy, after detergent check happened 
// =============================================================================

== Mandy_story_phase_2 ==
~ game_progression = 6
{ Mandy_story_phase_2 > 1:-> ask_mandy_questions}
~ TriggerAnimation("Mandy", "doRelax")

~ ChangeCamera("Player")
* [Ask about detergent]
You: Mrs Wong, there’s no heavy-duty laundry detergent left.
Mrs Wong: Is that so? I remember I put a lot of regular detergent there, isn’t that strong enough?
~ ChangeCamera("b1")
Thoughts: I don’t want to lie to Mrs Wong, but how can I explain...
  ~ ChangeCamera("Player")
  ** [Cat peed on the clothes. (Lie)]
  ~ lied_about_cat = true
  You: It’s a bit awkward situation. My cat peed on the sheets. 
  You: We have to wash these sheets with heavy-duty laundry detergent.
  Mrs Wong: Your cat? I thought your boyfriend didn’t allow you to keep a cat? 
  -> cat_secondary_questions
  ** [I got my period. (Lie)]
  ~ lie_about_period = true
  You: You know? I’m in the time of the month. I have to wash these sheets...
  You: Only heavy-duty detergent can get it out.
  ~ TriggerAnimation("Mandy", "doTalk")
  Mrs Wong: Oh, I understand. It’s awful that we women have to go through this every month.
 -> get_detergent_in_backroom
  ** [Dodge the question] 
         You: I think heavy-duty detergent gets clothes cleaner.
         Mrs Wong: Okay.
-> get_detergent_in_backroom

    
= cat_secondary_questions
~ ChangeCamera("b4")
Thoughts: Oh God, I completely forgot I’d ever said that to her...
Thoughts: A few months ago, Jason got mad at me because I suggested we get a kitten.
Thoughts: He said it was a waste of money.
Thoughts: How can I cover up my lie now...
~ ChangeCamera("Player")
* [Convinced boyfriend. (Lie)] 
You: I convinced him because the kitty is so cute.
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: Alright then. I didn’t expect someone as stubborn as your boyfriend to actually give in.
 ~ ChangeCamera("Player")
 ** [Defend Jason]
You: He is actually very gentle to me. He just doesn’t like cats that much.
Mrs Wong: If you say so. 
* [I missspoke. (Lie)]
You: Ah, I misspoke. It’s the neighbor’s cat.
Mrs Wong: It sneaked all the way into your room? What a wild cat.
You: Yeah, pretty wild.
-
-> get_detergent_in_backroom

= get_detergent_in_backroom
Mrs Wong: The heavy-duty detergents are in the backroom. 
Mrs Wong: I’m too tired to move... Can you get it yourself?
~ ChangeCamera("Player")
* [Sure.]
You: Yes sure, no worries.
Mrs Wong: Thank you. I will rest here then.
- 
-> Ending_mandy_story_phase_2

= Ending_mandy_story_phase_2
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("backroom_key")
Mrs Wong: Here is the key to the backroom. It’s near Washer Nr. 9. 
Mrs Wong: The detergent you want is called Enzyme Laundry Detergent, the blue one on the shelf.
You: Thank you, Mrs Wong.

~ ChangeCamera("Player")
-> END

= ask_mandy_questions
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [Ask where the backroom is]
You: Where is the backroom?
Mrs Wong: The backroom is at the corner, near the washer Nr. 9. -> ask_mandy_questions
+ [Ask how the heavy-duty detergent looks like]
You: Which one is the heavy-duty detergent again?  
Mrs Wong: It’s called Enzyme Laundry Detergent, the blue one on the shelf.
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
        Drunk Cop: Can’t get enough of me, huh?
    - else:
        Drunk Cop: Can’t get enough of me, huh?
    Drunk Cop: I told you you can get detergent at the desk
    }
    ~ ChangeCamera("Player")
    -> END

    = first_time
Drunk Cop: What does a beauty like you want with me?
        ~ ChangeCamera("Player")
        * [Just detergent.]
        You: Nothing. Just some heavy-duty detergents.
        * [Asking politely for detergent]
        You: Sorry to bother you. Do you have some heavy-duty detergent by chance?
        * [End the conversation] 
        You: Nothing.
            ~ ChangeCamera("Player")
            -> END
    
    - 
    Drunk Cop: Hm, what could I get in return?
    Drunk Cop: ...How about a little kiss?
    -> LAU_story_phase_2_continue_1


    = LAU_story_phase_2_continue_1
        ~ ChangeCamera("Player")
        * [No.] 
        You: No. Do you have the detergent or not?
        Drunk Cop: Why do you need heavy-duty detergent anyways?
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [I have a boyfriend.] You: ...I have a boyfriend.
            Drunk Cop: Wow, someone is deeply in love.
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [Disgust] 
        You: Ew, shouldn’t you be with your wife and kids?
            Drunk Cop: Ha, good question. They don’t talk to me anymore.
            * * [Being sarcastic] You: I can see why.
                Drunk Cop: Woah, that was harsh.
            * * [Why?] You: Why?
                Drunk Cop: I don’t know...
            * * [Apologize] You: I’m sorry, I didn’t mean to...
                Drunk Cop: It’s okay.
            - - Drunk Cop: I didn’t even see it coming. 
            Drunk Cop: I worked so hard day and night for her and the kid, 
            Drunk Cop: but it was still not enough?
            Drunk Cop: Anyways, I’m single now, so where is my kiss?
            -> LAU_story_phase_2_continue_1

    = reason_for_detergent
        Drunk Cop: You can find detergent on the table there.
        ~ ChangeCamera("Player")
        * [No heavy-duty detergent]
        You: They don’t have heavy-duty detergent anymore.
        Drunk Cop: Why do you need heavy-duty detergent anyways?
        ~ ChangeCamera("Player")
        ** [It makes clothes cleaner (Excuse)] 
         You: I think heavy-duty detergent gets clothes cleaner and makes them smell nicer.
         Drunk Cop: Really... What a strange habit.
* [Lie] You: My cat peed on the sheets. It’s stinky as hell.
            Drunk Cop: Really? I can’t smell it.
            ~ lau_cat_pee = true
            ~ ChangeCamera("Player")
            ** [Bluff]
            You: If you really want to take a whiff of my cat’s pee, go ahead.
            Drunk Cop: Haha, no need. I believe you.
            ** [There’s something wrong with your nose.]
            You: What’s wrong with your nose?
            Drunk Cop: What? I have the best nose of the whole precinct.
        -
        Drunk Cop: Anyways, I don’t have the special detergent for you. 
        Drunk Cop: You should go ask Mrs Wong.
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
    Thoughts: The look in Mrs Wong’s eyes seemed to hold a hint of pity.
    Thoughts: The police officer grinned, as if he’d known all along.
    Thoughts: Did they all know, but were just toying with me?
    Thoughts: Had they seen the bloodstains on those clothes?
    ~ ChangeCamera("Player")
    - 2: 
    ~ ChangeCamera("b1")
    Thoughts: I’ve yet to cry. Yet to be sorrowful or to mourn. 
    Thoughts: With each step further I become less of a human. How long do I need to hide?
    Thoughts: Do I need to carry this secret with me for the rest of our life?
    ~ ChangeCamera("Player")
    - 3:
    ~ ChangeCamera("b1")
    Thoughts: This won’t bring a dead man back. Regardless of how much detergent goes in there.
    Thoughts: Saying it does not bring him back. Then nothing will give me peace.
    Thoughts: And when I’m done with this, a murder awaits me at home.
    ~ ChangeCamera("Player")
    - 4:
    ~ ChangeCamera("b2")
    Thoughts: This won’t bring him back. Regardless of how much detergent goes in there.
    Thoughts: Saying it does not bring him back.
    Thoughts: But at least then the crime would exist somewhere outside my own skull. Somewhere  where it won’t grow.
    Thoughts: Does Jason think the same?
    ~ ChangeCamera("Player")
    - else: 
    ~ ChangeCamera("b2")
    Thoughts: For how long can I keep up this facade, these lies?
    ~ ChangeCamera("Player")
}

~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 9 Directly after the innervoice, pager beeps as interruption.
// =============================================================================
== Boyfriend_pager_phase_1 ==
~ game_progression = 9
J: Has anyone seen you?
J: Play it cool, bb.
J: Don’t forget — use heavy-duty detergent.
J: Packed up. Driving to the harbor.
J: Be quick with cleaning up!!!!
J: TTYL.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 10 After pager phase for around 5 sec, if player didn’t find the detergent yet.
// =============================================================================
== Thought_about_how_detergent_looks ==
~ game_progression = 11
~ ChangeCamera("b1")
Thoughts: Mrs Wong said that the heavy-duty detergent is blue and should be somewhere on the shelf.
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
Thoughts: That’s the correct detergent. I need to put these into washer Nr. 4 as soon as possible.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 12 During the first washing mini game
// =============================================================================
== Thought_washing_clothes_1 ==
~ game_progression = 13
~ ChangeCamera("b4")
Thoughts: Jason was wearing this shirt when we met. 
Thoughts: On a rainy day, I accidentally slipped and my groceries fell to the ground.
Thoughts: He was the one who helped me pick them up. We were together two weeks later.
Thoughts: I thought we would continue living this ordinary life together, in love.
Thoughts: Who could have thought...
Thoughts: I wonder if we could ever forget what happened after we washed these.
Thoughts: Could we really put these shirts on and pretend they were never stained with blood?
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 13 after the first washing mini game, commenting that she needs another round
// =============================================================================
== Thought_about_need_another_washer ==
~ game_progression = 14
~ ChangeCamera("b2")
Thoughts: Shit. Still many clothes left. 
Thoughts: I should have thought that one round is not enough.
Thoughts: I need to get another laundry coin.
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
    Mrs Wong: You get your change now?
    ~ ChangeCamera("Player")
    + [Where is the coin change machine?]
    You: Not yet. Where is the coin change machine again?
    Mrs Wong: It’s the one in the corner.
    You: Thanks.
    ~ ChangeCamera("Player")
    -> END
}
    
= Ask_for_laundry_coin_Mandy
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
* [Ask for another laundry coin]
You: Mrs Wong, can I buy another laundry coin?
Mrs Wong: Another one? Did all your clothes fall into a pit or something?
   ~ ChangeCamera("Player")
   ** [Summer sweating excuse]
   You: You know, sweating in the summer and stuff. I can’t stand smelly clothes.
   Mrs Wong: Hmm... I remember you were here just two days ago. 
   You: Well, clothes get dirty pretty fast in summer. 
   -> Mandy_phase_3_fair
    ** {lied_about_cat} [Cat peed everywhere. (Lie)]
    You: I told you, the cat peed on our clothes, so we have to wash all the bedding and stuff, because it stinks so much...
    Mrs Wong: Naughty cat. What’s the name? -> Cat_name_question
    ** {lie_about_period} [Period got everywhere]
     You: Well... 
     You: My period is especially heavy today...
     You: And I accidentally put my clothes on the bloodstained sheets.
     -> Mandy_phase_3_fair
* [Nothing]
~ ChangeCamera("Player")
-> END

= Mandy_phase_3_fair
Mrs Wong: Why didn’t your boyfriend come and let you rest?
~ ChangeCamera("b4")
Thoughts: He’s currently dealing with the body...
~ ChangeCamera("Player")
* [I volunteered (Lie)]
You: It was me who offered to help him. He is too busy with work.
* [No time]
You: He doesn’t have time.
-
Mrs Wong: Hah, such a typical excuse.
~ ChangeCamera("Player")
** [Defend Jason]
You: He works very hard to earn money though.
--
Mrs Wong: Hmm. I have to warn you, when a man stops getting involved in household, 
Mrs Wong: it’s normally a sign that he will start neglecting your feelings.
~ ChangeCamera("b3")
Thoughts: It’s true that Jason always asks me to do housework...
Thoughts: But he works a lot to make money. 
Thoughts: And his marriage proposal was so romantic...
~ ChangeCamera("Player")
** [Jason is different.]
You: Don’t worry, my boyfriend is different.
{ proposal_admit:
    Mrs Wong: I don’t mean to scare you so soon after you just accepted his proposal.
- else:
    Mrs Wong: I’m not saying your boyfriend is just like my useless husband.
}
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: But many of them always start out romantic, and then...
Mrs Wong: My husband used to buy me jewelry and take me to the docks for stargazing every other day.
Mrs Wong: Now, he won’t even talk to me, unless he wants food or needs me to cover his shifts.
Mrs Wong: And you can see Mr. Lau there, being drunk at 3am...
Mrs Wong: I have to say, some men really are useless...
Drunk Cop: You’re speaking a bit too loudly, aren’t you?
   ~ ChangeCamera("Player")
   *** [We were talking about you]
   You: We were talking about you.
   Drunk Cop: I? I’m not useless, I’m the best cop in the whole precinct.
   *** [Not talking about you]
   You: We were not talking about you.
   Drunk Cop: ...
   ---
   Mrs Wong: Haha. 

->Mandy_phase_3_ending

= Cat_name_question
~ ChangeCamera("b1")
Thoughts: Now I just have to come up with a name for this imaginary cat...
~ ChangeCamera("Player")
* [Jason.]
You: He’s Jason.
Mrs Wong: Interesting choice to name your cat after your boyfriend, haha.
  ~ ChangeCamera("Player")
  ** [We thought it’s funny.]
  You: ... We thought it’s funny.->Mandy_phase_3_fair
  ** [Haha]
  You: ... haha
  ->Mandy_phase_3_ending
  
* [Miao Miao.]
You: Miao Miao.
* [Caesar.]
You: Caesar.
-
Mrs Wong: Cute.
->Mandy_phase_3_ending

= Mandy_phase_3_ending
~ ChangeCamera("b2")
Thoughts: I should wash them quickly, Jason will get mad if I’m too slow...
~ ChangeCamera("Player")
* [Ask Mrs Wong for paying]
You: The second washer would cost 80 cents, too, right?
Mrs Wong: Yes.
~ ChangeCamera("b4")
Thoughts: (Damn, I’d completely forgotten.
Thoughts: (Blood had splattered on our money when it happened.
~ ChangeCamera("Player")
** [Wait, I need to get some change first.]
You: Uhm, give me one second. I have to get some change.
Mrs Wong: Are you sure? I have change here.
~ ChangeCamera("b3")
Thoughts: (Mrs Wong can’t see the blood on the bill. 
Thoughts: (I need to use the coin change machine.
   ~ ChangeCamera("Player")
   *** [I got this.]
   You: No worries, I got this.
   *** [Make an excuse]
   You: It’s okay, I need some change for payphones anyways.
   -
   Mrs Wong: Sure. The coin change machine is in the corner.
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
    Thoughts: Shit, this machine always has this problem. But trying again normally helps.
    ~ ChangeCamera("Player")
    - 2:
    ~ ChangeCamera("b2")
    Thoughts: Again? It needs to work...
    ~ ChangeCamera("Player")
    - else:
    ~ ChangeCamera("b3")
    Thoughts: Finally... I should go back to Mrs Wong to buy the laundry coin.
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 17 Will be triggered after the coin machine mini game
// =============================================================================    
== Boyfriend_pager_phase_2 ==
~ game_progression = 17
//pager beeps and vibrates
J: A cop stopped me.
J: I lied about the trunk.
J: Got away with a speeding ticket.
J: Hands shaking so bad I can barely drive.
J: Finally at the harbor.
J: HURRY UP WITH WASHING VIVIAN!
J: We got each other.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 18 Press E in front of Mandy, after get coins
// =============================================================================
== Mandy_story_phase_4 ==
~ game_progression = 18
~ TriggerAnimation("Mandy", "doRelax")
Mrs Wong: You get your change now?
~ ChangeCamera("Player")
* [Yes. (Pay)]
~ GiveAwayItem("change_coin_1")
~ GiveAwayItem("change_coin_2")
~ GiveAwayItem("change_coin_3")
~ GiveAwayItem("change_coin_4")
You: Yes, here.
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("second_laundry_coin")
Mrs Wong: Here you are. Washer Nr. 9.
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 19 Triggered automatically, as player drops cloth walking towards the other washer.
// =============================================================================
== LAU_story_phase_3 ==
~ game_progression = 19
~ PlayAudioClip("musicAccent_4")
Drunk Cop: There’s an awfully red stain on your clothes.
-> LAU_story_phase_3_continue_1

    = LAU_story_phase_3_continue_1
        ~ ChangeCamera("Player")
        * [Lie about an accident] 
        You: Well, my boyfriend accidentally broke a bottle and cut his hand.
        ~ lied_about_hand = true
        Drunk Cop: Uh, that must’ve hurt.
        Drunk Cop: Why is there blood all over the chest area of this shirt?
        ~ ChangeCamera("Player")
        ** [Try to cover the lie up]
        You: We tried to wrap his hand around his shirt to stop the bleeding.
        Drunk Cop: Wrap his hand around his shirt? Hahaha
        Drunk Cop: I’m starting to wonder if you’re actually more drunk than I am.
        ~ ChangeCamera("b1")
        Thoughts: Stupid misspoke...I hope he doesn’t notice that my hands are shaking.
        ~ ChangeCamera("Player")
        Drunk Cop: I hope your boyfriend is doing alright.
        ~ ChangeCamera("Player")
        *** [He is.]
        You: Yes, he is, thanks. That’s why he’s home.
        *** [Bluff]
        You: You’re the nosiest cop I’ve ever seen.
        Drunk Cop: Or the most observant.
        ---
        -> LAU_story_phase_3_continue_2
        
        ~ ChangeCamera("Player")
        * [Lie about spilled wine.] 
        You: Well, my boyfriend spilled red wine all over the place.
        ~ lied_about_wine = true
        Drunk Cop: Really... What kind of wine is that? It’s as red as blood.
        ~ ChangeCamera("b1")
        Thoughts: Damn, I know nothing about wine.
        ~ ChangeCamera("Player")
            * * [I don’t remember.] 
            You: I don’t remember. 
            You: He got this wine from the wine shop on the next street over.
            * * [Make something up]
            You: la... Toise. Something like it. 
            Drunk Cop: Interesting, never heard of that before. I thought I knew everything about wine.
            ~ ChangeCamera("Player")
            *** [It’s rare.]
            You: It’s rare, my boyfriend got it as a gift from a friend abroad.
            --
            Drunk Cop: How strange... 
            A fun fact about wine... it dries to a purple-ish color.
            Drunk Cop: Blood, on the other hand, turns dark, rust-red.
            -> LAU_story_phase_3_ending
            
        ~ ChangeCamera("Player")
        * {lie_about_period} [Lie about period.] 
        ~Cop_knows_period = true
        You: I’m... having the time of the month.
        Drunk Cop: Oh, I see. 
        Drunk Cop: But why is there blood all over the chest area of this shirt?
         ~ ChangeCamera("Player")
         ** [Try to explain]
         You: I accidentally put it in the wrong spot on the bedsheet.
         Drunk Cop: Okay... -> LAU_story_phase_3_ending 
        * [None of your business.] You: Just mind your own business.
        Drunk Cop: I’m a cop and I patrol this area: 
        Drunk Cop: Of course, I have to take care of other people’s business.
        ~ ChangeCamera("b4")
        Thoughts: I thought I could get away with this... 
        Thoughts: Now I have to come up with an excuse.
        ~ ChangeCamera("Player")
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
    Drunk Cop: But I thought he stayed at home because he’s sick?
    ~ ChangeCamera("Player")
    * [I said that]
    You: That as well.
    Drunk Cop: Cut his hand and got sick, that’s almost unrealistically tragic. Maybe you should stay home with him.
    ~ ChangeCamera("Player")
    ** [He’s sleeping.]
        You: He already fell asleep.
    ** [I want to do some housework.]
        You: He’s taking some rest, so I’d just do some housework before this here closes.
    - Drunk Cop: Okay, I thought the constant pager beeping would be from your poor boyfriend.
    ** [It was from my mom.]
        You: It was from my mom.
        -> LAU_story_phase_3_ending
    ** [No, from a friend.]
        You: No, it’s my best friend texting me. 
        You: She recently bought a pager and has been using it all the time.
        -> LAU_story_phase_3_ending
    
    = busy_reply_phase_3
    Drunk Cop: But I thought he stayed at home because he’s busy?
    ~ ChangeCamera("Player")
    * [Agree]
        You: Yes, he had some stuff to do for his work.
        -> LAU_story_phase_3_ending
    * [Busy with sleeping.]
        You: Busy with sleeping.
        -> LAU_story_phase_3_ending


    = LAU_story_phase_3_ending 
    Drunk Cop: This red stain reminds me of the crime scene I witnessed today. 
    Drunk Cop: A middle-aged man stabbed his wife to death. 
    Drunk Cop: He refused to plead guilty, so we had no choice but to put him in jail. 
    Drunk Cop: Her shirt also has this red stain...
    ~ ChangeCamera("Player")
    * {lied_about_wine} [Bluff]
    You: There’s no need to overthink it. 
    You: The bottle of wine my boyfriend spilled might just have been made differently.
    * {lied_about_wine} [...]
    * {lied_about_hand} [Tell him that he was overthinking]
    You: Why do you need to overthink so much? 
    You: Isn’t cutting your hand accidentally quite normal?
    * {lied_about_hand} [Show that you’re scared of the crime]
    You: ...Sounds scary.
    * {Cop_knows_period} [Tell him that he was overthinking]
    You: There’s no need to overthink it. 
    -
    Drunk Cop: All right then... I hope you’re being honest. 
    Drunk Cop: You do know what happens if you lie to a police officer, don’t you?
    ~ ChangeCamera("Player")
    * [Silence]
    You: ...
    Drunk Cop: Haha. I was just joking around. Go wash your clothes.
    ~ ChangeCamera("Player")
    -> END
    
    * [I won’t lie.]
    You: Of course I won’t lie to you.
    Drunk Cop: Sweet girl. 
    Drunk Cop: But if you’re hiding anything, you’d better tell me soon.
    Drunk Cop: Go wash your clothes first.
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
Thoughts: Before that person came in, Jason was holding me tightly.
Thoughts: He was wearing this. It was so warm and I felt so safe in his arm.
Thoughts: Why did we.. why did Jason do that.
~ ChangeCamera("Player")
Thoughts: I can’t forget the dead man’s open eyes...
~ ChangeCamera("b3")
Thoughts: I can’t forget the smell of blood when Jason held me after it happened.
Thoughts: He rocked me back and forth, as if I were trapped in a cradle...
Thoughts: How long do I need to hide? 
Thoughts: Maybe it’s okay to give up...
Thoughts: I suddenly felt a strange sense of relief.
Thoughts: But how could I possibly abandon the one I love so deeply,
Thoughts: the one who protected me and even killed someone to keep me safe?
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 21 after the second washing clothes mini game, as the lights turned off.
// =============================================================================
== Chaos_blackout ==
~ game_progression = 21
~ black_out_happened = true
~ SetBlackout(1)
Drunk Cop: Ah, what the hell is this?
Mrs Wong: Not again... I need to check the circuit box in the back room...
~ ChangeCamera("Player")
* [I will do it.]
You: I can do it. I’m standing next to it.
Mrs Wong: Thank you, Miss Lee.
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
Thoughts: Why did this happen...
Thoughts: Is it my destiny that I can never wash away our crime?
Thoughts: Should I just give up? But I got nowhere to hide...
Thoughts: Should I go back? But I’ll always be living in the shadow of this murder.
Thoughts: Should I betray Jason and report the murder? No, I can’t betray him.
Thoughts: He will get angry, he will hate me.
~ ChangeCamera("Player")

    - 2: 
    ~ ChangeCamera("b3")
    Thoughts: The cop grins like he already knows.
    Thoughts: Knows and toys with me, maybe he does, and maybe that’s a relief I won’t admit to.
    Thoughts: To be caught would at least mean the end of this act.
    ~ ChangeCamera("Player")
    - else:
    ~ ChangeCamera("b1")
    Thoughts: I don’t care about karma anymore.
    Thoughts: I wish someone could be by my side...
    Thoughts: Should I give up? But I got nowhere to hide...
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END


    
// =============================================================================
//  PHASE 23 After the inner voice phase
// =============================================================================
== Boyfriend_pager_phase_3 ==
~ game_progression = 23
J: It’s done.
J: Rocks in the body bag. 
J: Tossed the whole thing in the ocean.
J: Why are you so slow?
J: Hurry up!!!
J: You don’t want our lives ruined, right?
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 24 after boyfriend pager ( now it’s automatically switched in ink) (player can’t leave the backroom without light switching back on)
// =============================================================================
== How_to_turn_on_circuit_box ==
~ game_progression = 24
    ~ ChangeCamera("b4")
    Thoughts: I should turn on the power. 
    Thoughts: The circuit box should be on the wall.
    Thoughts: I guess I need to turn on the biggest switch there?
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 25 If the player attempts to leave the backroom while the power blackout is still there
// =============================================================================
== Attempt_leaving_backroom ==
~ game_progression = 25
    ~ ChangeCamera("b1")
    Thoughts: I can’t leave yet. I need to turn on the power.
    Thoughts: The circuit box should be on the wall.
    Thoughts: I guess I need to turn on the biggest switch there?
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 26 After player comes out of the backroom, mandy stands in front of the door and automatically start the conversation
// =============================================================================
== Mandy_smoking_scene_1 ==
~ game_progression = 26
~ TriggerAnimation("Mandy", "doRelax")
Mrs Wong: Is everything okay back there? You’ve been gone quite a while, Miss Lee.
~ ChangeCamera("Player")
* [Make an excuse]
You: Sorry, I couldn’t find the correct switch for the lights.
* [Sorry.]
You: I’m really sorry.
-
Mrs Wong: No need to apologize. Want a cigarette?

- (smoke_choice)
   ~ ChangeCamera("Player")
   * [No, thanks.]
   You: No, thank you.
   -
   Mrs Wong: Mind if we talk for a second? Just between us women.
      ~ ChangeCamera("Player")
      ** [What’s it about?]
      You: Sure. What’s it about?
      ** [Not sure.]
      You: I’m not sure...
      Mrs Wong: I’m just worried about you.
      - -
      Mrs Wong: Someone’s been paging you all the time, right? Is that your boyfriend?
         ~ ChangeCamera("Player")
         *** [Yes. (Make an excuse)]
         You: Yes, he just worried because it’s late.
         *** [Deny]
         You: No...
         - - -
         ~ TriggerAnimation("Mandy", "doTalk")
         Mrs Wong: You can be completely honest with me, Miss Lee.
         Mrs Wong: Every time you come out from the backroom like a ghost, and...
         Mrs Wong: There’s more red in your clothes than on your face.
         -> Mandy_smoking_scene_2


// =============================================================================
//  PHASE 27 If the player not admit at first, but come to her again by pressing E, and can choose to admit again
// =============================================================================
== Mandy_smoking_scene_2 ==
~ game_progression = 27
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [Tell her about the murder]
You: Mrs Wong, I’ve done something bad...
-> Admit_to_Mandy

+ (dont_tell) [Don’t tell her]
  {dont_tell:
  - 1:
  You: Mrs Wong, nothing happened.
  Mrs Wong: Then I must have been overthinking it. 
  Mrs Wong: I’m going to have a smoke here for a bit. 
  Mrs Wong: If you want to come talk to me later, feel free to stop by again.
  - else: 
  You: Nothing.
  Mrs Wong: Okay.
  Mrs Wong: I’m going to have a smoke here for a bit. 
  Mrs Wong: If you want to come talk to me later, feel free to stop by again.
  }
~ ChangeCamera("Player")
-> END

= Admit_to_Mandy
Mrs Wong: Calm down and take a deep breath. I’m here.
~ ChangeCamera("Player")
* [Try to calm down and explain]
You: Someone broke in... 
You: Jason... he... everything happened so fast. 
You: he told me if the blood was washed away, everything would go back to normal...
Mrs Wong: My goodness, Miss Lee...
Mrs Wong: Have you been lying all the time about the clothes?
~ ChangeCamera("Player")
** [Apologise]
You: I’m sorry, I was panicking.
Mrs Wong: (Sigh)
Mrs Wong: I can’t believe it. I should have known he was that kind of guy. 
~ ChangeCamera("Player")
*** [Jason is different]
You: But he loves me! He was protecting me, Mrs Wong... 
Mrs Wong: Did he do that for you, or for his own safety? 
Mrs Wong: But what are you going to do? 
Mrs Wong: They’ll find out sooner or later.
~ ChangeCamera("Player")
**** [I don’t know.]
You: I don’t know, Mrs Wong...
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: ...My cousin lives in Tou San. 
Mrs Wong: You can go there, but you’ll have to leave everything behind -
Mrs Wong: Including your boyfriend - and try to make a living there. 
Mrs Wong: I can’t guarantee it’s safe there, or that no one will find you.
Mrs Wong: But it’s better than continuing to hide here.
Mrs Wong: Think about it, Miss Lee. Take your time. 
-> Mandy_smoking_scene_3

// =============================================================================
//  PHASE 28 If the player admitted, but not accept her help yet.
// =============================================================================
== Mandy_smoking_scene_3 ==
~ TriggerAnimation("Mandy", "doRelax")
- (final_choices_mandy)
~ ChangeCamera("Player")
* [What is Tou San like?]
You: What is Tou San like?
Mrs Wong: My cousin said it’s a county town, not as bustling as here. 
Mrs Wong: If you put in some effort, you can still find a good way to make a living there. -> final_choices_mandy
+ [Accept help and escape (Ending)]-> Mandy_escape_ending
+ [Take time to think (You can come back later)]
You: I don’t know... I need to think.
Mrs Wong: Take your time. I’ll be here smoking for a while. 
Mrs Wong: you can come back to me whenever you’re ready.
~ game_progression = 28
~ ChangeCamera("Player")
-> END

= Mandy_escape_ending
~ ChangeCamera("b4")
Thoughts: It’s time to live on my own.
Thoughts: I don’t need to listen to Jason anymore,
Thoughts: and I don’t need to obey him all the time.
Thoughts: I have to escape this life. And escape him.
~ ChangeCamera("Player")
You: So, how can I go to Tou San?
Mrs Wong: There’s a ferry to go there every morning at 8:00. 
Mrs Wong: You need to head to the harbor now. 
Mrs Wong: So leave this man, start a new life.
~ ChangeCamera("Player")
* [Thank her]
You: Thank you, Mrs Wong...
~ TriggerAnimation("Mandy", "doTalk")
Mrs Wong: You can just call me Mandy. 
Mandy: I’ll send a message to my cousin Cindy to pick you up. 
Mandy: Also, Vivian... be independent. 
Mandy: That’s the most precious thing a woman can have. 
Mandy: Don’t rely on any men for your life and happiness.
 ~ ChangeCamera("Player")
 ** [Promise]
You: I promise you, Mandy.
Mandy: Goodbye, Vivian.
  ~ ChangeCamera("Player")
  *** [Goodbye. (Leave)]
You: Goodbye, Mandy. 
-
~ game_progression = 28
~ PlayEndingCutscene(1)
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 29 After you talk to mandy, you will recieve a message from your boyfriend.
// =============================================================================
== Boyfriend_pager_ending ==
~ game_progression = 29
J: Just arrived home. 
J: Write me as soon as you’re coming home.
~ ChangeCamera("Player")
-> END


// =============================================================================
//  PHASE 30 After mandy talks to you, if player goes to Lau and press E.
// =============================================================================
== Lau_confess_ending ==
Drunk Cop: What’s that face for, sweet heart? You look like you just saw a ghost.
~ ChangeCamera("Player")
+ [Nothing.]
You: I’m just tired.
~ ChangeCamera("Player")
-> END
* [Report boyfriend’s murder]
You: I want to report a murder.
Police Officer: Say that again, miss. What happened? 
-> murder_confess
= murder_confess
You: Someone broke in and threatened us. 
You: And my boyfriend... My boyfriend took a {kitchen_knife: kitchen knife}{gun_chosen: gun.}
You: It all happened so fast...
Police Officer: Were you a part of this?
  ~ ChangeCamera("Player")
  ** [I gave it to him.]
  You: He asked me to get a {kitchen_knife: knife}{gun_chosen: gun} for him when the intruder broke in. 
  You: And then asked me to wash these clothes.
  ** [No, I’m not part of this.]
  You: I didn’t touch the {kitchen_knife: knife}{gun_chosen: gun}. He asked me to wash those clothes, 
  You: I didn’t know what to do so I came here.
  -
  Police Officer: Where is the suspect right now?
  -(answer_suspect)
  ~ ChangeCamera("Player")
  *** [Say address]
  You: He’s at our apartment. 4th Floor, Block 3, 32 Lin Faa Street.
  *** [...]
  Police Officer: Miss, please cooperate.-> answer_suspect
  ---
  Police Officer: What’s your name?
  ~ ChangeCamera("Player")
  **** [Say your name]
  You: Vivian Lee.
  Police Officer: And his name?
  ~ ChangeCamera("Player")
  ***** [Say his name]
  You: Jason Ho.
  
  Police Officer: ...Miss, you did the right thing. 
  Police Officer: I knew something was fishy, but I’m glad you were the first to tell me.
  Police Officer: Otherwise, the crime of harboring a murderer is very serious.
  Police Officer: It took courage to confess and report a crime of your lover.
  Police Officer: Now, please come with me.
  ~ game_progression = 30
  ~ PlayEndingCutscene(2)
  ~ ChangeCamera("Player")
  -> END

// =============================================================================
//  PHASE 31 Standard dialogue after choosing to complete the mission on the pager.
// =============================================================================
== Boyfriend_ending_dialogue_final ==
~ game_progression = 31
You: I’m done.
J: I know that I can trust you, sweetheart.
J: Come back home. Can’t wait to cuddle.
J: Sorry this happened.
J: Things will get better. 
J: You’ve got me.
J: We should go on a trip. 
J: Setting off tomorrow.
J: Road trip for two, hh.
J: The night will pass.
J: We still have tomorrow.
~ ChangeCamera("Player")
-> END

//test
== Arrested_final ==
Police Officer: I think you should know why I’m standing here.
~ ChangeCamera("Player")
* [Try to get away with it]
* [Confess]
-
~ ChangeCamera("Player")
-> END
