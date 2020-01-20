using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGraphSaveData : ScriptableObject
{

    public List<NodeGraphSaveData.GraphSaveGroup> data = new List<NodeGraphSaveData.GraphSaveGroup>();

    // Cache the last data loaded or saved, to prevent so mean lookup :)
    int cachedDataIndex = -1; 

    /// <summary>
    /// Adds or updates existing save data.
    /// </summary>
    /// <param name="saveDataName"> the name of the graph to save to</param>
    /// <param name="graphData">the data to save</param>
    /// <param name="id">the index to save to, if less than 0 appends to end, if greater than length inserts defualt values between data end and id, if id exist data is over writen</param>
    public void UpdateGraphData ( string saveDataName, NodeGraphSaveData.GraphSaveGroup.Graph graphData, int id )
    {

        if ( IsCached( saveDataName ) )
        {
            UpdateCachedGraph( graphData, id );
        }

        // look it up and cache it!
        for ( int i = 0; i < data.Count; i++ )
        {
            if ( data[ i ].graphName == saveDataName )
            {
                cachedDataIndex = i;
                UpdateCachedGraph( graphData, id );
                return;
            }
        }

        cachedDataIndex = data.Count;
        data.Add( new NodeGraphSaveData.GraphSaveGroup( saveDataName ) );

        UpdateCachedGraph( graphData, id );

    }

    private void UpdateCachedGraph( NodeGraphSaveData.GraphSaveGroup.Graph graphData, int id)
    {

        int graphSize = data[ cachedDataIndex ].graph.Count;

        if ( id < 0 || id == graphSize )  // apend data.
        {
            data[ cachedDataIndex ].graph.Add( graphData );
        }
        else if ( id < graphSize )        // update
        {
            data[ cachedDataIndex ].graph[ id ] = graphData;
        }
        else if ( id > graphSize )       // Add range and apend to end
        {

            int elementsToAdd = id - graphSize;
            for ( int i = 0; i < elementsToAdd; i++ )
            {
                data[ cachedDataIndex ].graph.Add( new GraphSaveGroup.Graph( Vector2.zero ) );
            }

            data[ cachedDataIndex ].graph.Add( graphData );

        }


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
        return cachedDataIndex > -1 && data[ cachedDataIndex ].graphName == graphName;
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
            public Vector2 nodePosition = Vector2.zero;

            public Graph ( Vector2 pos )
            {
                nodePosition = pos;
            }
        }

    }

}