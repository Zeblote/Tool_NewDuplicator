// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_PlantCopyProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_PlantCopyProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Planting canceled!", 4);

	%client.ndSelection.cancelPlanting();
	%client.ndSetMode(NDM_PlantCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_PlantCopyProgress::getBottomPrint(%this, %client)
{
	%qIndex = %client.ndSelection.plantQueueIndex;
	%qCount = %client.ndSelection.plantQueueCount;

	%count = %client.ndSelection.brickCount;
	%planted = %client.ndSelection.plantSuccessCount;

	if(%qIndex == %qCount)
	{
		//Searching for a brick
		%pIndex = %client.ndSelection.plantSearchIndex;
		%percent = mFloor(%client.ndSelection.plantSearchIndex * 100 / %count);

		%title = "Finding Next Brick... (\c3" @ %percent @ "%\c6, \c3" @ %planted @ "\c6 planted)";
	}
	else
	{
		//Planting bricks
		%failed = %client.ndSelection.plantTrustFailCount + %client.ndSelection.plantBlockedFailCount;
		%percent = mFloor(%planted * 100 / %count);

		%title = "Planting... (\c3" @ %percent @ "%\c6, \c3" @ %failed @ "\c6 failed)";
	}

	%l0 = "[Cancel Brick]: Cancel planting";

	return ndFormatMessage(%title, %l0);
}
