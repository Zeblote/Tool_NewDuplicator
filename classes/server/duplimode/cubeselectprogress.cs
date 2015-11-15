// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_CubeSelectProgress
// *
// *    -------------------------------------------------------------------
// *    Cube select progress dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_CubeSelectProgress))
	NDDM_CubeSelectProgress.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_CubeSelectProgress)
	{
		class = "ND_DupliMode";
		num = $NDDM::CubeSelectProgress;

		allowedModes = $NDDM::CubeSelect;

		allowSwinging = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch away from this mode
function NDDM_CubeSelectProgress::onChangeMode(%this, %client, %nextMode)
{
	switch(%nextMode)
	{
		case $NDDM::Disabled:

			//Destroy the selection
			%client.ndSelection.delete();

			//Start de-highlighting the bricks
			%client.ndHighlightSet.deHighlight();

			//Remove selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();
	}
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDDM_CubeSelectProgress::onCancelBrick(%this, %client)
{
	//Cancel selecting
	%client.ndSelection.cancelCubeSelection();

	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled!", 4);

	//Switch back to stack selection
	%client.ndSetMode(NDDM_CubeSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_CubeSelectProgress::getBottomPrint(%this, %client)
{
	%pCount = $NS[%client.ndSelection, "Count"];
	%qCount = $NS[%client.ndSelection, "QueueCount"];

	if(%pCount <= 0)
	{
		%curr = %client.ndSelection.currChunk + 1;
		%num = %client.ndSelection.numChunks;

		%title = "Searching... (Chunk \c3" @ %curr @ "\c6 of \c3" @ %num @ "\c6, \c3" @ %qCount @ "\c6 Bricks)";
	}
	else
		%title = "Processing... (\c3" @ %pCount @ "\c6 / \c3" @ %qCount @ "\c6 Bricks)";

	%l0 = "[Cancel Brick]: Cancel selection";

	return ndFormatMessage(%title, %l0);
}
