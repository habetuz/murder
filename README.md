# Murder

A group game large groups.

## Domain

**Guest**

Single game user without login information
- name
 
**User**

Permanent user with login information
- name
- games
- login (adapter)

**Player**

Per game instance of a guest or user that is participating in a game
- kills

**Admin**

player administrating a game. Each game has one administrator

**Game**

A single round of murder
- id
- visibility
- player(s)
- admin
- murderChain
- startTime
- endTime

**MurderChain**

Responsible for keeping track of murder to victim assignments
- assignments

**Visibility (VO)**

Visibility of a game

Possible values: 
- `public` The game is accessible without knowing the game id
- `private` The game is only accessible by knowing the game id

