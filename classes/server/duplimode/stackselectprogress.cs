// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_StackSelectProgress
// *
// *    -------------------------------------------------------------------
// *    Stack select progress dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_StackSelectProgress))
	NDDM_StackSelectProgress.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_StackSelectProgress)
	{
		class = "ND_DupliMode";
		num = $NDDM::StackSelectProgress;

		allowedModes = $NDDM::StackSelect;

		allowSwinging = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch away from this mode
function NDDM_StackSelectProgress::onChangeMode(%this, %client, %nextMode)
{
	switch(%nextMode)
	{
		case $NDDM::Disabled:

			//Cancel selecting
			%client.ndSelectTask.cancel();

			//Clear the selection
			%client.ndSelection.clear();

			//Start de-highlighting the bricks
			%client.highlightSet.deHighlight();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();
	}
}



//Task callbacks
///////////////////////////////////////////////////////////////////////////

//Selection finished
function NDDM_StackSelectProgress::onSelectionFinish(%this, %client)
{
	//Create box to show total size of selection
	if(!isObject(%client.ndHighlightBox))
		%client.ndHighlightBox = ND_HighlightBox();

	%min = vectorAdd(%client.ndSelection.minSize, $NDS[%client.ndSelection, "RootPos"]);
	%max = vectorAdd(%client.ndSelection.maxSize, $NDS[%client.ndSelection, "RootPos"]);

	%client.ndHighlightBox.resize(%min, %max);

	//De-highlight the bricks after a few seconds
	%client.highlightSet.deHighlightDelayed($ND::HighlightTime);

	%client.ndSetMode(NDDM_StackSelect);
}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDDM_StackSelectProgress::onCancelBrick(%this, %client)
{
	//Cancel selecting
	%client.ndSelectTask.cancel();

	//Clear the selection
	%client.ndSelection.clear();

	//Start de-highlighting the bricks
	%client.highlightSet.deHighlight();

	//Remove highlight box
	if(isObject(%client.ndHighlightBox))
		%client.ndHighlightBox.delete();

	%client.ndSetMode(NDDM_StackSelect);
}

//Interface
///////////////////////////////////////////////////////////////////////////

//Build a bottomprint
function NDDM_StackSelectProgress::getBottomPrint(%this, %client)
{
	%count = $NDS[%client.ndSelection, "Count"];
	%qCount = $NDS[%client.ndSelection, "QueueCount"] - %count;

	%title = "Selecting... (\c3" @ %count @ "\c6 Bricks, \c3" @ %qCount @ "\c6 in Queue)";
	%l0 = "[Cancel Brick]: Cancel selection";

	return ND_FormatMessage(%title, %l0);
}
