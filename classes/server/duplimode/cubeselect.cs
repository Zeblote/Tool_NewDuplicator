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
			| $NDDM::CubeSelectProgress;

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
}

//Switch away from this mode
function NDDM_CubeSelect::onChangeMode(%this, %client, %nextMode)
{
	switch(%nextMode)
	{
		case $NDDM::StackSelect:

			//Clear selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.clear();

			//Remove the selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();

		case $NDDM::Disabled:

			//Delete selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.delete();

			//Remove the selection box
			if(isObject(%client.ndSelectionBox))
				%client.ndSelectionBox.delete();
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
			%client.ndSelectionBox = ND_SelectionBox();
			%client.ndUpdateBottomPrint();
		}

		%points = %obj.getWorldBox();
		%client.ndSelectionBox.resize(getWords(%points, 0, 2), getWords(%points, 3, 5));
	}
}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_CubeSelect::onLight(%this, %client)
{
	//Change to stack select mode
	%client.ndSetMode(NDDM_StackSelect);
}

//Next Seat
function NDDM_CubeSelect::onNextSeat(%this, %client)
{

}

//Prev Seat
function NDDM_CubeSelect::onPrevSeat(%this, %client)
{
	//Toggle limited mode
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();
}

//Shift Brick
function NDDM_CubeSelect::onShiftBrick(%this, %client, %x, %y, %z)
{
	//Select the side in the direction the brick was shifted
	if(isObject(%client.ndSelectionBox))
	{			
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
}

//Super Shift Brick
function NDDM_CubeSelect::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	//Select the side in the direction the brick was shifted
	if(isObject(%client.ndSelectionBox))
	{			
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
}

//Rotate Brick
function NDDM_CubeSelect::onRotateBrick(%this, %client, %direction)
{
	//Extend or retract the side
	if(isObject(%client.ndSelectionBox) && strLen(%client.ndSelectionBox.selectedSide))
		%client.ndSelectionBox.stepSide(%direction);
}

//Plant Brick
function NDDM_CubeSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelectionBox))
		return;

	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Cubic Selection isn't implemented yet :(\n<font:Verdana:16>\c6Press [Light] to switch back to Stack Selection.", 10);
}

//Cancel Brick
function NDDM_CubeSelect::onCancelBrick(%this, %client)
{
	//Deselect the side of the box
	%client.ndSelectionBox.deselectSide();
	%client.ndUpdateBottomPrint();
}

//Interface
///////////////////////////////////////////////////////////////////////////

//Build a bottomprint
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
			%r2 = "[Plant Brick]: Select bricks";
		}
		else
		{
			%r0 = "Click Brick: Place selection cube";
			%r1 = "[Move Brick]: Select side to move";
			%r2 = "[Plant Brick]: Select bricks";
		}
	}
	else
	{
		%r0 = "Click Brick: Place selection cube";
		%r1 = "";
		%r2 = "";
	}

	return ND_FormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2);
}
