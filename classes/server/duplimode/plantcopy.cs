// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function NDM_PlantCopy::onStartMode(%this, %client, %lastMode)
{
	if(%lastMode == $NDM::StackSelect
		|| %lastMode == $NDM::BoxSelect
		|| %lastMode == $NDM::CutProgress
		|| %lastMode == $NDM::LoadProgress)
	{
		%client.ndSelection.spawnGhostBricks(%client.ndSelection.rootPosition, 0);
		%client.ndSelection.angleIdReference = getAngleIDFromPlayer(%client.getControlObject());
	}

	%client.ndUpdateBottomPrint();
}

//Switch away from this mode
function NDM_PlantCopy::onChangeMode(%this, %client, %nextMode)
{
	if(%nextMode == $NDM::StackSelect || %nextMode == $NDM::BoxSelect)
	{
		%client.ndSelection.deleteData();
	}
}

//Kill this mode
function NDM_PlantCopy::onKillMode(%this, %client)
{
	//Destroy the selection
	%client.ndSelection.delete();
}



//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function NDM_PlantCopy::onSelectObject(%this, %client, %obj, %pos, %normal)
{
	%this.moveBricksTo(%client, %pos, %normal);
}



//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Prev Seat
function NDM_PlantCopy::onPrevSeat(%this, %client)
{
	%client.ndPivot = !%client.ndPivot;
	%client.ndUpdateBottomPrint();

	if($Pref::Server::ND::PlayMenuSounds)
		%client.play2d(%client.ndPivot ? lightOnSound : lightOffSound);
}

//Shift Brick
function NDM_PlantCopy::onShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.getControlObject()))
	{
		case 0: %newX =  %x; %newY =  %y;
		case 1: %newX = -%y; %newY =  %x;
		case 2: %newX = -%x; %newY = -%y;
		case 3: %newX =  %y; %newY = -%x;
	}

	%client.ndSelection.shiftGhostBricks(%newX / 2 SPC %newY / 2 SPC %z / 5);
}

//Super Shift Brick
function NDM_PlantCopy::onSuperShiftBrick(%this, %client, %x, %y, %z)
{
	switch(getAngleIDFromPlayer(%client.getControlObject()))
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
function NDM_PlantCopy::onRotateBrick(%this, %client, %direction)
{
	%client.ndSelection.rotateGhostBricks(%direction, %client.ndPivot);
}

//Plant Brick
function NDM_PlantCopy::onPlantBrick(%this, %client)
{
	//Check force plant
	if(%client.ndForcePlant)
	{
		if($Pref::Server::ND::FloatAdminOnly && !%client.isAdmin)
		{
			messageClient(%client, '', "\c6Force Plant has been disabled because it is admin only. Ask an admin for help.");
			%client.ndForcePlant = false;
		}
	}

	%this.conditionalPlant(%client, %client.ndForcePlant);
}

//Cancel Brick
function NDM_PlantCopy::onCancelBrick(%this, %client)
{
	if(%client.ndEquipped)
		%client.ndSetMode(%client.ndLastSelectMode);
	else
		%client.ndKillMode();
}

//Paste Selection
function NDM_PlantCopy::onPaste(%this, %client)
{
	%this.onPlantBrick(%client);
}



//Interface
///////////////////////////////////////////////////////////////////////////

//Create bottomprint for client
function NDM_PlantCopy::getBottomPrint(%this, %client)
{
	%count = %client.ndSelection.brickCount;

	%size = vectorSub(%client.ndSelection.maxSize, %client.ndSelection.minSize);

	%x = mFloor(getWord(%size, 0) * 2);
	%y = mFloor(getWord(%size, 1) * 2);
	%z = mFloor(getWord(%size, 2) * 5);

	if(%count == 1)
		%title = "Plant Mode (\c31\c6 Brick)";
	else if(%count <= $Pref::Server::ND::MaxGhostBricks)
		%title = "Plant Mode (\c3" @ %count @ "\c6 Bricks)";
	else
		%title = "Plant Mode (\c3" @ %count @ "\c6 Bricks, \c3" @ mFloor($Pref::Server::ND::MaxGhostBricks * 100 / %count) @ "%\c6 Ghosted)";

	%l0 = "Pivot: \c3" @ (%client.ndPivot ? "Whole Selection" : "Start Brick") @ "\c6 [Prev Seat]";

	if(isObject(%client.ndSelection.targetGroup))
		%l1 = "Planting as: \c3" @ %client.ndSelection.targetGroup.name;
	else
		%l1 = "Size: \c3" @ %x @ "\c6 x \c3" @ %y @ "\c6 x \c3" @ %z @ "\c6 Plates";

	%r0 = "Use normal ghost brick controls";
	%r1 = "[Cancel Brick] to exit plant mode";

	return ndFormatMessage(%title, %l0, %r0, %l1, %r1);
}



//Functions
///////////////////////////////////////////////////////////////////////////

//Move the bricks to a specific location, like with the brick tool
function NDM_PlantCopy::moveBricksTo(%his, %client, %pos, %normal)
{
	//Get half size of world box for offset
	if(%client.ndPivot)
		%box = %client.ndSelection.getGhostWorldBox();
	else
		%box = %client.ndSelection.ghostGroup.getObject(0).getWorldBox();

	%halfSize = vectorScale(vectorSub(getWords(%box, 3, 5), getWords(%box, 0, 2)), 0.5);

	//Point offset in correct direction based on normal
	%offX = getWord(%halfSize, 0) * mFloatLength(getWord(%normal, 0), 0);
	%offY = getWord(%halfSize, 1) * mFloatLength(getWord(%normal, 1), 0);
	%offZ = getWord(%halfSize, 2) * mFloatLength(getWord(%normal, 2), 0);
	%offset = %offX SPC %offY SPC %offZ;

	//Get shift vector
	%pos = vectorSub(vectorAdd(%pos, %offset), %client.ndSelection.ghostPosition);

	if(%client.ndPivot)
	{
		%toCenter = %client.ndSelection.rootToCenter;

		//Apply mirror
		if(%client.ndSelection.ghostMirrorX)
			%toCenter = -firstWord(%toCenter) SPC restWords(%toCenter);
		else if(%client.ndSelection.ghostMirrorY)
			%toCenter = getWord(%toCenter, 0) SPC -getWord(%toCenter, 1) SPC getWord(%toCenter, 2);

		if(%client.ndSelection.ghostMirrorZ)
			%toCenter = getWord(%toCenter, 0) SPC getWord(%toCenter, 1) SPC -getWord(%toCenter, 2);

		%pos = vectorSub(%pos, ndRotateVector(%toCenter, %client.ndSelection.ghostAngleID));
	}

	%client.ndSelection.shiftGhostBricks(%pos);

	//Offset required for New Brick Tool to display the tracer shape correctly
	if(%client.ndPivot)
		return vectorSub(%client.ndSelection.getGhostCenter(), %offset);
	else
		return vectorSub(%client.ndSelection.ghostGroup.getObject(0).getWorldBoxCenter(), %offset);
}

//Check time limit and attempt to plant bricks
function NDM_PlantCopy::conditionalPlant(%this, %client, %force)
{
	//Check timeout
	if(!%client.isAdmin && %client.ndLastPlantTime + ($Pref::Server::ND::PlantTimeoutMS / 1000) > $Sim::Time)
	{
		%remain = mCeil(%client.ndLastPlantTime + ($Pref::Server::ND::PlantTimeoutMS / 1000) - $Sim::Time);

		if(%remain != 1)
			%s = "s";

		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You need to wait\c3 " @ %remain @ "\c6 second" @ %s @ " before planting again!", 5);
		return;
	}

	//Check too far distance
	%offset = vectorSub(%client.ndSelection.getGhostCenter(), %client.getControlObject().position);

	if(vectorLen(%offset) > $Pref::Server::TooFarDistance)
	{
		messageClient(%client, 'MsgError', "");
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You can't plant so far away!", 5);
		return;
	}

	//Validate target group
	if(isObject(%client.ndSelection.targetGroup) &&
		getTrustLevel(%client, %client.ndSelection.targetGroup) < 1 &&
		(!%client.isAdmin || !$Pref::Server::ND::AdminTrustBypass2))
	{
		messageClient(%client, '', "\c6You need build trust with \c3"
			@ %client.ndSelection.targetGroup.name @ "\c6 to plant bricks in their group.");

		return;
	}

	%client.ndLastPlantTime = $Sim::Time;

	%pos = %client.ndSelection.ghostPosition;
	%ang = %client.ndSelection.ghostAngleID;

	%client.ndSetMode(NDM_PlantCopyProgress);
	%client.ndSelection.startPlant(%pos, %ang, %force);
}
