// --- SCENE START: 3:00 AM IN THE LAUNDROMAT ---
// Character Setup: Mandy (Wife of laundromat owner), Vivian (Player)

//Mandy smoking, Vivian walks in.

Mandy: "Oh! Look who it is. Miss Li, right? Doing laundry at this hour?

VIVIAN: "Uh, yeah. Just couldn't sleep."

Mandy: "Uff. I'm super tired on the contrary. But my man is at his friends' place playing mahjong, and Tian is sick, so I'm stuck covering the night shift... "

// FIRST CHOICE
- (vivian_choice_1)
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
Mandy: "It's fine. It's just my life."
-> second_conversation_beat


== second_conversation_beat ==
// SECOND CHOICE BEAT (Investigation vs. Hiding the Evidence)
* [1. "What happened to your arm?"] -> ask_about_arm
* [2. "These clothes..."] -> hand_over_clothes

== ask_about_arm ==
Mandy: "(Coldly) Hit it on the edge of an industrial dryer earlier. I'm clumsy. Now, what can I get you?"
// This loops you back so you are forced to eventually hand over the clothes to progress.
-> second_conversation_beat

== hand_over_clothes ==
//Mandy glances at your heavy bag, then points to the old spring scale sitting on the counter.
Mandy: "Put them up the scale then."
-> END