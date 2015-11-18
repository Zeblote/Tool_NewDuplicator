// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_StackSelect
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for stack selection mode
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDDM_StackSelect)
	{
		class = "NewDuplicatorMode";
		index = $NDDM::StackSelect;

		allowSelecting = true;
		allowUnMount   = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDDM_StackSelect::onStartMode(%this, %client, %lastMode)
{
	%client.ndLastSelectMode = %this;
	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDDM_StackSelect::onChangeMode(%this, %client, %nextMode)
{
	if(%nextMode == $NDDM::CubeSelect)
	{
		if(%client.ndSelection.brickCount)
		{
			%s = %client.ndSelection;

			%min = vectorAdd(%s.rootPosition, %s.minSize);
			%max = vectorAdd(%s.rootPosition, %s.maxSize);

			%client.ndSelectionBox = ND_SelectionBox();
			%client.ndSelectionBox.resize(%min, %max);
		}

		//Clear selection
		if(isObject(%client.ndSelection))
			%client.ndSelection.deleteData();
	}
	else if(%nextMode == $NDDM::PlantCopy)
	{
		//Start de-highlighting the bricks
		%client.ndSelection.deHighlight();
	}
}

//Kill this mode
function NDDM_StackSelect::onKillMode(%this, %client)
{
	//Destroy selection
	if(isObject(%client.ndSelection))
		%client.ndSelection.delete();
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDDM_StackSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if((%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType) == 0)
		return;
	
	//Prepare selection to copy the bricks
	if(isObject(%client.ndSelection))
		%client.ndSelection.deleteData();
	else
		%client.ndSelection = ND_Selection(%client);

	//Start selection
	%client.ndSetMode(NDDM_StackSelectProgress);
	%client.ndSelection.startStackSelection(%obj, %client.ndDirection, %client.ndLimited);
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_StackSelect::onLight(%this, %client)
{
	if($ND::PlayMenuSounds)
		%client.play2d(lightOnSound);

	%client.ndSetMode(NDDM_CubeSelect);
}

//Next Seat
function NDDM_StackSelect::onNextSeat(%this, %client)
{
	%client.ndDirection = !%client.ndDirection;
	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
		%client.play2d(%client.ndDirection ? lightOnSound : lightOffSound);
}

//Prev Seat
function NDDM_StackSelect::onPrevSeat(%this, %client)
{
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
		%client.play2d(%client.ndLimited ? lightOnSound : lightOffSound);
}

//Plant Brick
function NDDM_StackSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
		return;

	%client.ndSetMode(NDDM_PlantCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_StackSelect::getBottomPrint(%this, %client)
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

	%l0 = "Type: \c3Stack \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "Direction: \c3" @ (%client.ndDirection ? "Up" : "Down") @ " \c6[Next Seat]";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2);
}
