// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_CubeSelect
// *
// *    -------------------------------------------------------------------
// *    Cube select dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_CubeSelect))
	NDDM_CubeSelect.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_CubeSelect)
	{
		class = "ND_DupliMode";
		num = $NDDM::CubeSelect;

		allowedModes = $NDDM::StackSelect
			| $NDDM::CubeSelectProgress
			| $NDDM::PlaceCopy;

		allowSwinging = true;
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
	switch(%nextMode)
	{
		case $NDDM::StackSelect:

			//Clear selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.clearData();

			//Remove the selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();

			//Remove the highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

		case $NDDM::PlaceCopy:

			//Remove the selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.ndHighlightSet))
				%client.ndHighlightSet.deHighlight();

		case $NDDM::Disabled:

			//Delete selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.delete();

			//Remove the selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();

			//Remove the highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();
	}
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDDM_CubeSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if(%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType)
	{
		if(!isObject(%client.ndSelectionBox))
		{
			%name = %client.name;

			if(getSubStr(%name, strLen(%name - 1), 1) $= "s")
				%shapeName = %name @ "' Selection Cube";
			else
				%shapeName = %name @ "'s Selection Cube";				

			%client.ndSelectionBox = ND_SelectionBox(%shapeName);
		}

		%points = %obj.getWorldBox();
		%client.ndSelectionBox.resize(getWords(%points, 0, 2), getWords(%points, 3, 5));

		if(!%client.ndSelectionChanged)
		{
			messageClient(%client, 'MsgError', "");
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:16>\c6Press [Plant Brick] to select again.", 5);	
			%client.ndSelectionChanged = true;	

			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

			if(isObject(%client.ndHighlightSet))
				%client.ndHighlightSet.deHighlight();
		}

		%client.ndUpdateBottomPrint();
	}
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_CubeSelect::onLight(%this, %client)
{
	//Change to stack select mode
	%client.ndSetMode(NDDM_StackSelect);

	if($ND::PlayMenuSounds)
		%client.play2d(lightOffSound);
}

//Prev Seat
function NDDM_CubeSelect::onPrevSeat(%this, %client)
{
	//Toggle limited mode
	%client.ndLimited = !%client.ndLimited;

	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:16>\c6Press [Plant Brick] to select again.", 5);	
		%client.ndSelectionChanged = true;	

		if(isObject(%client.ndHighlightBox))
			%client.ndHighlightBox.delete();

		if(isObject(%client.ndHighlightSet))
			%client.ndHighlightSet.deHighlight();
	}

	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
	{
		if(%client.ndLimited)
			%client.play2d(lightOnSound);
		else
			%client.play2d(lightOffSound);
	}
}

//Shift Brick
function NDDM_CubeSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Select the side in the direction the brick was shifted
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX = %x; %newY = %y;
		case 1: %newX = -%y; %newY = %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX = %y; %newY = -%x;
	}

	%client.ndSelectionBox.selectSide(%newX, %newY, %z);
	%client.ndUpdateBottomPrint();
}

//Super Shift Brick
function NDDM_CubeSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Select the side in the direction the brick was shifted
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX = %x; %newY = %y;
		case 1: %newX = -%y; %newY = %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX = %y; %newY = -%x;
	}

	%client.ndSelectionBox.selectSide(%newX, %newY, %z);
	%client.ndUpdateBottomPrint();
}

//Rotate Brick
function NDDM_CubeSelect::onRotateBrick(%this, %client, %direction)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Extend or retract the side
	if(isObject(%client.ndSelectionBox) && strLen(%client.ndSelectionBox.selectedSide))
		%client.ndSelectionBox.stepSide(%direction);

	if(!%client.ndSelectionChanged)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Selection Box has been changed!\n<font:Verdana:16>\c6Press [Plant Brick] to select again.", 5);	
		%client.ndSelectionChanged = true;	

		if(isObject(%client.ndHighlightBox))
			%client.ndHighlightBox.delete();

		if(isObject(%client.ndHighlightSet))
			%client.ndHighlightSet.deHighlight();

		%client.ndUpdateBottomPrint();
	}
}

//Plant Brick
function NDDM_CubeSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	if(!%client.ndSelectionChanged)	
		%client.ndSetMode(NDDM_PlaceCopy);
	else
	{
		//Prepare a selection to copy the bricks
		if(isObject(%client.ndSelection))
			%client.ndSelection.clearData();
		else
			%client.ndSelection = ND_Selection(%client);

		//If the client already has a highlight set, start it first
		if(isObject(%client.ndHighlightSet))
			%client.ndHighlightSet.deHighlight();

		//Prepare a new highlight set
		%client.ndHighlightSet = ND_HighlightSet();

		//Start selection
		%client.ndSelectionBox.deselectSide();
		%box = %client.ndSelectionBox.getSize();

		%client.ndSetMode(NDDM_CubeSelectProgress);
		%client.ndSelection.startCubeSelection(%box, %client.ndLimited, %client.ndHighlightSet);
	}
}

//Cancel Brick
function NDDM_CubeSelect::onCancelBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	//Deselect the side of the box
	%client.ndSelectionBox.deselectSide();
	%client.ndUpdateBottomPrint();
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_CubeSelect::getBottomPrint(%this, %client)
{		
	%title = "Selection Mode";

	%l0 = "Type: \c3Cubic \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "";

	if(isObject(%client.ndSelectionBox))
	{
		if(strLen(%client.ndSelectionBox.selectedSide))
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
			%r2 = "[Plant Brick]: Place Mode";
	}
	else
	{
		%r0 = "Click Brick: Place selection cube";
		%r1 = "";
		%r2 = "";
	}

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2);
}
