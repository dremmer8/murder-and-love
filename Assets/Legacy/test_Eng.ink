VAR suspicion = 0       // =Sus level
VAR Mandy_affection = 0 // Affection value
VAR Fear = 0            // Fear value

// Act 1
-> Mandy_conversation_1

== Mandy_conversation_1 ==
// Mandy sees you coming in.

Mandy: Hey, isn't that Miss Lei! Doing laundry at this hour?

- (You_intro_choice)
* [1. Uh, yeah. I just couldn't sleep.] 
    You: Uh, yeah. I just couldn't sleep.
    -> chose_sleepless
* [2. ... (Stay silent)] 
    You: ...
    ~ suspicion = suspicion + 1
    -> chose_silence
* [3. Mrs. Wong, I have some clothes I urgently need for tomorrow. ] 
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
* [1. It's nothing.] 
    You: It's nothing.
    -> continue1
* [2. I'm just too tired.] 
    You: I'm just too tired. 
    -> continue1

== continue1 ==
Mandy: I see... *Sigh*, I want to go home and sleep too, but my man is playing mahjong, and little Tien is sick, so I'm stuck covering the night shift...
-> Mandy_choice_1


== Mandy_choice_1 ==
// Camera zooms in: You notice a bruise on her arm.
- (mandy_loop_1)
+ {not chose_mahjong} [1. Mr. Wong is playing mahjong?] You: Mr. Wong is playing mahjong? -> chose_mahjong
+ {not chose_tian} [2. Is Tian sick?]  You: Is Tian sick? -> chose_tian
* [3. I'm sorry, that sounds rough.] You: I'm sorry, that sounds rough. -> chose_rough

== chose_mahjong ==
Mandy: Yeah. Lao Wang had a few drinks tonight. Whenever he drinks, he goes to play mahjong.

    - (mahjong_sub_loop)
    // Questions can be asked after one question
    + {not ask_arm} [1.1 Mrs. Wong, what happened to your arm?] You: Mrs. Wong, what happened to your arm? -> ask_arm
    + {not ask_husband} [1.2 I haven't seen Mr. Wong in a while. Are you two doing okay?] You: I haven't seen Mr. Wong in a while. Are you two doing okay? -> ask_husband
    * [1.3 (Return to previous topic)] -> Mandy_choice_1

    = ask_arm
    Mandy: (Coldly) I accidentally bumped into the corner of the table while reaching for the laundry detergent. I was too sleepy.
    -> mahjong_sub_loop

    = ask_husband
    Mandy: It’s pretty much the same. ...Actually, we're doing quite well. He bought me a bracelet recently.
    -> mahjong_sub_loop

== chose_tian ==
Mandy: Yeah. Fever's been running high since this morning. But someone has to keep the shop running.
-> Mandy_choice_1

== chose_rough ==
Mandy: It's just how it is.
-> Mandy_conversation_beat2


== Mandy_conversation_beat2 ==
You: I want to wash these clothes.
Mandy: Just give the clothes to me, I'll wash them for you.

- (laundry_delivery_choice)
* [1. I'll do it myself—no need to trouble you. You must be tired.] You: I'll do it myself—no need to trouble you. You must be tired.  -> refuse_help
* [2. I want to wash them myself today.] You: I want to wash them myself today.
    ~ suspicion = suspicion + 1
    -> refuse_help

== refuse_help ==
Mandy: Really? That works, too. Let's separate the colored and white clothes like usual?

* [(Hurriedly pull it back)]
    You: No, thanks—I’ll just tuck everything in together.
    ~ suspicion = suspicion + 1
    Mandy: Alright. That will be 13.80 in total.
    -> pay_scene


== pay_scene ==
You: Okay.
// Player pays the money, Mandy gives the laundry token.
Mandy: You can use Machine 4. The laundry detergent is on the table.
(You take the laundry token from Mandy's hand.)
-> Go_To_Detergent_Ask

== Go_To_Detergent_Ask ==
(You realize the heavy-duty laundry detergent is empty. You go to ask Mrs. Wong.)

You: Mrs. Wong, the heavy-duty laundry detergent over there is empty.
Mandy: Is that so? Is the regular detergent not enough?
You: Not really.

- (Mandy_choice_2)
* [1. Our cat peed on my boyfriend's clothes. Only heavy-duty detergent can get it out.] You: Our cat peed on my boyfriend's clothes. Only heavy-duty detergent can get it out. (Lie)
    // If asked about cat
    Mandy: Your cat? I thought your boyfriend didn't allow you to keep a cat?
    -> cat_secondary_questions
* [2. I just want to get them a bit cleaner.] You: I just want to get them a bit cleaner.
    ~ suspicion = suspicion + 1 
    -> continue_detergent_4 
    
== cat_secondary_questions ==
* [1. I convinced him because the kitty is so cute. It's currently in heat.] You: I convinced him because the kitty is so cute. It's currently in heat. (Lie)
    -> continue_detergent_2
* [2. Ah, I misspoke. It's the neighbor's cat.] You: Ah, I misspoke. It's the neighbor's cat. (Lie)
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

* [(QTE): Let me get it for you!]
    You: Let me get it for you! Just sit down and rest.
    Mandy: Alright, that works.
    ~ Mandy_affection = Mandy_affection + 1
* [(Missed QTE)] Mandy: If you don't mind, go ahead and grab it for me.

- Mandy: It's on the shelf, the blue one. It should be labeled 'Brand M Enzyme Detergent'.
You: Got it.
-> Task3_Wash_the_second_round


== Task3_Wash_the_second_round ==
(You realize you need to do another round of washing)
You: Mrs. Wong, I want to buy another token.

Mandy: Didn't you just come to the laundromat a few days ago? Why do you have so many clothes to wash this week?

- (Mandy_choice_3)
// If lied about cat
+ [1. I had no choice, the cat peed a whole circle.]
    You: I had no choice, the cat peed a whole circle. The shirts smell terrible. (Lie)
    Mandy: Is that so? Funny, I didn't smell anything at all.
+ [2. (Find another excuse)]
    You: The weather has been so hot lately, so I've been changing my clothes frequently.
+ [3. (Make a joke)]
    You: The clothes are too big, washing them more might shrink them. (needs to change

- Mandy: Fair enough. That will be another 13.80.

You: Hold on, let me use the change machine. I only have large bills.
Mandy: Don't worry, I can break it for you.
- (Mandy_choice_4)
+ [1. Insist]
    You: It's fine, I was actually planning to get some change to use the public phone anyway, so I might as well just do it.
    Mandy: (Pointing to the corner) The change machine is over there. Let me know if you need anything. 
    -> buy_laundry_Coin
+ [2. Hand over the bill]
    You: (Handing over the money)
    -> buy_laundry_Coin


// Bloody clothes discovery sequence triggers here
== buy_laundry_Coin ==
(You pay the money)
Mandy: (Handing over the token, looking at your trembling hands) Are you sure... everything is alright?
- (Mandy_choice_5)
* [1. I just had a fight with my boyfriend.]
    You: I just had a fight with my boyfriend.
    -> argument
* [2. Everything is fine.]
    You: Everything is fine.
    -> fine

== argument ==
Mandy: Is that so... He really does always try to control you. What did you fight about?
- (Mandy_choice_6)
* [He blew up when he came home and saw that I let the plumber inside.]
    You: He blew up when he came home and saw that I let the plumber inside.
    Mandy: So what did he do? Did he kick the plumber out right then and there?
    You: Pretty much.
    Mandy: I guess that pipe isn't getting fixed anytime soon.
    You: He only gets jealous because he cares about me...
    -> care
* [I wasn't considerate enough.]
    You: I wasn't considerate enough.
    Mandy: (Sighs) It really feels like we can never meet their expectations, doesn't it.
    -> care

== fine ==
Mandy: If everything is fine, why are you here at this hour?
+ [Because the cat peed on his clothes, and he needs to use those exact clothes tomorrow.]
    You: Because the cat peed on his clothes, and he needs to use those exact clothes tomorrow. (Lie)
    -> care
+ [I volunteered to help him wash them.]
    You: I volunteered to help him wash them.
    -> care
* [I wasn't considerate enough.]
    You: I wasn't considerate enough.
    Mandy: (Sighs) It really feels like we can never meet their expectations, doesn't it.
    -> care

== care ==
Mandy: But in all the years you've been together, how come he always lets you do the laundry all by yourself?
You: I... well.

Mandy: (Loudly) Men really are completely useless sometimes.

Officer Lau suddenly cuts in.
Officer Lau: Who are you two talking about...

- (reply_to_lau)
* [1. Not talking about you.]
    You: Not talking about you.
* [2. We're talking about you.]
    You: We're talking about you.

- Officer Lau mutters and grunts, rolls over, and goes back to staring into nothingness.

// Act 3
-> END