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

//story phase nubmer
VAR story_phase = 1


// Unity (GlobalVariableOperator) syncs and stores this across dialogues.
VAR game_progression = 0

//story variables
VAR mahjong_mentioned = false
VAR kitchen_knife = false
VAR gun_chosen = false
VAR has_detergent = false
VAR lied_about_cat = false
VAR black_out_happened = false

VAR did_insult = false
VAR told_lie_sick = false
VAR told_lie_busy = false

VAR lied_about_wine = false
VAR lied_about_hand = false
VAR coin_machine_attempt = 0

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
On a sultry midnight in Lam Tong City. We were just about to go to sleep after a tiring day.
-> intro_intruder

= intro_intruder
But that peace was suddenly shattered by a <>
* loan shark
* gangster
* robber
-<>.

// change page
He tore through our home, destroyed what we'd built, searching for anything of value. My boyfriend said there was only one way out. He turned to me and asked for the 
* kitchen knife.
    ~ kitchen_knife = true
* gun.
    ~ gun_chosen = true
-

// change page
Silence returned to the night. Blood seeped deep into the carpet. One issue resolved. An even worse one arose. Covered in blood was
* his shirt.
* our bed sheet.
* my favorite dress.
-

// change page
He held me with his bloodied hands, telling me that as long as the blood was washed off our clothes, everything would be all right.
And so here I am, at 3 a.m., trying to wash away the crime we committed.
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
    * [(Greeting)] 
        You: Good Evening.
    * [(Continue)] 
        You: I'm just here for the laundry.
    * [(Insult)] 
        You: Fuck off.
        ~ did_insult = true
        Drunk Man: Woah, take it easy, young lady. I was just joking around.
       -
       -> flattery -> clothes_question

= flattery
Drunk Man: You're lucky to have me here, you know. 
Drunk Man: I'm a cop — no one would dare to harass a beautiful young lady like yourself in front of a police officer!
Drunk Cop: No villains can slip through my fingers.
-> clothes_question

= clothes_question
Drunk Cop: Whose clothes are you washing in the middle of the night?
    * [(Silence)] 
        You: ...
        -> silence_branch
    * [For my boyfriend.] 
        You: Just laundry for my boyfriend, Officer.
        Drunk Cop: Of course, pretty girls always have boyfriends.
        -> boyfriend_excuse
    * [None of your bussiness.] 
        You: It's none of your business.
        { did_insult:
        Drunk Cop: Ooh, pretty lady has some secrets.
        -> silence_branch
        - else:
        Drunk Man: Woah, take it easy, young lady. 
        }
        -> clothes_question

= silence_branch
Drunk Cop: Let me guess. 
Drunk Cop: You just broke up with your boyfriend, and now you're washing away the scent of him.
    * [That's your case, right?] 
        You: That's why you're here, right?
        Drunk Cop: ...
        Drunk Cop: I just can't sleep being reminded of her.
        * * [(Apologize)] 
            You: I'm sorry.
        * * [...] 
            You: ...
        - - -> ending
    * [(Deny)] 
        You: Why would I break up with him? He's so nice to me.
        -> boyfriend_excuse

= boyfriend_excuse
Drunk Cop: How touching. 
Drunk Cop: But he couldn't be bothered to accompany you at this hour?
    * [He's sick (Lie)] 
        You: He's sick and he needs his clothes for work tomorrow.
        ~ told_lie_sick = true
        Drunk Cop: Sick but still goes to work? Huh, that's how I lost my wife.
    * [He's busy tonight and I have nothing to do.] 
        You: He's busy tonight, and since I have nothing to do at home anyway, 
        You: I might as well help him with some chores.
        ~ told_lie_busy = true
        Drunk Cop: Busy at this hour, huh? 
        Drunk Cop: Are you sure he's not up to no good?
- 
* [Can't stop him.]
You: Well, I can't stop him from working.
* [(Excuse)]
You: ...He's a hardworking guy.
-
Drunk Cop: Fair. 
Drunk Cop: Your boyfriend is lucky to have someone like you to wash his clothes.
-> ending

= ending
Drunk Cop: Go on and wash your clothes then, young lady.
-> END

= repeat_visit
Drunk Cop: All good?
    + [Nothing.]
        You: ..
    + [Ask which machine]
        You: Which machine did I put my clothes in again?
        Drunk Cop: Hmmm... Nubmer four?
-
-> END

// =============================================================================
//  PHASE 2 getting into the laudromat
// =============================================================================
== Mandy_story_phase_1 ==
~ game_progression = 2
{ Mandy_story_phase_1 > 1: -> Mandy_phase_1_repeat }
~ TriggerAnimation("Mandy", "doRelax")
Mrs. Wong: Hey!
Mrs. Wong: Isn't that Miss Lee! Doing laundry at this hour?

- (You_intro_choice)
* [Cant't sleep] 
    You: Uh, yeah. I just couldn't sleep.
    Mrs. Wong: Poor girl. Is everything alright?
    -> MrsWong_ask_alright

* [... (Stay silent)] 
    You: ...
    Mrs. Wong: Why aren't you saying anything? Is everything alright?
    -> MrsWong_ask_alright

* [I really need to wash these.] 
    You: I have some clothes I really need for tomorrow.
    Mrs. Wong: What's the hurry? 
        * * [My boyfriend needs them for work.] 
            You: My boyfriend needs to wear these tomorrow at work.
        * * [My boyfriend told me to.] 
            You: My boyfriend told me to wash these because he needs them tomorrow.
        - 
        -> MrsWong_boyfriend_clothes
  = MrsWong_ask_alright 
* [Yeah, I'm fine.(lie)]
You: Yeah, I'm fine.
* [Sorry, just had a long day.]
You: Sorry, just had a long day.
-
Mrs. Wong: You look white as a sheet. You sure you're okay?
* [(Make an excuse)]
-> MrsWong_comment_sleep
*[Yes. (Lie)]
You: Yes. Just need to wash these for my boyfriend.
-> MrsWong_boyfriend_clothes


= MrsWong_comment_sleep
You: It's just too hot to fall asleep in this weather. 
Mrs. Wong: Fair. I'm the opposite, I'd fall asleep as soon as my head hit the pillow, if I don't need to be here.
-> Vi_ask_about_MrsWong_phase_1

= MrsWong_boyfriend_clothes
~ mahjong_mentioned = true
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: And my husband is playing Mahjong somewhere again, leaving me to run this place overnight. It's a wonder what we women put up with. 
-> Vivian_question_loop

= Vi_ask_about_MrsWong_phase_1
- (Vivian_question_loop)

*  {mahjong_mentioned == false} [Is everything alright?]
    You: Is everything alright?
    Mrs. Wong: You could say so.
    Mrs. Wong: My husband is off playing Mahjong again, someone has to run this place.
    -> Vivian_question_loop

* [Couldn't your son help?]
    -> MrsWong_son_situation 

+ [That sounds rough.]
    You: That sounds rough.
    Mrs. Wong: I'm used to it by now.
    -> MrsWong_phase_1_laundry_coin

= MrsWong_son_situation
You: Phew. Couldn't your son help?
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: He's sick.
Mrs. Wong: He had a fever this morning, but thank God it finally went down tonight.
Mrs. Wong: I asked him to stay home and rest.
   -> Vi_ask_about_MrsWong_phase_1 

= That_sounds_rough
You: I'm sorry, Mrs. Wong. That sounds rough.
Mrs. Wong: I'm used to it by now.
-> MrsWong_phase_1_laundry_coin

= MrsWong_phase_1_laundry_coin
Mrs. Wong: Okay, enough about me.
Mrs. Wong: Give me your clothes and I'll toss them in for you.
*[I will do it myself.]
You: I'll do it myself—no need to trouble you. You must be tired.
Mrs. Wong: Okay. Let me at least help you separate the colors from the whites—

- (laundry_delivery_choice)
* [(Refuse)] 
You: No, thanks—I'll just tuck everything in one big load.
Mrs. Wong: That would ruin your clothes, Miss Lee.
  ** [It will be fine.]
  You: It will be fine.
  ** [I don't mind.]
  You: Nah, I don't mind.
* [(Make an excuse to stop her)]
You: I'm short on cash, so I'll just wash one load.
-
    -> give_money

= give_money
Mrs. Wong: Sure. That comes to 80 cents in total.
* [(Give the money)]
You: Here.
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("first_laundry_coin")
Mrs. Wong: Here you go. Machine Nr. 4. It's the one on your left.
 * *[Thank you.]
 You: Thank you, Mrs. Wong. 
 * *[...]
 You: ...
-
    -> END
    
= Mandy_phase_1_repeat
~ TriggerAnimation("Mandy", "doRelax")
+ [(Ask which washer)]
You: Which washer was it again?
Mrs. Wong: Machine Nr. 4. It's the one on your left. -> END
+ [(Nothing)]
You: Nothing. I got this. -> END

// =============================================================================
//  PHASE 3 Asking LAU about detergent
// =============================================================================

// Will be triggered if the player is asking for the detergent



// =============================================================================
//  PHASE 4 after interacting with the washer nr. 4
// =============================================================================

== Thought_about_not_leaving_clothes ==
~ game_progression = 4
Thoughts: I shouldn't leave the clothes alone here. 
Thoughts: I saw some detergent on the table behind the chairs.
-> END

// =============================================================================
//  PHASE 5 after interacting with the detergent
// =============================================================================
== Thought_about_empty_detergent ==
~ game_progression = 5
Thoughts: Shit, they're out of heavy-duty detergent.
Thoughts: I need it to get the bloodstains out... 
Thoughts: I have to ask around to get some.
-> END


// =============================================================================
//  PHASE 6 Press E in front of Mandy, after detergent check happened 
// =============================================================================

== Mandy_story_phase_2 ==
~ game_progression = 6
{ Mandy_story_phase_2 > 1:-> ask_mandy_questions}
~ TriggerAnimation("Mandy", "doRelax")

*[(Ask about detergent)]
You: Mrs. Wong, there's no heavy-duty laundry detergent left.
Mrs. Wong: Is that so? I remember I put a lot of regular detergent there, isn't that enough?
  **[Cat peed on the clothes. (lie)]
  ~ lied_about_cat = true
  You: Our cat peed on my boyfriend's clothes. Only heavy-duty detergent can get it out.
  Mrs. Wong: Your cat? I thought your boyfriend didn't allow you to keep a cat? -> cat_secondary_questions
  **[I got my period. (lie)]
  You: You know? I'm in the time of the month. I have to wash these sheets...
  You: Only heavy-duty detergent can get it out.
  ~ TriggerAnimation("Mandy", "doTalk")
  Mrs. Wong: Oh, I understand. It's awful that we women have to go through this every month.
  Mrs. Wong: The heavy-duty detergents are in the backroom. Let me get them for you.
  -> Vi_insist_go_to_backroom

    
= cat_secondary_questions
* [Convinced boyfriend. (Lie)] 
You: I convinced him because the kitty is so cute. But he pees everywhere...
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: Alright then. I never expected someone like your boyfriend to actually compromise.
You: He is actually very gentle to me. He just doesn't like cats that much.
Mrs. Wong: If you say so. 
*[I missspoke. (lie)]
You: Ah, I misspoke. It's the neighbor's cat.
Mrs. Wong: It sneaked all the way into your room? What a wild cat.
You: Yeah, pretty wild.
-
-> Ending_mandy_story_phase_2

= Vi_insist_go_to_backroom
*[I will get them.]
You: No, I can just get it myself. I might need a whole bottle though.
Mrs. Wong: Sure...
*[No need to bother you.]
You: No need to bother you, I can just get it myself.
Mrs. Wong: I will rest here then.
- 
-> Ending_mandy_story_phase_2

= Ending_mandy_story_phase_2
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("backroom_key")
Mrs. Wong: Here is the key to the backroom. It's near Washer Nr. X. 
Mrs. Wong: The detergent you want is called Enzyme Laundry Detergent, the blue one on the shelf.
You: Thank you, Mrs. Wong.

-> END

= ask_mandy_questions
~ TriggerAnimation("Mandy", "doRelax")
+ [(Ask where the backroom is)]
You: Where is the backroom?
Mrs. Wong: The backroom is at the corner, near the washer Nr. X. -> ask_mandy_questions
+ [(Ask how the heavy-duty detergent look like)]
You: Which one is the heavy-duty detergent again?  
Mrs. Wong: It's called Enzyme Laundry Detergent, the blue one on the shelf.
-> ask_mandy_questions
+ [Nothing.] 
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
        Drunk Cop: Can't get enough of me, huh?
    - else:
        Drunk Cop: Can't get enough of me, huh?
    Drunk Cop: I told you you can get detergent at the desk
    }
    -> END

    = first_time
Drunk Cop: What does a beauty like you want with me?
        * [Just detergent.]
        You: Nothing. Just some heavy-duty detergents.
        * [(Asking politely for detergent)]
        You: Sorry to bother you. Do you have some heavy duty detergent by chance?
        * [(End the conversation)] 
        You: Nothing.
            -> END
    
    - 
    Drunk Cop: Hm, what could I get in return?
    Drunk Cop: …How about a little kiss?
    -> LAU_story_phase_2_continue_1


    = LAU_story_phase_2_continue_1
        * [No.] 
        You: No. Do you have the detergent or not?
        Drunk Cop: Why do you need heavy duty detergent anyways?
            ->reason_for_detergent
            -> END
            
        * [I have a boyfriend.] You: ...I have a boyfriend.
            Drunk Cop: Wow, someone is deeply in love.
            Drunk Cop: ...Why do you need heavy duty detergent anyways?
            ->reason_for_detergent
            -> END
            
        * [(Disgust)] 
        You: Ew, shouldn't you be with your wife and kids?
            Drunk Cop: Ha, good question. They don't talk to me anymore.
            * * [(Being sarcastic)] You: I can see why.
                Drunk Cop: Woah, that was harsh.
                But <>
            * * [Why?] You: Why?
                Drunk Cop: I don't know...
            * * [(Apologize)] You: I'm sorry, I didn't mean to...
                Drunk Cop: It's okay.
                But <>
            - - Drunk Cop: I didn't even see it coming. 
            Drunk Cop: I worked so hard day and night for her and the kid, 
            Drunk Cop: but it was still not enough?
            Drunk Cop: Anyways, I'm single now, so where is my kiss?
            -> LAU_story_phase_2_continue_1

    = reason_for_detergent
        * [It makes clothes cleaner (excuse)] You: I think heavy-duty detergent gets clothes cleaner and make them smell nicer.
            Drunk Cop: Really... What a strange habit.
        * [(Lie)] You: My cat peed on the sheets. It's stinky as hell.
            Drunk Cop: Really? I can't smell it.
            ** [(Bluff)]
            You: If you really want to take a whiff of my cat's pee, go ahead.
            Drunk Cop: Haha, no need. I believe you.
            ** [There's something wrong with your nose.]
            You: What's wrong with your nose?
            Drunk Cop: What? I have the best nose of the whole precinct.
        - Drunk Cop: Anyways, I don't have the special detergent for you. 
        Drunk Cop: You should go ask Mrs. Wong.
    -> END
    
    
// =============================================================================
//  PHASE 8 Triggered when you enter the backroom, and during that you can't go out. 
// =============================================================================
== Inner_voice_backroom_phase_1 ==
{ black_out_happened: 
    -> Inner_voice_phase_2 
}
~ game_progression = 8
{ Inner_voice_backroom_phase_1:
    - 1: 
    Thoughts: The police officer grinned, as if he'd known all along.
    Thoughts: The look in Mrs. Wong's eyes seemed to hold a hint of pity.
    Thoughts: Did they all know, but were just toying with me?
    Thoughts: Had they seen the bloodstains on those clothes?
    - 2: 
    Thoughts: I've yet to cry. Yet to be sorrowful or to mourn. 
    Thoughts: With each step further I become less of a human. How long do I need to hide?
    Thoughts: Do I need to carry this secret with me for the rest of our life?
    - 3:
    Thoughts: This won't bring a dead man back. Regardless of how much detergent goes in there.
    Thoughts: Saying it does not bring him back. Then nothing will give me peace.
    Thoughts: And when I'm done with this, a murder awaits me at home.
    - 4:
    Thoughts: This won't bring him back. Regardless of how much detergent goes in there.
    Thoughts: Saying it does not bring him back.
    Thoughts: But at least then the crime would exist somewhere outside my own skull. Somewhere  where it won't grow.
    Thoughts: Does Jason think the same?
    - else: 
    Thoughts: For how long can I keep up this facade, these lies?
}

-> END

// =============================================================================
//  PHASE 9 Directly after the innervoice, pager beeps as interruption.
// =============================================================================
== Boyfriend_pager_phase_1 ==
~ game_progression = 9
J: Has anyone seen you?
J: Play it cool, bb.
J: Don't forget — use heavy-duty detergent.
J: Packed up. Driving to the harbor.
J: Be quick with cleaning up!
J: TTYL.
-> END

// =============================================================================
//  PHASE 10 After pager phase for around 5 sec, if player didn't find the detergent yet.
// =============================================================================
== Thought_about_how_detergent_looks ==
~ game_progression = 11
Thoughts: Mrs. Wong said that the heavy-duty detergent is blue and should be somewhere on the shelf.
-> END

// =============================================================================
//  PHASE 11 After player got the detergent.
// =============================================================================
== Thought_about_got_right_detergent ==
~ game_progression = 12
~ has_detergent = true
Thoughts: That's the correct detergent. I need to put these into washer Nr. 4 as soon as possible.
-> END

// =============================================================================
//  PHASE 12 During the first washing mini game
// =============================================================================
== Thought_washing_clothes_1 ==
~ game_progression = 13
Thoughts: Jason was wearing this shirt when we met. 
Thoughts: On a rainy day, I accidentally slipped and my groceries fell to the ground.
Thoughts: He was the one who helped me pick them up. We were together two weeks later.
Thoughts: I thought we would continue living this ordinary life together, in love.
Thoughts: Who could have thought...
Thoughts: I wonder if we could ever forget what happened after we washed these.
Thoughts: Could we really put these shirts on and pretend they were never stained with blood?
-> END

// =============================================================================
//  PHASE 13 after the first washing mini game, commenting that she needs another round
// =============================================================================
== Thought_about_need_another_washer ==
~ game_progression = 14
Thoughts: Shit. Still many clothes left. 
Thoughts: I should have thought that one round is not enough.
Thoughts: I need to buy another laundry coin from Mrs. Wong.
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
    Mrs. Wong: You get your change now?
    + [Where is the coin change machine?]
    You: Not yet. Where is the coin change machine again?
    Mrs. Wong: It's the one in the corner.
    You: Thanks.
    -> END
}
    
= Ask_for_laundry_coin_Mandy
~ TriggerAnimation("Mandy", "doRelax")
*[(Ask for another laundry coin)]
You: Mrs. Wong, can I buy another laundry coin?
Mrs. Wong: Another one? Did all your clothes fall into a pit or something?
   **[(Summer sweating excuse)]
   You: You know, sweating in the summer and stuff. I can't stand smelly clothes.
   Mrs. Wong: Hmm... I remember you were here just two days ago. 
   You: Well, clothes get dirty pretty fast in summer. 
   -> Mandy_phase_3_fair
    **{lied_about_cat} [Cat peed everywhere.(lie)]
    You: I told you, the cat peed on our clothes, so we have to wash all the bedding and stuff, because it stinks so much...
    Mrs. Wong: Naughty cat. What's the name? -> Cat_name_question
       
= Mandy_phase_3_fair
Mrs. Wong: Why didn't your boyfriend come and let you rest?
*[(I volunteered) (lie)]
You: It was me who offered to help him. He is too busy with work.
*[(No time)]
You: He doesn't have time.
-
Mrs. Wong: Hah, such a typical excuse.
**[Someone has to take care of the housework]
You: Well, someone needs to wash these clothes.
**[He is hardworking]
You: He works very hard to earn money though.
--
You: He promised  me that we will go on a trip after.
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: They always start out romantic.
Mrs. Wong: My husband used to buy me jewelry and take me to the docks for stargazing every other day.
Mrs. Wong: Now, he won't even talk to me, unless he wants food or needs me to cover his shifts.
Mrs. Wong: Men really are useless sometimes.
Drunk Cop: You're speaking a bit too loudly, aren't you?
   ***[We were talking about you]
   You: We were talking about you.
   ***[Not talking about you]
   You: We were talking about you.
   ---
   Drunk Cop: ...
   Mrs. Wong: Haha. 
->Mandy_phase_3_ending

= Cat_name_question
*[Jason.]
You: He's Jason.
Mrs. Wong: Interesting choice to name your cat after your boyfriend, haha.
  **[We thought it's funny.]
  You: ... We thought it's funny.->Mandy_phase_3_fair
  **[Haha]
  You: ... haha
  ->Mandy_phase_3_ending
  
*[Miao Miao.]
You: Miao Miao.
*[Caesar.]
You: Caesar.
-
Mrs. Wong: Cute.
->Mandy_phase_3_ending

= Mandy_phase_3_ending
*[(Ask Mrs. Wong for paying)]
You: The second washer would cost 80 cents, too, right?
Mrs. Wong: Yes.
Thoughts: (Fuck, I forget that we got blood on the bill. I need to use the coin change machine.)
**[Wait, I need to get some change first.]
You: Uhm, give me one second. I have to get some change.
Mrs. Wong: Are you sure? I have change here.
   ***[No worries.]
   You: No worries, I got this.
   ***[(make an excuse)]
   You: It's okay, I need some change for payphones anyways.
   -
   Mrs. Wong: Sure. The coin change machine is in the corner.
-> END

// =============================================================================
//  PHASE 15 In front of coin machine, after press (E Interact)
// =============================================================================
== Thought_about_coin_machine_1 ==
Thoughts: I just need to put it in...
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
    Thoughts: Shit, this machine always has this problem. But trying again normally helps.
    - 2:
    Thoughts: Again? It needs to work...
    - else:
    Thoughts: Finally... I should go back to Mrs. Wong to buy the laundry coin.
}
-> END

// =============================================================================
//  PHASE 17 Will be triggered after the coin machine mini game
// =============================================================================    
== Boyfriend_pager_phase_2 ==
~ game_progression = 17
//pager beeps and vibrates
J: A cop just stopped me.
J: Asked a lot. 
J: I lied about the trunk.
J: Got away with a speeding ticket.
J: Hands are shaking. I can barely drive.
J: Finally at the harbor.
J: Hurry up with washing.
J: We got each other.
-> END

// =============================================================================
//  PHASE 18 Press E in front of Mandy, after get coins
// =============================================================================
== Mandy_story_phase_4 ==
~ game_progression = 18
~ TriggerAnimation("Mandy", "doRelax")
Mrs. Wong: You get your change now?
*[Yes. (pay)]
~ GiveAwayItem("change_coin_1")
~ GiveAwayItem("change_coin_2")
~ GiveAwayItem("change_coin_3")
~ GiveAwayItem("change_coin_4")
You: Yes, here.
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("second_laundry_coin")
Mrs. Wong: Here you are. Washer Nr. 9.
-> END

// =============================================================================
//  PHASE 19 Triggered automatically, as player drops cloth walking towards the other washer.
// =============================================================================
== LAU_story_phase_3 ==
~ game_progression = 19
Drunk Cop: Awfully dirty shirt... Also terribly red.
-> LAU_story_phase_3_continue_1

    = LAU_story_phase_3_continue_1
        * [(Lie about accident)] 
        You: Well, my boyfriend accidentally broke a bottle and cut his hand.
        ~ lied_about_hand = true
        Drunk Cop: Uh, that must've hurt.
        Drunk Cop: Why is there blood all over the chest area of this shirt?
        **[(Try to explain)]
        You: We tried to stop the bleeding by wrapping his hand in this shirt.
        Drunk Cop: Poor hand. I hope your boyfriend is doing alright.
        ***[He is.]
        You: Yes, he is, thanks. That's why he's home.
        -> LAU_story_phase_3_continue_2
        
        * [(Lie about spilled wine.)] 
        You: Well, my boyfriend spilled red wine all over the place.
        ~ lied_about_wine = true
        Drunk Cop: Really... What kind of wine is that? It's as red as blood.
            * * [I don't remember.] 
            You: I don't remember. 
            You: He got this wine from the wine shop on the next street over.
            * * [(Make something up)]
            You: la... Toise. Something like it. 
            Drunk Cop: Interesting, never heard of that before. I thought I knew everything about wine.
            ***[It's rare.]
            You: It's rare, my boyfriend got it as a gift from a friend abroad.
            --
            Drunk Cop: How strange... 
            Drunk Cop: You know, regular wine turns purplish-brown when it dries, not dark red.
            -> LAU_story_phase_3_ending
        
        * [None of your bussiness.] You: Just mind your own business.
        Drunk Cop: I'm a cop and I patrol this area: 
        Drunk Cop: Of course, I have to take care of other people's business.
        -> LAU_story_phase_3_continue_1
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
    Drunk Cop: But I thought he stayed at home because he's sick?
    * [(I said that)]
    You: That as well.
    Drunk Cop: Cut his hand and got sick, that's almost unrealistically tragic. Maybe you should stay home with him.
    ** [He's sleeping.]
        You: He already fell asleep.
    ** [I want to do some housework.]
        You: He's taking some rest, so I'd just do some housework before this here closes.
    - Drunk Cop: Okay, I thought the constant pager beeping would be from your poor boyfriend.
    ** [It was from my mom.]
        You: It was from my mom.
        -> LAU_story_phase_3_ending
    ** [No, from a friend.]
        You: No, it's my best friend texting me. 
        You: She recently bought a pager and has been using it all the time.
        -> LAU_story_phase_3_ending
    
    = busy_reply_phase_3
    Drunk Cop: But I thought he stayed at home because he's busy?
    * [(Agree)]
        You: Yes, he had some stuff to do for his work.
        -> LAU_story_phase_3_ending
    * [Busy with sleeping.]
        You: Busy with sleeping.
        -> LAU_story_phase_3_ending


    = LAU_story_phase_3_ending 
    Drunk Cop: This stain reminds me of the crime scene I witnesses today. 
    Drunk Cop: A middle-aged man stabbed his wife to death. 
    Drunk Cop: He refused to plead guilty, so we had no choice but to put him in jail first. 
    Drunk Cop: Her shirt was also this red...
    *{lied_about_wine} [(Make another excuse)]
    You: Sorry, it was not wine... I'm in the time of the month. 
    You: When I was folding clothes, it happened... 
    You: I found it too awkward to admit.
    *{lied_about_wine}[(Tell he that he was overthinking)]
    You: There's no need to overthink it. 
    You: The bottle of wine my boyfriend spilled might just have been made differently.
    *{lied_about_hand}[(Accuse him of overreacting)]
    You: Why do you need to overthink so much? 
    You: Isn't cutting hand accidentally quite normal?
    *{lied_about_hand}[(Show that you're scared)]
    You: ...Sounds scary.
    -
    Drunk Cop: All right then… I hope you're being honest. 
    Drunk Cop: You do know what happens if you lie to a police officer, don't you?
    * [(Silence)]
    You: ...
    Drunk Cop: Haha. I was just joking around. Go wash your clothes.
    -> END
    
    * [I won't lie.]
    You: Of course I won't lie to you.
    Drunk Cop: Sweet girl. Go wash your clothes.
    -> END
-> END

// =============================================================================
//  PHASE 20 During the second washing mini game, x lines in total, so maybe each action a line automatically
// =============================================================================
== Thought_washing_clothes_2 ==
~ game_progression = 20
Thoughts: Before that person came in, Jason was holding me tightly.
Thoughts: He was wearing this. It was so warm and I felt so safe in his arm.
Thoughts: Then chaos broke out. A nightmare I'll never forget.
Thoughts: He asked me with despair.
Thoughts: My arms were shaking as I handed it to him.
//...
Thoughts: He did it. Then, with his blood-stained hands, he held me close once more.
Thoughts: He rocked me back and forth, as if I were trapped in a cradle…
Thoughts: Will it really be just as he said? Will everything go back to normal once the blood is washed away?
Thoughts: But why can I still smell that sickening, metallic stench?
-> END

// =============================================================================
//  PHASE 21 after the second washing clothes mini game, as the lights turned off.
// =============================================================================
== Chaos_blackout ==
~ game_progression = 21
~ black_out_happened = true
~ SetBlackout(1)
Drunk Cop: Ah, what the hell is this?
Mrs. Wong: Not again… I need to check the circuit box in the back room...
*[I will do it.]
You: I can do it. I'm standing next to it.
Mrs. Wong: Thank you, Miss Lee.
-> END

// =============================================================================
//  PHASE 22 after enter the backroom
// =============================================================================
== Inner_voice_phase_2 ==
~ game_progression = 22
{ Inner_voice_phase_2:
    - 1: 
    Thoughts: ...I wish Jason could be here with me.
    Thoughts: Right now I have only myself, spinning, spinning, with nothing to hold onto.
    Thoughts: But at least I put all of the clothes in...
    - 2: 
    Thoughts: The cop grins like he already knows.
    Thoughts: Knows and toys with me, maybe he does, and maybe that's a relief I won't admit to.
    Thoughts: To be caught would at least mean the end of this act.
    - else:
    Thoughts: I don't care about karma anymore.
    Thoughts: I wish someone could be by my side...
    Thoughts: Should I give up? But I got nowhere to hide...
}
-> END


    
// =============================================================================
//  PHASE 23 After the inner voice phase
// =============================================================================
== Boyfriend_pager_phase_3 ==
~ game_progression = 23
J: It's done.
J: Rocks in the body bag. Tossed the whole thing in the ocean.
J: Hurry up!
J: You don't want our lives ruined because you're slow, right?
 -> END

// =============================================================================
//  PHASE 24 after boyfriend pager ( now it's automatically switched in ink) (player can't leave the backroom without light switching back on)
// =============================================================================
== How_to_turn_on_circuit_box ==
~ game_progression = 24
    Thoughts: I should turn on the power. 
    Thoughts: The circuit box should be on the wall.
    Thoughts: I guess I need to turn on the biggest switch there?
    -> END

// =============================================================================
//  PHASE 25 If the player attempts to leave the backroom while the power blackout is still there
// =============================================================================
== Attempt_leaving_backroom ==
~ game_progression = 25
    Thoughts: I can't leave yet. I need to turn on the power.
    Thoughts: The circuit box should be on the wall.
    Thoughts: I guess I need to turn on the biggest switch there?
    -> END

// =============================================================================
//  PHASE 26 After player comes out of the backroom, mandy stands in front of the door and automatically start the conversation
// =============================================================================
== Mandy_smoking_scene_1 ==
~ game_progression = 26
~ TriggerAnimation("Mandy", "doRelax")
Mrs. Wong: Is everything okay back there? You've been gone quite a while, Miss Lee.
*[(Make a excuse)]
You: Sorry, I couldn't find the correct switch for the lights.
*[Sorry.]
You: I'm really sorry.
-
Mrs. Wong: No need to apologize. Want a cigarette?

- (smoke_choice)
   *[No, thanks.]
   You: No, thank you.
   -
   Mrs. Wong: Mind if we talk for a second? Just between us women.
      **[What's it about?]
      You: Sure. What's it about?
      **[Not sure.]
      You: I'm not sure...
      Mrs. Wong: I'm just worried about you.
      - -
      Mrs. Wong: Someone's been texting you all the time, right? Is that your boyfriend?
         ***[Yes. (Make an excuse)]
         You: Yes, he just worried because it's late.
         ***[(Deny)]
         You: No...
         - - -
         ~ TriggerAnimation("Mandy", "doTalk")
         Mrs. Wong: You can be completely honest with me, Miss Lee.
         Mrs. Wong: Every time you come out from the backroom like a ghost, and...
         Mrs. Wong: There's more red in your clothes than on your face.
         -> Mandy_smoking_scene_2


// =============================================================================
//  PHASE 27 If the player not admit at first, but come to her again by pressing E, and can choose to admit again
// =============================================================================
== Mandy_smoking_scene_2 ==
~ game_progression = 27
~ TriggerAnimation("Mandy", "doRelax")
+ [(Tell her about the murder)]
You: Mrs. Wong, I've done something bad...
-> Admit_to_Mandy

+ (dont_tell) [(Don't tell her)]
  {dont_tell:
  - 1:
  You: Mrs. Wong, nothing happened.
  Mrs. Wong: Then I must have been overthinking it. 
  Mrs. Wong: I'm going to have a smoke here for a bit. 
  Mrs. Wong: If you want to come talk to me later, feel free to stop by again.
  - else: 
  You: Nothing.
  Mrs. Wong: Okay.
  Mrs. Wong: I'm going to have a smoke here for a bit. 
  Mrs. Wong: If you want to come talk to me later, feel free to stop by again.
  }
-> END

= Admit_to_Mandy
Mrs. Wong: Calm down and take a deep breath. I'm here.
*[(Try to calm down and explain)]
You: Someone broke in... 
You: Jason… he… everything happened so fast. 
You: he told me if the blood was washed away, everything would go back to normal...
Mrs. Wong: Men like that always expect women to clean up their messes.
**[(He's different)]
You: But he loves me! He was protecting me, Mrs. Wong... 
Mrs. Wong: Did he do that for you, or for his own safety? 
*** [(Ask her what I should do)]
You: Mrs. Wong… What should I do?
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: ...My cousin lives in Tou San. 
Mrs. Wong: You can go there, but you'll have to leave everything behind -
Mrs. Wong: Including your boyfriend - and try to make a living there. 
Mrs. Wong: I can't guarantee it's safe there, or that no one will find you.
Mrs. Wong: But it's better than continuing to hide here.
Mrs. Wong: Think about it, Miss Lee. Take your time. 
-> Mandy_smoking_scene_3

// =============================================================================
//  PHASE 28 If the player admitted, but not accept her help yet.
// =============================================================================
== Mandy_smoking_scene_3 ==
~ TriggerAnimation("Mandy", "doRelax")
- (final_choices_mandy)
* [How is Tou San like?]
You: How is Tou San like?
Mrs. Wong: My cousin said it's a county town, not as bustling as here. 
Mrs. Wong: If you put in some effort, you can still find a good way to make a living there. -> final_choices_mandy
+ [(Accept help and escape) (Ending)]-> Mandy_escape_ending
+ [(Take time to think about it)]
You: I don't know… I need to think.
Mrs. Wong: Take your time. I'll be here smoking for a while. 
Mrs. Wong: you can come back to me whenever you're ready.
~ game_progression = 28
-> END

= Mandy_escape_ending
You: So, how can I go to Tou San?
Mrs. Wong: There's a ferry to go there every morning at 8:00. 
Mrs. Wong: You need to head to the harbor now. 
Mrs. Wong: So leave this man, start a new life.
*[(Thank her)]
You: Thank you, Mrs. Wong...
~ TriggerAnimation("Mandy", "doTalk")
Mrs. Wong: You can just call me Mandy. 
Mandy: I'll send a message to my cousin Cindy to pick you up. 
Mandy: Also, Vivian… be independent. 
Mandy: That's the most precious thing a woman can have. 
Mandy: Don't rely on any men for your life and happiness.
 **[(Promise)]
You: I promise you, Mandy
Mandy: Goodbye, Vivian.
  ***[Goodbye.(leave)]
You: Goodbye, Mandy. 
  ***[(Ask her to come with you)]
You: You should come with me, Mandy.
Mandy: It's too late for me to start over now. I have my son to take care of, and we depend on my husband's income…
   ****[(Try to convince her)]
You: Mandy… Don't say that. If it's not too late for me, it's not too late for you.
You: If I make it there, please bring your son and come to me. 
Mandy: I will visit you. Take care of yourself, Vivian...
     *****[(See you.)]
You: See you in Tou San, Mandy.
-
~ game_progression = 28
~ PlayEndingCutscene(1)
-> END

// =============================================================================
//  PHASE 29 After you talk to mandy, you will recieve a message from your boyfriend.
// =============================================================================
== Boyfriend_pager_ending ==
~ game_progression = 29
J: Just arrived home. 
J: Write me as soon as you're done.
-> END


// =============================================================================
//  PHASE 30 After mandy talks to you, if player goes to Lau and press E.
// =============================================================================
== Lau_confess_ending ==
Drunk Cop: What's that face for, sweet heart? You look like you just saw a ghost.
+ [Nothing.]
You: I'm just tired.
-> END
*[(Report boyfriend's murder)]
You: I want to report a murder.
Police Officer: Say that again, miss. What happened?
You: Someone broke in and threatened us. 
You: And my boyfriend… My boyfriend took a {kitchen_knife: kitchen knife}{gun_chosen: gun.}
You: It all happened so fast…
Police Officer: Were you a part of this?
  ** [I gave it to him.]
  You: He asked me to get a {kitchen_knife: knife}{gun_chosen: gun} for him when the intruder broke in. 
  You: And then asked me to wash these clothes.
  ** [No, I'm not part of this.]
  You: I didn't touch the {kitchen_knife: knife}{gun_chosen: gun}. He asked me to wash those clothes, 
  You: I didn't know what to do so I came here.
  -
  Police Officer: Where is the suspect right now?
  -(answer_suspect)
  ***[(Say address)]
  You: He's at our apartment. 4th Floor, Block 3, 32 Lin Faa Street.
  ***[(...)]
  Police Officer: Miss, please cooperate.-> answer_suspect
  ---
  Police Officer: What's your name?
  ****[(Say your name)]
  You: Vivian Lee.
  Police Officer: And his name?
  *****[(Say his name)]
  You: Jason Ho.
  Police Officer: ...Miss, you did a right thing. 
  Police Officer: It took courage to confess and report a crime of your lover.
  Police Officer: You chose justice and relief.
  Police Officer: I see that some color has finally returned to your face.
  Police Officer: Now, please come with me.
  ~ game_progression = 30
  ~ PlayEndingCutscene(2)
  -> END

// =============================================================================
//  PHASE 31 Standard dialogue after choosing to complete the mission on the pager.
// =============================================================================
== Boyfriend_ending_dialogue_final ==
~ game_progression = 31
You: I'm done.
J: I know that I can trust you, sweetheart.
J: Come back home. Can't wait to cuddle.
J: Sorry this happened.
J: Things will get better. 
J: You've got me.
J: We should go on a trip. 
J: Setting off tomorrow.
J: Road trip for two, hh.
J: The night will pass.
J: We still have tomorrow.
-> END
