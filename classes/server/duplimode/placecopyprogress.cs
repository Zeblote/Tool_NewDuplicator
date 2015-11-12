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

	//Switch back to stack selection
	%client.ndSetMode(NDDM_PlaceCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_PlaceCopyProgress::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.plantSuccessCount;
	%fCount = %client.ndSelection.plantFailCount + %client.ndSelection.trustFailCount;

	%title = "Planting... (\c3" @ %count @ "\c6 planted, \c3" @ %fCount @ "\c6 failed)";
	%l0 = "[Cancel Brick]: Cancel planting";

	return ndFormatMessage(%title, %l0);
}
