using Antymology.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Agent
{
    public int MaxHealth = 10000;
    public AbstractBlock Nest = new NestBlock();
    public float BuildTimer = 0;
    public int NestBlocksBuilt = 0;
    public float DigTimer = 0;
    public NeuralNetwork queenNetwork;
    // Start is called before the first frame update
    void Start()
    {
        Health = 5000;
    }

    // Update is called once per frame
    void Update()
    {

        if (Health < 0)
        {
            Alive = false;
        }
        else
        {
            if (GetBlockTypeBelow() == "Acidic")
                Health -= 2 * HealthLostPerTick;
            else
                Health -= HealthLostPerTick;
        }

        if (!Alive)
        {
            // start the next generation if the queen dies!
            WorldManager.Instance.GenerateData();
            WorldManager.Instance.GenerateChunks();
            WorldManager.Instance.GenerateAnts();
            
        } else
        {

            if (moveTimer > 2)
            {
                moveTimer = 0;
                // move around & try to run into another ant
                this.transform.position = CalculateNextPosition();

            } else if (BuildTimer > 4)
            {
                if (Health > 200)
                {
                    NestBlocksBuilt++;
                    // place a nest block at position
                    Vector3 pos = GetCurrentBlockPosition();
                    pos.y += 1; // add one, since we want it to create the block WHERE it is, not block under it
                    SetBlockAt(pos, Nest);


                    transform.position = new Vector3(
                        transform.position.x,
                        transform.position.y + 1.0f,
                        transform.position.z);

                    Health = Mathf.RoundToInt(Health * 0.67f); // lose 33% of health
                }

                BuildTimer = 0;
            } else if (DigTimer > 4.5)
            {
                if (GetBlockTypeBelow() != "Nest")
                    Dig();
            }
            else 
            {
                moveTimer += Time.deltaTime;
                BuildTimer += Time.deltaTime;
            }

            // the queen will never share health, because she is the queen and deserves simps




        }
        
    }
}
