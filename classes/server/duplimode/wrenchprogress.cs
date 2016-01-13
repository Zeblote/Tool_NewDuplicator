// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_WrenchProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for wrench mode (in progress)
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Change away from this mode
function NDM_WrenchProgress::onChangeMode(%this, %client, %nextMode)
{
	//Restore selection box
	if(%nextMode == $NDM::CubeSelect)
	{
		%s = %client.ndSelection;

		%min = vectorAdd(%s.rootPosition, %s.minSize);
		%max = vectorAdd(%s.rootPosition, %s.maxSize);

		%client.ndSelectionBox = ND_SelectionBox(%shapeName);
		%client.ndSelectionBox.setSize(%min, %max);
	}
}

//Kill this mode
function NDM_WrenchProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
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
