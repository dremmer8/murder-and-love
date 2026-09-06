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
林塘市。又是一个潮湿的午夜。在疲惫的一天后，我们准备入睡。
-> intro_intruder

= intro_intruder
但这份宁静突然被打破了。破门而入的是<>
~ ChangeCamera("Player")
* 讨债的高利贷
* 我的前任
* 一位黑帮
-<>.

// change page
他闯入客厅，又摔又砸，不放过一分一毫。我男朋友说只有一条出路。他转过身来问我要了
* 一把菜刀。
* 一把水果刀。
-

// change page
~ ChangeCamera("Player")
夜晚重归寂静；鲜血浸透了地毯。一个问题看似解决了，另一个更棘手的问题随之而来。沾满鲜血的是
* 他的衬衫。
* 我们的床单。
* 我最喜欢的裙子。
-

// change page
他用沾满鲜血的双手紧紧搂住我，告诉我只要洗掉衣服上的血迹，一切都会好起来的。
于是，凌晨三点，我来到了这里，试图洗去我们犯下的罪行。
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 2 Trigger zone in front of drunk man (only once triggered)
// =============================================================================

== LAU_story_phase_1 ==
~ game_progression = 3
{ LAU_story_phase_1 > 1: -> repeat_visit }

// After talking to Mandy, then walking past Lau for the first time.
醉汉：这个点居然有美女？来找我吗？ # vo:p_2_l_1
- (Lau_choice_1)
    ~ ChangeCamera("Player")
    * [打招呼] 
        你：晚上好。 # vo:p_2_l_2
    * [继续] 
        你：我只是来洗衣服的。 # vo:p_2_l_3
    * [让他滚] 
        你：滚。 # vo:p_2_l_4
        ~ did_insult = true
        醉汉：唉，放轻松点，小姑娘。我开个玩笑不行吗。 # vo:p_2_l_5
       -
       -> flattery -> clothes_question

= flattery
醉汉：你在这里遇到我应该感到安心。 # vo:p_2_l_6
醉汉：在警察面前，没人敢对像你这样漂亮的小姑娘动手动脚！ # vo:p_2_l_7
醉警：没有哪个恶棍能从我的眼皮子底下溜走。 # vo:p_2_l_8
~did_insult = true
~ ChangeCamera("l3")
思绪：该死。我还以为他只是个普通酒鬼... # vo:p_2_l_9
~ ChangeCamera("Player")
* [...] 
你：... # vo:p_2_l_10
醉警：... # vo:p_2_l_11 -> clothes_question
* [你看起来不像警察]
你：不好意思，但你看起来一点也不像警察。 # vo:p_2_l_12
醉警：怎么，警察白天忙着调查犯罪现场， # vo:p_2_l_13
醉警：大半夜还不能来洗衣服？ # vo:p_2_l_14
** [算了]
你：当我没说。 # vo:p_2_l_15
-> clothes_question

= clothes_question
{ boyfriend_needs_clothes:
    醉警：所以呢？你男朋友让你大半夜出来洗衣服？ # vo:p_2_l_16
- else:
    醉警：你为什么大半夜跑来洗衣服？ # vo:p_2_l_17
}

-(questions_clothes)
* {not boyfriend_needs_clothes} [睡不着] 
你：我睡不着。 # vo:p_2_l_18
醉警：但你为什么要凌晨三点跑来洗衣店？ # vo:p_2_l_19
醉警：你年轻又漂亮，还有男朋友， # vo:p_2_l_20
醉警：跟我恰恰相反。 # vo:p_2_l_21
醉警：你总不可能和我一样，是跑来洗掉衣服上前妻的气味吧，哈哈。 # vo:p_2_l_22
        * * [抱歉] 
            你：我很抱歉。 # vo:p_2_l_23
            醉警：我只是想起她就睡不着。 # vo:p_2_l_24
            -> need_to_answer
        * * [...] 
            -> need_to_answer
* [谎言] 
        你：警官，我男朋友明天工作需要穿这些衣服。 # vo:p_2_l_25
        -> boyfriend_excuse
        
* { not boyfriend_needs_clothes} [为了我男朋友]
你：警官，我需要帮我男朋友洗些衣服。 # vo:p_2_l_26
-> boyfriend_excuse

* [不关你的事。] 
        你：跟你没关系。 # vo:p_2_l_27
        { did_insult:
        醉警：哦，小姑娘遮遮掩掩的。 # vo:p_2_l_28
-> need_to_answer
        - else:
        醉汉：哇，放轻松点，小姑娘。 # vo:p_2_l_29
        -> flattery
        }

= need_to_answer
{ need_to_answer > 1:
    醉警：但别给我耍花招，我可见怪不怪了。 # vo:p_2_l_30
    醉警：直接回答问题。 # vo:p_2_l_31
- else:
    醉警：但你还是得回答我的问题。 # vo:p_2_l_32
}
醉警：你为什么大半夜跑来洗衣服？ # vo:p_2_l_33
-> questions_clothes

= boyfriend_excuse
醉警：但他男朋友就懒得在这个时间陪你出来？ # vo:p_2_l_34
    ~ ChangeCamera("Player")
    * [他生病了（谎言）] 
        你：他生病了，明天工作需要穿这些衣服。 # vo:p_2_l_35
        ~ told_lie_sick = true
        醉警：生病了还要去工作？呵，我当年就是这么失去我妻子的。 # vo:p_2_l_36
    * [他今晚很忙。] 
        你：他今晚很忙，反正我在家也没事干， # vo:p_2_l_37
        你：不如顺便帮他做点家务。 # vo:p_2_l_38
        ~ told_lie_busy = true
        醉警：这个点还在忙，呵？ # vo:p_2_l_39
        醉警：你确定他没在干什么坏事？ # vo:p_2_l_40
- 
~ ChangeCamera("l2")
* [为他开脱。]
你：...他打工很辛苦的。 # vo:p_2_l_41
* [借口]
你：...打工人，没办法。 # vo:p_2_l_42
-
~ ChangeCamera("Player")
醉警：也对。钱确实很重要。 # vo:p_2_l_43
醉警：你男朋友能有你这样的女友真幸运。 # vo:p_2_l_44
-> ending

= ending
醉警：那快去洗你的衣服吧。 # vo:p_2_l_45
~ ChangeCamera("Player")
-> END

= repeat_visit
~ game_progression = 3
醉警：你还好吗？ # vo:p_2_l_46
    ~ ChangeCamera("Player")
    + [没什么。]
        你：.. # vo:p_2_l_47
    + [询问用哪个洗衣机]
        你：我刚才把衣服放进几号洗衣机来着？ # vo:p_2_l_48
        醉警：嗯... 4号？ # vo:p_2_l_49
    + [询问在哪里能找到洗衣粉]
        你：我在哪里能找到洗衣粉？ # vo:p_2_l_50
        醉警：就在我身后的桌子上。 # vo:p_2_l_51
-
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 2 getting into the laundromat无新消息欸~简体中文
// =============================================================================
== Mandy_story_phase_1 ==
~ game_progression = 2
{ Mandy_story_phase_1 > 1: -> Mandy_phase_1_repeat }
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
王太太：嘿！ # vo:p_3_l_1
王太太：那不是李小姐吗！这个点来洗衣服？ # vo:p_3_l_2

- (You_intro_choice)
* [睡不着] 
    你：额，是的。我只是有点睡不着。 # vo:p_3_l_3
    ~ cant_sleep = true
    王太太：唉，我懂的。 # vo:p_3_l_4
* [...（保持沉默）] 
    你：... # vo:p_3_l_5
    王太太：你怎么不说话？ # vo:p_3_l_6
* [急着洗衣服。] 
    你：我有些衣服急着洗，明天要用。 # vo:p_3_l_7
    王太太：什么事这么急？ # vo:p_3_l_8
- 
-> MrsWong_ask_alright

  = MrsWong_ask_alright 
王太太：你还好吗？ # vo:p_3_l_9
~ ChangeCamera("m4")
思绪：王太太总是对我这么好... # vo:p_3_l_10
思绪：我以前什么事都会和她说。 # vo:p_3_l_11
思绪：但现在我的手已经沾上了鲜血... # vo:p_3_l_12
~ ChangeCamera("Player")
* [说你很好。（谎言）]
你：嗯，我很好。你呢？ # vo:p_3_l_13
王太太：哎，我太累了。 # vo:p_3_l_14
* {not cant_sleep} [只是累了。]
你：抱歉，今天实在是太累了。 # vo:p_3_l_15
王太太：我也是呀。 # vo:p_3_l_16
* {cant_sleep} [为睡不着找借口]
你：这天气又湿又热，我根本睡不着。 # vo:p_3_l_17
王太太：这样嘛。我呀，相反， # vo:p_3_l_18
* [需要帮宇杰洗衣服]
你：只是需要帮我男朋友洗一下衣服。 # vo:p_3_l_19
~ boyfriend_needs_clothes = true
王太太：急什么呢？现在可是凌晨三点。 # vo:p_3_l_20 -> explain_hurry
-
王太太：如果不是非得守着这店，我脑袋一沾枕头就能睡着。 # vo:p_3_l_21
-> Vi_ask_about_MrsWong_phase_1

=explain_hurry
~ ChangeCamera("Player")
* [谎言]
你：我男朋友明天上班得穿这些。 # vo:p_3_l_22
* [是他叫我洗的。]
你：嗯，我男朋友刚刚叫我来洗的，因为他明天急着用。 # vo:p_3_l_23
-
王太太：这样啊。 # vo:p_3_l_24
王太太：我老公又跑去哪里打麻将了，留下我一个人通宵看店。 # vo:p_3_l_25
~ mahjong_mentioned = true
~ TriggerAnimation("Mandy", "doTalk")
王太太：我们女人真是任劳任怨啊…… # vo:p_3_l_26
-> Vivian_question_loop

= Vi_ask_about_MrsWong_phase_1
- (Vivian_question_loop)

~ ChangeCamera("Player")
* {mahjong_mentioned == false} [你还好吗？]
    你：你还好吗？ # vo:p_3_l_27
    王太太：还行吧。 # vo:p_3_l_28
    王太太：我老公又跑去搓麻将了，总得有人看店呗。 # vo:p_3_l_29
    -> Vivian_question_loop

* [你儿子不能来帮忙吗？]
你：你儿子不能来帮忙吗？ # vo:p_3_l_30
~ TriggerAnimation("Mandy", "doTalk")
王太太：他生病了。 # vo:p_3_l_31
王太太：我让他留在家休息。 # vo:p_3_l_32
   -> Vi_ask_about_MrsWong_phase_1 
+ [太辛苦了。（继续）] -> That_sounds_rough
=That_sounds_rough
    你：你太辛苦啦，王太太。 # vo:p_3_l_33
    王太太：我都习惯了。 # vo:p_3_l_34
    -> MrsWong_phase_1_proposol

= MrsWong_phase_1_proposol
王太太：好了，不说我了。 # vo:p_3_l_35
王太太：我好久没看到你男朋友了，你们最近怎么样？ # vo:p_3_l_36
~ ChangeCamera("b4")
思绪：一周前，他向我求婚了，我答应了。 # vo:p_3_l_37
思绪：我当时真的，久违地感受到了由衷的幸福。 # vo:p_3_l_38
思绪：为什么这种可怕的事要发生在我们身上…… # vo:p_3_l_39
~ ChangeCamera("Player")
* [告诉她求婚的事]
~ proposal_admit = true
你：宇杰上周刚刚向我求婚了... # vo:p_3_l_40
王太太：哇，恭喜你，李小姐！等等，你答应了吗？ # vo:p_3_l_41
王太太：你看起来心事重重的，一点也不像高兴的样子。 # vo:p_3_l_42
-> proposal_admitted
* [不告诉她]
你：宇杰最近工作挺忙的。我们过得蛮好的。 # vo:p_3_l_43
王太太：这样呀。 # vo:p_3_l_44
-> MrsWong_phase_1_laundry_coin

= proposal_admitted
~ ChangeCamera("Player")
思绪：如果这件事没有发生，我现在大概正依偎在宇杰怀里，期盼着我们的婚礼吧... # vo:p_3_l_45
* [强颜欢笑并说自己很高兴]
你：当然了，我都等他的求婚等了好几个月了！ # vo:p_3_l_46
* [为看起来忧心忡忡找借口]
你：我答应了。抱歉，只是最近发生的事情太多了... # vo:p_3_l_47
-
王太太：没事，我懂的。你穿婚纱的样子一定很美。 # vo:p_3_l_48
~ ChangeCamera("Player")
** [感谢她]
你：哈哈，哪里哪里，王太太你太客气了。 # vo:p_3_l_49
王太太：真好啊。 # vo:p_3_l_50
--
-> MrsWong_phase_1_laundry_coin

= MrsWong_phase_1_laundry_coin
~ ChangeCamera("Player")
王太太：好啦，你把衣服给我吧，我帮你扔进去洗。 # vo:p_3_l_51
~ ChangeCamera("m5")
思绪：不行，她不能碰这些衣服，上面全是血... # vo:p_3_l_52
~ ChangeCamera("Player")
* [我自己来。]
你：我自己来就好——不麻烦你啦。你大半夜一定很累了。 # vo:p_3_l_53
王太太：好吧。那至少让我帮你把深色和浅色的衣服分开—— # vo:p_3_l_54

- (laundry_delivery_choice)
~ ChangeCamera("Player")
* [拒绝] 
你：不用了，谢谢——我全部塞在一起洗一缸就好了。 # vo:p_3_l_55
王太太：那会毁了你的衣服的，李小姐。 # vo:p_3_l_56
  ** [没事的。]
  你：没事的。 # vo:p_3_l_57
  ** [我不介意。]
  你：哎呀，我不介意。 # vo:p_3_l_58
* [找借口阻止她]
你：我手头现金不太够，所以只洗一缸。 # vo:p_3_l_59
-
    -> give_money

= give_money
王太太：行吧。一共是8角钱。 # vo:p_3_l_60
~ ChangeCamera("Player")
* [给钱]
你：这里。 # vo:p_3_l_61
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("first_laundry_coin")
王太太：拿去吧。4号机。就在你左边那台。 # vo:p_3_l_62
 * * [谢谢。]
 你：谢啦，王太太。 # vo:p_3_l_63
 * * [...]
 你：... # vo:p_3_l_67
-
    ~ ChangeCamera("Player")
    -> END
    
= Mandy_phase_1_repeat
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [询问是哪台洗衣机]
你：请问是哪台洗衣机来着？ # vo:p_3_l_65
王太太：4号机。就在你左边那台。 # vo:p_3_l_66
~ ChangeCamera("Player")
-> END
+ [没事]
你：没事。我自己能搞定。 # vo:p_3_l_67
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
思绪：就是这台洗衣机。 # vo:p_4_l_1
思绪：我要去拿一些洗衣液，应该在椅子后面的桌子上。 # vo:p_4_l_2
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 5 after interacting with the detergent
// =============================================================================
== Thought_about_empty_detergent ==
~ game_progression = 5
~ ChangeCamera("b1")
思绪：可恶，没有强力洗衣液了…… # vo:p_5_l_1
思绪：普通的洗衣液洗不掉血迹... # vo:p_5_l_2
思绪：我得去问一下别人。 # vo:p_5_l_3
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

* [要洗衣液]
你：王太太，这里没有强力洗衣剂了。 # vo:p_6_l_1
王太太：是吗？我记得我在那放了很多普通洗衣液，那个不够吗？ # vo:p_6_l_2
~ ChangeCamera("m4")
思绪：我不想对王太太撒谎，但我该怎么解释... # vo:p_6_l_3
  ~ ChangeCamera("Player")
  ** [我的猫尿在衣服上了。（说谎）]
  ~ lied_about_cat = true
  你：情况有点尴尬。我的猫在床单上尿尿了。 # vo:p_6_l_4
  你：我们必须用强力洗衣剂来洗这些床单。 # vo:p_6_l_5
  王太太：你的猫？我以为你男朋友不让你养猫呢？ # vo:p_6_l_6
  -> cat_secondary_questions
  ** [我来月经了。（说谎）]
  ~ lie_about_period = true
  你：你懂的……我这几天刚好例假来了。我得把这些床单洗了…… # vo:p_6_l_7
  你：只有强力洗衣剂才能洗干净。 # vo:p_6_l_8
  ~ TriggerAnimation("Mandy", "doTalk")
  王太太：哦，我懂。我们女人每个月都要受这罪，真是太折磨人了。 # vo:p_6_l_9
 -> get_detergent_in_backroom
  ** [回避问题] 
         你：我觉得强力洗衣剂能把衣服洗得更干净。 # vo:p_6_l_10
         王太太：哦，行吧。 # vo:p_6_l_11
-> get_detergent_in_backroom

    
= cat_secondary_questions
~ ChangeCamera("b4")
思绪：天哪，我完全忘了我以前跟她提到过... # vo:p_6_l_12
思绪：几个月前，因为我想要养只小猫，宇杰当时突然大发雷霆。 # vo:p_6_l_13
思绪：他说让我不要浪费钱。 # vo:p_6_l_14
思绪：我现在该怎么圆这个谎... # vo:p_6_l_15
~ ChangeCamera("Player")
* [说服了男朋友。（谎言）] 
你：我说是他了，因为那小猫实在是太可爱了。 # vo:p_6_l_16
~ TriggerAnimation("Mandy", "doTalk")
王太太：好吧。我还真没想到像你男朋友那么固执的人竟然会妥协。 # vo:p_6_l_17
 ~ ChangeCamera("Player")
 ** [维护宇杰]
你：其实他对我挺温柔的。他只是没那么喜欢猫罢了。 # vo:p_6_l_18
王太太：随你怎么说吧。 # vo:p_6_l_19
* [我说错话了。（谎言）]
你：啊，我说错话了。那是邻居家的猫。 # vo:p_6_l_20
王太太：它一路溜进你房间里了？真是够野的猫。 # vo:p_6_l_21
你：是啊，挺野的。 # vo:p_6_l_22
-
-> get_detergent_in_backroom

= get_detergent_in_backroom
~ ChangeCamera("b5")
王太太：备用的强力洗衣液在后面隔间里。 # vo:p_6_l_23
王太太：我太累了动不了...你要么自己去拿一下吧？ # vo:p_6_l_24
~ ChangeCamera("Player")
* [没问题。]
你：好的没问题。 # vo:p_6_l_25
王太太：谢谢你啦。那我就在这休息会儿。 # vo:p_6_l_26
- 
-> Ending_mandy_story_phase_2

= Ending_mandy_story_phase_2
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("backroom_key")
王太太：这是后面隔间的钥匙。就在9号洗衣机旁边。 # vo:p_6_l_27
王太太：你要找的洗衣液叫蛋白酶洗衣液，架子上蓝色那瓶就是。 # vo:p_6_l_28
你：谢谢你，王太太。 # vo:p_6_l_29
~ knows_backroom = true

~ ChangeCamera("Player")
-> END

= ask_mandy_questions
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
+ [询问后面隔间在哪]
你：后面隔间在哪里？ # vo:p_6_l_30
王太太：后面隔间在角落里，靠近9号洗衣机。 # vo:p_6_l_31 -> ask_mandy_questions
+ [询问强力洗衣剂长什么样]
你：强力洗衣剂又是哪一种来着？ # vo:p_6_l_32
王太太：叫蛋白酶洗衣液，架子上蓝色那瓶就是。 # vo:p_6_l_33
-> ask_mandy_questions
+ [没事。] 
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
        醉警：来找我，对吧？ # vo:p_7_l_1
    - else:
    醉警：我跟你说过了，去前台桌子那拿洗衣粉 # vo:p_7_l_2
    }
    ~ ChangeCamera("Player")
    -> END

    = first_time
醉警：美女找我什么事呀？ # vo:p_7_l_3
        ~ ChangeCamera("Player")
        * [只要洗衣剂。]
        你：没什么。只需要一些强力洗衣剂。 # vo:p_7_l_4
        * [礼貌地询问洗衣剂]
        你：打扰一下。请问你手头刚好有强力洗衣剂吗？ # vo:p_7_l_5
        * [结束对话] 
        你：没事了。 # vo:p_7_l_6
            ~ ChangeCamera("Player")
            -> END
    
    - 
    ~ ChangeCamera("l2")
    醉警：哼，那我能得到什么回报？ # vo:p_7_l_7
    醉警：...来个香吻怎么样？ # vo:p_7_l_8
    ~ ChangeCamera("Player")
    -> LAU_story_phase_2_continue_1


    = LAU_story_phase_2_continue_1
        ~ ChangeCamera("Player")
        * [不行。] 
        你：不行。你到底有没有洗衣剂？ # vo:p_7_l_9
        醉警：话说回来，你干嘛非要强力洗衣剂不可？ # vo:p_7_l_10
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [我有男朋友。] 
        你：...我有男朋友。 # vo:p_7_l_11
            醉警：哇哦，看来有人陷入爱河不能自拔啊。 # vo:p_7_l_12
            ->reason_for_detergent
            ~ ChangeCamera("Player")
            -> END
            
        * [阴阳怪气] 
        你：恶心，你现在不应该陪在你老婆孩子身边吗？ # vo:p_7_l_13
            醉警：呵，问得好。他们已经离开我了。 # vo:p_7_l_14
            * * [继续阴阳怪气] 
            你：那是当然。 # vo:p_7_l_15
                醉警：哇，这话可够伤人的。 # vo:p_7_l_16
            * * [为什么？] 
            你：为什么？ # vo:p_7_l_17
                醉警：我不知道... # vo:p_7_l_18
            * * [道歉] 
            你：对不起，我不是那个意思... # vo:p_7_l_19
                醉警：没事。 # vo:p_7_l_20
            - - 
            ~ ChangeCamera("l4")
            醉警：我甚至根本没料到他们会离开。 # vo:p_7_l_21
            醉警：我为了她和孩子日夜拼命工作， # vo:p_7_l_22
            醉警：难道这还不是爱的证明吗？ # vo:p_7_l_23
            ~ ChangeCamera("Player")
            醉警：反正我现在又是单身了，所以我的香吻呢？ # vo:p_7_l_24
            -> LAU_story_phase_2_continue_1

    = reason_for_detergent
        醉警：那边桌子上有洗衣液，你不是去过了嘛。 # vo:p_7_l_25
        ~ ChangeCamera("Player")
        * [没有强力洗衣剂]
        你：那里没有强力洗衣剂了。 # vo:p_7_l_26
        醉警：你到底干嘛非要强力洗衣液不可？ # vo:p_7_l_27
        ~ ChangeCamera("Player")
        ** [能洗得更干净（借口）] 
         你：我觉得强力洗衣剂洗得更干净，味道也更好闻。 # vo:p_7_l_28
         Drunk Cop: 是吗... 好怪的习惯。 # vo:p_7_l_29
* [谎言] 你：我的猫在床单上撒了一泡尿。臭得要命。 # vo:p_7_l_30
            醉警：真的吗？我怎么没闻到。 # vo:p_7_l_31
            ~ lau_cat_pee = true
            ~ ChangeCamera("l3")
            ** [虚张声势]
            你：要是你真想闻闻我猫的尿味，请便啊。 # vo:p_7_l_32
            醉警：哈哈，那倒不必。我信你。 # vo:p_7_l_33
            ** [你的鼻子出问题了吧。]
            你：你的鼻子出什么问题了吗？ # vo:p_7_l_34
            醉警：什么？我可是全警署灵敏度第一的鼻子。 # vo:p_7_l_35
        -
        ~ ChangeCamera("Player")
        醉警：反正我手头没有你要的那种特殊洗衣液。 # vo:p_7_l_36
        醉警：你去问问王太太吧。 # vo:p_7_l_37
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
    思绪：王太太眼神里似乎带着一丝同情。 # vo:p_8_l_1
    思绪：警察那副表情仿佛他早就看穿了我。 # vo:p_8_l_2
    思绪：他们是不是全都心知肚明，只是在戏弄我？ # vo:p_8_l_3
    思绪：他们看到衣服上的血迹了吗？ # vo:p_8_l_4
    ~ ChangeCamera("Player")
    - 2: 
    ~ ChangeCamera("b1")
    思绪：我还没有哭。还没有悲伤，也没有哀悼。 # vo:p_8_l_5
    思绪：每向前走一步，我就越发不像一个人。我还要躲多久？ # vo:p_8_l_6
    思绪：余生我都必须背负着这个秘密吗？ # vo:p_8_l_7
    ~ ChangeCamera("Player")
    - 3:
    ~ ChangeCamera("b1")
    思绪：无论倒进去多少洗衣剂，死人都不可能复活了。 # vo:p_8_l_8
    思绪：说出来救不回他。那便再无任何事能让我心安。 # vo:p_8_l_9
    思绪：而等我搞定这一切，家里还有一场谋杀在等着我。 # vo:p_8_l_10
    ~ ChangeCamera("Player")
    - 4:
    ~ ChangeCamera("b2")
    思绪：无论倒进去多少洗衣剂，这都救不回他。 # vo:p_8_l_11
    思绪：说出来救不回他。 # vo:p_8_l_12
    思绪：但至少那样罪行就会存在于我脑袋之外的地方。存在于一个不会再生长蔓延的地方。 # vo:p_8_l_13
    思绪：宇杰也是这么想的吗？ # vo:p_8_l_14
    ~ ChangeCamera("Player")
    - else: 
    ~ ChangeCamera("b2")
    思绪：这幅伪装、这些谎言，我究竟还能维持多久？ # vo:p_8_l_15
    ~ ChangeCamera("Player")
}

~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 9 Directly after the innervoice, pager beeps as interruption.
// =============================================================================
== Boyfriend_pager_phase_1 ==
~ game_progression = 9
杰：有人看到你了吗？ # vo:p_10_l_1
杰：放宽心吧，薇薇。 # vo:p_10_l_2
杰：别忘了 —— 用强力洗衣液。 # vo:p_10_l_3
杰：我收拾好了。在开往码头。 # vo:p_10_l_4
杰：你清洗快点！！！ # vo:p_10_l_5
杰：晚点再说。 # vo:p_10_l_6
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 10 After pager phase for around 5 sec, if player didn’t find the detergent yet.
// =============================================================================
== Thought_about_how_detergent_looks ==
~ game_progression = 11
~ ChangeCamera("b1")
思绪：王太太说强力洗衣剂是蓝色的，应该在架子上某个地方。 # vo:p_11_l_1
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
思绪：就是这个洗衣液。我得尽快把衣服放进4号洗衣机里。 # vo:p_12_l_1
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 12 During the first washing mini game
// =============================================================================
== Thought_washing_clothes_1 ==
~ game_progression = 13
~ ChangeCamera("b4")
思绪：我们第一次见面时，宇杰就穿着这件衬衫。 # vo:p_13_l_1
思绪：那是个下雨天，我不小心滑倒，买的东西掉了一地。 # vo:p_13_l_2
思绪：是他帮我把东西捡起来的。两周后我们就在一起了。 # vo:p_13_l_3
思绪：我以为我们会一直这样相爱，过着平凡的生活。 # vo:p_13_l_4
思绪：谁能想到... # vo:p_13_l_5
思绪：不知道洗完这些衣服之后，我们是否就能忘记发生过的事。 # vo:p_13_l_6
思绪：我们真的能重新穿上这些衣服，假装它们从未沾过血吗？ # vo:p_13_l_7
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 13 after the first washing mini game, commenting that she needs another round
// =============================================================================
== Thought_about_need_another_washer ==
~ game_progression = 14
~ ChangeCamera("b2")
思绪：该死。还有好多衣服没放进去。 # vo:p_14_l_1
思绪：我早就该想到洗一缸是不够的。 # vo:p_14_l_2
思绪：我得再去弄一枚洗衣币。 # vo:p_14_l_3
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
    王太太：你换好零钱了吗？ # vo:p_15_l_1
    ~ ChangeCamera("Player")
    + [硬币兑换机在哪里？]
    你：还没有。请问硬币兑换机在哪里来着？ # vo:p_15_l_2
    王太太：角落里那台就是。 # vo:p_15_l_3
    你：谢谢。 # vo:p_15_l_4
    ~ ChangeCamera("Player")
    -> END
}
    
= Ask_for_laundry_coin_Mandy
~ TriggerAnimation("Mandy", "doRelax")
~ ChangeCamera("Player")
* [购买另一枚洗衣币]
你：王太太，我能再买一枚洗衣币吗？ # vo:p_15_l_5
王太太：还要一枚？你整个衣柜怕不是掉到下水道里了吧？ # vo:p_15_l_6
   ~ ChangeCamera("Player")
   ** [夏天流汗借口]
   你：你也知道，夏天容易出汗什么的。我受不了衣服有臭味。 # vo:p_15_l_7
   王太太：唔...我记得你两天前才刚来过啊。 # vo:p_15_l_8
   你：哎呀，夏天衣服脏得快嘛。 # vo:p_15_l_9
   -> Mandy_phase_3_fair
    ** {lied_about_cat} [猫到处乱尿。（谎言）]
    你：我跟你说过，猫在衣服上尿尿了，所以我们得把所有床上用品都洗了，味道实在太重了... # vo:p_15_l_10
    王太太：真是不听话的猫。叫什么名字？ # vo:p_15_l_11 -> Cat_name_question
    ** {lie_about_period} [经血弄得到处都是]
     你：额... # vo:p_15_l_12
     你：我今天的量特别大... # vo:p_15_l_13
     你：然后我不小心把衣服放在了沾有血迹的床单上。 # vo:p_15_l_14
     -> Mandy_phase_3_fair
~ ChangeCamera("Player")
-> END

= Mandy_phase_3_fair
王太太：你怎么不让你男朋友来洗，好让你休息休息？ # vo:p_15_l_15
~ ChangeCamera("m4")
思绪：他现在正在处理尸体... # vo:p_15_l_16
~ ChangeCamera("Player")
* [我自愿的（谎言）]
你：是我主动提出帮他的。他工作太忙了。 # vo:p_15_l_17
* [没时间]
你：他没时间。 # vo:p_15_l_18
-
王太太：哈，多么典型的借口。 # vo:p_15_l_19
~ ChangeCamera("Player")
** [维护宇杰]
你：但他真的很拼命赚钱啊。 # vo:p_15_l_20
--
王太太：哼。我得提醒你，当一个男人不再分担家务时， # vo:p_15_l_21
王太太：这通常意味着他要开始忽视你的感受了。 # vo:p_15_l_22
思绪：宇杰确实总是叫我做家务... # vo:p_15_l_23
思绪：但他工作很辛苦赚钱。 # vo:p_15_l_24
思绪：而且他的求婚又是那么浪漫... # vo:p_15_l_25
~ ChangeCamera("Player")
** [宇杰是不一样的。]
你：别担心，我男朋友不一样。 # vo:p_15_l_26
{ proposal_admit:
    王太太：你这才刚答应他的求婚，我也不想这么快泼你冷水。 # vo:p_15_l_27
- else:
    王太太：我不是说你男朋友就跟我那没用的老公一样。 # vo:p_15_l_28
}
~ TriggerAnimation("Mandy", "doTalk")
王太太：但很多男人刚开始都很浪漫，然后... # vo:p_15_l_29
王太太：我老公以前隔三差五就会给我买首饰，带我去码头看星星。 # vo:p_15_l_30
王太太：现在呢，除非想吃饭或者要我帮他代班，他连话都懒得跟我说。 # vo:p_15_l_31
王太太：你看看那边的刘先生，凌晨三点还在喝酒... # vo:p_15_l_32
王太太：我不得不说，有些男人真的是没用... # vo:p_15_l_33
醉警：你们说话声音是不是太大了点，嗯？ # vo:p_15_l_34
   ~ LookAtTarget("Lau1", 0.75)
   *** [我们刚才在说你]
   你：我们刚才在说你。 # vo:p_15_l_35
   ~ ChangeCamera("l1")
   醉警：我？我才不没用，我是整个警署最厉害的警察。 # vo:p_15_l_36
   *** [没在说你]
    ~ ChangeCamera("l1")
   你：我们没在说你。 # vo:p_15_l_37
   醉警：... # vo:p_15_l_38
   ---
   ~ RestoreLook(0.75)
   王太太：哈哈。 # vo:p_15_l_39

->Mandy_phase_3_ending

= Cat_name_question
~ ChangeCamera("Player")
思绪：现在我得凭空想出这只不存在的猫的名字... # vo:p_15_l_40
* [宇杰。]
你：它叫宇杰。 # vo:p_15_l_41
王太太：拿你男朋友的名字给猫命名，这选择真有趣，哈哈。 # vo:p_15_l_42
  ~ ChangeCamera("Player")
  ** [我们觉得挺好玩的。]
  你：... 我们觉得挺好玩的。 # vo:p_15_l_43 ->Mandy_phase_3_fair
  ** [哈哈]
  你：... 哈哈 # vo:p_15_l_44
  ->Mandy_phase_3_ending
  
* [咪咪。]
你：咪咪。 # vo:p_15_l_45
* [恺撒。]
你：恺撒。 # vo:p_15_l_46
-
王太太：真可爱。 # vo:p_15_l_47
->Mandy_phase_3_ending

= Mandy_phase_3_ending
~ ChangeCamera("Player")
思绪：我得赶紧洗完，要是我太慢的话宇杰会发火的... # vo:p_15_l_48
* [问王太太付款的事]
你：第二台洗衣机也是要8角钱，对吧？ # vo:p_15_l_49
王太太：是的。 # vo:p_15_l_50
思绪：该死，我完全给忘了。 # vo:p_15_l_51
思绪：出事的时候血溅到了我们的钱上。 # vo:p_15_l_52
~ ChangeCamera("Player")
** [等等，我得先去换点零钱。]
你：额，等我一下。我得去换点零钱。 # vo:p_15_l_53
王太太：你确定吗？我这里有零钱。 # vo:p_15_l_54
~ ChangeCamera("b6")
思绪：我可不能让王太太看到钞票上的血。 # vo:p_15_l_55
思绪：我得用兑换机换个币。 # vo:p_15_l_56
   ~ ChangeCamera("Player")
   *** [我自己可以的。]
   你：不用担心，我自己可以。 # vo:p_15_l_57
   *** [找借口]
   你：没关系，反正我之后也需要些零钱打公用电话。 # vo:p_15_l_58
   -
   王太太：好的。硬币兑换机在角落里。 # vo:p_15_l_59
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 15 In front of coin machine, after press (E Interact)
// =============================================================================
== Thought_about_coin_machine_1 ==
~ ChangeCamera("b3")
思绪：我只需要把纸币塞进去...
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
    思绪：该死，这台机器老是有这个问题。不过多试一次通常就好了。 # vo:p_16_l_1
    ~ ChangeCamera("Player")
    - 2:
    ~ ChangeCamera("b2")
    思绪：又来？给我吐出来啊... # vo:p_16_l_2
    ~ ChangeCamera("Player")
    - else:
    // Win: unlock Collect 5 coins (TaskManager storyPhase 17).
    ~ game_progression = 17
    ~ ChangeCamera("b3")
    思绪：终于...我得把这些硬币拿去买另一枚洗衣币。 # vo:p_16_l_3
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 17 Pager interruption (progression 17 is set on coin-machine win above).
// =============================================================================    
== Boyfriend_pager_phase_2 ==
//pager beeps and vibrates
杰：刚刚被警察拦下了。 # vo:p_17_l_1
杰：关于后备箱我扯了几句谎。 # vo:p_17_l_2
杰：拿了张超速罚单。 # vo:p_17_l_3
杰：妈的，我手抖得厉害。连车都开不稳。 # vo:p_17_l_4
杰：终于到码头了。 # vo:p_17_l_5
杰：快点洗衣服！ # vo:p_17_l_6
杰：我靠你了。 # vo:p_17_l_7
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 18 Press E in front of Mandy, after get coins
// =============================================================================
== Mandy_story_phase_4 ==
~ game_progression = 18
~ TriggerAnimation("Mandy", "doRelax")
王太太：你换好零钱了吗？ # vo:p_18_l_1
~ ChangeCamera("Player")
* [（付款）]
~ GiveAwayItem("change_coin_1")
~ GiveAwayItem("change_coin_2")
~ GiveAwayItem("change_coin_3")
~ GiveAwayItem("change_coin_4")
你：是的，在这里。 # vo:p_18_l_2
王太太：李小姐，等等。 # vo:p_18_l_3
王太太：你的手臂怎么了？ # vo:p_18_l_4
~ ChangeCamera("Player")
思绪：我差点以为我手上还有血。 # vo:p_18_l_5
思绪：只是一块淤青。 # vo:p_18_l_6
思绪：宇杰叫我去拿刀的时候紧紧抓住了我的手臂。 # vo:p_18_l_7
思绪：我甚至都没注意到这片淤青原来这么明显。 # vo:p_18_l_8
** [谎言]
你：哦，是我太笨手笨脚了，不小心撞到了桌角。 # vo:p_18_l_9
王太太：真的吗？我还以为这是...你懂的。 # vo:p_18_l_10
** [回避]
你：哦，你不需要知道这个的，王太太。 # vo:p_18_l_11
-
王太太：我知道这种淤青是怎么来的。 # vo:p_18_l_12
王太太：每次我老公喝得酩酊大醉回家时，我也得忍受这些。 # vo:p_18_l_13
思绪：宇杰今晚出事前也喝了几杯。 # vo:p_18_l_14
思绪：喝了酒之后他有时就像变了个人。 # vo:p_18_l_15
思绪：他会变得...相当粗暴。 # vo:p_18_l_16
思绪：但那主要是因为他工作压力太大。 # vo:p_18_l_17
思绪：抛开这些发酒疯的时候，他是我见过最温柔的人。 # vo:p_18_l_18
** [替宇杰说话]
你：不，宇杰不是那样的，那只是个意外。 # vo:p_18_l_19
** [为王太太感到难过]
你：我懂...我真为很抱歉。 # vo:p_18_l_20
--
王太太：如果你有什么困难，请一定要告诉我，好吗？ # vo:p_18_l_21
  ~ ChangeCamera("m6")
~ TriggerAnimation("Mandy", "doGiveItem")
~ UnhideItem("second_laundry_coin")
王太太：这是你的洗衣币。你可以用9号洗衣机。 # vo:p_18_l_22
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 19 Triggered automatically, as player drops cloth walking towards the other washer.
// =============================================================================
== LAU_story_phase_3 ==
~ game_progression = 19
~ PlayAudioClip("musicAccent_4")
醉警：你那衣服上的红迹可真够刺眼的。 # vo:p_19_l_1
-> LAU_story_phase_3_continue_1

    = LAU_story_phase_3_continue_1
        ~ ChangeCamera("Player")
        * [是个意外（撒谎）] 
        你：哦，我男朋友不小心弄碎了瓶子划伤了手。 # vo:p_19_l_2
        ~ lied_about_hand = true
        醉警：哦，那肯定很疼。 # vo:p_19_l_3
        醉警：那为什么这件衬衫胸口那块都是血呢？ # vo:p_19_l_4
        ~ ChangeCamera("Player")
        ** [试图圆谎]
        你：我们当时想用他的手裹住衬衫来止血。 # vo:p_19_l_5
        醉警：用手裹住衬衫？哈哈哈哈 # vo:p_19_l_6
        醉警：我都开始怀疑你是不是比我喝得还多了。 # vo:p_19_l_7
        ~ ChangeCamera("l3")
        思绪：愚蠢的口误...希望他没注意到我的手在发抖。 # vo:p_19_l_8
        ~ ChangeCamera("Player")
        醉警：希望你男朋友没什么大碍。 # vo:p_19_l_9
        ~ ChangeCamera("Player")
        *** [他没事。]
        你：他没事，谢谢。所以他现在呆在家里。 # vo:p_19_l_10
        *** [转移话题]
        你：你真是我见过的最多管闲事的警察。 # vo:p_19_l_11
        醉警：或者是最敏锐的警察。 # vo:p_19_l_12
        ---
        -> LAU_story_phase_3_continue_2
        
        ~ ChangeCamera("Player")
        * [洒了红酒（撒谎）] 
        你：额，我男朋友把红酒洒得满地都是。 # vo:p_19_l_13
        ~ lied_about_wine = true
        醉警：真的吗... 那是什么红酒？怎么跟血一样红。 # vo:p_19_l_14
        ~ ChangeCamera("l4")
        思绪：该死，我对红酒一无所知。 # vo:p_19_l_15
        ~ ChangeCamera("Player")
            * * [我不记得了。] 
            你：我不记得了。 # vo:p_19_l_16
            你：这酒是他从隔壁街的酒铺买来的。 # vo:p_19_l_17
            * * [编一个名字]
            你：拉... 托斯。大概叫这名字。 # vo:p_19_l_18
醉警：有意思，从来没听说过。我还以为我懂所有的葡萄酒呢。 # vo:p_19_l_19
            ~ ChangeCamera("Player")
            *** [这很罕见。]
            你：这很罕见，是我男朋友国外的朋友送给他的礼物。 # vo:p_19_l_20
            --
            醉警：真奇怪... # vo:p_19_l_21
            醉警：给你科普个关于红酒的冷知识... 它干了之后会变成偏紫色。 # vo:p_19_l_22
            醉警：而血，干了之后会变成暗沉的锈红色。 # vo:p_19_l_23
            -> LAU_story_phase_3_ending
            
        ~ ChangeCamera("Player")
        * {lie_about_period} [拿经期撒谎。] 
        ~Cop_knows_period = true
        你：我...这几天刚好来例假了。 # vo:p_19_l_24
        醉警：哦，知道了。 # vo:p_19_l_25
        醉警：但为什么这件衬衫胸口那一块全是血？ # vo:p_19_l_26
         ~ ChangeCamera("Player")
         ** [试图解释]
         你：我不小心把它放到了床单沾血的地方上了。 # vo:p_19_l_27
         醉警：行吧... # vo:p_19_l_28
         -> LAU_story_phase_3_ending 
        * [不关你的事。] 
        你：管好你自己的事吧。 # vo:p_19_l_29
        醉警：我是警察，我负责巡逻这片区域： # vo:p_19_l_30
        醉警：我当然得管管别人的事。 # vo:p_19_l_31
        ~ ChangeCamera("Player")
        思绪：我本以为我能瞒过去的... # vo:p_19_l_32
        思绪：现在我得找个借口了。 # vo:p_19_l_33
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
    醉警：但我以为他呆在家是因为生病了？ # vo:p_19_l_34
    ~ ChangeCamera("Player")
    * [也是生病]
    你：对呀，生病也是呆在家的原因之一。 # vo:p_19_l_35
    醉警：划伤了手还生病了！他下次指不定走个路都能摔断腿！ # vo:p_19_l_36
    ~ ChangeCamera("Player")
    ** [他睡着了。]
        你：他已经睡着了。 # vo:p_19_l_37
    ** [我想做点家务。]
        你：他在休息，所以我想在关门前先把家务给做了。 # vo:p_19_l_38
    - 
    醉警：好吧，我还以为这响个不停的传呼机声是你那可怜的男朋友发来的呢。 # vo:p_19_l_39
    ** [是我妈妈发来的。]
        你：是我妈妈发来的。 # vo:p_19_l_40
        -> LAU_story_phase_3_ending
    ** [不，是朋友发的。]
        你：不是，是我闺蜜发给我的。 # vo:p_19_l_41
        你：她最近买了个传呼机，一直在用。 # vo:p_19_l_42
        -> LAU_story_phase_3_ending
    
    = busy_reply_phase_3
    醉警：但我以为他留在家是因为工作很忙？ # vo:p_19_l_43
    ~ ChangeCamera("Player")
    * [赞同]
        你：是的，他有些工作上的事情要处理。 # vo:p_19_l_44
        -> LAU_story_phase_3_ending
    * [忙着睡觉。]
        你：忙着睡觉呢。 # vo:p_19_l_45
        -> LAU_story_phase_3_ending


    = LAU_story_phase_3_ending 
    醉警：这红印让我想起了今天的案发现场。 # vo:p_19_l_46
    醉警：一个中年男人把他妻子刺死了。 # vo:p_19_l_47
    醉警：他拒绝认罪，所以我们别无选择，只能把他押进关起来。 # vo:p_19_l_48
    醉警：她衬衫上也有一块这样的红印... # vo:p_19_l_49
    ~ ChangeCamera("l1")
    * {lied_about_wine} [虚张声势]
    你：没必要想那么多。 # vo:p_19_l_50
    你：我男朋友打翻的那瓶葡萄酒可能只是酿造工艺不同。 # vo:p_19_l_51
    * {lied_about_wine} [...]
    * {lied_about_hand} [告诉他他想多了]
    你：你为什么非要想那么多呢？ # vo:p_19_l_52
    你：不小心割伤手不是很正常的事吗？ # vo:p_19_l_53
    * {lied_about_hand} [表现出你对犯罪感到害怕]
    你：...听起来真吓人。 # vo:p_19_l_54
    * {Cop_knows_period} [告诉他他想多了]
    你：没必要想那么多。 # vo:p_19_l_55
    -
    ~ ChangeCamera("Player")
    醉警：那好吧...希望你没有撒谎。 # vo:p_19_l_56
    醉警：你应该知道对警察撒谎会有什么后果吧？ # vo:p_19_l_57
    * [沉默]
    你：... # vo:p_19_l_58
    醉警：哈哈。我开玩笑的。去洗衣服吧。 # vo:p_19_l_59
    -> END
    
    * [我不会撒谎的。]
    你：我当然不会对你撒谎。 # vo:p_19_l_60
    醉警：真乖。 # vo:p_19_l_61
    醉警：不过你要是隐瞒了什么，最好快点告诉我。 # vo:p_19_l_62
    醉警：先去洗衣服吧。 # vo:p_19_l_63
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
思绪：在那个人闯进来之前，宇杰喝了几杯酒。 # vo:p_20_l_1
思绪：他当时就穿着这件。 # vo:p_20_l_2
思绪：为什么我们要.. 为什么宇杰要那么做。 # vo:p_20_l_3
~ ChangeCamera("Player")
思绪：我忘不了宇杰刺下去时那个人的眼神... # vo:p_20_l_4
~ ChangeCamera("b3")
思绪：我忘不了事情发生后宇杰抱住我时那股血腥味。 # vo:p_20_l_5
思绪：我还要躲多久？ # vo:p_20_l_6
思绪：我受够了继续维持这些谎言... # vo:p_20_l_7
思绪：但我怎么可能抛弃我深爱的人， # vo:p_20_l_8
思绪：那个为了保护我甚至不惜杀人的人？ # vo:p_20_l_9
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 21 after the second washing clothes mini game, as the lights turned off.
// =============================================================================
== Chaos_blackout ==
~ game_progression = 21
~ black_out_happened = true
~ SetBlackout(1)
醉警：该死，搞什么鬼？ # vo:p_21_l_1
王太太：怎么又来了... # vo:p_21_l_2
王太太：李小姐，能帮我个忙去后备间检查一下配电箱吗？ # vo:p_21_l_3
~ ChangeCamera("Player")
* [我去吧。]
你：我去吧。我正好就在旁边。 # vo:p_21_l_4
王太太：谢谢你，李小姐。 # vo:p_21_l_5
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
思绪：为什么会变成这样... # vo:p_22_l_1
思绪：难道这就是我的宿命，永远洗不掉我们犯下的罪？ # vo:p_22_l_2
思绪：我是不是应该放弃了？但我根本无处可藏... # vo:p_22_l_3
思绪：我该回去吗？但余生我都将活在这场谋杀的阴影下。 # vo:p_22_l_4
思绪：我该向王太太求助吗？但宇杰会恨我的... # vo:p_22_l_5
思绪：...还是我背叛他，检举他的谋杀？ # vo:p_22_l_6
~ ChangeCamera("Player")

    - 2: 
    ~ ChangeCamera("b3")
    思绪：那警察笑得就好像他早已看穿了一切。 # vo:p_22_l_7
    思绪：看穿了还在戏弄我，也许他确实如此，也许那对我来说是一种我不愿承认的解脱。 # vo:p_22_l_8
    思绪：被抓至少意味着这场戏终于演完了。 # vo:p_22_l_9
    ~ ChangeCamera("Player")
    - else:
    ~ ChangeCamera("b1")
    思绪：我再也不在乎什么因果报应了。 # vo:p_22_l_10
    思绪：我希望有人能陪伴在我身边... # vo:p_22_l_11
    思绪：我该放弃吗？但我根本无处可藏... # vo:p_22_l_12
    ~ ChangeCamera("Player")
}
~ ChangeCamera("Player")
-> END


    
// =============================================================================
//  PHASE 23 After the inner voice phase
// =============================================================================
== Boyfriend_pager_phase_3 ==
~ game_progression = 23
杰：搞定了。 # vo:p_23_l_1
杰：往那个麻袋里塞了些石头。 # vo:p_23_l_2
杰：扔进大海里了。 # vo:p_23_l_3
杰：你怎么这么慢？ # vo:p_23_l_4
杰：快一点！！！ # vo:p_23_l_5
杰：你总不想毁了我们的生活吧？ # vo:p_23_l_6
~ ChangeCamera("Player")
-> END

// =============================================================================
//  PHASE 24 after boyfriend pager ( now it’s automatically switched in ink) (player can’t leave the backroom without light switching back on)
// =============================================================================
== How_to_turn_on_circuit_box ==
~ game_progression = 24
    ~ ChangeCamera("b4")
    思绪：我得合上闸通电。 # vo:p_24_l_1
    思绪：电闸箱应该就在墙上。 # vo:p_24_l_2
    思绪：我想我得合上上面最大的那个开关？ # vo:p_24_l_3
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 25 If the player attempts to leave the backroom while the power blackout is still there
// =============================================================================
== Attempt_leaving_backroom ==
~ game_progression = 25
    ~ ChangeCamera("b1")
    思绪：我还不能离开。我需要合闸通电。 # vo:p_25_l_1
    思绪：电闸箱应该就在墙上。 # vo:p_25_l_2
    思绪：我想我得合上上面最大的那个开关？ # vo:p_25_l_3
    ~ ChangeCamera("Player")
    -> END

// =============================================================================
//  PHASE 26 After player comes out of the backroom, mandy stands in front of the door and automatically start the conversation
// =============================================================================
== Mandy_smoking_scene_1 ==
~ game_progression = 26
~ TriggerAnimation("Mandy", "doRelax")
王太太：里面一切都好吧？李小姐，你进去了蛮久的。 # vo:p_26_l_1
* [找借口]
你：抱歉，我刚刚没找到电闸开关。 # vo:p_26_l_2
* [抱歉。]
你：真的非常抱歉。 # vo:p_26_l_3
-
王太太：不用道歉。来抽根烟吗？ # vo:p_26_l_4

- (smoke_choice)
   * [谢谢]
   你：不用了，谢谢。 # vo:p_26_l_5
   -
   王太太：介意我们聊聊吗？就我们女人之间私下聊聊。 # vo:p_26_l_6
      ** [关于什么？]
      你：好啊。聊些什么？ # vo:p_26_l_7
      ** [不确定。]
      你：我不确定... # vo:p_26_l_8
      王太太：我只是有点担心你。 # vo:p_26_l_9
      - -
      王太太：一直有人在给你发信息对吧？是你男朋友吗？ # vo:p_26_l_10
         *** [是的。]
         你：是的，太晚了他有点担心我。 # vo:p_26_l_11
         *** [否认]
         你：不是... # vo:p_26_l_12
         - - -
         ~ TriggerAnimation("Mandy", "doTalk")
         王太太：你可以对我完全坦白的，李小姐。 # vo:p_26_l_13
         王太太：每次你从后备间出来都跟个鬼似的，而且... # vo:p_26_l_14
         王太太：你衣服上的血色比你脸上的血色还要多。 # vo:p_26_l_15
         -> Mandy_smoking_scene_2


// =============================================================================
//  PHASE 27 If the player not admit at first, but come to her again by pressing E, and can choose to admit again
// =============================================================================
== Mandy_smoking_scene_2 ==
~ game_progression = 27
~ TriggerAnimation("Mandy", "doRelax")
+ [向她坦白谋杀]
你：王太太，我做了一件坏事... # vo:p_27_l_1
-> Admit_to_Mandy

+ (dont_tell) [不告诉她]
  {dont_tell:
  - 1:
  你：王太太，什么都没发生。 # vo:p_27_l_2
  王太太：看来是我多虑了。 # vo:p_27_l_3
  王太太：我在这待会儿。 # vo:p_27_l_4
  王太太：如果你待会儿想找我聊聊，随时过来。 # vo:p_27_l_5
  - else: 
  你：没什么。 # vo:p_27_l_6
  王太太：好吧。 # vo:p_27_l_7
  王太太：我在这待会儿。 # vo:p_27_l_8
  王太太：如果你待会儿想找我聊聊，随时过来。 # vo:p_27_l_9
  }
-> END

= Admit_to_Mandy
王太太：冷静下来，深呼吸。有我在呢。 # vo:p_27_l_10
* [试图冷静下来并解释]
你：有人闯进我家... # vo:p_27_l_11
你：宇杰...他...一切发生得太快了。 # vo:p_27_l_12
你：他告诉我只要把血洗掉，一切都会恢复原样... # vo:p_27_l_13
王太太：天哪，李小姐... # vo:p_27_l_14
王太太：所以那些谎言...你只是想要保护自己，是吗？ # vo:p_27_l_15
** [道歉]
你：对不起，我当时太慌张了。 # vo:p_27_l_16
王太太：（叹气） # vo:p_27_l_17
王太太：我早就该看出来他是那种人的。 # vo:p_27_l_18
*** [宇杰是不一样的]
你：但他爱我啊！他是在保护我，王太太... # vo:p_27_l_19
王太太：他那是为了你，还是为了他自己的安全？ # vo:p_27_l_20
王太太：但你接下来打算怎么办？ # vo:p_27_l_21
王太太：他们迟早会发现的。 # vo:p_27_l_22
**** [我不知道。]
你：我不知道，王太太... # vo:p_27_l_23
~ TriggerAnimation("Mandy", "doTalk")
王太太：...我表妹住在陶山。 # vo:p_27_l_24
王太太：你可以去那里，但你必须抛下这里的一切，# vo:p_27_l_25
王太太：包括你的男朋友。你要试着在那里谋生。 # vo:p_27_l_26
王太太：我不能保证那里绝对安全，或者没人能找到你。 # vo:p_27_l_27
王太太：但总比继续躲在这里强。 # vo:p_27_l_28
王太太：好好想想吧，李小姐。 # vo:p_27_l_29
-> Mandy_smoking_scene_3

// =============================================================================
//  PHASE 28 If the player admitted, but not accept her help yet.
// =============================================================================
== Mandy_smoking_scene_3 ==
~ TriggerAnimation("Mandy", "doRelax")
- (final_choices_mandy)
* [陶山是什么样的？]
你：陶山的生活是什么样的？ # vo:p_28_l_1
王太太：我表妹说那是座小县城，没这里这么繁华。 # vo:p_28_l_2
王太太：如果你肯努力，在那里还是能找到活路的。 # vo:p_28_l_3 -> final_choices_mandy
+ [接受帮助并逃跑（结局）]-> Mandy_escape_ending
+ [花点时间考虑]
你：我不知道...我需要想想。 # vo:p_28_l_4
王太太：慢慢想。我会在这里留一会儿。 # vo:p_28_l_5
王太太：你准备好了随时可以回来找我。 # vo:p_28_l_6
~ game_progression = 28
-> END

= Mandy_escape_ending
~ PlayEndingCutscene(1)
思绪：是时候为自己而活了。 # vo:p_28_l_7
思绪：我不需要再听宇杰的了， # vo:p_28_l_8
思绪：也不需要事事都顺从他。 # vo:p_28_l_9
思绪：我必须逃离这种生活。逃离他。 # vo:p_28_l_10
你：那我该怎么去陶山？ # vo:p_28_l_1
王太太：每天早上8点都有去那里的轮渡。 # vo:p_28_l_12
王太太：你现在就得往码头赶了。 # vo:p_28_l_13
王太太：离开那个男人，开启新生活吧。 # vo:p_28_l_14
你：谢谢你，王太太... # vo:p_28_l_15
~ TriggerAnimation("Mandy", "doTalk")
王太太：你可以直接叫我曼婷。 # vo:p_28_l_16
曼婷：我会给我表妹阿欣发个信息，让她去接你。 # vo:p_28_l_17
曼婷：还有，薇薇...保持独立。 # vo:p_28_l_18
曼婷：那是女人拥有的最宝贵的东西。 # vo:p_28_l_19
曼婷：不要将你的生活和幸福寄托在任何男人身上。 # vo:p_28_l_20
你：我向你保证，曼婷。 # vo:p_28_l_21
曼婷：再见，薇薇。 # vo:p_28_l_22
你：再见，曼婷。 # vo:p_28_l_23
-
~ game_progression = 28
-> END

// =============================================================================
//  PHASE 29 After you talk to mandy, you will recieve a message from your boyfriend.
// =============================================================================
== Boyfriend_pager_ending ==
~ game_progression = 29
杰：刚到家。 # vo:p_29_l_1
杰：你要回家了就马上给我发消息。 # vo:p_29_l_2
~ ChangeCamera("Player")
-> END


// =============================================================================
//  PHASE 30 After mandy talks to you, if player goes to Lau and press E.
// =============================================================================
== Lau_confess_ending ==
醉警：脸色怎么这么难看，小姑娘？搞得像刚撞见鬼似的。 # vo:p_30_l_1
+ [没什么。]
你：我只是太累了。 # vo:p_30_l_2
-> END
* [检举男朋友谋杀]
你：我要报案，有人杀人了。 # vo:p_30_l_3
警官：再重复一遍，小姐。发生什么事了？ # vo:p_30_l_4
~ TriggerAnimation("Lau", "doPager")
~ PlayEndingCutscene(2)
-> murder_confess
= murder_confess
你：有人闯进我家威胁我们。 # vo:p_30_l_5
你：然后我男朋友...我男朋友想用刀解决。 # vo:p_30_l_6
你：一切发生得太快了... # vo:p_30_l_7
警官：你有参与其中吗？ # vo:p_30_l_8
  你：歹徒闯进来的时候，他让我帮他去拿刀。 # vo:p_30_l_9
  你：然后他又叫我来把这些衣服给洗了。 # vo:p_30_l_10
  警官：嫌疑人现在在哪里？ # vo:p_30_l_13
  你：他在我们的公寓里。莲花街32号3座4楼。 # vo:p_30_l_14
  警官：你叫什么名字？ # vo:p_30_l_16
  你：李薇。 # vo:p_30_l_17
  警官：他叫什么名字？ # vo:p_30_l_18
  你：何宇杰。 # vo:p_30_l_19
  ~ UnhideItem("police_lights")
  警官：...小姐，你做得很对。 # vo:p_30_l_20
  警官：我早就觉得有些不对劲，不过至少是你主动告诉我的。 # vo:p_30_l_21
  警官：要不然，窝藏杀人犯的罪名可是非常严重的。 # vo:p_30_l_22
  警官：坦白并举报爱人的罪行是需要巨大勇气的。 # vo:p_30_l_23
  警官：现在，请跟我走一趟吧。 # vo:p_30_l_24
  ~ game_progression = 30
  -> END

// =============================================================================
//  PHASE 31 Standard dialogue after choosing to complete the mission on the pager. Maybe shown after the black screen?
// =============================================================================
== Boyfriend_ending_dialogue_final ==
~ game_progression = 31
你：我已经搞定了。 # vo:p_31_l_1
宇杰：我也刚到家。 # vo:p_31_l_2
宇杰：我爱你。 # vo:p_31_l_3
你：我也爱你。 # vo:p_31_l_4
宇杰：一切都会过去的。 # vo:p_31_l_5
宇杰：我们还有明天。 # vo:p_31_l_6
宇杰：我们拥有彼此。 # vo:p_31_l_7
你：我们拥有彼此。 # vo:p_31_l_8
宇杰：现在，试着把它忘了吧。 # vo:p_31_l_9
宇杰：到我怀里来吧，亲爱的。 # vo:p_31_l_10
-> END

