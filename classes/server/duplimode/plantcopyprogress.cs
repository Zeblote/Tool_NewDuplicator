// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_PlantCopyProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for plant mode (in progress)
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDDM_PlantCopyProgress)
	{
		class = "NewDuplicatorMode";
		index = $NDDM::PlantCopyProgress;

		allowSelecting = false;
		allowUnMount   = true;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDDM_PlantCopyProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDDM_PlantCopyProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Planting canceled!", 4);

	%client.ndSelection.cancelPlanting();
	%client.ndSetMode(NDDM_PlantCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_PlantCopyProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%planted = %client.ndSelection.plantSuccessCount;

	if(%client.ndSelection.plantStackIndex < 1)
	{
		//Searching for a brick
		%pIndex = %client.ndSelection.plantSearchIndex;
		%title = "Finding Next Brick... (\c3" @ %pIndex @ "\c6 / \c3" @ %count @ "\c6, \c3" @ %planted @ "\c6 planted)";
	}
	else
	{
		//Planting bricks
		%failed = %client.ndSelection.plantTrustFailCount + %client.ndSelection.plantBlockedFailCount;
		%title = "Planting... (\c3" @ %planted @ "\c6 Bricks, \c3" @ %failed @ "\c6 failed)";
	}

	%l0 = "[Cancel Brick]: Cancel planting";

	return ndFormatMessage(%title, %l0);
}
