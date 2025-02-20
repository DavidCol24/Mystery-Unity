// Global variables
VAR player_name = ""
VAR player_vitality = 100
VAR has_magic_water = false
VAR gnome_friendship = 0
VAR visited_locations = 0
VAR time_of_day = "morning"

// Global functions
=== function decrease_vitality() ===
    ~ player_vitality = player_vitality - 10
    
=== function update_time() ===
    {
        - time_of_day == "morning": ~ time_of_day = "afternoon"
        - time_of_day == "afternoon": ~ time_of_day = "evening"
        - time_of_day == "evening": ~ time_of_day = "night"
    }

// Start knot is the explicit entry point
=== start ===
# forest
Welcome to your adventure! You're about to embark on a journey through strange and wondrous lands.

+ [Begin your journey] -> theChoiceGame

=== theChoiceGame === 
# forest
{visited_locations == 0: The sun beat down on your face as you stood at a crossroads. | You return to the familiar crossroads.}
To the left, a winding path led into a deep, dark forest. To the right, a sun-dappled meadow beckoned.
{time_of_day == "evening": The light is fading - you should consider finding shelter soon.}
{player_vitality < 50: You feel weary from your travels.}

Which way would you go?
+ [Go Left] -> theForestAirWasC
+ [Go Right] -> theMeadowWasARio

=== theForestAirWasC ===
# forest
~ visited_locations = visited_locations + 1
~ decrease_vitality()
~ update_time()

The forest air was cool and damp. {time_of_day == "evening": The growing darkness makes the trees cast long shadows.}
The trees towered above you, their branches forming a dense canopy overhead. You could hear the distant calls of unseen creatures.
{player_vitality < 30: Your exhaustion makes every step a challenge.}

As you ventured deeper, you came across a small, wooden cabin. Smoke curled from its chimney. Should you investigate or continue on your way?
+ [Investigate the cabin] -> youApproachedThe
+ [Continue On] -> theForestGrewDar

=== theMeadowWasARio ===
# meadow
~ visited_locations = visited_locations + 1
~ decrease_vitality()
~ update_time()

The meadow was a riot of color, with wildflowers of every hue blooming in profusion. {time_of_day == "afternoon": The afternoon sun bathes everything in golden light.}
Butterflies flitted among the blossoms, and a gentle breeze rustled through the tall grasses.
{player_vitality < 40: You feel the need to rest among the flowers.}

As you wandered through the meadow, you stumbled upon a hidden grotto. A sparkling waterfall cascaded into a crystal-clear pool. Would you take a drink from the pool or admire it from afar?
+ [Drink from the pool] -> youKneltDownAndC
+ [Admire the waterfall] -> youStoodMesmeriz

=== youApproachedThe ===
# cabin
~ gnome_friendship = gnome_friendship + 1
~ update_time()

You approached the cabin cautiously. As you drew closer, you heard the sound of music coming from within. You hesitated for a moment, then knocked on the door. A gruff voice bade you enter. Inside, you found a gnome tinkering with a clockwork contraption.
He offered you a cup of tea and regaled you with tales of his travels. {gnome_friendship > 1: The gnome recognizes you from previous visits and seems especially happy to see you again.}

You spent the afternoon chatting with the gnome, listening to his stories and marveling at his inventions.
+ [Continue] -> theChoiceGame
+ [End Adventure] -> thanksForPlaying

=== theForestGrewDar ===
# forest
~ decrease_vitality()
~ update_time()

The forest grew darker and more ominous as you pressed on. The sound of your footsteps echoed through the trees, and the only other sound was the occasional rustle of leaves. {time_of_day == "night": The darkness is nearly complete, making it hard to see the path ahead.}

You felt a sense of unease creeping over you, and you longed for the safety of the crossroads. You turned back, retracing your steps until you found yourself once again at the fork in the road.
{player_vitality < 20: You're exhausted and worried you might collapse if you continue much further.}

+ [Go Home] -> youLeaveAndRetur
+ [Go Right] -> theMeadowWasARio

=== youKneltDownAndC ===
# grotto
~ has_magic_water = true
~ player_vitality = 100

You knelt down and cupped your hands in the cool water. As you drank, you felt a strange tingling sensation throughout your body. You stood up, feeling refreshed and invigorated. {has_magic_water: The magical water courses through your veins, restoring your strength.}
You continued on your way, feeling a newfound sense of energy and purpose.

+ [Continue] -> theChoiceGame
+ [Leave with that power] -> youLeaveWithThis

=== youStoodMesmeriz ===
# grotto
~ player_vitality = player_vitality + 20

You stood mesmerized by the sight of the waterfall, its water crashing down into the pool below. You felt a sense of peace and tranquility wash over you. {player_vitality > 80: The peaceful scene seems to restore your energy somewhat.}
You lingered for a while, enjoying the beauty of the scene. Finally, you tore yourself away from the grotto and continued on your journey.

+ [Continue] -> theChoiceGame
+ [Go back to the waterfall] -> youDecidedToGoBa

=== thanksForPlaying ===
# forest
Thanks for playing. You finished with:
Vitality: {player_vitality}
Visited locations: {visited_locations}
Gnome friendship level: {gnome_friendship}
{has_magic_water: You obtained magical water during your journey. | You didn't find the magical water this time.}
-> END

=== youLeaveAndRetur ===
# forest
You leave and return home for the night. Enjoying a nourishing meal and a warm cozy bed.
Your final stats:
Vitality: {player_vitality}
Gnome friendship: {gnome_friendship}
-> END

=== youLeaveWithThis ===
# forest
You leave with this newfound power and prepare for the next journey.
Your magical water will serve you well in future adventures.
Final vitality: {player_vitality}
-> END

=== youDecidedToGoBa ===
# grotto
You decided to go back to the waterfall and spend the day admiring its beauty.
{player_vitality < 50: The peaceful atmosphere helped restore your energy.}
As night fell, you decided it was time to head home.
-> END