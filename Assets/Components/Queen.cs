using Antymology.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Agent
{
    // Start is called before the first frame update
    void Start()
    {
        
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
            WorldManager.Instance.GenerateAnts();
        }
        
    }
}
