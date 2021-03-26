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
    public int Health { get; set; } = 1000;

    public bool Alive { get; set; } = true;

    public NeuralNetwork network; 

    private float moveTimer = 0; // makes it so that the ant doesn't just jump around at 30fps

    private float timeAlive = 0;

    public const int HealthLostPerTick = 1;

    private AbstractBlock air = new AirBlock();

    private float[] input = new float[125 + 4];

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
            if (GetBlockTypeBelow() == "Acidic")
                Health -= 2 * HealthLostPerTick;
            else
                Health -= HealthLostPerTick;
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
                int xblock = x - 2;
                for (int j = 0; j < 5; j++)
                {
                    int yblock = y - 2;
                    for (int k = 0; k<5; k++)
                    {
                        int zblock = z - 2 + k;
                        blocks[i, j, k] = GetBlockTypeInt(WorldManager.Instance.GetBlock(xblock, yblock, zblock).BlockType);
                    }
                }
            }


            // get output from the neural network


            // act accordingly



            if (moveTimer > 1)
            {
                transform.position = CalculateNextPosition();
                moveTimer = 0;
            }
            else
            {
                moveTimer += Time.deltaTime;
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


    void Die()
    {
        Destroy(gameObject);
    }


    /// <summary>
    /// Causes the ant to dig
    /// </summary>
    void Dig()
    {
        string typeToDig = GetBlockTypeBelow();
        Debug.Log("digging " + typeToDig);
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
    void SetBlockAt(Vector3 position, AbstractBlock block)
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
    string GetBlockTypeBelow()
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
    Vector3 CalculateNextPosition()
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

    /// <summary>
    /// Gets the position that an ant is currently ON TOP OF. 
    /// </summary>
    /// <returns></returns>
    Vector3 GetCurrentBlockPosition()
    {
        Vector3 pos = this.transform.position;
        pos.y = Mathf.Floor(pos.y);
        return pos;

    }

    void ShareHealth()
    {
        // find an ant with same position
        // subtract health from this ant
        // add health to other ant
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
        network.fitness = timeAlive; //updates fitness of network for sorting
    }
}
