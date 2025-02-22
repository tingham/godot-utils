using System;

namespace URBANFORT.Utilities.State
{
    public class Transition
    {
        public string Name {
            get {
                return $"{From.Name} -> {To.Name}";
            }
        }

        public State From { get; set; }
        public State To { get; set; }
        public Func<bool> Condition { get; set; }

        public Transition(State from, State to, Func<bool> condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }
}