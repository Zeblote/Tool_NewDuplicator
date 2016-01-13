// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_FillColorProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for color mode (in progress)
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_FillColorProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_FillColorProgress::onCancelBrick(%this, %client)
{
	%client.ndSelection.cancelFillColor();
	%client.ndSetMode(NDM_FillColor);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_FillColorProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%percent = mFloor(%client.ndSelection.paintIndex * 100 / %count);

	%title = "Painting... (\c3" @ %percent @ "%\c6)";
	%l0 = "[Cancel Brick]: Cancel painting";

	return ndFormatMessage(%title, %l0);
}
