// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_Selection
// *
// *    -------------------------------------------------------------------
// *    Store bricks and their parameters in global variables
// *
// * ######################################################################

//Create selection
function ND_Selection()
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_Selection)
	);

	return %this;
}

//Record a brick
function ND_Selection::queueBrick(%this, %brick)
{
	%i = $NDS[%this, "QueueCount"];
	$NDS[%this, "QueueCount"]++;

	$NDS[%this, "Brick", %i] = %brick;

	//Reverse ID lookup (brick id -> selection index)
	$NDS[%this, "ID", %brick] = %i;

	return %i;
}

//Record info about a brick
function ND_Selection::recordBrickData(%this, %i)
{
	//Return false if brick no longer exists
	if(!isObject(%brick = $NDS[%this, "Brick", %i]))
		return false;

	///////////////////////////////////////////////////////////
	//Variables required for every brick

	//Datablock
	%datablock = %brick.getDatablock();
	$NDS[%this, "Data", %i] = %datablock;

	//Offset from base brick
	$NDS[%this, "Pos", %i] = vectorSub(%brick.getPosition(), $NDS[%this, "RootPos"]);

	//Rotation
	$NDS[%this, "Rot", %i] = %brick.getAngleID();

	//Colors
	if(%brick.ndHighlightSet)
		$NDS[%this, "Color", %i] = %brick.ndColor;
	else
		$NDS[%this, "Color", %i] = %brick.getColorID();

	///////////////////////////////////////////////////////////
	//Optional variables only required for few bricks

	if(%brick.getColorFxID())
		$NDS[%this, "ColorFx", %i] = %brick.getColorFxID();

	if(%brick.getShapeFxID())
		$NDS[%this, "ShapeFx", %i] = %brick.getShapeFxID();

	//Wrench settings
	if(%brick.getName() !$= "")
		$NDS[%this, "Name", %i] = getSubStr(%brick.getName(), 1, 999);

	if(%brick.light)
		$NDS[%this, "Light", %i] = %brick.light;

	if(%brick.emitter)
	{
		$NDS[%this, "Emitter", %i] = %brick.emitter;
		$NDS[%this, "EmitDir", %i] = %brick.emitterDirection;
	}

	if(%brick.item)
	{
		$NDS[%this, "Item", %i] = %brick.item;
		$NDS[%this, "ItemPos", %i] = %brick.itemPosition;
		$NDS[%this, "ItemDir", %i] = %brick.itemDirection;
		$NDS[%this, "ItemTime", %i] = %brick.itemRespawnTime;
	}

	if(!%brick.isRaycasting())
		$NDS[%this, "NoRay", %i] = true;

	if(!%brick.isColliding())	
		$NDS[%this, "NoCol", %i] = true;

	if(!%brick.isRendering())
		$NDS[%this, "NoRender", %i] = true;

	//Prints
	if(%datablock.hasPrint)
		$NDS[%this, "Print", %i] = %brick.printID;

	//Events
	if(%numEvents = %brick.numEvents)
	{
		$NDS[%this, "EvNum", %i] = %numEvents;

		for(%j = 0; %j < %numEvents; %j++)
		{
			$NDS[%this, "EvEnable", %i, %j] = %brick.eventEnabled[%j];
			$NDS[%this, "EvDelay", %i, %j] = %brick.eventDelay[%j];
			$NDS[%this, "EvInput", %i, %j] = %brick.eventInputIdx[%j];
			$NDS[%this, "EvOutput", %i, %j] = %brick.eventOutputIdx[%j];

			%target = %brick.eventTargetIdx[%j];
			$NDS[%this, "EvTarget", %i, %j] = %target;

			if(%target == -1)
				$NDS[%this, "EvNT", %i, %j] = %brick.eventNT[%j];

			$NDS[%this, "EvPar1", %i, %j] = %brick.eventOutputParameter[%j, 1];
			$NDS[%this, "EvPar2", %i, %j] = %brick.eventOutputParameter[%j, 2];
			$NDS[%this, "EvPar3", %i, %j] = %brick.eventOutputParameter[%j, 3];
			$NDS[%this, "EvPar4", %i, %j] = %brick.eventOutputParameter[%j, 4];
		}
	}

	//Update total selection size
	%box = %brick.getWorldBox();
	%minX = getWord(%box, 0);
	%minY = getWord(%box, 1);
	%minZ = getWord(%box, 2);
	%maxX = getWord(%box, 3);
	%maxY = getWord(%box, 4);
	%maxZ = getWord(%box, 5);

	if(%i)
	{
		if(%minX < $NDS[%this, "MinX"])
			$NDS[%this, "MinX"] = %minX;

		if(%minY < $NDS[%this, "MinY"])
			$NDS[%this, "MinY"] = %minY;

		if(%minZ < $NDS[%this, "MinZ"])
			$NDS[%this, "MinZ"] = %minZ;

		if(%maxX > $NDS[%this, "MaxX"])
			$NDS[%this, "MaxX"] = %maxX;

		if(%maxY > $NDS[%this, "MaxY"])
			$NDS[%this, "MaxY"] = %maxY;

		if(%maxZ > $NDS[%this, "MaxZ"])
			$NDS[%this, "MaxZ"] = %maxZ;
	}
	else
	{
		$NDS[%this, "MinX"] = %minX;
		$NDS[%this, "MinY"] = %minY;
		$NDS[%this, "MinZ"] = %minZ;
		$NDS[%this, "MaxX"] = %maxX;
		$NDS[%this, "MaxY"] = %maxY;
		$NDS[%this, "MaxZ"] = %maxZ;			
	}

	return %brick;
}

//Set the size variables after selecting bricks
function ND_Selection::updateSize(%this)
{
	%this.minSize = vectorSub($NDS[%this, "MinX"] SPC $NDS[%this, "MinY"] SPC $NDS[%this, "MinZ"], $NDS[%this, "RootPos"]);
	%this.maxSize = vectorSub($NDS[%this, "MaxX"] SPC $NDS[%this, "MaxY"] SPC $NDS[%this, "MaxZ"], $NDS[%this, "RootPos"]);

	%this.brickSizeX = (($NDS[%this, "MaxX"] - $NDS[%this, "MinX"]) * 2) | 0;
	%this.brickSizeY = (($NDS[%this, "MaxY"] - $NDS[%this, "MinY"]) * 2) | 0;
	%this.brickSizeZ = (($NDS[%this, "MaxZ"] - $NDS[%this, "MinZ"]) * 5) | 0;

	%this.rootToCenter = vectorAdd(%this.minSize, vectorScale(vectorSub(%this.maxSize, %this.minSize), 0.5));
}

//Delete all the selection variables, allowing re-use of object
function ND_Selection::clear(%this)
{
	//If count isn't at least 1, assume variables were never set
	if($NDS[%this, "Count"] < 1)
		return;

	//Variables follow the pattern $NDS[object]_[type]_[...], allowing a single iteration to remove all
	deleteVariables("$NDS" @ %this @ "_*");
}

//Remove data when selection is deleted
function ND_Selection::onRemove(%this)
{	
	%this.clear();
	%this.clearGhostBricks();
}

//Spawn ghost bricks at a specific location
function ND_Selection::spawnGhostBricks(%this, %position, %angleID)
{	
	if(isObject(%this.ghostGroup))
	{
		talk("Ghost set already exists");
		return;
	}

	//Keep this inside allowed range
	%angleID = %angleID % 4;
	%this.ghostAngleID = %angleID;

	//Create simGroup to hold the ghost bricks
	%this.ghostGroup = new ScriptGroup(ND_GhostGroup);

	//Scoping is broken for ghost bricks, make temp list of spawned clients to use later
	%numClients = 0;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.hasSpawnedOnce)
		{
			%client[%numClients] = %cl;
			%numClients++;
		}
	}

	//Figure out correct increment to spawn no more than the max number of ghost bricks
	%max = $NDS[%this, "Count"];
	%increment = 1;

	if(%max > $ND::MaxGhostBricks)
	{
		if($ND::GhostBySelectionOrder)
			%max = 5000;
		else
			%increment = %max / $ND::MaxGhostBricks;
	}

	%ghostGroup = %this.ghostGroup;

	//Spawn ghost bricks
	for(%f = 0; %f < %max; %f += %increment)
	{
		%i = %f | 0;

		//Offset position
		%bPos = vectorAdd(ndRotateVector($NDS[%this, "Pos", %i], %angleID), %position);

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NDS[%this, "Rot", %i] + %angleID ) % 4;

		switch(%bAngle)
		{
			case 0: %bRot = "1 0 0 0";
			case 1: %bRot = "0 0 1 90.0002";
			case 2: %bRot = "0 0 1 180";
			case 3: %bRot = "0 0 -1 90.0002";
		}

		//Spawn ghost brick
		%brick = new FxDTSBrick()
		{
			datablock = $NDS[%this, "Data", %i];
			isPlanted = false;

			position = %bPos;
			rotation = %bRot;
			angleID = %bAngle;

			colorID = $NDS[%this, "Color", %i];
			printID = $NDS[%this, "Print", %i];

			//Used in shiftGhostBricks
			selectionIndex = %i;
		};

		//Add ghost brick to ghost set
		%ghostGroup.add(%brick);

		//Scope ghost brick to all clients we found earlier
		for(%j = 0; %j < %numClients; %j++)
			%brick.scopeToClient(%client[%j]);
	}

	//Update variables
	%this.ghostPosition = %position;
}

//Delete ghost bricks
function ND_Selection::clearGhostBricks(%this)
{	
	if(!isObject(%this.ghostGroup))
		return;

	cancel(%this.ghostMoveSchedule);

	%this.ghostGroup.deletionTick();
	%this.ghostGroup = false;
}

//Move ghost bricks to an offset position
function ND_Selection::shiftGhostBricks(%this, %offset)
{
	//Fix to grid
	%x = mFloatLength(getWord(%offset, 0) * 2, 0) / 2;
	%y = mFloatLength(getWord(%offset, 1) * 2, 0) / 2;
	%z = mFloatLength(getWord(%offset, 2) * 5, 0) / 5;

	if(%x == 0 && %y == 0 && %z == 0)
		return;

	cancel(%this.ghostMoveSchedule);
	%max = %this.ghostGroup.getCount();

	if(%max > $ND::InstantGhostBricks)
	{
		%max = $ND::InstantGhostBricks;

		//Start schedule to move remaining ghost bricks
		%this.ghostMoveSchedule = %this.schedule($ND::GhostBricksInitialDelay, updateGhostBricks, %max);
	}

	%ghostGroup = %this.ghostGroup;

	%offset = %x SPC %y SPC %z;

	//Move the instant ghost bricks right now
	for(%i = 0; %i < %max; %i++)
	{
		%brick = %ghostGroup.getObject(%i);
		%brick.setTransform(vectorAdd(%brick.position, %offset));
	}

	//Update variables
	%this.ghostPosition = %this.ghostGroup.getObject(0).getPosition();
}

//Rotate ghost bricks left/right
function ND_Selection::rotateGhostBricks(%this, %direction, %useSelectionCenter)
{
	cancel(%this.ghostMoveSchedule);
	%max = %this.ghostGroup.getCount();

	if(%max > $ND::InstantGhostBricks)
	{
		%max = $ND::InstantGhostBricks;

		//Start schedule to move remaining ghost bricks
		%this.ghostMoveSchedule = %this.schedule($ND::GhostBricksInitialDelay, updateGhostBricks, %max);
	}

	//First brick is root brick
	%rootBrick = %this.ghostGroup.getObject(0);

	//Figure out the pivot and shift values
	if(%useSelectionCenter)
	{
		%pivot = %this.getGhostCenter();

		%brickSizeX = %this.brickSizeX;
		%brickSizeY = %this.brickSizeY;
	}
	else
	{
		%pivot = %this.ghostPosition;

		%brickSizeX = %rootBrick.getDatablock().brickSizeX;
		%brickSizeY = %rootBrick.getDatablock().brickSizeY;
	}

	//Even x odd sized rectangles can't be rotated around their center to stay in the grid
	%shiftCorrect = "0 0 0";	

	if((%brickSizeX % 2) != (%brickSizeY % 2))
	{
		if(%this.ghostAngleID % 2)
			%shiftCorrect = "-0.25 -0.25 0";
		else
			%shiftCorrect = "0.25 0.25 0";
	}

	//Get vector from pivot to root brick
	%pOffset = vectorSub(%rootBrick.getPosition(), %pivot);

	//Rotate offset vector 90 degrees
	%pOffset = ndRotateVector(%pOffset, %direction);

	//Add shift correction
	%pOffset = vectorAdd(%pOffset, %shiftCorrect);

	%this.ghostAngleID = (%this.ghostAngleID + %direction) % 4;

	//Get angleID for root brick
	%bAngle = ($NDS[%this, "Rot", 0] + %this.ghostAngleID) % 4;

	switch(%bAngle)
	{
		case 0: %bRot = "1 0 0 0";
		case 1: %bRot = "0 0 1 1.5708";
		case 2: %bRot = "0 0 1 3.14150";
		case 3: %bRot = "0 0 -1 1.5708";
	}

	//Transform root brick
	%rootBrick.setTransform(vectorAdd(%pivot, %pOffset) SPC %bRot);

	//Rootbrick snapped to brick grid, get new position
	%rootPos = %rootBrick.getPosition();

	%angle = %this.ghostAngleID;
	%ghostGroup = %this.ghostGroup;

	//Now move some other bricks
	for(%i = 1; %i < %max; %i++)
	{
		%brick = %ghostGroup.getObject(%i);
		%j = %brick.selectionIndex;

		//Offset position
		%bPos = vectorAdd(ndRotateVector($NDS[%this, "Pos", %j], %angle), %rootPos);

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NDS[%this, "Rot", %j] + %angle ) % 4;

		switch(%bAngle)
		{
			case 0: %bRot = "1 0 0 0";
			case 1: %bRot = "0 0 1 1.5708";
			case 2: %bRot = "0 0 1 3.14150";
			case 3: %bRot = "0 0 -1 1.5708";
		}

		%brick.setTransform(%bPos SPC %bRot);
	}

	//Update variables
	%this.ghostPosition = %rootPos;
}

//Update some of the ghost bricks to the latest position/rotation
function ND_Selection::updateGhostBricks(%this, %start)
{
	cancel(%this.ghostMoveSchedule);
	%max = %this.ghostGroup.getCount();

	if(%max - %start > $ND::GhostBricksPerTick)
	{
		%max = %start + $ND::GhostBricksPerTick;

		//Start schedule to move remaining ghost bricks
		%this.ghostMoveSchedule = %this.schedule($ND::GhostBricksTickDelay, updateGhostBricks, %max);
	}

	%pos = %this.ghostPosition;
	%angle = %this.ghostAngleID;
	%ghostGroup = %this.ghostGroup;

	//Update the ghost bricks in this tick
	for(%i = %start; %i < %max; %i++)
	{
		%brick = %ghostGroup.getObject(%i);
		%j = %brick.selectionIndex;

		//Offset position
		%bPos = vectorAdd(%pos, ndRotateVector($NDS[%this, "Pos", %j], %angle));

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NDS[%this, "Rot", %j] + %angle ) % 4;

		switch(%bAngle)
		{
			case 0: %bRot = "1 0 0 0";
			case 1: %bRot = "0 0 1 1.5708";
			case 2: %bRot = "0 0 1 3.14150";
			case 3: %bRot = "0 0 -1 1.5708";
		}

		%brick.setTransform(%bPos SPC %bRot);
	}
}

//World box center for ghosted selection
function ND_Selection::getGhostCenter(%this)
{
	if(!isObject(%this.ghostGroup))
		return "0 0 0";

	%pos = %this.ghostGroup.getObject(0).getPosition();
	%offset = ndRotateVector(%this.rootToCenter, %this.ghostAngleID);

	return vectorAdd(%pos, %offset);
}

//World box for ghosted selection
function ND_Selection::getGhostWorldBox(%this)
{
	if(!isObject(%this.ghostGroup))
		return "0 0 0 0 0 0";

	%pos = %this.ghostGroup.getObject(0).getPosition();
	%min = ndRotateVector(%this.minSize, %this.ghostAngleID);
	%max = ndRotateVector(%this.maxSize, %this.ghostAngleID);

	%minX = getMin(getWord(%min, 0), getWord(%max, 0));
	%minY = getMin(getWord(%min, 1), getWord(%max, 1));
	%minZ = getMin(getWord(%min, 2), getWord(%max, 2));

	%maxX = getMax(getWord(%min, 0), getWord(%max, 0));
	%maxY = getMax(getWord(%min, 1), getWord(%max, 1));
	%maxZ = getMax(getWord(%min, 2), getWord(%max, 2));

	return vectorAdd(%pos, %minX SPC %minY SPC %minZ) SPC vectorAdd(%pos, %maxX SPC %maxY SPC %maxZ);
}
