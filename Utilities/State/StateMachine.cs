using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace URBANFORT.Utilities.State
{
    public class StateMachine
    {
        public State DefaultState { get; set; }
        public State CurrentState { get; set; }
        public Dictionary<string, object> Data { get; set; } = [];

        public delegate void TransitionHandler(Transition transition);
        public event TransitionHandler OnTransition;

        public List<State> States
        {
            get
            {
                // Compile a unique list of states from all transitions
                var states = new List<State> { CurrentState };
                states.AddRange(CurrentState.Transitions.Select(t => t.To));
                var leafStates = new List<State>();
                foreach (var state in states) {
                    foreach(var transition in state.Transitions) {
                        if (!states.Contains(transition.To)) {
                            leafStates.Add(transition.To);
                        }
                    }
                }
                states.AddRange(leafStates);
                return [.. states.Distinct()];
            }
        }

        public State Append (string name, Action enter, Action<double> update, Action exit)
        {
            var state = new State {
                Name = name,
                Enter = enter,
                Update = update,
                Exit = exit,
                Parent = this
            };
            return state;
        }

        public void Update(double delta)
        {
            if (DefaultState == null) {
                throw new Exception("Default state not set");
            }
            if (CurrentState == null) {
                ChangeState(DefaultState);
                return;
            }

            if (CurrentState != null)
            {
                CurrentState.Update(delta);
            }
            foreach (var transition in CurrentState.Transitions)
            {
                if (transition.Condition())
                {
                    if (transition.To == null)
                    {
                        ChangeState(DefaultState);
                    }
                    else
                    {
                        OnTransition?.Invoke(transition);
                        ChangeState(transition.To);
                    }
                    break;
                }
            }
        }

        public void ChangeState(State newState)
        {
            if (newState != CurrentState)
            {
                if (CurrentState != null)
                {
                    CurrentState.Exit();
                }
                newState.Enter();
            }
            CurrentState = newState;
        }

        public float GetStateFloat (string key, float defaultValue = 0f)
        {
            if (Data.TryGetValue(key, out object value))
            {
                return (float)value;
            }
            return defaultValue;
        }

        public bool GetStateBoolean (string key, bool defaultValue = false)
        {
            if (Data.TryGetValue(key, out object value))
            {
                return (bool)value;
            }
            return defaultValue;
        }

        public object GetStateObject (string key, object defaultValue = null)
        {
            if (Data.TryGetValue(key, out object value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}