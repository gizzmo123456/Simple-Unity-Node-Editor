using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGraphSaveData : ScriptableObject
{

    public List<NodeGraphSaveData.GraphSaveGroup> data = new List<NodeGraphSaveData.GraphSaveGroup>();

    // Cache the last data loaded or saved, to prevent so many lookups :)
    int cachedDataIndex = -1; 

    /// <summary>
    /// Adds or updates existing save data.
    /// </summary>
    /// <param name="saveDataName"> the name of the graph to save to</param>
    /// <param name="graphData">the data to save</param>
    /// <param name="nodeId">the index to save to, if less than 0 appends to end, if greater than length inserts defualt values between data end and id, if id exist data is over writen</param>
    /// <param name="uniqueId">The unique id of the node, for most case this can just be the node id, but for cases where the node id can change this needs to be somthink unique to keep trak of nodes durring sessions</param>
	/// <returns>true if the graph data changed</returns> 
    public bool UpdateGraphData ( string saveDataName, NodeGraphSaveData.GraphSaveGroup.Graph graphData, int nodeId )
    {

        if ( IsCached( saveDataName ) )
        {
            return UpdateCachedGraph( graphData, nodeId );
        }

        // look it up and cache it!
        for ( int i = 0; i < data.Count; i++ )
        {
            if ( data[ i ].graphName == saveDataName )
            {
                cachedDataIndex = i;
                return UpdateCachedGraph( graphData, nodeId );
                
            }
        }

        cachedDataIndex = data.Count;
        data.Add( new NodeGraphSaveData.GraphSaveGroup( saveDataName ) );

        return UpdateCachedGraph( graphData, nodeId );

    }

    private bool UpdateCachedGraph( NodeGraphSaveData.GraphSaveGroup.Graph graphData, int nodeId)
    {

		// find if the alread contaitns the uid in graphData if not append it to the end of the save data.

		for ( int i = 0; i < data[cachedDataIndex].graph.Count; i++ )
		{
			if (data[cachedDataIndex].graph[i].uniqueId == graphData.uniqueId)
			{
				// overwrite.	// TODO: check that the data has changed!
				data[ cachedDataIndex ].graph[ i ] = graphData;
				return true;
			}
		}

		// apend to end.
		data[ cachedDataIndex ].graph.Add( graphData );

		return true;

        int graphSize = data[ cachedDataIndex ].graph.Count;
        bool updated = false;

        if ( nodeId < 0 || nodeId == graphSize )  // apend data.
        {
            data[ cachedDataIndex ].graph.Add( graphData );
            updated = true;
        }
        else if ( nodeId < graphSize )        // update
        {
            updated = !data[ cachedDataIndex ].graph[ nodeId ].CompareTo( graphData );
            if ( updated )
                data[ cachedDataIndex ].graph[ nodeId ] = graphData;
        }
        else if ( nodeId > graphSize )       // Add range and apend to end
        {

            int elementsToAdd = nodeId - graphSize;
            for ( int i = 0; i < elementsToAdd; i++ )
            {
                data[ cachedDataIndex ].graph.Add( new GraphSaveGroup.Graph( -1, Vector2.zero ) );
            }

            data[ cachedDataIndex ].graph.Add( graphData );
            updated = true;
        }

        return updated;

    }

    public List<NodeGraphSaveData.GraphSaveGroup.Graph> GetGraphData( string graphName )
    {
        if ( IsCached(graphName) )
        {
            return data[ cachedDataIndex ].graph;
        }

        for(int i = 0; i < data.Count; i++ )
        {
            if ( data[i].graphName == graphName )
            {
                cachedDataIndex = i;
                return data[ i ].graph;
            }
        }

        cachedDataIndex = -1;
        return null;

    }

    private bool IsCached( string graphName )
    {
        return cachedDataIndex > -1 && cachedDataIndex < data.Count && data[ cachedDataIndex ].graphName == graphName;
    }

    [ System.Serializable]
    public class GraphSaveGroup
    {
        public string graphName;
        public List<Graph> graph;

        public GraphSaveGroup ( string name )
        {
            graphName = name;
            graph = new List<Graph>();
        }

        [System.Serializable]
        public class Graph
        {
			public int uniqueId = -1;
            public Vector2 nodePosition = Vector2.zero;

            public Graph ( int _uniqueId, Vector2 pos )
            {
                nodePosition = pos;
				uniqueId = _uniqueId;

			}

            public bool CompareTo( Graph other )
            {
                return other.nodePosition == nodePosition;
            }
        }

    }

}