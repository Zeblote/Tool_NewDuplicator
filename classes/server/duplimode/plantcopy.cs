// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDDM_PlantCopy
// *
// *    -------------------------------------------------------------------
// *    Handles inputs for plant mode
// *
// * ######################################################################

//Create object to receive callbacks
ND_ServerGroup.add(
	new ScriptObject(NDDM_PlantCopy)
	{
		class = "NewDuplicatorMode";
		index = $NDDM::PlantCopy;

		allowSelecting = true;
		allowUnMount   = true;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDDM_PlantCopy::onStartMode(%this, %client, %lastMode)
{
	if(%lastMode != $NDDM::PlantCopyProgress)
		%client.ndSelection.spawnGhostBricks(%client.ndSelection.rootPosition, 0);

	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDDM_PlantCopy::onChangeMode(%this, %client, %nextMode)
{	
	if(%nextMode != $NDDM::PlantCopyProgress)
		%client.ndSelection.deleteData();
}

//Kill this mode
function NDDM_PlantCopy::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDDM_PlantCopy::onSelectObject(%this, %client, %obj, %pos, %normal)
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
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Prev Seat
function NDDM_PlantCopy::onPrevSeat(%this, %client)
{
	%client.ndPivot = !%client.ndPivot;
	%client.ndUpdateBottomPrint();

	if($ND::PlayMenuSounds)
		%client.play2d(%client.ndPivot ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDDM_PlantCopy::onShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX =  %x; %newY =  %y;
		case 1: %newX = -%y; %newY =  %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX =  %y; %newY = -%x;
	}

	%client.ndSelection.shiftGhostBricks(%newX / 2 SPC %newY / 2 SPC %z / 5);
}

//Super Shift Brick
function NDDM_PlantCopy::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.player))
	{
		case 0: %newX =  %x; %newY =  %y;
		case 1: %newX = -%y; %newY =  %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX =  %y; %newY = -%x;
	}

	if(%client.ndPivot)
		%box = %client.ndSelection.getGhostWorldBox();
	else
		%box = %client.ndSelection.ghostGroup.getObject(0).getWorldBox();

	%newX *= (getWord(%box, 3) - getWord(%box, 0));
	%newy *= (getWord(%box, 4) - getWord(%box, 1));
	%z    *= (getWord(%box, 5) - getWord(%box, 2));

	%client.ndSelection.shiftGhostBricks(%newX SPC %newY SPC %z);
}

//Rotate Brick
function NDDM_PlantCopy::onRotateBrick(%this, %client, %direction)
{
	if(%direction > 0)
		%client.player.playThread("3", "rotCW");
	else
		%client.player.playThread("3", "rotCCW");

	%client.ndSelection.rotateGhostBricks(%direction, %client.ndPivot);
}

//Plant Brick
function NDDM_PlantCopy::onPlantBrick(%this, %client)
{
	%pos = %client.ndSelection.ghostPosition;
	%ang = %client.ndSelection.ghostAngleID;

	%client.ndSetMode(NDDM_PlantCopyProgress);
	%client.ndSelection.startPlant(%pos, %ang);
}

//Cancel Brick
function NDDM_PlantCopy::onCancelBrick(%this, %client)
{
	if(%client.ndEquipped)
		%client.ndSetMode(%client.ndLastSelectMode);
	else
		%client.ndKillMode();
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDDM_PlantCopy::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;

	%size = vectorSub(%client.ndSelection.maxSize, %client.ndSelection.minSize);

	%x = mFloor(getWord(%size, 0) * 2);
	%y = mFloor(getWord(%size, 1) * 2);
	%z = mFloor(getWord(%size, 2) * 5);

	if(%count == 1)
		%title = "Plant Mode (\c31\c6 Brick)";
	else if(%count <= $ND::MaxGhostBricks)
		%title = "Plant Mode (\c3" @ %count @ "\c6 Bricks)";
	else
		%title = "Plant Mode (\c3" @ %count @ "\c6 Bricks, \c3" @ mFloor($ND::MaxGhostBricks * 100 / %count) @ "%\c6 Ghosted)";

	%l0 = "Pivot: \c3" @ (%client.ndPivot ? "Whole Selection" : "Start Brick") @ "\c6 [Prev Seat]";
	%l1 = "Size: \c3" @ %x @ "\c6 x \c3" @ %y @ "\c6 x \c3" @ %z @ "\c6 Plates";

	%r0 = "Use normal ghost brick controls";
	%r1 = "[Cancel Brick] to exit plant mode";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1);
}
