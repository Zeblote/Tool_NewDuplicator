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

			//Destroy the selection
			%client.ndSelection.delete();

			//Start de-highlighting the bricks
			%client.ndHighlightSet.deHighlight();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();
	}
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDDM_StackSelectProgress::onCancelBrick(%this, %client)
{
	//Cancel selecting
	%client.ndSelection.cancelStackSelection();

	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled!", 4);

	//Switch back to stack selection
	%client.ndSetMode(NDDM_StackSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_StackSelectProgress::getBottomPrint(%this, %client)
{
	%count = $NS[%client.ndSelection, "Count"];
	%qCount = $NS[%client.ndSelection, "QueueCount"] - %count;

	%title = "Selecting... (\c3" @ %count @ "\c6 Bricks, \c3" @ %qCount @ "\c6 in Queue)";
	%l0 = "[Cancel Brick]: Cancel selection";

	return ndFormatMessage(%title, %l0);
}
