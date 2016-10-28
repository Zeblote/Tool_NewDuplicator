// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_SaveProgress::onKillMode(%this, %client)
{
	//Destroy selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_SaveProgress::onCancelBrick(%this, %client)
{
	%client.ndSelection.cancelSaving();
	%client.ndSetMode(NDM_PlantCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_SaveProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%index = %client.ndSelection.saveIndex / 2 + %client.ndSelection.saveStage * %count / 2;

	%percent = mFloor(%index * 100 / %count);

	%title = "Saving Selection... (\c3" @ %percent @ "%\c6)";
	%l0 = "[Cancel Brick]: Cancel saving";

	return ndFormatMessage(%title, %l0);
}
