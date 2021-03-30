# CPSC 565 Assignment 3: Antymology

This project takes a somewhat simple minecraft-esque environment and adds ants to the mix. 

The goal of this project was to create emergent behaviour through the modelling of the ants - specifically with the intention to maximize 'nest production'.

Every ant is considered a sort of worker ant, with a single queen ant in the population. 

Each worker is controlled by a neural network that is trained through neuro-evolution (ie: each ant has a neural network, which is trained by a genetic algorithm). 

The queen ant is a special type of ant, and is the only ant with the ability to place nest blocks. 

You may simply open up this project in Unity to see it in action. 

### Ants

The ants were required to have the following behaviours, from DaviesCooper (our prof's) repo: 

##### Ant Behaviour
- Ants must have some measure of health. When an ants health hits 0, it dies and needs to be removed from the simulation
- Every timestep, you must reduce each ants health by some fixed amount
- Ants can refill their health by consuming Mulch blocks. To consume a mulch block, and ant must be directly ontop of a mulch block. After consuming, the mulch block must be removed from the world.
- Ants cannot consume mulch if another ant is also on the same mulch block
- When moving from one black to another, ants are not allowed to move to a block that is greater than 2 units in height difference
- Ants are able to dig up parts of the world. To dig up some of the world, an ant must be directly ontop of the block. After digging, the block is removed from the map
- Ants cannot dig up a block of type ContainerBlock
- Ants standing on an AcidicBlock will have the rate at which their health decreases multiplied by 2.
- Ants may give some of their health to other ants occupying the same space (must be a zero-sum exchange)
- Among your ants must exists a singular queen ant who is responsible for producing nest blocks
- Producing a single nest block must cost the queen 1/3rd of her maximum health.
- No new ants can be created during each evaluation phase (you are allowed to create as many ants as you need for each new generation though).

I did modify the health sharing to work if the ants were in close proximity as opposed to only having them share health on the same block to make life a bit easier. 

#### Approach to the problem

The neural network of each ant takes in a bit of information about the ant, and attempts to determine what the best action for the ant to perform at each tick is. 

### The mind of an ant

I should mention that the neural network code I used was mostly based off of: 

https://towardsdatascience.com/building-a-neural-network-framework-in-c-16ef56ce1fef

A simple evolution scheme was used: keep the fittest neural networks, copy the fittest half over to the weakest half, and mutate them. 

Note that the evolutional process / the generation reset (which happens to also regenerate the environment) was triggered by the queens death. 

##### Variations of the neural network inputs

The first approach I took to try and maximize nest production was to encode:

1. the 5x5 area around the ant (with the ant at the center)
2. the direction of the queen
4. the health of the queen
5. the direction of the nearest ant
6. the health of the nearest ant

Surely with enough time training, the ants would learn the relationships of all the data, right? In my dreams. 

As it turns out, this made it incredibly hard for the ants to learn! I left them to train overnight, but they made very little progress. 

The second approach I cut out a huge chunk of the data - taking out all of the encoded world data (leaving me with points 2 to 5), and adding another piece of data in place of it: the type of block the ant was currently on. 

The transition from the first approach to the second was slow, since I was also trying to play with the fitness function...

#### Variations of the fitness function

To build a nest, the queen ant must stay alive, and so the ants are motivated by their fitness functions to essentially "funnel" their health into the queen. 

The very first approach I took to this was maximizing the time the ants stayed alive. This encouraged them to just dig down as far as possible, and had nothing to do with the queen! (At that point, I had not even implemented the queen yet). 

I figured this was making the learning curve way too steep, so I made the fitness functions a bit more rigorous - recreating it with the following criteria:

- How many nest blocks the queen ant built in that generation
- How many times an specific ant shared health with the queen

This was making the overall fitness really dependent on the queen, so I scrapped the first item above, making my new criteria:

- The number of times an ant shared health with the queen
- The number of times the ants moved towards the queen (in hopes that this would teach my young ants about direction)

The final little bit of critera I added into the fitness calculation was to void the fitness altogether if an ant removed a nest block: this was done to discourage ants from destroying what I had been trying to get them to achieve!

## Some emergent behaviours my ants did produce

I should add as a note here that my ants never really got that smart - I don't have a 3080 and didn't have the time to port the code into a more trainable environment. 

One big pattern that emerged across the generations, though, is that the ants usually all defaulted to the same strategy: 

- Find a 'good' direction - the ants would usually choose one direction that they would inevitably all proceed over.  
- In some cases, the ants would stop just before reaching the nest / the queen (as pictured). This was especially common when the queen had started building the nest near a wall in the direction that the ants had started to mostly move in. 
- In earlier stages, the ants would move in a mix of directions - sometimes moving towards the queen! (this was mostly a coincidence of the queen being in the right place, though) but this behaviour was eventually filtered out as there seemed to be a more agreed upon 'good' direction.

### Conclusion

I would like to see someday if my ants could eventually build the brain power to come up with better strategies and actually start to realize how to move in the direction of the queen, but I also don't want to melt my CPU. 

Overall, this assignment was a lot of fun and I'm glad I had the oppourtunity to get my hands dirty with neural networks like this - especially given that my groups topical presentation was based off of neuro-evolution. Just a bit sad that my ants couldn't quite see the light. 

Thanks for reading!

