using Antymology.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{

    #region Properties
    /// <summary>
    /// Represents the health of an ant
    /// </summary>
    public int Health { get; set; } = 7000;

    public bool Alive { get; set; } = true;

    public NeuralNetwork network; 

    public float moveTimer = 0; // makes it so that the ant doesn't just jump around at 30fps


    public const int HealthLostPerTick = 20;

    private AbstractBlock air = new AirBlock();

    private float[] input = new float[125 + 11];

    private int HealthShared = 0;

    private int MovedTowardsQueen = 0;

    
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Take away some health
        
        if (Health < 0)
        {
            Alive = false;
        } else
        {
            //if (GetBlockTypeBelow() == "Acidic")
            //Health -= 2 * HealthLostPerTick;
            //else
            //Health -= HealthLostPerTick;
            Alive = true;
        }




        if (Alive)
        {


            // gather the inputs to the neural network

            // gather the terrain data near the ant
            Vector3 position = GetCurrentBlockPosition();
            int x = Mathf.RoundToInt(position.x);
            int y = Mathf.RoundToInt(position.y + 1);
            int z = Mathf.RoundToInt(position.z);

            // we are going to get the 3d terrain around the ant :) 
            int[,,] blocks = new int[5,5,5];
            for (int i = 0; i< 5; i++)
            {
                int xblock = x - 2 + i;
                for (int j = 0; j < 5; j++)
                {
                    int yblock = y - 2 + j;
                    for (int k = 0; k<5; k++)
                    {
                        int zblock = z - 2 + k;
                        blocks[i, j, k] = GetBlockTypeInt(WorldManager.Instance.GetBlock(xblock, yblock, zblock).BlockType);
                    }
                }
            }

            // flatten blocks!
            int[] blocks1d = Flattener.Flatten<int>(blocks);
            //Debug.Log("LENGTH OF BLOCK BUFFER: " + blocks1d.Length);
            // get the other values

            // distance to the queen:
            int distanceFromQueen = 1000000;
            // health of the queen
            int healthQueen = WorldManager.Instance.queen.Health;

            // health of closest ant
            int healthClosestAnt = 0;
            // distance of closest ant
            int distanceAnt = 1000000;
            Vector3 posClosest = new Vector3();
            // actually set closest ant distance and health
            List<Agent> ants = WorldManager.Instance.ants;
            for (int i = 0; i < ants.Count; i++)
            {
                int dist = Mathf.RoundToInt(Vector3.Distance(this.transform.position, ants[i].transform.position));
                if (dist < distanceAnt)
                {
                    distanceAnt = dist;
                    healthClosestAnt = ants[i].Health;
                    posClosest = ants[i].GetCurrentBlockPosition();
                }
            }

            int queenX, queenY, queenZ, closestX, closestY, closestZ;

            Vector3 queenPos = WorldManager.Instance.queen.GetCurrentBlockPosition();
            Vector3 thisPos = GetCurrentBlockPosition();
            // encode directions of queen and closest ant

            float initialDistance = Vector3.Distance(queenPos, thisPos);


            // should x be lower, equal or same?
            queenX = comparePos(queenPos.x, thisPos.x);
            closestX = comparePos(posClosest.x, thisPos.x);
            // should y be lower, equal or same?
            queenY = comparePos(queenPos.y, thisPos.y);
            closestY = comparePos(posClosest.y, thisPos.y);
            // should z be lower, equal or same?
            queenZ = comparePos(queenPos.z, thisPos.z);
            closestZ = comparePos(posClosest.z, thisPos.z);

            // combine into input

            int[] secondary = { queenX, queenY, queenZ, healthQueen, distanceFromQueen, closestX, closestY, closestZ, healthClosestAnt, distanceAnt, Health };
            //Debug.Log("Length of other variables " +secondary.Length);
            // get output from the neural network

            input = System.Array.ConvertAll(blocks1d.Concatenate(secondary), l=> (float)l);
            //Debug.Log("Concatenated length: "+ input.Length);
            float[] output = network.FeedForward(input); // call to network to feed forward

            int forward = Mathf.RoundToInt(output[0]);
            int backward = Mathf.RoundToInt(output[1]);
            int left = Mathf.RoundToInt(output[2]);
            int right = Mathf.RoundToInt(output[3]);
            int toDig = Mathf.RoundToInt(output[4]);
            int moveUp = Mathf.RoundToInt(output[5]);
            int moveDown = Mathf.RoundToInt(output[6]);
            int shareHealth = Mathf.RoundToInt(output[7]);
            // act accordingly

            // first, parse the movement. remember that this is relative to ant's current position. 

            int deltaX = forward - backward;
            int deltaY = moveUp - moveDown;
            int deltaZ = left - right;

            position.x += deltaX;
            position.y += deltaY;
            position.z += deltaZ;


            if (toDig == 1)
            {
                Dig();
            }

            if (shareHealth == 1)
            {
                ShareHealth();
            }

            // add support for sharing 

            if (GetBlockTypeAt(position) != "Air" && GetBlockTypeAbove(position) == "Air")
            {

                
                // then the movement is valid!
                position.y += 0.75f;
                transform.position = position;

            } else // support for jumps of height 2
            {
                // add an extra delta in Y
                position.y += deltaY;
                if (GetBlockTypeAt(position) != "Air" && GetBlockTypeAbove(position) == "Air")
                {
                    position.y += 0.75f;
                    transform.position = position;
                }
            }

            float finalDistance = Vector3.Distance(WorldManager.Instance.queen.GetCurrentBlockPosition(), GetCurrentBlockPosition());


            if (finalDistance < initialDistance)
            {
                MovedTowardsQueen += 1;
            } else
            {
                MovedTowardsQueen -= 1;
            }

        }

        // add eating mulch block to restore health 
        // - cannot consume if another ant is on the block

        // ants can move between blocks, but cannot move to a block that is greater than 2 units in height diff

        // ants can dig
        // - CANNOT dig a container block

        // if ant on an acidic block, will die 2x faster

        // ants may give some of their health to other ants on same space (zero-sum exchange!)

        // A singular queen must exist


        // No new ants can be created during each evaluation phase (can add as many as needed for new generation)
        // - evaluation phase is "live" phase between generations where ants are generated
        // - can add as many then

        

    }


    /// <summary>
    /// Causes the ant to dig
    /// </summary>
    void Dig()
    {
        string typeToDig = GetBlockTypeBelow();
        // do not "dig" mulch blocks
        if (typeToDig != "Container")
        {
            // if the block is not a container, replace it with air. 

            Vector3 pos = GetCurrentBlockPosition();
            SetBlockAt(pos, air);

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - 1.0f,
                transform.position.z);

            // if it was mulch, replenish health
            if (typeToDig == "Mulch")
            {
                Health += 1000;
            }

        } 
    }

    // These two were borrowed from the UITerrain editor. 
    public void SetBlockAt(Vector3 position, AbstractBlock block)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        SetBlockAt(x, y, z, block);
    }

    void SetBlockAt(int x, int y, int z, AbstractBlock block)
    {
        WorldManager.Instance.SetBlock(x, y, z, block);
    }


    // Gets the type of block currently below an agent. 
    public string GetBlockTypeBelow()
    {
        Vector3 position = GetCurrentBlockPosition();
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        AbstractBlock block = WorldManager.Instance.GetBlock(x,y,z);
        return block.BlockType;
    }

    /// <summary>
    /// Checks for block type AT specific coordinates
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    string GetBlockTypeAt(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);
        AbstractBlock block = WorldManager.Instance.GetBlock(x, y, z);
        return block.BlockType;
    }

    /// <summary>
    /// Checks for the block type ABOVE the block at specified coordinates
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    string GetBlockTypeAbove(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y + 1);
        int z = Mathf.RoundToInt(position.z);
        AbstractBlock block = WorldManager.Instance.GetBlock(x, y, z);
        return block.BlockType;
    }

    /// <summary>
    /// Helper for move.
    /// </summary>
    /// <returns></returns>
    public Vector3 CalculateNextPosition()
    {
        List<Vector3> nextBlockPositions = new List<Vector3>();
        List<Vector3> filteredPositions = new List<Vector3>();

        // gets the current block position
        Vector3 position = GetCurrentBlockPosition();
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);


        // we want to consider 4 directions of movement (forward, back, left, right)
        // of these, we want to also consider 
        // - moving down, for up to 2 blocks
        // - moving up, for up to 2 blocks


        // forward (consider x as forwards for now)
        nextBlockPositions.Add(new Vector3(position.x + 1, position.y, position.z));
        // backwards
        nextBlockPositions.Add(new Vector3(position.x - 1, position.y, position.z));
        // left
        nextBlockPositions.Add(new Vector3(position.x, position.y, position.z + 1));
        // right
        nextBlockPositions.Add(new Vector3(position.x, position.y, position.z - 1));

        // now take everything, add vertical movement possibilities
        int size = nextBlockPositions.Count;
        for (int i = 0; i < size; i++)
        {
            var pos = nextBlockPositions[i];
            nextBlockPositions.Add(new Vector3(pos.x, pos.y - 2, pos.z)); // move down 2
            nextBlockPositions.Add(new Vector3(pos.x, pos.y - 1, pos.z)); // move down 1
            nextBlockPositions.Add(new Vector3(pos.x, pos.y + 1, pos.z)); // move up 1
            nextBlockPositions.Add(new Vector3(pos.x, pos.y + 2, pos.z)); // move up 2
        }

        // validate the positions (ie: there is a block to stand on, with air above it)
        foreach (var pos in nextBlockPositions)
        {
            // add validated position to filteredPositions
            if (GetBlockTypeAt(pos) != "Air" && GetBlockTypeAbove(pos) == "Air")
            {
                filteredPositions.Add(pos);
            }
        }

        if (filteredPositions.Count > 0)
        {        
            // pick from filtered positions:
            int chosenIndex = Random.Range(0, filteredPositions.Count - 1);
            // add offset so that agent appears to be "on" the surface
            Vector3 chosenPos = filteredPositions[chosenIndex];
            chosenPos.y = chosenPos.y +0.75f;

            return chosenPos;
        } else
        {
            return transform.position;
        }




    }


    int comparePos (float a, float b)
    {
        if (a > b)
            return 1;
        else if (a == b)
            return 0;
        else
            return -1;
    }

    /// <summary>
    /// Gets the position that an ant is currently ON TOP OF. 
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurrentBlockPosition()
    {
        Vector3 pos = this.transform.position;
        pos.y = Mathf.Floor(pos.y);
        return pos;

    }

    void ShareHealth()
    {
        int ShareAmount = 1000;
        // find an ant with same position
        // subtract health from this ant
        // add health to other ant
        if (Vector3.Distance(GetCurrentBlockPosition(),WorldManager.Instance.queen.GetCurrentBlockPosition()) < 3)
        {
            this.Health -= ShareAmount;
            WorldManager.Instance.queen.Health += ShareAmount;
            HealthShared += 1;
        } else
        {
            Agent closestAnt = null;
            float distance = 10000000;
            // find the closest ant
            List<Agent> ants = WorldManager.Instance.ants;
            for (int i = 0; i< ants.Count; i++)
            {
                float antDistance = Vector3.Distance(GetCurrentBlockPosition(), ants[i].GetCurrentBlockPosition());
                if (antDistance < distance)
                {
                    distance = antDistance;
                    closestAnt = ants[i];
                }
            }

            if (distance < 5)
            {
                this.Health -= ShareAmount;
                closestAnt.Health += ShareAmount;
            }


        }


    }


    int GetBlockTypeInt(string block)
    {
        switch (block)
        {
            case ("Acidic"):
                return 0;
            case ("Air"):
                return 1;
            case ("Container"):
                return 2;
            case ("Grass"):
                return 3;
            case ("Mulch"):
                return 4;
            case ("Nest"):
                return 6;
            case ("Stone"):
                return 7;
            default: return 8;
        }
    }

    // this is the fitness function!
    public void UpdateFitness()
    {

        Vector3 distanceFromQueen = WorldManager.Instance.queen.GetCurrentBlockPosition();

        int dist = Mathf.RoundToInt(Vector3.Distance(distanceFromQueen, GetCurrentBlockPosition()));

        network.fitness = MovedTowardsQueen + HealthShared; //updates fitness of network for sorting
    }
}
