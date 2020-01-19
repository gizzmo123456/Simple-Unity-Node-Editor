# Simple Node window class for unity!



# TODO:
[C] Fix issue in BaseNodeEditor.NodeWindow  
[C] Add node graph editor  
[C] Make BaseVerticalNodeEditor Abstract  
[C] Make BaseNodeGraphEditor Abstract  

[C] Apple window position to node position  
[C] Add Scrol bar to node window  

[C] Add some basic UI bits  
&nbsp;    [C] Pannel Frame  
&nbsp;    [R] Text types, ie. normal, bold ect...  
&nbsp;    [R] Node Frame  
[C] Add handle position to nodes  

[C] Add Connections to BaseNodeGraphEditor and its base data nodes  
[C] Add Bezier curves to BaseNodeGraph Editor  

[C] Draw Node Pins  

[C] Add order changed callback to Vertical Editor  
[C] Add NodeConnection changed callback to graph editor   

[ ] Add Save/Load  
[ ] Add limits to node connections  
&nbsp;    [ ] Types  
&nbsp;    [ ] Count  
&nbsp;    [C] input/output pin count  

# BUGS:  
[C] Can only have one Node Editor instance active at a time  
&nbsp;&nbsp;&nbsp;&nbsp;    Since all need to be draw in 1 set of StartWindow and EndWindows  
&nbsp;&nbsp;&nbsp;&nbsp;    and they must all have a unique ID  
&nbsp;    [C] Give all windows a unique ID  
&nbsp;    [C] Apply position offset to all node positions ect...  
[F] Fixed input positions in node graph  
[F] Fixed NodeConnectionData inputPin_startPosition not updateing  

[ ] Fix node outputs using itself for pin size it should use the node that it is connecting to.

# ##
[ ] move node size functions into nodeData
[ ] move the node window into nodeData
this will alow different nodes data to to exist in the same graph and each one will be its own node type.
[ ] futher move add a base class for all the serialized data and functions :)