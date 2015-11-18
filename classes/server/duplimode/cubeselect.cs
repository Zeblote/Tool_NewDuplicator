// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_CubeSelect
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for cubic selection mode
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDDM_CubeSelect)
	{
		class = "NewDuplicatorMode";
		index = $NDDM::CubeSelect;

		allowSelecting = true;
		allowUnMount   = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDDM_CubeSelect::onStartMode(%this, %client, %lastMode)
{
	%client.ndLastSelectMode = %this;
	%client.ndUpdateBottomPrint();

	if(%lastMode != $NDDM::CubeSelectProgress)
		%client.ndSelectionChanged = true;
}

//Switch away from this mode
function NDDM_CubeSelect::onChangeMode(%this, %client, %nextMode)
{
	if(%nextMode == $NDDM::StackSelect)
	{
		//Clear selection
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
	else if(%nextMode == $NDDM::PlantCopy)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();

		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
}

//Kill this mode
function NDDM_CubeSelect::onKillMode(%this, %client)
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
function NDDM_CubeSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if(%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType)
	{
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

		%client.ndSelectionBox.resize(%obj.getWorldBox());
		%client.ndUpdateBottomPrint();
	}
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_CubeSelect::onLight(%this, %client)
{
	if($ND::PlayMenuSounds)
		%client.play2d(lightOffSound);

	%client.ndSetMode(NDDM_StackSelect);
}

//Prev Seat
function NDDM_CubeSelect::onPrevSeat(%this, %client)
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

	if($ND::PlayMenuSounds)
		%client.play2d(%client.ndLimited ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDDM_CubeSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Select the side in the direction the brick was shifted
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX =  %x; %newY =  %y;
		case 1: %newX = -%y; %newY =  %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX =  %y; %newY = -%x;
	}

	if(%newX > 0)
		%side = 1;
	else if(%newY > 0)
		%side = 2;
	else if(%z > 0)
		%side = 3;
	else if(%newX < 0)
		%side = 4;
	else if(%newY < 0)
		%side = 5;
	else if(%z < 0)
		%side = 6;
	else
		%side = 0;

	%client.ndSelectionBox.selectSide(%side);
	%client.ndUpdateBottomPrint();
}

//Super Shift Brick
function NDDM_CubeSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	%this.onShiftBrick(%client, %x, %y, %z);
}

//Rotate Brick
function NDDM_CubeSelect::onRotateBrick(%this, %client, %direction)
{
	if(!isObject(%client.ndSelectionBox) || !%client.ndSelectionBox.selectedSide)
		return;

	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:17>\c6Press [Plant Brick] to select again.", 5);

		%client.ndSelectionChanged = true;
		%client.ndSelection.deleteData();
		%client.ndUpdateBottomPrint();
	}

	if(%client.isAdmin)
		%limit = $ND::MaxCubeSizeAdmin;
	else
		%limit = $ND::MaxCubeSizePlayer;

	//Extend or retract the side
	if(%client.ndSelectionBox.stepSide(%direction, %limit))
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops!\n<font:Verdana:17>\c6Your selection box is limited to \c3" @ mFloor(%limit * 2) @ " \c6studs.", 5);
}

//Plant Brick
function NDDM_CubeSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(%client.ndSelectionChanged)	
	{
		//Prepare a selection to copy the bricks
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();
		else
			%client.ndSelection = ND_Selection(%client);

		//Start selection
		%client.ndSelectionBox.deselectSide();
		%box = %client.ndSelectionBox.getSize();

		%client.ndSetMode(NDDM_CubeSelectProgress);
		%client.ndSelection.startCubeSelection(%box, %client.ndLimited);
	}
	else
		%client.ndSetMode(NDDM_PlantCopy);
}

//Cancel Brick
function NDDM_CubeSelect::onCancelBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	%client.ndSelectionBox.deselectSide();
	%client.ndUpdateBottomPrint();
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_CubeSelect::getBottomPrint(%this, %client)
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
		if(%client.ndSelectionBox.selectedSide)
		{
			%r0 = "[Move Brick]: Select side";
			%r1 = "[Rotate Brick]: Extend side";
		}
		else
		{
			%r0 = "Click Brick: Place selection cube";
			%r1 = "[Move Brick]: Select side to move";
		}

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
