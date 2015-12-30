// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDM_FillColor
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for color mode
// *
// * ######################################################################

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_FillColor::onStartMode(%this, %client, %lastMode)
{
	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDM_FillColor::onChangeMode(%this, %client, %nextMode)
{
	//Restore selection box
	if(%nextMode == $NDM::CubeSelect)
	{
		%s = %client.ndSelection;

		%min = vectorAdd(%s.rootPosition, %s.minSize);
		%max = vectorAdd(%s.rootPosition, %s.maxSize);

		%client.ndSelectionBox = ND_SelectionBox(%shapeName);
		%client.ndSelectionBox.setSize(%min, %max);
	}
}

//Kill this mode
function NDM_FillColor::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Plant Brick
function NDM_FillColor::onPlantBrick(%this, %client)
{
	%client.ndSetMode(NDM_FillColorProgress);

	if(%client.currentFxColor $= "")
		%client.ndSelection.startFillColor(0, %client.currentColor);
	else if(%client.currentFxColor < 7)
		%client.ndSelection.startFillColor(1, %client.currentFxColor);
	else
		%client.ndSelection.startFillColor(2, %client.currentFxColor - 7);
}

//Cancel Brick
function NDM_FillColor::onCancelBrick(%this, %client)
{
	%client.ndSetMode(%client.ndLastSelectMode);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_FillColor::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;
	%title = "Paint Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");

	if(%client.currentFxColor !$= "")
	{
		switch(%client.currentFxColor)
		{
			case 0: %color = "\c3Fx - None";
			case 1: %color = "\c3Fx - Pearl";
			case 2: %color = "\c3Fx - Chrome";
			case 3: %color = "\c3Fx - Glow";
			case 4: %color = "\c3Fx - Blink";
			case 5: %color = "\c3Fx - Swirl";
			case 6: %color = "\c3Fx - Rainbow";
			case 7: %color = "\c3Fx - Stable";
			case 8: %color = "\c3Fx - Undulo";
		}
	}
	else
	{
		%color = "<font:impact:20>" @ ndGetPaintColorCode(%client.currentColor) @ "|||||<font:Verdana:16>\c3";

		%alpha = mFloor(100 * getWord(getColorIdTable(%client.currentColor), 3));

		if(%alpha != 100)
			%color = %color SPC %alpha @ "%"; 
	}


	%l0 = "Select paint can to chose color";
	%l1 = "Color: " @ %color;

	%r0 = "[Plant Brick]: Paint bricks";
	%r1 = "[Cancel Brick]: Exit mode";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2);
}
