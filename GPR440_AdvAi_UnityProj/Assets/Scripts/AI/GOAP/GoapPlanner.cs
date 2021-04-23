using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Lots of code taken from: 
 */

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
            ProcessPlanRequest();
    }

    private void ProcessPlanRequest()
    {
        // get time at start of function call
        float startTime = Time.time;

        // get plan from front of queue
        PlanRequest planRequest = _requests.Dequeue();

        // build out the graph
        // -> check if too much time has elapsed
        //    -> if too much time elapsed, save world graph and re-enter queue
        // when finished pop off graph and send plan to agent
    }
    private bool BuildGraph(GraphNode parent,List<GraphNode> leaves, HashSet<GoapAction> actionsLeft, HashSet<Effect> goalsLeft)
    {
        bool foundSolution = false;
        
        // check thru all actions
        // -> check if agent fulfils the requirements of action
        //    -> YES: apply action to world state
        //       -> check if reached goal
        //          -> YES: new solution; add to leaves
        //          -> NO: call build graph again
        
        return foundSolution;
    }
    
    /// <summary>
    /// Request a plan from the GOAP Planner 
    /// </summary>
    /// <param name="agent">The agent requesting the plan</param>
    /// <param name="desiredEffects">(The Goal) What the desired outcome is</param>
    public void RequestPlan(GoapAgent agent, HashSet<Effect> desiredEffects)
    {
        // enqueue a new plan request 
        _requests.Enqueue(new PlanRequest(agent, desiredEffects,new WorldState()));
    }
}

class PlanRequest
{
    public GoapAgent agent; // agent requesting plan
    public HashSet<Effect> goals; // the desired effects
    public List<GoapAction> plan; // current state of the plan

    public GraphNode root;  // reference to starting node
    public List<GraphNode> leaves; // reference to all nodes that end in a solution

    public PlanRequest(GoapAgent a, HashSet<Effect> g, WorldState wS)
    {
        agent = a;
        goals = g;
        root = new GraphNode(null, 0, g, null);
        
        plan = new List<GoapAction>();
        leaves = new List<GraphNode>();
    }
}

class GraphNode
{
    public GraphNode parent;
    public int costSoFar;
    public HashSet<Effect> goalsLeft;
    public GoapAction action;

    public GraphNode(GraphNode p, int c, HashSet<Effect> g, GoapAction a)
    {
        parent = p;
        costSoFar = c;
        goalsLeft = g;
        action = a;
    }
}