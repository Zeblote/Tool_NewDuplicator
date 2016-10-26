// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_BoxSelect
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for box selection mode
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_BoxSelect::onStartMode(%this, %client, %lastMode)
{
	if(%lastMode == $NDM::StackSelect)
	{
		if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
		{
			//Create selection box from the size of the previous selection
			%root = %client.ndSelection.rootPosition;
			%min = vectorAdd(%root, %client.ndSelection.minSize);
			%max = vectorAdd(%root, %client.ndSelection.maxSize);

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
				commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops!\n<font:Verdana:17>" @
					"\c6Your selection box is limited to \c3" @ mFloor(%limit * 2) @ " \c6studs.", 5);

			%client.ndSelection.deleteData();
		}

		%client.ndSelectionAvailable = false;
	}
	else if(%lastMode == $NDM::BoxSelectProgress && %client.ndSelection.brickCount > 0)
	{
		%client.ndSelectionBox.setDisabledMode();
		%client.ndSelectionAvailable = true;
	}
	else if(%lastMode != $NDM::FillColor && %lastMode != $NDM::WrenchProgress)
		%client.ndSelectionAvailable = false;

	%client.ndLastSelectMode = %this;
	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDM_BoxSelect::onChangeMode(%this, %client, %nextMode)
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
	}
	else if(%nextMode == $NDM::WrenchProgress)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();
	}
	else if(%nextMode == $NDM::LoadProgress)
	{
		//Remove the selection box
		if(isObject(%client.ndSelectionBox))
			%client.ndSelectionBox.delete();
	}
}

//Kill this mode
function NDM_BoxSelect::onKillMode(%this, %client)
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
function NDM_BoxSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if((%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType) == 0)
		return;

	if(!ndTrustCheckMessage(%obj, %client))
		return;

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled! " @
			"You can now edit the box again.", 5);

		%client.ndSelectionAvailable = false;
		%client.ndSelection.deleteData();
		%client.ndSelectionBox.setNormalMode();
		%client.ndUpdateBottomPrint();
	}

	if(isObject(%client.ndSelectionBox))
	{
		if(%client.ndMultiSelect)
		{
			%box1 = %client.ndSelectionBox.getWorldBox();
			%box2 = ndGetPlateBoxFromRayCast(%pos, %normal);

			%p1 = getMin(getWord(%box1, 0), getWord(%box2, 0))
				SPC getMin(getWord(%box1, 1), getWord(%box2, 1))
				SPC getMin(getWord(%box1, 2), getWord(%box2, 2));

			%p2 = getMax(getWord(%box1, 3), getWord(%box2, 3))
				SPC getMax(getWord(%box1, 4), getWord(%box2, 4))
				SPC getMax(getWord(%box1, 5), getWord(%box2, 5));
		}
		else
		{
			%box = %obj.getWorldBox();
			%p1 = getWords(%box, 0, 2);
			%p2 = getWords(%box, 3, 5);
		}
	}
	else
	{
		%name = %client.name;

		if(getSubStr(%name, strLen(%name - 1), 1) $= "s")
			%shapeName = %name @ "' Selection Box";
		else
			%shapeName = %name @ "'s Selection Box";

		%client.ndSelectionBox = ND_SelectionBox(%shapeName);

		if(%client.ndMultiSelect)
			%box = ndGetPlateBoxFromRayCast(%pos, %normal);
		else
			%box = %obj.getWorldBox();

		%p1 = getWords(%box, 0, 2);
		%p2 = getWords(%box, 3, 5);
	}

	%client.ndSelectionBox.setSizeAligned(%p1, %p2, %client.getControlObject());
	%client.ndUpdateBottomPrint();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDM_BoxSelect::onLight(%this, %client)
{
	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(lightOffSound);

	%client.ndSetMode(NDM_StackSelect);
}

//Prev Seat
function NDM_BoxSelect::onPrevSeat(%this, %client)
{
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();

	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(%client.ndLimited ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDM_BoxSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//If we have a selection, enter plant mode!
	if(%client.ndSelectionAvailable)
	{
		%client.ndSetMode(NDM_PlantCopy);
		NDM_PlantCopy.onShiftBrick(%client, %x, %y, %z);

		return;
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

	if(!%client.ndMultiSelect)
	{
		if(%client.isAdmin)
			%limit = $Pref::Server::ND::MaxBoxSizeAdmin;
		else
			%limit = $Pref::Server::ND::MaxBoxSizePlayer;

		if(%client.ndSelectionBox.shiftCorner(%newX SPC %newY SPC %z, %limit))
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops!\n<font:Verdana:17>" @
				"\c6Your selection box is limited to \c3" @ mFloor(%limit * 2) @ " \c6studs.", 5);

		%client.ndUpdateBottomPrint();
	}
	else
	{
		%client.ndSelectionBox.shift(%newX SPC %newY SPC %z);
	}
}

//Super Shift Brick
function NDM_BoxSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	//If we have a selection, enter plant mode!
	if(%client.ndSelectionAvailable)
	{
		%client.ndSetMode(NDM_PlantCopy);
		NDM_PlantCopy.onSuperShiftBrick(%client, %x, %y, %z);

		return;
	}

	%this.onShiftBrick(%client, %x * 8, %y * 8, %z * 20);
}

//Rotate Brick
function NDM_BoxSelect::onRotateBrick(%this, %client, %direction)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//If we have a selection, enter plant mode!
	if(%client.ndSelectionAvailable)
	{
		%client.ndSetMode(NDM_PlantCopy);
		NDM_PlantCopy.onRotateBrick(%client, %direction);

		return;
	}

	if(!%client.ndMultiSelect)
		%client.ndSelectionBox.switchCorner();
	else
	{
		%client.ndSelectionBox.rotate(%direction);
		%client.ndUpdateBottomPrint();
	}

}

//Plant Brick
function NDM_BoxSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//If we have a selection, enter plant mode!
	if(%client.ndSelectionAvailable)
	{
		%client.ndSetMode(NDM_PlantCopy);
		return;
	}

	//Check timeout
	if(!%client.isAdmin && %client.ndLastSelectTime + ($Pref::Server::ND::SelectTimeoutMS / 1000) > $Sim::Time)
	{
		%remain = mCeil(%client.ndLastSelectTime + ($Pref::Server::ND::SelectTimeoutMS / 1000) - $Sim::Time);

		if(%remain != 1)
			%s = "s";

		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You need to wait\c3 " @
			%remain @ "\c6 second" @ %s @ " before selecting again!", 5);

		return;
	}

	%client.ndLastSelectTime = $Sim::Time;

	//Prepare a selection to copy the bricks
	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();
	else
		%client.ndSelection = ND_Selection(%client);

	//Start selection
	%box = %client.ndSelectionBox.getWorldBox();

	%client.ndSetMode(NDM_BoxSelectProgress);
	%client.ndSelection.startBoxSelection(%box, %client.ndLimited);
}

//Cancel Brick
function NDM_BoxSelect::onCancelBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection canceled! " @
			"You can now edit the box again.", 5);

		%client.ndSelectionAvailable = false;
		%client.ndSelection.deleteData();
		%client.ndSelectionBox.setNormalMode();
		%client.ndUpdateBottomPrint();

		return;
	}

	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();

	%client.ndSelectionBox.delete();
	%client.ndSelectionAvailable = false;
	%client.ndUpdateBottomPrint();
}

//Copy Selection
function NDM_BoxSelect::onCopy(%this, %client)
{
	%this.onPlantBrick(%client);
}

//Cut Selection
function NDM_BoxSelect::onCut(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(!%client.ndSelectionAvailable)
	{
		%this.onPlantBrick(%client);
		return;
	}

	%client.ndSetMode(NDM_CutProgress);
	%client.ndSelection.startCutting();
}

//Supercut selection
function NDM_BoxSelect::onSuperCut(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Prepare a selection to handle the callback
	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();
	else
		%client.ndSelection = ND_Selection(%client);

	if(!$ND::SimpleBrickTableCreated)
		ndCreateSimpleBrickTable();

	//Start supercut
	%box = %client.ndSelectionBox.getWorldBox();

	%client.ndSetMode(NDM_SuperCutProgress);
	%client.ndSelection.startSuperCut(%box);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_BoxSelect::getBottomPrint(%this, %client)
{
	if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
	{
		%count = %client.ndSelection.brickCount;
		%title = "Selection Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");
	}
	else
		%title = "Selection Mode";

	%l0 = "Type: \c3Box \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";

	if(isObject(%client.ndSelectionBox))
	{
		%size = %client.ndSelectionBox.getSize();
		%x = mFloatLength(getWord(%size, 0) * 2, 0);
		%y = mFloatLength(getWord(%size, 1) * 2, 0);
		%z = mFloatLength(getWord(%size, 2) * 5, 0);
		%l2 = "Size: \c3" @ %x @ "\c6 x \c3" @ %y @ "\c6 x \c3" @ %z @ "\c6 Plates";
	}

	if(!isObject(%client.ndSelectionBox))
	{
		%r0 = "Click Brick: Place selection box";
		%r1 = "";
		%r2 = "";
	}
	else if(!%client.ndSelectionAvailable)
	{
		%r0 = "[Shift Brick]: Move corner";
		%r1 = "[Rotate Brick]: Switch corner";
	}
	else
	{
		%r0 = "[Cancel Brick]: Adjust box";
		%r1 = "[Plant Brick]: Duplicate";
	}

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2);
}
