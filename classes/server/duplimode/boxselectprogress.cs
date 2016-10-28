// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_BoxSelectProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();

	//Remove selection box
	%client.ndSelectionBox.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_BoxSelectProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled!", 4);

	%client.ndSelection.cancelBoxSelection();
	%client.ndSetMode(NDM_BoxSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_BoxSelectProgress::getBottomPrint(%this, %client)
{
	%qCount = %client.ndSelection.queueCount;
	%bCount = %client.ndSelection.brickCount;

	if(%bCount <= 0)
	{
		%curr = %client.ndSelection.currChunk + 1;
		%num = %client.ndSelection.numChunks;

		%percent = mFloor(%curr * 100 / %num);
		%title = "Searching... (\c3" @ %percent @ "%\c6, \c3" @ %qCount @ "\c6 Bricks)";
	}
	else
	{
		%percent = mFloor(%bCount * 100 / %qCount);
		%title = "Processing... (\c3" @ %percent @ "%\c6)";
	}

	%l0 = "[Cancel Brick]: Cancel selection";

	return ndFormatMessage(%title, %l0);
}
