using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{
    public class ObservablePort : Port
    {
        public ObservablePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            var connectorListener = new DefaultExposedEdgeConnectorListener();
            m_EdgeConnector = new EdgeConnector<Edge>(connectorListener);

            this.AddManipulator(m_EdgeConnector);

        }
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            using (PortChangedEvent evt = PortChangedEvent.GetPooled(connections))
            {
                evt.target = this;
                SendEvent(evt);
            }

        }
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);

            using (PortChangedEvent evt = PortChangedEvent.GetPooled(connections))
            {
                evt.target = this;
                SendEvent(evt);
            }
        }
        public override void DisconnectAll()
        {
            base.DisconnectAll();
            using (PortChangedEvent evt = PortChangedEvent.GetPooled(connections))
            {
                evt.target = this;
                SendEvent(evt);
            }
        }
        public void ConnectWithoutNotify(Edge edge)
        {
            base.Connect(edge);
        }
        public void DisconnectWithoutNotify(Edge edge)
        {
            base.Disconnect(edge);
        }
        public void DisconnectAllWithoutNotify()
        {
            base.DisconnectAll();
        }
        public Edge ConnectToWithoutNotify(Port other)
        {
            return ConnectTo<Edge>(other);
        }

        public T ConnectToWithoutNotify<T>(Port other) where T : Edge, new()
        {
            if (other == null)
                throw new ArgumentNullException("Port.ConnectTo<T>() other argument is null");

            if (other.direction == this.direction)
                throw new ArgumentException("Cannot connect two ports with the same direction");

            var edge = new T();

            edge.output = direction == Direction.Output ? this : other;
            edge.input = direction == Direction.Input ? this : other;


            if (other is ObservablePort effectGraphPort)
                effectGraphPort.ConnectWithoutNotify(edge);
            else
                other.Connect(edge);

            return edge;
        }

    }
    public class DefaultExposedEdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate;
        private List<GraphElement> m_EdgesToDelete;

        public DefaultExposedEdgeConnectorListener()
        {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();

            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) { }
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);

            // We can't just add these edges to delete to the m_GraphViewChange
            // because we want the proper deletion code in GraphView to also
            // be called. Of course, that code (in DeleteElements) also
            // sends a GraphViewChange.
            m_EdgesToDelete.Clear();
            if (edge != null && edge.input != null && edge.input.capacity == Port.Capacity.Single)
                foreach (Edge edgeToDelete in edge.input.connections)
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
            if (edge != null && edge.output != null && edge.output.capacity == Port.Capacity.Single)
                foreach (Edge edgeToDelete in edge.output.connections)
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
            if (m_EdgesToDelete.Count > 0)
            {
                graphView.DeleteElements(m_EdgesToDelete);
            }

            var edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (Edge e in edgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e);
            }
        }
    }
}