# BrightChain
- BrightChain Engine in C#/.Net 6
- BrightNet BlockStore and API for BrightChain: The Revolution(ary) Network
- https://github.com/The-Revolution-Network/brightChainAPI/wiki/Introduction

Recent thoughts:
 - API throttle will continue as planned to be mathematically rate limited by minimal proof of works for bad actors and an algorithm to determine the maximum request r/w rates.
 - Block consensus will be proof of stake
 - its more of a brightnet blockstore and an ethos... what it needs still is some math and a little more vision in the areas I'm not seeing.
 - The whole goal is to reward people who contribute good, frequently accessed content, the storage for it, etc.
 - Everything is tracked in terms of a unit called the Joule - wondering about using a micro-unit Jansky.
 - - Attempting to be somewhat synonymous with the real work unit.
 - - There should ideally be a direct maths.
  - Ultimately bad users are just bad blocks and will get flushed and expired out while good stuff will get extended on forever. They will have to work too hard for their network access/wasteful contribution/access and will leave.
  - It is not the necessity of this chain to store every bit forever. The chain is very transparent and ultimately when things do expire out if not needed by the system, others can easily back them up. Some blocks of course are immediately set immutable with a DateTime.MaxValue and don't need to be renewed.
