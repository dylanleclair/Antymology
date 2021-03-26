using Antymology.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Agent
{
    // Start is called before the first frame update
    void Start()
    {
        Health = 2000;
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
                Health -= 2 * HealthLostPerTick;
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

            } else
            {
                moveTimer += Time.deltaTime;
            }


        }
        
    }
}
