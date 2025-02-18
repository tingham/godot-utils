using System;
using System.Collections.Generic;

namespace URBANFORT.Utilities.State
{
    public class State
    {
        public List<Transition> Transitions { get; set; } = new List<Transition>();
        public StateMachine Parent { get; set; }

        public Action Enter { get; set; }
        public Action<double> Update { get; set; }
        public Action Exit { get; set; }

        public void AddTransition(State toState, Func<bool> condition)
        {
            if (Transitions.Find(t => t.To == toState && t.From == this) != null)
            {
                throw new Exception($"Transition already exists {this} -> {toState}");
            }
            Transitions.Add(new Transition(this, toState, condition));
        }
    }
}