// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_CutProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for cut mode (in progress)
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch away from this mode
function NDM_CutProgress::onChangeMode(%this, %client, %nextMode)
{	
	if(%nextMode != $NDM::PlantCopy)
		%client.ndSelection.deleteData();
}

//Kill this mode
function NDM_CutProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_CutProgress::onCancelBrick(%this, %client)
{
	%client.ndSelection.cancelCutting();
	%client.ndSetMode(%client.ndLastSelectMode);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_CutProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%percent = mFloor(%client.ndSelection.cutIndex * 100 / %count);

	%title = "Cutting... (\c3" @ %percent @ "%\c6)";
	%l0 = "[Cancel Brick]: Cancel cut";

	return ndFormatMessage(%title, %l0);
}
