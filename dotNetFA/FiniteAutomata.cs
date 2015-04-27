using System;
using System.Text;
using System.Collections.Generic;

namespace dotNetFA
{
    #region Finite Autonoma

    /// <summary>
    /// Implements the FA Transition definition class
    /// </summary>
    /// <typeparam name="T">Generic IComparable type T</typeparam>
    public class Transition<T> : ICloneable, IEquatable<Transition<T>>, IComparable<Transition<T>> where T : IComparable
    {
        /// <summary>
        /// The initial/previous state before T Symbol
        /// </summary>
        public int PreviousState
        {
            get;
            set;
        }

        /// <summary>
        /// Transition trigger symbol
        /// </summary>
        public T Symbol
        {
            get;
            set;
        }

        /// <summary>
        /// The final/next state after T Symbol
        /// </summary>
        public int NextState
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a Transition object
        /// </summary>
        /// <param name="initialState">The initial state</param>
        /// <param name="symbol">The trigger symbol</param>
        /// <param name="nextState">The final/next state</param>
        public Transition(int initialState, T symbol, int nextState)
        {
            this.PreviousState = initialState;
            this.NextState = nextState;
            this.Symbol = symbol;
        }

        /// <summary>
        /// Implementation of the IEquatable interface
        /// </summary>
        /// <param name="other">Other transition to be compared to</param>
        /// <returns>True if it is equall and false otherwise</returns>
        public bool Equals(Transition<T> other)
        {
            return (this.NextState == other.NextState && this.PreviousState == other.PreviousState && this.Symbol.CompareTo(other.Symbol) == 0);
        }

        /// <summary>
        /// Implementation of the IComparable interface
        /// </summary>
        /// <param name="other">Other transition to be compared to</param>
        /// <returns>Returns 0 if it is equal. 1 or -1 otherwise</returns>
        public int CompareTo(Transition<T> other)
        {
            if (this.PreviousState != other.PreviousState)
            {
                return this.PreviousState - other.PreviousState;
            }

            if (this.Symbol.CompareTo(other.Symbol) != 0)
            {
                return this.Symbol.CompareTo(other.Symbol);
            }

            if (this.NextState != other.NextState)
            {
                return this.NextState - other.NextState;
            }

            return 0;
        }

        /// <summary>
        /// Implements the IClonable interface
        /// </summary>
        /// <returns>Returns a clone of the Transition object</returns>
        public object Clone()
        {
            return new Transition<T>(this.PreviousState, this.Symbol, this.NextState);
        }
    }

    /// <summary>
    /// Implements a collection of Transitions
    /// </summary>
    /// <typeparam name="T">Generic IComparable type T</typeparam>
    public class TransitionsCollection<T> : List<Transition<T>> where T : IComparable
    {
    }

    /// <summary>
    /// Implements an abstract Finite Automata class
    /// This is a base class and can't be used by it's one. Defines the base behaviour of Finite Automata
    /// </summary>
    /// <typeparam name="T">Generic IComparable type T</typeparam>
    public abstract class FA<T> where T : IComparable
    {
        /// <summary>
        /// Gets or sets the transitions the FA.
        /// </summary>
        public TransitionsCollection<T> Transitions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the FA initial state
        /// </summary>
        public int InitialState
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the FA accepting/final states
        /// </summary>
        public List<int> AcceptingStates
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the list of Symbols (Alphabet) of the FA
        /// </summary>
        public virtual List<T> Symbols
        {
            get
            {
                List<T> result = new List<T>();

                foreach (Transition<T> t in this.Transitions)
                {
                    if (!result.Contains(t.Symbol))
                    {
                        result.Add(t.Symbol);
                    }
                }

                result.Sort();

                return result;
            }
        }

        /// <summary>
        /// Gets the next available/free state in the FA
        /// </summary>
        public int NextAvailableState
        {
            get
            {
                int result = 0;
                foreach (Transition<T> t in this.Transitions)
                {
                    result = Math.Max(result, t.PreviousState);
                    result = Math.Max(result, t.NextState);
                }

                foreach (int acc in this.AcceptingStates)
                {
                    result = Math.Max(result, acc);
                }

                return ++result;
            }
        }

        /// <summary>
        /// Gets the list of internal states of the FA
        /// </summary>
        public List<int> States
        {
            get
            {
                List<int> result = new List<int>();

                foreach (Transition<T> t in this.Transitions)
                {
                    if (!result.Contains(t.NextState))
                    {
                        result.Add(t.NextState);
                    }

                    if (!result.Contains(t.PreviousState))
                    {
                        result.Add(t.PreviousState);
                    }
                }

                foreach (int acc in this.AcceptingStates)
                {
                    if (!result.Contains(acc))
                    {
                        result.Add(acc);
                    }
                }

                if (!result.Contains(this.InitialState))
                {
                    result.Add(this.InitialState);
                }

                return result;
            }
        }

        /// <summary>
        /// Initializes the FA
        /// </summary>
        public FA()
        {
            this.InitialState = 0;
            this.Transitions = new TransitionsCollection<T>();
            this.AcceptingStates = new List<int>();
        }

        /// <summary>
        /// Gets the state transtition in the FA given an starting state and trigger symbol
        /// </summary>
        /// <param name="state">Starting state</param>
        /// <param name="symbol">Trigger symbol</param>
        /// <returns>Returns the next state. If the transitions is not defined returns -1</returns>
        public int GetNextState(int state, T symbol)
        {
            foreach (Transition<T> t in this.Transitions)
            {
                if (t.PreviousState == state && t.Symbol.CompareTo(symbol) == 0)
                {
                    return t.NextState;
                }
            }

            return -1;
        }

        /// <summary>
        /// Offsets all internal states based on = initial state + offset
        /// </summary>
        /// <param name="newInitial">The offset from the <see cref="InitialState">InitialState</see>/></param>
        public void OffsetStates(int newInitial)
        {
            int offset = newInitial - this.InitialState;

            this.InitialState = offset;

            for (int i = 0; i < this.Transitions.Count; i++)
            {
                this.Transitions[i].PreviousState += offset;
                this.Transitions[i].NextState += offset;
            }

            for (int i = 0; i < this.AcceptingStates.Count; i++)
            {
                this.AcceptingStates[i] += offset;
            }
        }

        /// <summary>
        /// Renumbers an internal state
        /// </summary>
        /// <param name="oldNumber">State old number</param>
        /// <param name="newNumber">State new number</param>
        public void RenumberState(int oldNumber, int newNumber)
        {
            if (this.InitialState == oldNumber)
            {
                this.InitialState = newNumber;
            }

            for (int i = 0; i < this.Transitions.Count; i++)
            {
                if (this.Transitions[i].PreviousState == oldNumber)
                {
                    this.Transitions[i].PreviousState = newNumber;
                }

                if (this.Transitions[i].NextState == oldNumber)
                {
                    this.Transitions[i].NextState = newNumber;
                }
            }

            if (this.AcceptingStates.Contains(oldNumber))
            {
                this.AcceptingStates[this.AcceptingStates.IndexOf(oldNumber)] = newNumber;
            }
        }

        /// <summary>
        /// Overrides the ToString method.
        /// </summary>
        /// <returns>Returns the FA in a pretty-print form</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            this.Transitions.Sort();

            foreach (Transition<T> t in this.Transitions)
            {
                string initial = string.Format("({0})", t.PreviousState);
                string final = string.Format("({0})", t.NextState);

                initial = this.InitialState == t.PreviousState ? ">" + initial : initial;
                initial = this.AcceptingStates.Contains(t.PreviousState) ? "(" + initial + ")" : initial;

                final = this.InitialState == t.NextState ? ">" + final : final;
                final = this.AcceptingStates.Contains(t.NextState) ? "(" + final + ")" : final;

                sb.AppendLine(string.Format("{0}->{1}->{2}", initial, t.Symbol, final));
            }

            return sb.ToString();
        }
    }

    #endregion

    #region Non deterministic finite automata

    /// <summary>
    /// Implements a Non Determinitic Finite Automata class
    /// </summary>
    /// <typeparam name="T">Generic IComparable type T</typeparam>
    public sealed class NFA<T> : FA<T>, ICloneable where T : IComparable
    {
        /// <summary>
        /// Gets the Epsilon value for type T
        /// </summary>
        public T Epsilon
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of Symbols (Alphabet) of the NFA 
        /// </summary>
        public override List<T> Symbols
        {
            get
            {
                List<T> result = new List<T>();

                foreach (Transition<T> t in this.Transitions)
                {
                    if (t.Symbol.CompareTo(this.Epsilon) != 0 && !result.Contains(t.Symbol))
                    {
                        result.Add(t.Symbol);
                    }
                }

                result.Sort();

                return result;
            }
        }

        /// <summary>
        /// Initializes the NFA class and defines the value of <see cref="Epsilon"/>
        /// </summary>
        /// <param name="emptySymbol">Generic type T value of Epsilon</param>
        public NFA(T emptySymbol)
            : base()
        {
            this.Epsilon = emptySymbol;
        }

        /// <summary>
        /// Initializes the NFA class and defines the value of <see cref="Epsilon"/>
        /// </summary>
        /// <param name="emptySymbol">Generic type T value of Epsilon</param>
        /// <param name="symbol">Defines an initial transition given symbol</param>
        public NFA(T emptySymbol, T symbol)
            : base()
        {
            this.Epsilon = emptySymbol;
            int nextavail = this.NextAvailableState;
            this.Transitions.Add(new Transition<T>(this.InitialState, symbol, nextavail));
            this.AcceptingStates.Add(nextavail);
        }

        /// <summary>
        /// Performs the concatenation of two NFA's
        /// </summary>
        /// <param name="nfa">NFA to be concatenated</param>
        /// <returns>The concatenated NFA</returns>
        public NFA<T> And(NFA<T> nfa)
        {
            NFA<T> result = this.Clone() as NFA<T>;
            NFA<T> andparcel = nfa.Clone() as NFA<T>;
            int nextavail = result.NextAvailableState;
            andparcel.OffsetStates(nextavail - 1);
            result.Transitions.AddRange(andparcel.Transitions);
            result.AcceptingStates.Clear();
            result.AcceptingStates.AddRange(andparcel.AcceptingStates);

            return result;
        }

        /// <summary>
        /// Performs the alternation of two NFA's
        /// </summary>
        /// <param name="nfa">NFA to be alternation</param>
        /// <returns>The alternation NFA</returns>
        public NFA<T> Or(NFA<T> nfa)
        {
            NFA<T> result = this.Clone() as NFA<T>;
            NFA<T> orparcel = nfa.Clone() as NFA<T>;
            int nextavail = result.NextAvailableState;
            //first state will be reconverted (avoid state number loss)
            orparcel.OffsetStates(nextavail - 1);
            orparcel.RenumberState(orparcel.InitialState, result.InitialState);
            orparcel.RenumberState(orparcel.AcceptingStates[0], result.AcceptingStates[0]);
            result.Transitions.AddRange(orparcel.Transitions);

            return result;
        }

        /// <summary>
        /// Performs the kleene/star operation of the NFA
        /// </summary>
        /// <returns>The kleene/star NFA</returns>
        public NFA<T> Kleene()
        {
            NFA<T> result = this.Clone() as NFA<T>;
            result.OffsetStates(1);
            int nextavail = result.NextAvailableState;
            result.Transitions.Add(new Transition<T>(0, this.Epsilon, 1));
            result.Transitions.Add(new Transition<T>(result.AcceptingStates[0], this.Epsilon, nextavail));
            result.Transitions.Add(new Transition<T>(nextavail, this.Epsilon, 1));
            result.Transitions.Add(new Transition<T>(0, this.Epsilon, nextavail));
            result.AcceptingStates.Clear();
            result.AcceptingStates.Add(nextavail);
            result.InitialState = 0;

            return result;
        }

        /// <summary>
        /// Performs the replacement of all Transitions given a symbol by a full NFA
        /// </summary>
        /// <param name="symbol">Generic type T value of the trigger to be replaced</param>
        /// <param name="replacement">The NFA be inserted</param>
        /// <returns>A NFA with all the replacemente in place</returns>
        public NFA<T> ReplaceTransitions(T symbol, NFA<T> replacement)
        {
            NFA<T> result = this.Clone() as NFA<T>;

            for (int i = 0; i < result.Transitions.Count; i++)
            {
                if (result.Transitions[i].Symbol.CompareTo(symbol) == 0)
                {
                    NFA<T> insert = replacement.Clone() as NFA<T>;
                    insert.OffsetStates(result.NextAvailableState);
                    insert.RenumberState(insert.InitialState, result.Transitions[i].PreviousState);
                    insert.RenumberState(insert.AcceptingStates[0], result.Transitions[i].NextState);
                    result.Transitions.AddRange(insert.Transitions);
                    result.Transitions.RemoveAt(i);
                    i--;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a list of the Epsilon-Closures for a given state
        /// </summary>
        /// <param name="state">The initial state</param>
        /// <returns>A list of states reachable via Epsilon symbols</returns>
        public List<int> EpsilonClosure(int state)
        {
            List<int> result = new List<int>();
            RecursiveEpsilonClosure(state, result);

            return result;
        }

        /// <summary>
        /// internal recursive Epsilon-Closure method
        /// </summary>
        /// <param name="state">The initial state</param>
        /// <param name="result">A list of states reachable via Epsilon symbols</param>
        private void RecursiveEpsilonClosure(int state, List<int> result)
        {
            result.Add(state);

            foreach (Transition<T> t in this.Transitions)
            {
                if (t.Symbol.CompareTo(this.Epsilon) == 0 && t.PreviousState == state)
                {
                    if (!result.Contains(t.NextState))
                    {
                        RecursiveEpsilonClosure(t.NextState, result);
                    }
                }
            }

            result.Sort();
        }

        /// <summary>
        /// Implementation of the IClonable interface
        /// </summary>
        /// <returns>Returns a clone of the NFA object</returns>
        public object Clone()
        {
            NFA<T> result = new NFA<T>(this.Epsilon);
            result.Transitions.Clear();
            foreach (Transition<T> t in this.Transitions)
            {
                Transition<T> clonet = new Transition<T>(t.PreviousState, t.Symbol, t.NextState);
                if (!result.Transitions.Contains(clonet))
                {
                    result.Transitions.Add(new Transition<T>(t.PreviousState, t.Symbol, t.NextState));
                }
            }
            result.AcceptingStates.AddRange(this.AcceptingStates.ToArray());
            result.InitialState = this.InitialState;

            return result;
        }
    }

    #endregion

    #region Deterministic finite automata

    /// <summary>
    /// Implementation of a custom IEqualityComparer for DFA Subsets
    /// </summary>
    public class SubsetEqualityComparer : IEqualityComparer<List<int>>
    {
        /// <summary>
        /// Implementation of the IEqualityComparer
        /// </summary>
        /// <param name="x">List of states</param>
        /// <param name="y">List of states</param>
        /// <returns>True if both lists have the same elements (order is ignored) or false otherwise</returns>
        public bool Equals(List<int> x, List<int> y)
        {
            x.Sort();
            y.Sort();

            if (x == null || y == null)
            {
                return false;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            foreach (int i in x)
            {
                if (!y.Contains(i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Implementation of the IEqualityComparer
        /// </summary>
        /// <param name="obj">List of states</param>
        /// <returns>Returns a sum of all states</returns>
        public int GetHashCode(List<int> obj)
        {
            int result = 0;

            foreach (int i in obj)
            {
                result += i;
            }

            return result;
        }
    }

    /// <summary>
    /// Implements a Determinitic Finite Automata class
    /// </summary>
    /// <typeparam name="T">Generic IComparable type T</typeparam>
    public sealed class DFA<T> : FA<T> where T : IComparable
    {
        /// <summary>
        /// Inernal dicionary of NFA state-subsets and corresponding DFA states
        /// </summary>
        private Dictionary<List<int>, int> _SubSets = new Dictionary<List<int>, int>(new SubsetEqualityComparer());

        /// <summary>
        /// Initializes an empty DFA
        /// </summary>
        public DFA()
            : base()
        {
        }

        /// <summary>
        /// Initializes a DFA and generates all the states and transitions given a NFA
        /// </summary>
        /// <param name="nfa">NFA from which the DFA will be populated</param>
        public DFA(NFA<T> nfa)
            : base()
        {
            List<T> symbols = nfa.Symbols;
            this.BuildSubset(nfa, nfa.InitialState);
        }

        /// <summary>
        /// Performs the NFA to DFA convertion (recursive)
        /// </summary>
        /// <param name="nfa">The NFA object to convert</param>
        /// <param name="start">The strting state</param>
        private void BuildSubset(NFA<T> nfa, int start)
        {
            Stack<List<int>> unprocessedSubsets = new Stack<List<int>>();
            int availableState = 0;
            List<int> startPoint = nfa.EpsilonClosure(start);
            unprocessedSubsets.Push(startPoint);
            _SubSets[startPoint] = availableState++;
            if (startPoint.Contains(nfa.AcceptingStates[0]) && !this.AcceptingStates.Contains(_SubSets[startPoint]))
            {
                this.AcceptingStates.Add(_SubSets[startPoint]);
            }

            while (unprocessedSubsets.Count > 0)
            {
                List<int> subset = unprocessedSubsets.Pop();

                for (int i = 0; i < subset.Count; i++)
                {
                    foreach (T symbol in nfa.Symbols)
                    {
                        int nextstate = nfa.GetNextState(subset[i], symbol);
                        if (nextstate >= 0)
                        {
                            List<int> newsubset = nfa.EpsilonClosure(nextstate);
                            if (!_SubSets.ContainsKey(newsubset))
                            {
                                unprocessedSubsets.Push(newsubset);
                                _SubSets[newsubset] = availableState++;
                            }
                            this.Transitions.Add(new Transition<T>(_SubSets[subset], symbol, _SubSets[newsubset]));
                            if (newsubset.Contains(nfa.AcceptingStates[0]) && !this.AcceptingStates.Contains(_SubSets[newsubset]))
                            {
                                this.AcceptingStates.Add(_SubSets[newsubset]);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion
}
