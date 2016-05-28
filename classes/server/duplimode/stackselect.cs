// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_StackSelect
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for stack selection mode
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_StackSelect::onStartMode(%this, %client, %lastMode)
{
	%client.ndLastSelectMode = %this;

	if(%lastMode != $NDM::StackSelectProgress)
		%client.ndMultiSelect = false;

	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDM_StackSelect::onChangeMode(%this, %client, %nextMode)
{
	if(%nextMode == $NDM::BoxSelect)
	{
		if(%client.ndSelection.brickCount)
		{
			%s = %client.ndSelection;

			%min = vectorAdd(%s.rootPosition, %s.minSize);
			%max = vectorAdd(%s.rootPosition, %s.maxSize);

			if(%client.isAdmin)
				%limit = $Pref::Server::ND::MaxBoxSizeAdmin;
			else
				%limit = $Pref::Server::ND::MaxBoxSizePlayer;

			if((getWord(%max, 0) - getWord(%min, 0) <= %limit)
			&& (getWord(%max, 1) - getWord(%min, 1) <= %limit)
			&& (getWord(%max, 2) - getWord(%min, 2) <= %limit))
			{
				%name = %client.name;

				if(getSubStr(%name, strLen(%name - 1), 1) $= "s")
					%shapeName = %name @ "' Selection Box";
				else
					%shapeName = %name @ "'s Selection Box";

				%client.ndSelectionBox = ND_SelectionBox(%shapeName);
				%client.ndSelectionBox.setSizeAligned(%min, %max, %client.getControlObject());
			}
			else
				commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops!\n<font:Verdana:17>\c6Your selection box is limited to \c3" @ mFloor(%limit * 2) @ " \c6studs.", 5);
		}

		//Clear selection
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();
	}
	else if(%nextMode == $NDM::PlantCopy)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();
	}
	else if(%nextMode == $NDM::FillColor)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();
	}
	else if(%nextMode == $NDM::WrenchProgress)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();
	}
}

//Kill this mode
function NDM_StackSelect::onKillMode(%this, %client)
{
	//Destroy selection
	if(isObject(%client.ndSelection))
		%client.ndSelection.delete();
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDM_StackSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if((%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType) == 0)
		return;

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

	if(!ndTrustCheckMessage(%obj, %client))
		return;

	//Prepare selection to copy the bricks
	if(!isObject(%client.ndSelection))
		%client.ndSelection = ND_Selection(%client);

	//Start selection
	%client.ndSetMode(NDM_StackSelectProgress);

	if(%client.ndMultiSelect)
		%client.ndSelection.startStackSelectionAdditive(%obj, %client.ndDirection, %client.ndLimited);
	else
		%client.ndSelection.startStackSelection(%obj, %client.ndDirection, %client.ndLimited);
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDM_StackSelect::onLight(%this, %client)
{
	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(lightOnSound);

	%client.ndSetMode(NDM_BoxSelect);
}

//Next Seat
function NDM_StackSelect::onNextSeat(%this, %client)
{
	%client.ndDirection = !%client.ndDirection;
	%client.ndUpdateBottomPrint();

	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(%client.ndDirection ? lightOnSound : lightOffSound);
}

//Prev Seat
function NDM_StackSelect::onPrevSeat(%this, %client)
{
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();

	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(%client.ndLimited ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDM_StackSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	//Change to plant mode and apply the shift
	%client.ndSetMode(NDM_PlantCopy);
	NDM_PlantCopy.onShiftBrick(%client, %x, %y, %z);
}

//Super Shift Brick
function NDM_StackSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	//Change to plant mode and apply the shift
	%client.ndSetMode(NDM_PlantCopy);
	NDM_PlantCopy.onSuperShiftBrick(%client, %x, %y, %z);
}

//Rotate Brick
function NDM_StackSelect::onRotateBrick(%this, %client, %dir)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	//Change to plant mode and apply the shift
	%client.ndSetMode(NDM_PlantCopy);
	NDM_PlantCopy.onRotateBrick(%client, %dir);
}

//Plant Brick
function NDM_StackSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	%client.ndSetMode(NDM_PlantCopy);
}

//Cancel Brick
function NDM_StackSelect::onCancelBrick(%this, %client)
{
	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();

	%client.ndUpdateBottomPrint();
}

//Copy Selection
function NDM_StackSelect::onCopy(%this, %client)
{
	%this.onPlantBrick(%client);
}

//Cut Selection
function NDM_StackSelect::onCut(%this, %client)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	%client.ndSetMode(NDM_CutProgress);
	%client.ndSelection.startCutting();
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_StackSelect::getBottomPrint(%this, %client)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
	{
		%title = "Selection Mode";
		%r0 = "Click Brick: Select stack " @ (%client.ndDirection ? "up" : "down");
		%r1 = "";
	}
	else
	{
		%count = %client.ndSelection.brickCount;

		%title = "Selection Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");
		%r0 = "Click Brick: Select again";
		%r1 = "[Plant Brick]: Plant Mode";
	}

	%l0 = "Type: \c3" @ (%client.ndMultiSelect ? "Multi-" : "") @ "Stack \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "Direction: \c3" @ (%client.ndDirection ? "Up" : "Down") @ " \c6[Next Seat]";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2);
}
