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

		// find all currently available actions
		HashSet<GoapAction> availableActions = new HashSet<GoapAction>();
		foreach(GoapAction gA in agent.AvailableActions)
		{
			if(gA.SatisfiesPreconditions(agent))
				availableActions.Add(gA);
		}
		
		// create new list for the leaves
		List<GraphNode> solutions = new List<GraphNode>();
		
        // build out the graph
		GraphNode root = new GraphNode();
		bool success = BuildGraph(root, solutions, availableActions, planRequest.goals);
		
		// return null if no plan was found
		if(!success)
		{
			Debug.LogWarning("NO PLAN");
			return null;
		}
		
		// otherwise find the solution w the cheapest path
		GraphNode cheapest = solutions[0];
		for(int i=1;i<solutions.Count;i++)
		{
			if(solutions[i].costSoFar < cheapest.costSoFar)
				cheapest = solutions[i];
		}
		
		// work backwards to create the plan
		
		// give plan to agent
		
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
	public bool started;
	
    // basic request data
	public GoapAgent agent; // agent requesting plan
    public HashSet<Effect> goals; // the desired effects
    public List<GoapAction> plan; // current state of the plan

	// planning data
    public GraphNode root;  // reference to starting node
    public GraphNode lastParent;  // reference to last non-leaf node processed
    public List<GraphNode> leaves; // reference to all nodes that end in a solution

    public PlanRequest(GoapAgent a, HashSet<Effect> g, WorldState wS)
    {
		started=false;
		
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