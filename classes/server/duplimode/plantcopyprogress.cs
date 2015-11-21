// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_PlantCopyProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for plant mode (in progress)
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDM_PlantCopyProgress)
	{
		class = "NewDuplicatorMode";
		index = $NDM::PlantCopyProgress;

		allowSelecting = false;
		allowUnMount   = true;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_PlantCopyProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
	%client.ndSetBlueImage(false);
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
