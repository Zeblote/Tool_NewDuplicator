// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_StackSelect
// *
// *    -------------------------------------------------------------------
// *    Stack select dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_StackSelect))
	NDDM_StackSelect.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_StackSelect)
	{
		class = "ND_DupliMode";
		num = $NDDM::StackSelect;

		allowedModes = $NDDM::CubeSelect
			| $NDDM::StackSelectProgress
			| $NDDM::PlaceCopy;

		allowSwinging = true;
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
	switch(%nextMode)
	{
		case $NDDM::CubeSelect:

			//Clear selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.clearData();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.ndHighlightSet))
				%client.ndHighlightSet.deHighlight();

		case $NDDM::Disabled:

			//Delete selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.delete();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.ndHighlightSet))
				%client.ndHighlightSet.deHighlight();

		case $NDDM::PlaceCopy:

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.ndHighlightSet))
				%client.ndHighlightSet.deHighlight();
	}
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDDM_StackSelect::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	if((%obj.getType() & $TypeMasks::FxBrickAlwaysObjectType) == 0)
		return;
	
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
	%client.ndSetMode(NDDM_StackSelectProgress);
	%client.ndSelection.startStackSelection(%obj, %client.ndDirection, %client.ndLimited, %client.ndHighlightSet);
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_StackSelect::onLight(%this, %client)
{
	//Change to cube select mode
	%client.ndSetMode(NDDM_CubeSelect);

	%client.play2d(lightOnSound);
}

//Next Seat
function NDDM_StackSelect::onNextSeat(%this, %client)
{
	//Toggle direction up/down
	%client.ndDirection = !%client.ndDirection;
	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
	{
		if(%client.ndDirection)
			%client.play2d(lightOnSound);
		else
			%client.play2d(lightOffSound);
	}
}

//Prev Seat
function NDDM_StackSelect::onPrevSeat(%this, %client)
{
	//Toggle limited mode
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
	{
		if(%client.ndLimited)
			%client.play2d(lightOnSound);
		else
			%client.play2d(lightOffSound);
	}
}

//Plant Brick
function NDDM_StackSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelection) || !$NS[%client.ndSelection, "Count"])
		return;

	%client.ndSetMode(NDDM_PlaceCopy);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_StackSelect::getBottomPrint(%this, %client)
{
	if(!isObject(%client.ndSelection) || !$NS[%client.ndSelection, "Count"])
	{
		%title = "Selection Mode";
		%r0 = "Click Brick: Select stack " @ (%client.ndDirection ? "up" : "down");
		%r1 = "";
	}
	else
	{
		%count = $NS[%client.ndSelection, "Count"];

		%title = "Selection Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");
		%r0 = "Click Brick: Select again";
		%r1 = "[Plant Brick]: Place Mode";
	}

	%l0 = "Type: \c3Stack \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "Direction: \c3" @ (%client.ndDirection ? "Up" : "Down") @ " \c6[Next Seat]";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2);
}
