using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FlowManager
{
    public class FlowManager
    {
        private Dictionary<String, FlowData> data;
        public Action<FlowManager> NextAction;
        private Action<FlowManager> StartLoopAction, ExitLoopAction;
        private int level;

        public int Level { get => level;}

        /// <summary>
        /// Flow model
        /// Uses for handle flow internal data
        /// </summary>
        private class FlowData
        {
            public int level { get; }
            public object value { get; set; }

            public FlowData(int level, object value)
            {
                this.level = level;
                this.value = value;
            }
        }

        /// <summary>
        /// Initialize the manager.
        /// </summary>
        /// <param name="first">The first action of the flow</param>
        /// <param name="init_data">The outside data needed for the flow when it running </param>
        public FlowManager(Action<FlowManager> first = null, Dictionary<string, object> init_data = null)
        {
            data = new Dictionary<string, FlowData>();
            NextAction = first;
            level = 0;

            if (init_data != null)
            {
                foreach (KeyValuePair<string, object> element in init_data)
                {
                    setData(element.Key, element.Value);
                }
            }
        }

        /// <summary>
        /// Override or create new variable for the flow.
        /// </summary>
        /// <param name="name">the variable name</param>
        /// <param name="value">variable data</param>
        public void setData(string name, object value)
        {
            if (data.ContainsKey(name))
            {
                FlowData sData = data[name];
                sData.value = value;
                data[name] = sData;
            }
            else
            {
                data.Add(name, new FlowData(level, value));
            }
        }

        /// <summary>
        /// Getting existing variable of the flow.
        /// Note: if the variable not exists in the current context then this method will return null
        /// </summary>
        /// <param name="name">the variable name</param>
        /// <returns>data of type object. or null if the variable name is not existed</return>
        public object getData(string name)
        {
            if (data.ContainsKey(name)) return data[name].value;
            return null;
        }

        /// <summary>
        /// Execute next action in the flow
        /// </summary>
        public void Continue()
        {
            if (NextAction == null) return;
            NextAction(this);
        }

        /// <summary>
        /// Tells the flow to start a loop from current point of execution.
        /// the loop can be nested
        /// Note: the loop must end with EndLoop method to get out of the loop
        /// </summary>
        /// <param name="enumerator">a data structure that can be iterate through</param>
        /// <param name="element_name">The name of the variable which currently iterate to</param>
        /// <param name="exit_flow">an action to be execute when the loop is over</param>
        /// <see cref="EndLoop"/>
        public void StartLoop(IEnumerator enumerator, string element_name, Action<FlowManager> exit_loop)
        {
            if (enumerator.MoveNext())
            {
                this.setData("__loop" + level, enumerator);
                this.setData("__start" + level, NextAction);
                this.setData("__stop" + level, exit_loop);
                this.setData("__el" + level, element_name);
                level++;
                // store loop special point to encounter nested loop
                this.setData(element_name, enumerator.Current);
                this.StartLoopAction = NextAction;
                this.ExitLoopAction = exit_loop;
            }
            else
            {
                this.NextAction = exit_loop;
            }
        }

        /// <summary>
        /// Indicate an end of the loop
        /// Note: this method must be call after calling the StartLoop method
        /// </summary>
        /// <see cref="StartLoop"/>
        public void EndLoop()
        {
            IEnumerator enumerator = this.getData("__loop" + (level-1)) as IEnumerator;
            if (enumerator.MoveNext())
            {
                NextAction = StartLoopAction;
                collectGarbage();
                this.setData(this.getData("__el" + (level-1)) as string, enumerator.Current);
            }
            else
            {
                NextAction = ExitLoopAction;
                collectGarbage();
                level--;
                data.Remove("__loop" + level);
                data.Remove("__le" + level);
                data.Remove("__start" + level);
                data.Remove("__stop" + level);
                StartLoopAction = getData("__start" + (level-1)) as Action<FlowManager>;
                ExitLoopAction = getData("__stop" + (level-1)) as Action<FlowManager>;
                if (NextAction == null && StartLoopAction != null) EndLoop();
            }
        }

        /// <summary>
        /// free all resources in the deepest level used by the flow when that level is no longer be used. e.g. get out of the loop
        /// </summary>
        private void collectGarbage()
        {
            foreach (var item in data.Where(element => element.Value.level >= level).ToList())
            {
                data.Remove(item.Key);
            }
        }
    }
}
