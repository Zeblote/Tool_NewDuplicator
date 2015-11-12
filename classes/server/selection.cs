// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_Selection
// *
// *    -------------------------------------------------------------------
// *    Store bricks and their parameters in global variables
// *
// * ######################################################################

//General
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Create selection
function ND_Selection(%client)
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_Selection)
		{
			client = %client;
		}
	);

	return %this;
}

//Delete all the selection variables, allowing re-use of object
function ND_Selection::clearData(%this)
{
	//If count isn't at least 1, assume variables were never set
	if($NS[%this, "Count"] < 1)
		return;

	//Variables follow the pattern $NS[object]_[type]_[...], allowing a single iteration to remove all
	deleteVariables("$NS" @ %this @ "_*");
}

//Remove data when selection is deleted
function ND_Selection::onRemove(%this)
{	
	%this.clearData();
	%this.clearGhostBricks();
}



//Stack Selection
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin stack selection
function ND_Selection::startStackSelection(%this, %brick, %direction, %limited, %highlightSet)
{	
	//Set root position
	$NS[%this, "RootPos"] = %brick.getPosition();

	//Process first brick
	$NS[%this, "Brick", 0] = %brick;
	$NS[%this, "ID", %brick] = 0;

	%this.recordBrickData(0);
	%highlightSet.addBrick(%brick);

	$NS[%this, "QueueCount"] = 1;
	$NS[%this, "Count"] = 1;

	%this.highlightSet = %highlightSet;

	if(%direction == 1)
	{
		//Set lower height limit
		%heightLimit = $NS[%this, "MinZ"] - 0.01;

		//Add all up bricks to queue
		%upCount = %brick.getNumUpBricks();

		for(%i = 0; %i < %upCount; %i++)
		{
			%nextBrick = %brick.getUpBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
				%nId = %this.queueBrick(%nextBrick);

			$NS[%this, "UpId", 0, %i] = %nId;
		}

		//Start brick only has up bricks
		$NS[%this, "UpCnt", 0] = %upCount;
		$NS[%this, "DownCnt", 0] = 0;
	}
	else
	{
		//Set upper height limit
		%heightLimit = $NS[%this, "MaxZ"] + 0.01;

		//Add all down bricks to queue
		%downCount = %brick.getNumDownBricks();

		for(%i = 0; %i < %downCount; %i++)
		{
			%nextBrick = %brick.getDownBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
				%nId = %this.queueBrick(%nextBrick);

			$NS[%this, "DownId", 0, %i] = %nId;
		}

		//Start brick only has down bricks
		$NS[%this, "UpCnt", 0] = 0;
		$NS[%this, "DownCnt", 0] = %downCount;
	}

	messageClient(%this.client, 'MsgUploadStart', "");

	//First selection tick
	return %this.tickStackSelection(%direction, %limited, %heightLimit, %highlightSet);
}

//Tick stack selection
function ND_Selection::tickStackSelection(%this, %direction, %limited, %heightLimit, %highlightSet)
{
	//Continue processing where we left off last tick
	cancel(%this.stackSelectSchedule);
	%i = $NS[%this, "Count"];
	
	//Process exactly 200 bricks
	for(%p = 0; %p < $ND::StackSelectPerTick; %p++)
	{
		//If no more bricks are queued, we're done!
		if(%i >= $NS[%this, "QueueCount"])
		{
			%this.finishStackSelection();
			return true;
		}

		//Record data for next brick in queue
		%brick = ND_Selection::recordBrickData(%this, %i);

		if(!%brick)
		{
			messageClient(%this.client, '', "\c0Error: Queued brick does not exist anymore. Do not modify the build during selection!");
			%this.cancelStackSelection();

			if(%this.client.ndModeNum != $NDDM::StackSelect)
				%this.client.ndSetMode(NDDM_StackSelect);
		}

		//Highlight brick
		ND_HighlightSet::addBrick(%highlightSet, %brick);

		//Queue all up bricks
		%upCount = %brick.getNumUpBricks();
		%realUpCnt = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%nextBrick = %brick.getUpBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 0 && getWord(%nextBrick.getWorldBox(), 5) > %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an id
			%nextIndex = $NS[%this, "ID", %nextBrick];

			if(%nextIndex $= "")
			{
				//Copy of ND_Selection::queueBrick
				%nextIndex = $NS[%this, "QueueCount"];
				$NS[%this, "QueueCount"]++;

				$NS[%this, "Brick", %nextIndex] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %i;
			}

			$NS[%this, "UpId", %i, %realUpCnt] = %nextIndex;
			%realUpCnt++;
		}

		$NS[%this, "UpCnt", %i] = %realUpCnt;

		//Queue all down bricks
		%downCount = %brick.getNumDownBricks();
		%realDownCnt = 0;

		for(%j = 0; %j < %downCount; %j++)
		{
			%nextBrick = %brick.getDownBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 1 && getWord(%nextBrick.getWorldBox(), 2) < %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an id
			%nextIndex = $NS[%this, "ID", %nextBrick];

			if(%nextIndex $= "")
			{
				//Copy of ND_Selection::queueBrick
				%nextIndex = $NS[%this, "QueueCount"];
				$NS[%this, "QueueCount"]++;

				$NS[%this, "Brick", %nextIndex] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %i;
			}

			$NS[%this, "DownId", %i, %realDownCnt] = %nextIndex;
			%realDownCnt++;
		}

		$NS[%this, "DownCnt", %i] = %realDownCnt;

		//Save how far we got
		$NS[%this, "Count"]++;

		%i++;
	}

	//Tell the client how much we selected this tick
	%this.client.ndUpdateBottomPrint();

	//Schedule next tick
	%this.stackSelectSchedule = %this.schedule($ND::StackSelectTickDelay, tickStackSelection, %direction, %limited, %heightLimit, %highlightSet);

	return false;
}

//Finish stack selection
function ND_Selection::finishStackSelection(%this)
{
	%this.updateSize();
	%this.highlightSet.deHighlightDelayed($ND::HighlightTime);

	//De-highlight the bricks after a few seconds
	%this.highlightSet.deHighlightDelayed($ND::HighlightTime);

	//Create box to show total size of selection
	if(!isObject(%this.client.ndHighlightBox))
		%this.client.ndHighlightBox = ND_HighlightBox();

	%min = vectorAdd(%this.minSize, $NS[%this, "RootPos"]);
	%max = vectorAdd(%this.maxSize, $NS[%this, "RootPos"]);

	%this.client.ndHighlightBox.resize(%min, %max);

	messageClient(%this.client, 'MsgUploadEnd', "");

	if(%this.client.ndModeNum != $NDDM::StackSelect)
		%this.client.ndSetMode(NDDM_StackSelect);
}

//Cancel stack selection
function ND_Selection::cancelStackSelection(%this)
{
	cancel(%this.stackSelectSchedule);

	//Start de-highlighting the bricks
	%this.highlightSet.deHighlight();

	//Remove highlight box
	if(isObject(%this.client.ndHighlightBox))
		%this.client.ndHighlightBox.delete();

	%this.clearData();
}



//Recording Brick Data
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Record a brick
function ND_Selection::queueBrick(%this, %brick)
{
	%i = $NS[%this, "QueueCount"];
	$NS[%this, "QueueCount"]++;

	$NS[%this, "Brick", %i] = %brick;

	//Reverse ID lookup (brick id -> selection index)
	$NS[%this, "ID", %brick] = %i;

	return %i;
}

//Record info about a brick
function ND_Selection::recordBrickData(%this, %i)
{
	//Return false if brick no longer exists
	if(!isObject(%brick = $NS[%this, "Brick", %i]))
		return false;

	///////////////////////////////////////////////////////////
	//Variables required for every brick

	//Datablock
	%datablock = %brick.getDatablock();
	$NS[%this, "Data", %i] = %datablock;

	//Offset from base brick
	$NS[%this, "Pos", %i] = vectorSub(%brick.getPosition(), $NS[%this, "RootPos"]);

	//Rotation
	$NS[%this, "Rot", %i] = %brick.getAngleID();

	//Colors
	if(%brick.ndHighlightSet)
		$NS[%this, "Color", %i] = %brick.ndColor;
	else
		$NS[%this, "Color", %i] = %brick.getColorID();

	///////////////////////////////////////////////////////////
	//Optional variables only required for few bricks

	if(%brick.getColorFxID())
		$NS[%this, "ColorFx", %i] = %brick.getColorFxID();

	if(%brick.getShapeFxID())
		$NS[%this, "ShapeFx", %i] = %brick.getShapeFxID();

	//Wrench settings
	if(%brick.getName() !$= "")
		$NS[%this, "Name", %i] = getSubStr(%brick.getName(), 1, 999);

	if(%brick.light)
		$NS[%this, "Light", %i] = %brick.light;

	if(%brick.emitter)
	{
		$NS[%this, "Emitter", %i] = %brick.emitter;
		$NS[%this, "EmitDir", %i] = %brick.emitterDirection;
	}

	if(%brick.item)
	{
		$NS[%this, "Item", %i] = %brick.item;
		$NS[%this, "ItemPos", %i] = %brick.itemPosition;
		$NS[%this, "ItemDir", %i] = %brick.itemDirection;
		$NS[%this, "ItemTime", %i] = %brick.itemRespawnTime;
	}

	if(!%brick.isRaycasting())
		$NS[%this, "NoRay", %i] = true;

	if(!%brick.isColliding())	
		$NS[%this, "NoCol", %i] = true;

	if(!%brick.isRendering())
		$NS[%this, "NoRender", %i] = true;

	//Prints
	if(%datablock.hasPrint)
		$NS[%this, "Print", %i] = %brick.printID;

	//Events
	if(%numEvents = %brick.numEvents)
	{
		$NS[%this, "EvNum", %i] = %numEvents;

		for(%j = 0; %j < %numEvents; %j++)
		{
			$NS[%this, "EvEnable", %i, %j] = %brick.eventEnabled[%j];
			$NS[%this, "EvDelay", %i, %j] = %brick.eventDelay[%j];
			$NS[%this, "EvInput", %i, %j] = %brick.eventInputIdx[%j];
			$NS[%this, "EvOutput", %i, %j] = %brick.eventOutputIdx[%j];

			%target = %brick.eventTargetIdx[%j];
			$NS[%this, "EvTarget", %i, %j] = %target;

			if(%target == -1)
				$NS[%this, "EvNT", %i, %j] = %brick.eventNT[%j];

			$NS[%this, "EvPar1", %i, %j] = %brick.eventOutputParameter[%j, 1];
			$NS[%this, "EvPar2", %i, %j] = %brick.eventOutputParameter[%j, 2];
			$NS[%this, "EvPar3", %i, %j] = %brick.eventOutputParameter[%j, 3];
			$NS[%this, "EvPar4", %i, %j] = %brick.eventOutputParameter[%j, 4];
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
		if(%minX < $NS[%this, "MinX"])
			$NS[%this, "MinX"] = %minX;

		if(%minY < $NS[%this, "MinY"])
			$NS[%this, "MinY"] = %minY;

		if(%minZ < $NS[%this, "MinZ"])
			$NS[%this, "MinZ"] = %minZ;

		if(%maxX > $NS[%this, "MaxX"])
			$NS[%this, "MaxX"] = %maxX;

		if(%maxY > $NS[%this, "MaxY"])
			$NS[%this, "MaxY"] = %maxY;

		if(%maxZ > $NS[%this, "MaxZ"])
			$NS[%this, "MaxZ"] = %maxZ;
	}
	else
	{
		$NS[%this, "MinX"] = %minX;
		$NS[%this, "MinY"] = %minY;
		$NS[%this, "MinZ"] = %minZ;
		$NS[%this, "MaxX"] = %maxX;
		$NS[%this, "MaxY"] = %maxY;
		$NS[%this, "MaxZ"] = %maxZ;			
	}

	return %brick;
}

//Set the size variables after selecting bricks
function ND_Selection::updateSize(%this)
{
	%this.minSize = vectorSub($NS[%this, "MinX"] SPC $NS[%this, "MinY"] SPC $NS[%this, "MinZ"], $NS[%this, "RootPos"]);
	%this.maxSize = vectorSub($NS[%this, "MaxX"] SPC $NS[%this, "MaxY"] SPC $NS[%this, "MaxZ"], $NS[%this, "RootPos"]);

	%this.brickSizeX = mFloor(($NS[%this, "MaxX"] - $NS[%this, "MinX"]) * 2);
	%this.brickSizeY = mFloor(($NS[%this, "MaxY"] - $NS[%this, "MinY"]) * 2);
	%this.brickSizeZ = mFloor(($NS[%this, "MaxZ"] - $NS[%this, "MinZ"]) * 5);

	%this.rootToCenter = vectorAdd(%this.minSize, vectorScale(vectorSub(%this.maxSize, %this.minSize), 0.5));
}



//Ghost bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Spawn ghost bricks at a specific location
function ND_Selection::spawnGhostBricks(%this, %position, %angleID)
{	
	if(isObject(%this.ghostGroup))
		return;

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
	%max = $NS[%this, "Count"];
	%increment = 1;

	if(%max > $ND::MaxGhostBricks)
	{
		if($ND::ScatterGhostBricks)
			%increment = %max / $ND::MaxGhostBricks;
		else
			%max = 5000;
	}

	%ghostGroup = %this.ghostGroup;

	//Spawn ghost bricks
	for(%f = 0; %f < %max; %f += %increment)
	{
		%i = %f | 0;

		//Offset position
		%bPos = vectorAdd(ndRotateVector($NS[%this, "Pos", %i], %angleID), %position);

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NS[%this, "Rot", %i] + %angleID ) % 4;

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
			datablock = $NS[%this, "Data", %i];
			isPlanted = false;

			position = %bPos;
			rotation = %bRot;
			angleID = %bAngle;

			colorID = $NS[%this, "Color", %i];
			printID = $NS[%this, "Print", %i];

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

//Move ghost bricks to an offset position
function ND_Selection::shiftGhostBricks(%this, %offset)
{
	if(!isObject(%this.ghostGroup))
		return;

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
	if(!isObject(%this.ghostGroup))
		return;

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
	%bAngle = ($NS[%this, "Rot", 0] + %this.ghostAngleID) % 4;

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
		%bPos = vectorAdd(ndRotateVector($NS[%this, "Pos", %j], %angle), %rootPos);

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NS[%this, "Rot", %j] + %angle ) % 4;

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
	if(!isObject(%this.ghostGroup))
		return;

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
		%bPos = vectorAdd(%pos, ndRotateVector($NS[%this, "Pos", %j], %angle));

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NS[%this, "Rot", %j] + %angle ) % 4;

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

//Delete ghost bricks
function ND_Selection::clearGhostBricks(%this)
{	
	if(!isObject(%this.ghostGroup))
		return;

	cancel(%this.ghostMoveSchedule);

	%this.ghostGroup.deletionTick();
	%this.ghostGroup = false;
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



//Planting bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Start planting bricks!
function ND_Selection::startPlanting(%this, %position, %angleID)
{
	if(!isObject(%this.ghostGroup))
		return false;

	%this.plantSuccessCount = 0;
	%this.plantFailCount = 0;
	%this.trustFailCount = 0;

	messageClient(%this.client, 'MsgUploadStart', "");

	%this.tickPlanting(0, %position, %angleID);
}

//Plant some bricks
function ND_Selection::tickPlanting(%this, %start, %position, %angleID)
{
	cancel(%this.plantSchedule);
	%max = $NS[%this, "Count"];

	if(%max - %start > $ND::PlantBricksPerTick)
	{
		%max = %start + $ND::PlantBricksPerTick;

		//Start schedule to move remaining ghost bricks
		%this.plantSchedule = %this.schedule($ND::PlantBricksTickDelay, tickPlanting, %max, %position, %angleID);
	}
	else
		%done = true;

	%bg = %this.client.brickGroup;
	%bl_id = %this.client.bl_id;

	%successCount = 0;
	%plantFailCount = 0;
	%trustFailCount = 0;

	for(%i = %start; %i < %max; %i++)
	{
		%error = ND_Selection::plantBrick(%this, %i, %position, %angleID, %bg, %bl_id);

		if(!%error)
			%successCount++;
		else if(%error == -1)
			%trustFailCount++;
		else
			%plantFailCount++;
	}

	%this.plantSuccessCount += %successCount;
	%this.plantFailCount += %plantFailCount;
	%this.trustFailCount += %trustFailCount;

	if(%done)
		%this.finishPlanting();
	else
		%this.client.ndUpdateBottomPrint();
}

//Attempt to plant brick with id %i
function ND_Selection::plantBrick(%this, %i, %position, %angleID, %brickGroup, %bl_id)
{
	//Get position and rotation
	%bPos = vectorAdd(ndRotateVector($NS[%this, "Pos", %i], %angleID), %position);
	%bAngle = ($NS[%this, "Rot", %i] + %angleID) % 4;

	switch(%bAngle)
	{
		case 0: %bRot = "1 0 0 0";
		case 1: %bRot = "0 0 1 90.0002";
		case 2: %bRot = "0 0 1 180";
		case 3: %bRot = "0 0 -1 90.0002";
	}

	//Attempt to plant brick	
	%brick = new FxDTSBrick()
	{
		datablock = $NS[%this, "Data", %i];
		client = %client;
		stackBL_ID = %client.BL_ID;
		isPlanted = true;

		position = %bPos;
		rotation = %bRot;
		angleID = %bAngle;

		colorID = $NS[%this, "Color", %i];
		printID = $NS[%this, "Print", %i];
	};

	if(%error = %brick.plant())
	{
		%brick.delete();
		return %error;
	}

	//Check for trust
	%cnt = %brick.getNumDownBricks();

	for(%j = 0; %j < %cnt; %j++)
	{
		%group = %brick.getDownBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] >= $TrustLevel::BuildOn)
			continue;

		if(%group.isPublicDomain)
			continue;

		%brick.delete();
		return -1;
	}

	%cnt = %brick.getNumUpBricks();

	for(%j = 0; %j < %cnt; %j++)
	{
		%group = %brick.getUpBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] >= $TrustLevel::BuildOn)
			continue;

		if(%group.isPublicDomain)
			continue;

		%brick.delete();
		return -1;
	}

	%brickGroup.add(%brick);
	%brick.setTrusted(true);

	//Apply properties
	if($NS[%this, "NoRender", %i])
		%brick.setRendering(false);

	if($NS[%this, "NoRay", %i])
		%brick.setRaycasting(false);

	if($NS[%this, "NoCol", %i])
		%brick.setColliding(false);
}

//Finish planting bricks
function ND_Selection::finishPlanting(%this)
{
	if(%this.client.ndModeNum != $NDDM::PlaceCopy)
		%this.client.ndSetMode(NDDM_PlaceCopy);

	messageClient(%this.client, 'MsgProcessComplete', "");

	commandToClient(%this.client, 'centerPrint', "<font:Verdana:20>\c6Planted \c3" @ %this.plantSuccessCount @ "\c6 Bricks!", 4);
}

//Cancel planting bricks
function ND_Selection::cancelPlanting(%this)
{
	cancel(%this.plantSchedule);
}
