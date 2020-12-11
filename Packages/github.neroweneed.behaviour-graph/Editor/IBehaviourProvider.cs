using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public interface IBehaviourProvider {
        public void Collect(Type behaviourType, Assembly assembly, List<BehaviourInfo> info);
    }
    public struct BehaviourInfo {
        public string identifier;
        public string displayName;
        public Type configurationType;
        public MethodInfo method;
    }
    public class StandardBehaviourGraphProvider : IBehaviourProvider {
        public void Collect(Type behaviourType, Assembly assembly, List<BehaviourInfo> info) {
            foreach (var type in assembly.GetTypes()) {
                CollectBehaviourInfo(info, behaviourType, type);
            }
        }
        private void CollectBehaviourInfo(List<BehaviourInfo> info, Type behaviourType, Type type) {
            if (type.IsSealed && type.IsAbstract && type.GetCustomAttribute<BurstCompileAttribute>() != null) {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                    var behaviourAttributes = method.GetCustomAttributes<BehaviourAttribute>().Where(a => a.BehaviourType == behaviourType);
                    if (method.GetCustomAttribute<BurstCompileAttribute>() != null && behaviourAttributes != null) {
                        foreach (var behaviourAttribute in behaviourAttributes) {
                            info.Add(new BehaviourInfo
                            {
                                identifier = behaviourAttribute.Identifier,
                                displayName = string.IsNullOrEmpty(behaviourAttribute.DisplayName) ? behaviourAttribute.Identifier : behaviourAttribute.DisplayName,
                                configurationType = behaviourAttribute.ConfigurationType,
                                method = method
                            });
                        }
                    }
                }
            }
        }
    }
}