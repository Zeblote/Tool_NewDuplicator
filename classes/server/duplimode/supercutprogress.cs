// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_SuperCutProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for super cut mode (in progress)
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_SuperCutProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();

	//Remove selection box
	%client.ndSelectionBox.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_SuperCutProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Super-Cut canceled!", 4);

	%client.ndSelection.cancelSuperCut();
	%client.ndSetMode(NDM_BoxSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_SuperCutProgress::getBottomPrint(%this, %client)
{
	//%qCount = %client.ndSelection.queueCount;
	//%bCount = %client.ndSelection.brickCount;

	//if(%bCount <= 0)
	//{
		//%curr = %client.ndSelection.currChunk + 1;
		//%num = %client.ndSelection.numChunks;

		//%percent = mFloor(%curr * 100 / %num);
		//%title = "Searching... (\c3" @ %percent @ "%\c6, \c3" @ %qCount @ "\c6 Bricks)";
		%title = "Super-Cut in progres...";
	//}
	//else
	//{
		//%percent = mFloor(%bCount * 100 / %qCount);
		//%title = "Processing... (\c3" @ %percent @ "%\c6)";
	//}

	%l0 = "[Cancel Brick]: Cancel super-cut";

	return ndFormatMessage(%title, %l0);
}
