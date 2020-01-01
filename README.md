# README.md
Simple Node window class for unity!

# TODO:
[C] Fix issue in BaseNodeEditor.NodeWindow  
[C] Add node graph editor
[ ] Make BaseVerticalNodeEditor Abstract
[ ] Make BaseNodeGraphEditor Abstract

[C] Apple window position to node position 
[C] Add Scrol bar to node window

[ ] Add some basic UI bits
	[C] Pannel Frame
	[ ] Text types, ie. normal, bold ect...
	[ ] Node Frame
[C] Add handle position to nodes

[C] Add Connections to BaseNodeGraphEditor and its base data nodes
[C] Add Bezier curves to BaseNodeGraph Editor

[C] Draw Node Pins

[ ] Add order changed callback to Vertical Editor
[ ] Add NodeConnection changed callback to graph editor 

# BUGS:
[C] Can only have one Node Editor instance active at a time
	Since all need to be draw in 1 set of StartWindow and EndWindows
	and they must all have a unique ID
	[C] Give all windows a unique ID
	[C] Apply position offset to all node positions ect...
[F] Fixed input positions in node graph
[ ] Fixed NodeConnectionData inputPin_startPosition not updateing
