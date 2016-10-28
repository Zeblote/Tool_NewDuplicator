// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_StackSelectProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_StackSelectProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled!", 4);

	%client.ndSelection.cancelStackSelection();
	%client.ndSetMode(NDM_StackSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_StackSelectProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%qCount = %client.ndSelection.queueCount - %count;

	%title = "Selecting... (\c3" @ %count @ "\c6 Bricks, \c3" @ %qCount @ "\c6 in Queue)";
	%l0 = "[Cancel Brick]: Cancel selection";

	return ndFormatMessage(%title, %l0);
}
