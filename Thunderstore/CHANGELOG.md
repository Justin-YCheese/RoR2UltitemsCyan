## 0.14.0

- Added Recipies!
    - Check readme for the list of recipies
- Added New Items!
    - Permaglaze
    - Worm Holes
- Fixed and Readding
    - Obsolute
    - Pig's Spork
- Reworked Pig's Spork
    - No longer heals on bleed ticks
    - Grants bleed chance per missing health
    - When hit below 25% still explode but inflicting bleed now splashes
- 1000 Degree Scissors
    - Now uses isConsumed, thus nolonger melting tonic afflictions
- Buffed Deluged Pail speed per red item 10% -> 15%
- Buffed Xenon Ampoule's Damage
    - Base damage 1600% -> 2000%
    - Stack damage 400% -> 600%
    - Long cooldown max multiplier 4 -> 5
        - (Max damage with equipments of 300s and 5x damage instead of 240s and 4x)
- Fixed Deluged Pail restacking lunar items (Didn't realize was different when I readded it)
- Koala Sticker min damage nerfed from 5 -> 8
- Corroding Vault give 10 to 15 items instead of just 15 items
- Rocky Taffy Shield 32% -> 30%
- Fixed various inaccurate item tool tips

- Worm Holes visually doesn't work in multiplayer
- Will update permafrost visuals

## 0.13.4

- Fixed and Readded
    - Deluged Pail
- Silver Thread
    - Now applies cost multiplier to scrappers (I didn't fix this when I readded it)
- Removed a few debug logs

## 0.13.3

- All items can be temparary! Again...
    - hadn't realized they needed an item tag...
- Fixed and Readding
    - Grapevine
    - Silver Thread
    - Koala Sticker
    - Bithday Candle & Rotten Bones
    - HMT & Zorse Pill
- Rotten Bones
    - Reduced time required for buff from 3 -> 2.5 mins
- Corroding Vault
    - A temporary vault will be used before permanent vaults, and grant temporary items
- Grapevine
    - Drop chance is 50% -> 40%
    - Stacks increase grapes gained instead of drop chance
    - Block chance 80% -> 70%
    - Now only uses one grape at a time

## 0.13.2

- Now I actually removed Birthday Candles...

## 0.13.1

- Mod no work. Need to fix.

## 0.13.0
- Updated for Alloyed Convergence DLC
    - All items can be temparary!
- Removed some items that don't work now. Plan on adding them back in
    - Deluged Pail
    - Grapevine
    - Silver Thread
    - Koala Sticker
    - Pig's Spork
    - Bithday Candle & Rotten Bones
    - HMT & Zorse Pill
    - Oboslute
- Pot of Regolith
    - Mininum damage 5% -> 4%
    - Chance to deal 20% increased 20% -> 25%
    - Cannot deal maximum damage if user below 25% health
- ViralEssence
    - Now has a max count of 6 status effects +6 per item
- InhabitedCoffin
    - Clarified description that "Bad luck grants more Coffins"


## 0.12.2

- Fixed Tiny Igloo for multiplayer
- Fixed Orbital Quark for multiplayer
- Fixed Yield Sign effect (was using opposite equipment for clients)
- Fixed Zorse Pill (Phase 3 randomly broke it with damage displays)
- Zorse Pill affect changed to be more visible
- Tiny Igloo now works with regular healing instead of just over healing
    - (because it was easier to implement for multiplayer)
- Change Xenon to visually use big laser for normal length equipments
    - But hitbox size is unchanged for normal and long delay equipments
- Renamed Viral Smog -> Viral Essence
- Rocky Taffy Shield 40% -> 32%
- Flea Bag
    - drop chance of crit attacks 300% -> 200%
    - buff duraction 12 -> 15 seconds
    - crit per buff 15 -> 16 percent
- Obsolute
    - Cooldown 60 -> 30 seconds
    - Added earning money when deleting items

## 0.12.1

- Add Tiny Igloo
- Add Orbital Quark
- Added visual effects for various items
- Added item notifications to Silver Thread
- Reduced Macroseismograph launch so that on the moon you can pillar skip
- Reworked Overclocked GPU to increase max stack instead of stats per buff
    - Because I over estimated the power of bolstering lantern
- Removed nearly all debug messages (Silver Thread and other items)

## 0.12.0

- Readded Koala Sticker
- Readded Grapevine
    - Block chance per status 85% -> 80%
- Readded Silver Thread
    - 50% chance to lose most recent item stack instead of dying
- Readded Pig's Spork
    - Radius of Hemmerage blast increase with stacks
- Toy Robot
    - Barrier gained 6 -> 8
    - Now works with Elusive Antlers
- Xenon Ampoule
    - Mininum cooldown with Equipment .4 -> .3 seconds
    - Equipments with <= 20 seconds base cooldown's laser: Damage 50% -> 70%, Size 1.5 -> 1.6
- Birthday Candles
    - Damage 32% -> 30%
    - Stacks increase duration by 20 seconds. After stage start, lose one stack every 20 seconds starting at 5 minutes
- 9 Ice Cubes Barrier changed 75% -> 100 + 50%
- Sue's Madndibles was given a status timer for the duration of it's effect
- Rocky Taffy gives barrier only after having a full shield using a buff, instead after you have any amount of shield
- Hopefully fixed Yield Sign swapping bug

## 0.11.1

- Removed debug random launch for Macroseismograph

## 0.11.0

- Updated for DLC2
- Retuning some items to adjust for DLC
- Viral Smog speed per status 25% -> 20%
- Overclocked GPU max attack speed per item 30% -> 35%
- Dream Fuel speed 120% -> 125
    - Root Duration 2s -> 1.5s
- 9 Ice Cubes barrier 80% -> 75%
- Macroseismograph launch power 7450 -> 10500
- Sonorous Pail renamed to Deluged Pail
- Removed problem items (plan to add them back in when I figure out how to...)
    - Grapevine
    - Silver Thread
    - Koala Sticker
    - Pig's Spork
    - Quantum Peel

## 0.10.2

- Fixed Zorse buff icon
- Fixed Jealous Foe removal bug

## 0.10.1

- Added Jealous Foe
- Added Quantum Peel
- Pig's Spork
    - Felt underwhelming so I added more sounds effects
    - Bleed chance 100% -> 200%
    - Added a Hemerage blast when entering low health
- Fixed hurting pots null reference bug
- Updated buff icons so they more closely match vanila buffs
- Added categories to configs
- Inflated Silver Thread

## 0.10.0

- Added Config File
- Added Pig's Spork
- Rotten Bones Description is now accurate
- Inflated Frisbee
- Renamed Chrysotope -> Crysotope
- Renamed Universal Solute -> Obsolute
- HMT base damage 400% -> 300%
    - damage per stack 125% -> 100%
- Yield Sign damage 350% -> 300%
    - Now neither version can be triggered by Bottled Chaos (Didn't do anything in the first place)
- Removed a few frequent log print messages

## 0.9.3

- Toy Robot Barrier 5 -> 6
- Zorse Pill
    - Starve Effect nolonger spawns when dealing any DoTs
    - Deals proper TOTAL damage
- Sonorous Pail
    - Now gives jump height per Lunar
    - Sonorous Pail no longer disables Bands nor VoidBear (this was actually happens with vanilla restacking, so I made my own method)
    - New restack doesn't regives items, therefore no longer synergizes with silver thread (used to be an immortality build with Sue's Mandables)

## 0.9.2

- Added item table to readme

## 0.9.1

- Fixing the asset bundle?

## 0.9.0

- Early Release