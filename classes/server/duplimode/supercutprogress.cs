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
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Supercut canceled!", 4);

	%client.ndSelection.cancelSuperCut();
	%client.ndSetMode(NDM_BoxSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_SuperCutProgress::getBottomPrint(%this, %client)
{
	%curr = %client.ndSelection.currChunk + 1;
	%num = %client.ndSelection.numChunks;

	%percent = mFloor(%curr * 100 / %num);
	%deleted = %client.ndSelection.superCutCount;
	%planted = %client.ndSelection.superCutPlacedCount;

	%title = "Supercut in progress... (\c3" @ %percent @ "%\c6, \c3" @ %deleted @ "\c6 deleted, \c3" @ %planted @ "\c6 planted)";
	%l0 = "[Cancel Brick]: Cancel supercut";

	return ndFormatMessage(%title, %l0);
}
