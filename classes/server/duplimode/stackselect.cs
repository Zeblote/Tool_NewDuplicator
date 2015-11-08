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
				%client.ndSelection.clear();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.highlightSet))
				%client.highlightSet.deHighlight();

		case $NDDM::Disabled:

			//Delete selection
			if(isObject(%client.ndSelection))
				%client.ndSelection.delete();

			//Remove highlight box
			if(isObject(%client.ndHighlightBox))
				%client.ndHighlightBox.delete();

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.highlightSet))
				%client.highlightSet.deHighlight();

		case $NDDM::PlaceCopy:

			//If there's a dehighlight task that isn't started yet, start it
			if(isObject(%client.highlightSet))
				%client.highlightSet.deHighlight();
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
		%client.ndSelection.clear();
	else
		%client.ndSelection = ND_Selection();

	//If the client already has a highlight set, start it first
	if(isObject(%client.highlightSet))
		%client.highlightSet.deHighlight(%client);

	//Prepare a new highlight set
	%client.highlightSet = ND_HighlightSet();

	//Start selection task
	%client.ndSelectTask = NDAT_StackSelect(%client, %client.ndSelection, 
		%client.highlightSet, %obj, %client.ndDirection, %client.ndLimited);

	%client.ndSelectTask.start();

	//If the select task didn't finish instantly, change mode
	if(isObject(%client.ndSelectTask))
	{
		%client.ndSetMode(NDDM_StackSelectProgress);
		%client.ndSelectTask.addCallback(NDDM_StackSelectProgress, onSelectionFinish, %client);
	}
	else
	{
		//Create box to show total size of selection
		if(!isObject(%client.ndHighlightBox))
			%client.ndHighlightBox = ND_HighlightBox();

		%min = vectorAdd(%client.ndSelection.minSize, $NDS[%client.ndSelection, "RootPos"]);
		%max = vectorAdd(%client.ndSelection.maxSize, $NDS[%client.ndSelection, "RootPos"]);
		
		%client.ndHighlightBox.resize(%min, %max);
			
		//Schedule a new dehighlight task for the selected bricks
		%client.highlightSet.deHighlightDelayed($ND::HighlightTime);

		%client.ndUpdateBottomPrint();
	}
}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_StackSelect::onLight(%this, %client)
{
	//Change to cube select mode
	%client.ndSetMode(NDDM_CubeSelect);
}

//Next Seat
function NDDM_StackSelect::onNextSeat(%this, %client)
{
	//Toggle direction up/down
	%client.ndDirection = !%client.ndDirection;
	%client.ndUpdateBottomPrint();
}

//Prev Seat
function NDDM_StackSelect::onPrevSeat(%this, %client)
{
	//Toggle limited mode
	%client.ndLimited = !%client.ndLimited;
	%client.ndUpdateBottomPrint();
}

//Plant Brick
function NDDM_StackSelect::onPlantBrick(%this, %client)
{
	if(!isObject(%client.ndSelection) || !$NDS[%client.ndSelection, "Count"])
		return;

	%client.ndSetMode(NDDM_PlaceCopy);
}

//Interface
///////////////////////////////////////////////////////////////////////////

//Build a bottomprint
function NDDM_StackSelect::getBottomPrint(%this, %client)
{
	if(!isObject(%client.ndSelection) || !$NDS[%client.ndSelection, "Count"])
	{
		%title = "Selection Mode";
		%r0 = "Click Brick: Select stack " @ (%client.ndDirection ? "up" : "down");
		%r1 = "";
	}
	else
	{
		%count = $NDS[%client.ndSelection, "Count"];

		%title = "Selection Mode (\c3" @ %count @ "\c6 Brick" @ (%count > 1 ? "s)" : ")");
		%r0 = "Click Brick: Select again";
		%r1 = "[Plant Brick]: Place Mode";
	}

	%l0 = "Type: \c3Stack \c6[Light]";
	%l1 = "Limited: " @ (%client.ndLimited ? "\c3Yes" : "\c0No") @ " \c6[Prev Seat]";
	%l2 = "Direction: \c3" @ (%client.ndDirection ? "Up" : "Down") @ " \c6[Next Seat]";

	return ND_FormatMessage(%title, %l0, %r0, %l1, %r1, %l2);
}
