using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: learn how to put on own thread
public class GoapPlanner : MonoBehaviour
{
    public static GoapPlanner instance; // singleton of planner
    
    public float timeAllowed = 0.0015f;
    
    private Queue<PlanRequest> _requests; // queue of current requests

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Process a plan request if there are plans
        if (_requests.Count > 0)
            ProcessPlan();
    }

    private void ProcessPlan()
    {
        // get time at start of function call
        
        // get plan from front of queue
        
        // continue to build graph
        //  -> check if too much time has elapsed
        //      -> if too much time elapsed, save world graph and re-enter queue
        // when finished pop off graph and send plan to agent
    }
    
    /// <summary>
    /// Request a plan from the GOAP Planner 
    /// </summary>
    /// <param name="agent">The agent requesting the plan</param>
    /// <param name="desiredEffect">(The Goal) What the desired outcome is</param>
    public void RequestPlan(GoapAgent agent, Effect desiredEffect)
    {
        // enqueue a new plan request 
        _requests.Enqueue(new PlanRequest(agent, desiredEffect));
    }
}

class PlanRequest
{
    public GoapAgent agent; // agent requesting plan
    public Effect goal; // the desired effects
    public List<GoapAction> plan; // current state of the plan
    // a variable to hold world states/graph

    public PlanRequest(GoapAgent a, Effect g)
    {
        agent = a;
        goal = g;

        plan = new List<GoapAction>();
    }
}