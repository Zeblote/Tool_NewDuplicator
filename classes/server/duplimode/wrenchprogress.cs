// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_WrenchProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();

	//Remove the selection box
	if(isObject(%client.ndSelectionBox))
		%client.ndSelectionBox.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_WrenchProgress::onCancelBrick(%this, %client)
{
	%client.ndSelection.cancelFillWrench();
	%client.ndSetMode(%client.ndLastSelectMode);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_WrenchProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%percent = mFloor(%client.ndSelection.wrenchIndex * 100 / %count);

	%title = "Applying... (\c3" @ %percent @ "%\c6)";
	%l0 = "[Cancel Brick]: Cancel";

	return ndFormatMessage(%title, %l0);
}
