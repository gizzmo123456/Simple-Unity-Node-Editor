# Simple Node Graph
###### Simple set of classes for creating different types of node graphs in unity!

NodeGraph is a set of classes to inherit form, to create different styles of nodes and graphs  
There are 3 different types of node each with there own graph window

wip


[Node Graph Doxygen Docs](http://blog.ashleysands.co.uk/NodeGraphDocs/)
##### Docs coming soon!!



# TODO:
- [ ] Sort out Readme.md :)
- [x] Fix issue in BaseNodeEditor.NodeWindow  
- [x] Add node graph editor  
- [x] Make BaseVerticalNodeEditor Abstract  
- [x] Make BaseNodeGraphEditor Abstract  

- [x] Apple window position to node position  
- [x] Add Scrol bar to node window  

- [x] Add some basic UI bits  
- - [x] Pannel Frame  
- [R] Text types, ie. normal, bold ect...  
- [R] Node Frame  
- [x] Add handle position to nodes  

- [x] Add Connections to BaseNodeGraphEditor and its base data nodes  
- [x] Add Bezier curves to BaseNodeGraph Editor  

- [x] Draw Node Pins  

- [x] Add order changed callback to Vertical Editor  
- [x] Add NodeConnection changed callback to graph editor   

- [x] Add Save/Load  
- [ ] Add limits to node connections  
- - [ ] Types  
- - [x] Count  
- - [x] input/output pin count  

- [ ] Add snaping and grid
- [ ] Add Zoom
- [ ] Select multiple nodes]

# BUGS:  
- [x] Can only have one Node Editor instance active at a time  
 Since all need to be draw in 1 set of StartWindow and EndWindows  
 and they must all have a unique ID  
- - [x] Give all windows a unique ID  
- - [x] Apply position offset to all node positions ect...  
- [x] Fixed input positions in node graph  
- [x] Fixed NodeConnectionData inputPin_startPosition not updateing  

- [x] Fix node outputs using itself for pin size it should use the node that it is connecting to.  
- [ ] Finish applying LockNodesInPlaymode. It might be worth moving this in to the node data.  
[Fixed] Fix BaseNodeGraphData, generating incorrect node size when there is zero pins.  

# ##  
- [x] move node size functions into nodeData  
- [x] move the node window into nodeData  
this will alow different nodes data to to exist in the same graph and each one will be its own node type.  
- [R] futher more add a base class for all the serialized data and functions :)  

## Refacter
Any functions that are specific to nodes need to be moved into the node data class  
This way it will be much easyer to define different nodes in the same graph.  

^^ this will also fix the function duplercation and confusion on what function to use!  

Also a serialized data class needs to be added to help keep the serialized data   
orginized and serparate from the graph and editor window but designed to be used as a common asset :D.  

=========== BASE NODE EDITOR  
- [x] NodeSize  
- [x] Node window  
- [x] GetNodeContentsPosition  
- [x] DrawNodeUI  
- [x] GetNodeDragableArea  
- [x] NodeLocked (and finish imperlermenting)  

=========== BASE NODE GRAPH EDITOR  
- [x] DrawNodePins  
- [x] AddPin_toNode 			(replace or remove)  
- [x] AddOutputPin_toNode		(replace or remove)  
- [x] RemovePin_fromNode		(replace or remove)  

=========== MAYBE MAKE THE EMPTY BASE CLASS AND ADD A NEW CLASS FOR HOZ PINS  

=========== BASE NODE GRAPH VERT PINS  



