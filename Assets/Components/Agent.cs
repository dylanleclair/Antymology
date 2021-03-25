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
    public int Health { get; set; } = 10000;

    //public WorldManager world {get;set;}
    private AbstractBlock air = new AirBlock();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //world = GameObject.FindGameObjectWithTag("GameController").GetComponent<WorldManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        // Take away some health
        
        if (Health < 0)
        {
            Die();
        } else
        {
            Health -= 10;
        }

        // Subtract twice the health if on acidic block
        if (GetBlockTypeBelow() == "Acidic")
        {
            Health -= 10;
        }


        Dig();
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

        } 
    }

    /// <summary>
    /// A special case of dig
    /// </summary>
    void Consume()
    {
        string typeToDig = GetBlockTypeBelow();
        // do not "dig" mulch blocks
        if (typeToDig == "Mulch")
        {
            // if the block is not a container, replace it with air. 
            Vector3 pos = GetCurrentBlockPosition();
            SetBlockAt(pos, air);

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - 1.0f,
                transform.position.z);

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
    /// Helper for move.
    /// </summary>
    /// <returns></returns>
    Vector3 CalculateNextPosition()
    {
        List<Vector3> nextBlockPositions = new List<Vector3>();
        // gets the current block position
        Vector3 position = GetCurrentBlockPosition();
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);


        // we want to consider 4 directions of movement (forward, back, left, right)
        // of these, we want to also consider 
        // - moving down, for up to 2 blocks
        // - moving up, for up to 2 blocks


        // forward

        // backwards

        // left

        // right

        // now take everything, add vertical movement possibilities

        // validate the positions (ie: there is a block to stand on, with air above it)


        return new Vector3();
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


}
