# README.md
Simple Node window class for unity!

# TODO:
[ ] Fix issue in BaseNodeEditor.NodeWindow  
[C] Add node graph editor
[ ] Make BaseVerticalNodeEditor Abstract
[ ] Make BaseNodeGraphEditor Abstract

[C] Apple window position to node position 
[C] Add Scrol bar to node window

[ ] Add some basic UI bits
	[ ] Pannel Frame
	[ ] Text types, ie. normal, bold ect...
	[ ] Node Frame
[ ] Added handle position to nodes

[C] Add Connections to BaseNodeGraphEditor and its base data nodes
[C] Add Bezier curves to BaseNodeGraph Editor

# BUGS:
[ ] Can only have one Node Editor instance active at a time
	Since all need to be draw in 1 set of StartWindow and EndWindows
	and they must all have a unique ID
	[C] Give all windows a unique ID
	[ ] Apply position offset to all node positions ect...