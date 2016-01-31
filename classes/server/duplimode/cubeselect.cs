// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_CubeSelect
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for cubic selection mode
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_CubeSelect::onStartMode(%this, %client, %lastMode)
{
	%client.ndLastSelectMode = %this;
	%client.ndUpdateBottomPrint();

	if(%lastMode != $NDM::CubeSelectProgress && %lastMode != $NDM::FillColor)
		%client.ndSelectionChanged = true;
}

//Switch away from this mode
function NDM_CubeSelect::onChangeMode(%this, %client, %nextMode)
{
	if(%nextMode == $NDM::StackSelect)
	{
		//Clear selection
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDM::PlantCopy)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDM::CutProgress)
	{
		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDM::FillColor)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDM::WrenchProgress)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDM::LoadProgress)
	{
		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
}

//Kill this mode
function NDM_CubeSelect::onKillMode(%this, %client)
{
	//Destroy selection
	if(isObject(%client.ndSelection))
		%client.ndSelection.delete();

	//Delete the selection box
	if(isObject(%client.ndSelectionBox))
		%client.ndSelectionBox.delete();
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDM_CubeSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if((%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType) == 0)
		return;

	if(!ndTrustCheckMessage(%obj, %client))
		return;

	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:17>\c6Press [Plant Brick] to select again.", 5);

		%client.ndSelectionChanged = true;
		%client.ndSelection.deleteData();
	}

	if(!isObject(%client.ndSelectionBox))
	{
		%name = %client.name;

		if(getSubStr(%name, strLen(%name - 1), 1) $= "s")
			%shapeName = %name @ "' Selection Cube";
		else
			%shapeName = %name @ "'s Selection Cube";				

		%client.ndSelectionBox = ND_SelectionBox(%shapeName);
	}

	%client.ndSelectionBox.setSize(%obj.getWorldBox());
	%client.ndUpdateBottomPrint();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDM_CubeSelect::onLight(%this, %client)
{
	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(lightOffSound);

	%client.ndSetMode(NDM_StackSelect);
}

//Prev Seat
function NDM_CubeSelect::onPrevSeat(%this, %client)
{
	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:17>\c6Press [Plant Brick] to select again.", 5);
		
		%client.ndSelectionChanged = true;
		%client.ndSelection.deleteData();
	}

	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();

	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(%client.ndLimited ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDM_CubeSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:17>\c6Press [Plant Brick] to select again.", 5);

		%client.ndSelectionChanged = true;
		%client.ndSelection.deleteData();
		%client.ndUpdateBottomPrint();
	}

	//Move the corner
	switch(getAngleIDFromPlayer(%client.getControlObject()))
	{
		case 0: %newX =  %x; %newY =  %y;
		case 1: %newX = -%y; %newY =  %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX =  %y; %newY = -%x;
	}

	%newX = mFloor(%newX) / 2;
	%newY = mFloor(%newY) / 2;
	%z    = mFloor(%z   ) / 5;

	if(%client.isAdmin)
		%limit = $Pref::Server::ND::MaxCubeSizeAdmin;
	else
		%limit = $Pref::Server::ND::MaxCubeSizePlayer;

	if(%client.ndSelectionBox.shiftCorner(%newX SPC %newY SPC %z, %limit))
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops!\n<font:Verdana:17>\c6Your selection box is limited to \c3" @ mFloor(%limit * 2) @ " \c6studs.", 5);
}

//Super Shift Brick
function NDM_CubeSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	%this.onShiftBrick(%client, %x * 8, %y * 8, %z * 20);
}

//Rotate Brick
function NDM_CubeSelect::onRotateBrick(%this, %client, %direction)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	%client.ndSelectionBox.switchCorner();
}

//Plant Brick
function NDM_CubeSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(%client.ndSelectionChanged)	
	{
		//Check timeout
		if(!%client.isAdmin && %client.ndLastSelectTime + $Pref::Server::ND::SelectTimeout > $Sim::Time)
		{
			%remain = mCeil(%client.ndLastSelectTime + $Pref::Server::ND::SelectTimeout - $Sim::Time);

			if(%remain != 1)
				%s = "s";

			messageClient(%client, 'MsgError', "");
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You need to wait\c3 " @ %remain @ "\c6 second" @ %s @ " before selecting again!", 5);
			return;
		}

		%client.ndLastSelectTime = $Sim::Time;

		//Prepare a selection to copy the bricks
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();
		else
			%client.ndSelection = ND_Selection(%client);

		//Start selection
		%box = %client.ndSelectionBox.getSize();

		%client.ndSetMode(NDM_CubeSelectProgress);
		%client.ndSelection.startCubeSelection(%box, %client.ndLimited);
	}
	else
		%client.ndSetMode(NDM_PlantCopy);
}

//Cancel Brick
function NDM_CubeSelect::onCancelBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();

	%client.ndSelectionBox.delete();
	%client.ndSelectionChanged = true;
	%client.ndUpdateBottomPrint();
}

//Copy Selection
function NDM_CubeSelect::onCopy(%this, %client)
{
	%this.onPlantBrick(%client);
}

//Cut Selection
function NDM_CubeSelect::onCut(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(%client.ndSelectionChanged)
	{
		%this.onPlantBrick(%client);
		return;
	}

	%client.ndSetMode(NDM_CutProgress);
	%client.ndSelection.startCutting();
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_CubeSelect::getBottomPrint(%this, %client)
{		
	if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
	{
		%count = %client.ndSelection.brickCount;
		%title = "Selection Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");
	}
	else
		%title = "Selection Mode";

	%l0 = "Type: \c3Cubic \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "";

	if(isObject(%client.ndSelectionBox))
	{
		%r0 = "[Shift Brick]: Move corner";
		%r1 = "[Rotate Brick]: Switch corner";

		if(%client.ndSelectionChanged)
			%r2 = "[Plant Brick]: Select bricks";
		else
			%r2 = "[Plant Brick]: Plant Mode";
	}
	else
	{
		%r0 = "Click Brick: Place selection cube";
		%r1 = "";
		%r2 = "";
	}

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2);
}
