


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Tool
{
    public static class EventBus
    {
        // 存储事件的字典，键为事件名称，值为对应的委托列表
        private static readonly Dictionary<string, List<Action<object>>> eventDictionary = new Dictionary<string, List<Action<object>>>();

        // 注册事件
        public static void RegisterEvent(string eventName, Action<object> eventAction)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = new List<Action<object>>();
            }

            eventDictionary[eventName].Add(eventAction);
        }

        // 注销事件
        public static void UnregisterEvent(string eventName, Action<object> eventAction)
        {
            if (eventDictionary.TryGetValue(eventName, out List<Action<object>> eventActions))
            {
                eventActions.Remove(eventAction);
                if (eventActions.Count == 0)
                {
                    eventDictionary.Remove(eventName);
                }
            }
            else
            {
                throw new KeyNotFoundException($"No event found with the name '{eventName}' to unregister.");
            }
        }

        // 触发事件
        public static void SendEvent(string eventName, object obj = null)
        {
            if (eventDictionary.TryGetValue(eventName, out List<Action<object>> eventActions))
            {
                foreach (var action in eventActions)
                {
                    action?.Invoke(obj);
                }
            }
            else
            {
                //throw new KeyNotFoundException($"No event found with the name '{eventName}'.");
            }
        }
    }
}
