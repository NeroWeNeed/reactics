using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Core.Battle;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Reactics.Core.Unit;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class EffectGraphModule : BaseObjectGraphNodeModule, IVariableProvider {
        public static readonly Type[] SuperTypes = { typeof(IEffect<Point>), typeof(IEffect<MapBodyDirection>), typeof(IEffect<MapBodyTarget>) };
        public const string PortClassName = "effect-graph-node-port";
        public override string NodeClassName { get; } = "effect";
        public IObjectGraphVariableProvider[] VariableTypes { get; } = new IObjectGraphVariableProvider[] { new ObjectGraphVariableProvider(typeof(MapBody)) {
            validator = ValidateVariables
        } };


        public EffectGraphModule() : this(new Settings
        {
            portName = "Effects",
            portType = typeof(IEffect),
            portClassName = PortClassName,
            portColor = new Color(1f, 0.498f, 0f),
            superTypes = SuperTypes,
            serializer = new ObjectGraphAssetSerializer<IEffect, EffectAsset>()

            /* serializer = new BehaviourModelObjectGraphSerializer<EffectModel, IEffect, LinearEffect, VariableEffect>
            {
                properties = new BehaviourModelProperties("effect")
            } */
        }) {

        }
        private static void ValidateVariables(ObjectGraphView graphView, ObjectGraphVariable[] variables) {
            var targetTypes = graphView.GetRoots("effect").Select((root) => root.Type).Distinct().Select((superTargetType) => superTargetType.GenericTypeArguments[0]).ToArray();
            foreach (var variable in variables) {
                if (variable.containerType.IsConstructedGenericType && variable.containerType.GetGenericTypeDefinition().Equals(typeof(EffectPayload<>))) {
                    variable.valid = targetTypes.Any((t) => t == variable.containerType.GenericTypeArguments[0]);
                }

            }
        }
        public override Type GetNodeType(Type type) {
            return ObjectGraphModuleUtility.GetTargetType(typeof(IEffect<>), type, SuperTypes);
        }
        public EffectGraphModule(Settings settings) : base(settings) { }

        public override bool IsValidType(Type type) => !typeof(IUtilityEffect).IsAssignableFrom(type);
        /*         public bool ValidateGraph(ObjectGraphView view) {
                    foreach (var root in roots) {
                        root.ClearNotifications();
                    }
                    roots.Clear();
                    var masterNodePort = view.MasterNode.Q<Port>(null, PortClassName);
                    masterNodePort.ClearNotifications();
                    roots.AddRange(view.GetRoots<EffectGraphNode>());
                    if (roots.Count == 0) {
                        masterNodePort.ErrorNotification("No Roots found.");
                        return false;
                    }
                    var rootTypes = roots.Select((root) => root.SuperTargetType).Distinct().Count();
                    if (rootTypes > 1) {
                        foreach (var root in roots) {
                            root.ErrorNotification("Multiple Matching Types found for root nodes");
                        }
                        return false;
                    }
                    else if (rootTypes == 1) {
                        return true;
                    }
                    else {
                        foreach (var root in roots) {
                            root.ErrorNotification("All root nodes must be of the same type.");
                        }
                        return false;
                    }
                } */
        /*         public void OnPostDeserialize(SerializedObject obj, ObjectGraphView graphView, ref SortedDictionary<string, ObjectGraphModel.Entry> entries) {
                    var processed = new HashSet<string>();
                    var ids = entries.Keys.ToArray();
                    var oldEntries = entries.Values.ToArray();

                    entries.Clear();
                    for (int i = 0; i < ids.Length; i++) {
                        if (processed.Contains(ids[i]))
                            continue;
                        if (oldEntries[i].type.Equals(typeof(LinearEffect))) {
                            int next = (int)oldEntries[i].values["next"];
                            int effect = (int)oldEntries[i].values["effect"];
                            var target = oldEntries[effect];
                            if (next == -2) {
                                target.next = graphView.MasterNode.viewDataKey;
                            }
                            else if (next >= 0) {
                                target.next = ids[next];
                            }
                            else {
                                target.next = null;
                            }
                            entries[ids[i]] = target;

                            processed.Add(ids[effect]);

                        }
                        else {
                            entries[ids[i]] = oldEntries[i];
                        }


                    }
                }
         */
        /*         public void OnPreSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
                    var data = payload.entries.Select((e) => e.data).ToList();
                    for (int i = 0; i < payload.entries.Count; i++) {
                        var payloadEntry = payload.entries[i].entry;
                        var payloadData = payload.entries[i].data;
                        if (string.IsNullOrEmpty(payloadEntry.next))
                            continue;
                        if (payloadEntry.next == payload.graphView.MasterNode.viewDataKey) {
                            data.Add(payloadData);
                            data[i] = new LinearEffect
                            {
                                effect = data.Count - 1,
                                next = -2
                            };
                        }
                        else {
                            var index = payload.entries.FindIndex((e) => e.key == payloadEntry.next);
                            if (index >= 0) {
                                data.Add(payloadData);
                                data[i] = new LinearEffect
                                {
                                    effect = data.Count - 1,
                                    next = index
                                };
                            }
                        }
                    }

                    for (int i = 0; i < data.Count; i++) {
                        if (i < payload.entries.Count) {
                            payload.entries[i] = new ObjectGraphSerializerPayload.Entry
                            {
                                key = payload.entries[i].key,
                                data = data[i],
                                entry = payload.entries[i].entry,
                                node = payload.entries[i].node
                            };
                        }
                        else {
                            payload.entries.Add(new ObjectGraphSerializerPayload.Entry
                            {
                                key = null,
                                data = data[i],
                                entry = default,
                                node = null
                            });
                        }
                    }
                } */

        /*         public void OnPostSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
                    var property = obj.FindProperty("type");
                    var targetType = payload.graphView.GetRoots<EffectGraphNode>()?.FirstOrDefault()?.SuperTargetType;
                    if (typeof(IEffect<>).IsAssignableFrom(targetType)) {
                        property.enumValueIndex = (int)TargetTypeUtility.GetType(targetType.GenericTypeArguments[0]);
                    }
                    else {
                        property.enumValueIndex = 0;
                    }
                } */


    }

}