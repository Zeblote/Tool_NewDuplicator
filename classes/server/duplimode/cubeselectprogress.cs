// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_CubeSelectProgress
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for stack selection mode (in progress)
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDM_CubeSelectProgress)
	{
		class = "NewDuplicatorMode";
		index = $NDM::CubeSelectProgress;
		image = "ND_Image_Cube";
		spin = true;

		allowSelecting = false;
		allowUnMount   = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Kill this mode
function NDM_CubeSelectProgress::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();

	//Remove selection box
	%client.ndSelectionBox.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Cancel Brick
function NDM_CubeSelectProgress::onCancelBrick(%this, %client)
{
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled!", 4);
	
	%client.ndSelection.cancelCubeSelection();
	%client.ndSetMode(NDM_CubeSelect);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_CubeSelectProgress::getBottomPrint(%this, %client)
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
