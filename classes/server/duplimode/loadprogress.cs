// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_LoadProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for load mode (in progress)
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_LoadProgress::onStartMode(%this, %client, %lastMode)
{
	//Prepare selection to load data into
	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();
	else
		%client.ndSelection = ND_Selection(%client);
}

//Kill this mode
function NDM_LoadProgress::onKillMode(%this, %client)
{
	//Destroy selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_LoadProgress::onCancelBrick(%this, %client)
{
	%client.ndSelection.cancelLoading();
	%client.ndSelection.delete();

	%client.ndSetMode(%client.ndLastSelectMode);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_LoadProgress::getBottomPrint(%this, %client)
{
	if(%client.ndSelection.loadStage == 0)
	{
		%count = %client.ndSelection.loadExpectedBrickCount;

		if(%count != 0)
		{
			%percent = mFloor(%client.ndSelection.brickCount * 100 / %count);

			%title = "Loading Bricks... (\c3" @ %percent @ "%\c6)";
		}
		else
			%title = "Loading Bricks... (\c3" @ %client.ndSelection.brickCount @ "\c6 Bricks)";
	}
	else
	{
		%count = %client.ndSelection.loadExpectedConnectionCount;

		if(%count != 0)
		{
			%percent = mFloor(%client.ndSelection.connectionCount * 100 / %count);

			%title = "Loading Connections... (\c3" @ %percent @ "%\c6)";
		}
		else
			%title = "Loading Connections... (\c3" @ %client.ndSelection.connectionCount @ "\c6 Connections)";
	}

	%l0 = "[Cancel Brick]: Cancel loading";

	return ndFormatMessage(%title, %l0);
}
