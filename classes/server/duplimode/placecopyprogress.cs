// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_PlaceCopyProgress
// *
// *    -------------------------------------------------------------------
// *    Place copy progress dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_PlaceCopyProgress))
	NDDM_PlaceCopyProgress.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_PlaceCopyProgress)
	{
		class = "ND_DupliMode";
		num = $NDDM::PlaceCopyProgress;

		allowedModes = $NDDM::PlaceCopy;

		allowSwinging = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch away from this mode
function NDDM_PlaceCopyProgress::onChangeMode(%this, %client, %nextMode)
{
	switch(%nextMode)
	{
		case $NDDM::Disabled:

			//Destroy the selection
			%client.ndSelection.delete();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();
	}
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDDM_PlaceCopyProgress::onCancelBrick(%this, %client)
{
	//Cancel selecting
	%client.ndSelection.cancelPlanting();

	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Planting canceled!", 4);

	//Switch back to stack selection
	%client.ndSetMode(NDDM_PlaceCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_PlaceCopyProgress::getBottomPrint(%this, %client)
{
	%qIndex = %client.ndSelection.plantQueueIndex;
	%qCount = %client.ndSelection.plantQueueCount;

	%count = $NS[%client.ndSelection, "Count"];
	%planted = %client.ndSelection.plantSuccessCount;

	if(%qIndex == %qCount)
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
