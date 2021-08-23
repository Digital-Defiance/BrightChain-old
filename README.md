# BrightChain
## (pre-alpha work in progress. Limited alpha testing coming soon!) 
[![.NET](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/dotnet.yml/badge.svg)](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/dotnet.yml)
[![Docker Image CI](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/docker.yml/badge.svg)](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/docker.yml)
[![CodeQL](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/codeql-analysis.yml)
[![Generate Documentation](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/generate-docs.yml/badge.svg)](https://github.com/The-Revolution-Network/BrightChain/actions/workflows/generate-docs.yml)

A Lightweight BlockChain- LightChain based on a Brightnet Blockstore- BrightChain.
All the benefits of blockchain dApps and contracts without the mining and waste.
Unlimited storage for everyone, and a mathematically reinforced and moderated community that will last for the ages.

- BrightChain Engine in C#/.Net 6 (Currently requires VS 2022 Preview, or IntelliJ Rider)
- Uses Microsoft's FASTER
- BrightNet BlockStore and API for BrightChain: The Revolution(ary) Network
- The new wiki: https://github.com/The-Revolution-Network/BrightChain/wiki (old will be rewritten/migrated)
- The old wiki: https://github.com/The-Revolution-Network/brightChainAPI/wiki/Introduction
- Auto-generated documentation: http://apidocs.therevolution.network/api/index.html
  - Note that some of the classes don't have docblocks yet, but I've gotten many. The "TODO/example" text is in place as well, but if you click into the sections the content is there.

# One-Pager
The BrightChain "One-Pager" is about 3 pages at the moment, but pending some slimming down is about as concise a document as I've put together.
- https://apertureimagingcom-my.sharepoint.com/:w:/g/personal/jessica_mulein_com/EYoQU8qG_xlGpD0_A-mxhvoBqv3OylrfjeRAohvoC0gDQg?e=0U2XDa
  - PDF https://github.com/The-Revolution-Network/BrightChain/blob/main/BrightChain-One-Pager.pdf as of 08/13/21 09:44 Pacfic time.
  - Feedback welcome.

# "Long-Paper"
The BrightChain "LongPaper" is longer and a work in progress, but goes into plain language detail on all aspects.
- https://apertureimagingcom-my.sharepoint.com/:w:/g/personal/jessica_mulein_com/EQGi-tzRmL9KotpkENN0OXcB5LQwpT7ox3vFo3eIJZrqcg?e=MZOanw
  - PDF https://github.com/The-Revolution-Network/BrightChain/blob/main/BrightChain-LongPaper.pdf as of 08/16/21 13:00 Pacfic time.
  - Feedback welcome of course.

Recent thoughts:
 - I think calling BrightChain a LightChain makes sense. it's a lightweight blockchain. It's got blockchain features people want, but without the actual overhead of blockchain, which is unnecessary.
 - If you haven't noticed- BrightChain is almost BrightChainS with an S. It's a pool of chains. Each with its own value.
 - Something I was implementing in the old code but haven't yet gotten to is also a deduplication for public blocks. Basically any chain that has its CBL block committed to the network will have the hash of the overall chain checked for deduplication.
 - API throttle will continue as planned to be mathematically rate limited by minimal proof of works for bad actors and an algorithm to determine the maximum request r/w rates.
 - Block consensus will be proof of stake
 - its more of a brightnet blockstore and an ethos... what it needs still is some math and a little more vision in the areas I'm not seeing.
 - The whole goal is to reward people who contribute good, frequently accessed content, the storage for it, etc.
 - Everything is tracked in terms of a unit called the Joule - wondering about using a micro-unit Jansky.
 - - Attempting to be somewhat synonymous with the real work unit.
 - - There should ideally be a direct maths.
  - Ultimately bad users are just bad blocks and will get flushed and expired out while good stuff will get extended on forever. They will have to work too hard for their network access/wasteful contribution/access and will leave.
  - It is not the necessity of this chain to store every bit forever. The chain is very transparent and ultimately when things do expire out if not needed by the system, others can easily back them up. Some blocks of course are immediately set immutable with a DateTime.MaxValue and don't need to be renewed.

# Eventually
* CLR dApps & Digital Contracts
* Hope to provide a low-overhead digital contract / dApp ecosystem based on the CIL/CLR, without the computational overhead of traditional blockchain, making use of the efficiencies of Brightnet Blockstores and still benefitting from the power of blockchain like properties.

# Disclaimers
* This project is still pre-Alpha and is not suitable nor warranted for any level of fitness or function.
* This project is not affiliated with GitHub, The DotNet Foundation, Microsoft, or any of its affiliates or holdings.
* This software is open source, and offered as a "best-effort" thereoetical construct at this time and it may well lose all your data at this point in time.

<!-- this timestamp is updated by a pre-commit hook in git-hooks/pre-commit then added to .git/hooks -->
Last Updated: <time class="timestamp" timestamp="ISO 8601 string">2021-08-26T10:23:55-0700</time>
