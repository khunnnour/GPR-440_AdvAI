using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/*
 * Lots of code taken from: https://github.com/sploreg/goap
 */

// TODO: learn how to put on own thread
public class GoapPlanner : MonoBehaviour
{
	public static GoapPlanner Instance; // singleton of planner

	public float timeAllowed = 0.0015f;

	private Queue<PlanRequest> _requests; // queue of current requests

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		// Process a plan request if there are plans
		if (_requests.Count > 0)
			ProcessPlanRequest();
	}

	private void ProcessPlanRequest()
	{
		// TODO: make interpretable
		// get time at start of function call
		float startTime = Time.time;
		Stopwatch watch = Stopwatch.StartNew();

		// get plan from front of queue
		PlanRequest planRequest = _requests.Dequeue();

		// find all currently available actions
		HashSet<GoapAction> availableActions = new HashSet<GoapAction>();
		foreach (GoapAction gA in planRequest.agent.AvailableActions)
		{
			if (gA.SatisfiesPreconditions(planRequest.agent))
				availableActions.Add(gA);
		}

		// create new list for the leaves
		List<GraphNode> solutions = new List<GraphNode>();

		// create the world state
		HashSet<Effect> initWorldState = ConvertWorldState(planRequest.initialWorldstate); // start w world state struct
		// add anything from agent
		ApplyAgentToWorldState(ref initWorldState, planRequest.agent);

		// build out the graph
		GraphNode root = new GraphNode(null, 0, planRequest.goals,initWorldState, null);
		bool success = BuildGraph(root, solutions, availableActions, planRequest.goals);

		// return null if no plan was found
		if (!success)
		{
			Debug.LogWarning("NO PLAN");
			return;
		}

		// otherwise find the solution w the cheapest path
		GraphNode cheapest = solutions[0];
		for (int i = 1; i < solutions.Count; i++)
		{
			if (solutions[i].costSoFar < cheapest.costSoFar)
				cheapest = solutions[i];
		}

		// work backwards to create the plan
		List<GoapAction> plan = new List<GoapAction>();
		// leaf is the end of the plan
		GraphNode node = cheapest;
		while (node != null)
		{
			plan.Insert(0, node.action); // insert at head to reverse order
			node = node.parent;
		}

		// give plan to agent
		planRequest.agent.SetPlan(plan);

		// debug out how long whole process took
		Debug.Log(planRequest.agent.name + " plan took " + watch.ElapsedMilliseconds + "ms");
	}

	private bool BuildGraph(GraphNode parent, List<GraphNode> leaves, HashSet<GoapAction> actionsLeft,
		HashSet<Effect> goalsLeft)
	{
		bool foundSolution = false;

		// check thru all actions
		foreach (GoapAction action in actionsLeft)
		{
			// check if action is currently usable
			if (action.SatisfiesPreconditions(parent.state))
			{
				// create new state and apply action to it
				HashSet<Effect> currState = parent.state;
				ApplyActionToWorldState(ref currState, action);

				// remove completed goals
				HashSet<Effect> newGoalsLeft = parent.goalsLeft;
				ApplyActionToGoals(ref newGoalsLeft, action);

				// create new graph node
				GraphNode node = new GraphNode(parent, parent.costSoFar + 1, newGoalsLeft, currState, action);

				// check if solution
				if (newGoalsLeft.Count == 0)
				{
					// -> YES: new solution; add to leaves
					leaves.Add(node);
					foundSolution = true; // set to true so you can break out
				}
				else
				{
					// -> NO: call build graph again
					// get the action list less the one just performed
					HashSet<GoapAction> lessActions = actionsLeft;
					actionsLeft.Remove(action);
					if (BuildGraph(node, leaves, lessActions, newGoalsLeft))
						foundSolution = true;
				}
			}
		}

		return foundSolution;
	}

	// converts world state struct to a hashset of effects
	private HashSet<Effect> ConvertWorldState(WorldState state)
	{
		// create new world state hash set
		HashSet<Effect> newState = new HashSet<Effect>();

		// go thru every var and apply relevant effect --
		// if you have a bunch of a resource then there's no need to make it
		if (state._foodAmt > 3)
			newState.Add(Effect.MAKE_FOOD);
		if (state._weapAmt > 2)
			newState.Add(Effect.MAKE_WEAPON);
		if (state._oreAmt > 3)
			newState.Add(Effect.MAKE_ORE);

		return newState;
	}

	// apply information on agent requesting plan to the world state
	private void ApplyAgentToWorldState(ref HashSet<Effect> wS, GoapAgent agent)
	{
		if (agent.HasWeapon)
			wS.Add(Effect.EQUIP_WEAPON);
	}
	
	private void ApplyActionToWorldState(ref HashSet<Effect> wS, GoapAction action)
	{
		foreach (Effect effect in action.Effects)
		{
			switch (effect)
			{
				case Effect.MAKE_ORE:
					if (!wS.Contains(Effect.MAKE_ORE))
						wS.Add(Effect.MAKE_ORE);
					break;
				case Effect.MAKE_FOOD:
					if (!wS.Contains(Effect.MAKE_FOOD))
						wS.Add(Effect.MAKE_FOOD);
					break;
				case Effect.MAKE_WEAPON:
					if (!wS.Contains(Effect.MAKE_WEAPON))
						wS.Add(Effect.MAKE_WEAPON);
					break;
			}
		}
	}

	// removes any effects an action has from goal(s)
	private void ApplyActionToGoals(ref HashSet<Effect> goals, GoapAction action)
	{
		foreach (Effect effect in action.Effects)
		{
			if (goals.Contains(effect)) goals.Remove(effect);
		}
	}

	/// <summary>
	/// Request a plan from the GOAP Planner 
	/// </summary>
	/// <param name="agent">The agent requesting the plan</param>
	/// <param name="desiredEffects">(The Goal) What the desired outcome is</param>
	public void RequestPlan(GoapAgent agent, HashSet<Effect> desiredEffects)
	{
		// enqueue a new plan request 
		_requests.Enqueue(new PlanRequest(agent, desiredEffects, new WorldState()));
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
	public WorldState initialWorldstate; // world state at time of request
    public GraphNode root;  // reference to starting node
    public GraphNode lastParent;  // reference to last non-leaf node processed
    public List<GraphNode> leaves; // reference to all nodes that end in a solution

    public PlanRequest(GoapAgent a, HashSet<Effect> g, WorldState wS)
    {
		started=false;

		initialWorldstate = wS;
        agent = a;
        goals = g;
        root = null;

        plan = new List<GoapAction>();
        leaves = new List<GraphNode>();
    }
}

class GraphNode
{
    public GraphNode parent;
    public int costSoFar;
    public HashSet<Effect> goalsLeft;
    public HashSet<Effect> state;
    public GoapAction action;

    public GraphNode(GraphNode p, int c, HashSet<Effect> g, HashSet<Effect> s, GoapAction a)
    {
        parent = p;
        costSoFar = c;
        goalsLeft = g;
        state = s;
        action = a;
    }
}