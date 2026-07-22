EXTERNAL ChangeCamera(cameraId)
EXTERNAL TriggerAnimation(targetId, animationName)
EXTERNAL PlayAudioClip(soundKey)
EXTERNAL SetMandyAffection(value)
EXTERNAL SetStoryAct(act)
EXTERNAL SetActProgression(progress)

// --- SCENE START: 3:00 AM IN THE LAUNDROMAT ---

~ SetStoryAct(1)
~ ChangeCamera("laundromat_counter")

//Mandy smoking, Vivian walks in.
~ TriggerAnimation("Mandy", "smoke_idle")

Mandy: "Oh! Look who it is. Miss Li, right? Doing laundry at this hour?

VIVIAN: "Uh, yeah. Just couldn't sleep."

Mandy: "Uff. I'm super tired on the contrary. But my man is at his friends' place playing mahjong, and Tian is sick, so I'm stuck covering the night shift... "

// FIRST CHOICE
- (vivian_choice_1)
~ ChangeCamera("Player")
* [1. "Your husband is playing mahjong?"] -> chose_mahjong
* [2. "Little Tien is sick?"] -> chose_sick
* [3. "I'm sorry, that sounds rough."] -> chose_rough

== chose_mahjong ==
Mandy: "Yeah. He had a few drinks tonight. When he gets like that, nobody can stop him."
-> second_conversation_beat

== chose_sick ==
Mandy: "Yeah. Fever's been running high since yesterday. But someone has to keep the machines running."
-> second_conversation_beat

== chose_rough ==
~ SetMandyAffection(1)
Mandy: "It's fine. It's just my life."
-> second_conversation_beat


== second_conversation_beat ==
// SECOND CHOICE BEAT (Investigation vs. Hiding the Evidence)
~ ChangeCamera("Player")
* [1. "What happened to your arm?"] -> ask_about_arm
* [2. "These clothes..."] -> hand_over_clothes

== ask_about_arm ==
Mandy: "(Coldly) Hit it on the edge of an industrial dryer earlier. I'm clumsy. Now, what can I get you?"
-> second_conversation_beat

== hand_over_clothes ==
~ SetActProgression(1)
~ PlayAudioClip("fabric_rustle")
Mandy: "Put them up the scale then."
~ ChangeCamera("Player")
-> END
