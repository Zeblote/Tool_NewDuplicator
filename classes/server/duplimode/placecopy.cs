// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_PlaceCopy
// *
// *    -------------------------------------------------------------------
// *    Place copy dupli mode
// *
// * ######################################################################

//Create object to receive callbacks
if(isObject(NDDM_PlaceCopy))
	NDDM_PlaceCopy.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_PlaceCopy)
	{
		class = "ND_DupliMode";
		num = $NDDM::PlaceCopy;

		allowedModes = $NDDM::StackSelect;

		allowSwinging = true;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDDM_PlaceCopy::onStartMode(%this, %client, %lastMode)
{
	%client.ndSelection.spawnGhostBricks($NDS[%client.ndSelection, "RootPos"], 0);

	//Create blue highlight box around ghost selection
	if(!isObject(%client.ndHighlightBox))
		%client.ndHighlightBox = ND_HighlightBox();

	%client.ndHighlightBox.resize(%client.ndSelection.getGhostWorldBox());
	%client.ndHighlightBox.borderColor = "0.2 0.2 1 1";
	%client.ndHighlightBox.recolor();

	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDDM_PlaceCopy::onChangeMode(%this, %client, %nextMode)
{	
	switch(%nextMode)
	{
		case $NDDM::Disabled:

			//Delete highlight box
			%client.ndHighlightBox.delete();

			//Delete the selection
			%client.ndSelection.delete();

		case $NDDM::StackSelect:

			//Delete highlight box
			%client.ndHighlightBox.delete();

			//Remove ghost bricks
			%client.ndSelection.clearGhostBricks();
	}
}

//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDDM_PlaceCopy::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	//Get half size of world box for offset
	if(%client.ndPivot)
		%box = %client.ndSelection.getGhostWorldBox();
	else
		%box = %client.ndSelection.ghostGroup.getObject(0).getWorldBox();

	%half = vectorScale(vectorSub(getWords(%box, 3, 5), getWords(%box, 0, 2)), 0.5);
	%hX = getWord(%half, 0);
	%hY = getWord(%half, 1);
	%hZ = getWord(%half, 2);

	%nX = getWord(%normal, 0);
	%nY = getWord(%normal, 1);
	%nZ = getWord(%normal, 2);

	//Point offset in correct direction based on normal
	if(%nX > 0.1)
		%offset = %hX @ " 0 0";

	else if(%nX < -0.1)
		%offset = -%hX @ " 0 0";

	else if(%nY > 0.1)
		%offset = "0 " @ %hY @ " 0";

	else if(%nY < -0.1)
		%offset = "0 " @ -%hY @ " 0";

	else if(%nZ > 0.1)
		%offset = "0 0 " @ %hZ;

	else if(%nZ < -0.1)
		%offset = "0 0 " @ -%hZ;

	//Get shift vector
	%pos = vectorSub(vectorAdd(%pos, %offset), %client.ndSelection.ghostPosition);

	if(%client.ndPivot)
		%pos = vectorSub(%pos, ndRotateVector(%client.ndSelection.rootToCenter, %client.ndSelection.ghostAngleID));

	%client.ndSelection.shiftGhostBricks(%pos);
	%client.ndHighlightBox.resize(%client.ndSelection.getGhostWorldBox());
}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function NDDM_PlaceCopy::onLight(%this, %client)
{

}

//Next Seat
function NDDM_PlaceCopy::onNextSeat(%this, %client)
{

}

//Prev Seat
function NDDM_PlaceCopy::onPrevSeat(%this, %client)
{
	%client.ndPivot = !%client.ndPivot;
	%client.ndUpdateBottomPrint();
}

//Shift Brick
function NDDM_PlaceCopy::onShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX = %x; %newY = %y;
		case 1: %newX = -%y; %newY = %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX = %y; %newY = -%x;
	}

	%client.ndSelection.shiftGhostBricks(%newX / 2 SPC %newY / 2 SPC %z / 5);
	%client.ndHighlightBox.resize(%client.ndSelection.getGhostWorldBox());
}

//Super Shift Brick
function NDDM_PlaceCopy::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX = %x; %newY = %y;
		case 1: %newX = -%y; %newY = %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX = %y; %newY = -%x;
	}

	if(%client.ndPivot)
		%box = %client.ndSelection.getGhostWorldBox();
	else
		%box = %client.ndSelection.ghostGroup.getObject(0).getWorldBox();

	%newX *= (getWord(%box, 3) - getWord(%box, 0));
	%newy *= (getWord(%box, 4) - getWord(%box, 1));
	%z    *= (getWord(%box, 5) - getWord(%box, 2));

	%client.ndSelection.shiftGhostBricks(%newX SPC %newY SPC %z);
	%client.ndHighlightBox.resize(%client.ndSelection.getGhostWorldBox());
}

//Rotate Brick
function NDDM_PlaceCopy::onRotateBrick(%this, %client, %direction)
{
	if(%direction > 0)
		%client.player.playThread("3", "rotCW");
	else
		%client.player.playThread("3", "rotCCW");

	%client.ndSelection.rotateGhostBricks(%direction, %client.ndPivot);
	%client.ndHighlightBox.resize(%client.ndSelection.getGhostWorldBox());
}

//Plant Brick
function NDDM_PlaceCopy::onPlantBrick(%this, %client)
{

}

//Cancel Brick
function NDDM_PlaceCopy::onCancelBrick(%this, %client)
{
	%client.ndSetMode(%client.ndLastSelectMode);
}

//Interface
///////////////////////////////////////////////////////////////////////////

//Build a bottomprint
function NDDM_PlaceCopy::getBottomPrint(%this, %client)
{
	%count = $NDS[%client.ndSelection, "Count"];

	%size = vectorSub(%client.ndSelection.maxSize, %client.ndSelection.minSize);

	%x = (getWord(%size, 0) * 2) | 0;
	%y = (getWord(%size, 1) * 2) | 0;
	%z = (getWord(%size, 2) * 5) | 0;

	if(%count == 1)
		%title = "Place Mode (\c31\c6 Brick)";
	else if(%count <= $ND::MaxGhostBricks)
		%title = "Place Mode (\c3" @ %count @ "\c6 Bricks)";
	else
		%title = "Place Mode (\c3" @ %count @ "\c6 Bricks, \c3" @ (($ND::MaxGhostBricks * 100 / %count) | 0) @ "%\c6 Ghosted)";

	%l0 = "Pivot: \c3" @ (%client.ndPivot ? "Selection" : "Start Brick") @ "\c6 [Prev Seat]";
	%l1 = "Size: \c3" @ %x @ "\c6 x \c3" @ %y @ "\c6 x \c3" @ %z;

	%r0 = "Use normal ghost brick controls";
	%r1 = "[Cancel Brick] to exit place mode";

	return ND_FormatMessage(%title, %l0, %r0, %l1, %r1);
}
