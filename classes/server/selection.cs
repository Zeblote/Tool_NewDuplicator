// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_Selection
// *
// *    -------------------------------------------------------------------
// *    Selects, ghosts and plants bricks
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
function ND_Selection::deleteData(%this)
{
	//If count isn't at least 1, assume there is no data
	if(%this.queueCount < 1 && %this.brickCount < 1)
		return;

	%this.rootPosition = "0 0 0";
	%this.queueCount = 0;
	%this.brickCount = 0;

	//Variables follow the pattern $NS[object]_[type]_[...], allowing a single iteration to remove all
	deleteVariables("$NS" @ %this @ "_*");

	%this.deHighlight();
	%this.deleteHighlightBox();
	%this.deleteGhostBricks();
}

//Remove data when selection is deleted
function ND_Selection::onRemove(%this)
{	
	%this.deleteData();

	if(isEventPending(%this.plantSchedule))
		%this.cancelPlanting();
}



//Stack Selection
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin stack selection
function ND_Selection::startStackSelection(%this, %brick, %direction, %limited)
{
	//Create a highlight set
	if(isObject(%this.highlightSet))
		%this.highlightSet.deHighlight();

	%highlightSet = ND_HighlightSet();

	//Root position is position of the first selected brick
	%this.rootPosition = %brick.getPosition();

	//Process first brick
	%queueCount = 1;
	%brickCount = 1;

	$NS[%this, "BR", 0] = %brick;
	$NS[%this, "ID", %brick] = 0;

	%this.recordBrickData(0);
	%highlightSet.addBrick(%brick);

	//Add bricks connected to the first brick to queue
	if(%direction == 1)
	{
		//Set lower height limit
		%heightLimit = $NS[%this, "MinZ"] - 0.01;

		%upCount = %brick.getNumUpBricks();

		for(%i = 0; %i < %upCount; %i++)
		{
			%nextBrick = %brick.getUpBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;

				%queueCount++;
			}

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

		%downCount = %brick.getNumDownBricks();

		for(%i = 0; %i < %downCount; %i++)
		{
			%nextBrick = %brick.getDownBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;
				
				%queueCount++;
			}

			$NS[%this, "DownId", 0, %i] = %nId;
		}

		//Start brick only has down bricks
		$NS[%this, "UpCnt", 0] = 0;
		$NS[%this, "DownCnt", 0] = %downCount;
	}

	%this.highlightSet = %highlightSet;
	%this.queueCount = %queueCount;
	%this.brickCount = %brickCount;

	messageClient(%this.client, 'MsgUploadStart', "");

	//First selection tick
	if(%queueCount > %brickCount)
		%this.tickStackSelection(%direction, %limited, %heightLimit);
	else
		%this.finishStackSelection();
}

//Tick stack selection
function ND_Selection::tickStackSelection(%this, %direction, %limited, %heightLimit)
{
	cancel(%this.stackSelectSchedule);

	%highlightSet = %this.highlightSet;
	%queueCount = %this.queueCount;

	//Continue processing where we left off last tick
	%start = %this.brickCount;
	%end = %start + $ND::StackSelectPerTick;
	
	for(%i = %start; %i < %end; %i++)
	{
		//If no more bricks are queued, we're done!
		if(%i >= %queueCount)
		{
			%this.queueCount = %queueCount;
			%this.brickCount = %i;

			%this.finishStackSelection();
			return;
		}

		//Record data for next brick in queue
		%brick = ND_Selection::recordBrickData(%this, %i);

		if(!%brick)
		{
			messageClient(%this.client, 'MsgError', "\c0Error: Queued brick does not exist anymore. Do not modify the build during selection!");

			%this.cancelStackSelection();
			%this.client.ndSetMode(NDDM_StackSelect);
			return;
		}

		ND_HighlightSet::addBrick(%highlightSet, %brick);

		//Queue all up bricks
		%upCnt = %brick.getNumUpBricks();
		%realUpCnt = 0;

		for(%j = 0; %j < %upCnt; %j++)
		{
			%nextBrick = %brick.getUpBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 0 && getWord(%nextBrick.getWorldBox(), 5) > %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an i
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;				
				%queueCount++;
			}

			$NS[%this, "UpId", %i, %realUpCnt] = %nId;
			%realUpCnt++;
		}

		$NS[%this, "UpCnt", %i] = %realUpCnt;

		//Queue all down bricks
		%downCnt = %brick.getNumDownBricks();
		%realDownCnt = 0;

		for(%j = 0; %j < %downCnt; %j++)
		{
			%nextBrick = %brick.getDownBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 1 && getWord(%nextBrick.getWorldBox(), 2) < %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;				
				%queueCount++;
			}

			$NS[%this, "DownId", %i, %realDownCnt] = %nId;
			%realDownCnt++;
		}

		$NS[%this, "DownCnt", %i] = %realDownCnt;
	}

	%this.queueCount = %queueCount;
	%this.brickCount = %i;

	//Tell the client how much we selected this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Schedule next tick
	%this.stackSelectSchedule = %this.schedule($ND::StackSelectTickDelay, tickStackSelection, %direction, %limited, %heightLimit);
}

//Finish stack selection
function ND_Selection::finishStackSelection(%this)
{
	%this.updateSize();
	%this.updateHighlightBox();

	//De-highlight the bricks after a few seconds
	%this.highlightSet.deHighlightDelayed($ND::HighlightTime);

	messageClient(%this.client, 'MsgUploadEnd', "");
	commandToClient(%this.client, 'centerPrint', "<font:Verdana:20>\c6Selected \c3" @ %this.brickCount @ "\c6 Bricks!", 4);

	%this.client.ndSetMode(NDDM_StackSelect);
}

//Cancel stack selection
function ND_Selection::cancelStackSelection(%this)
{
	cancel(%this.stackSelectSchedule);
	%this.deleteData();
}



//Cube Selection
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin cube selection
function ND_Selection::startCubeSelection(%this, %box, %limited)
{
	//Create a highlight set
	if(isObject(%this.highlightSet))
		%this.highlightSet.deHighlight();

	%highlightSet = ND_HighlightSet();

	//Save the chunk sizes
	%this.chunkX1 = getWord(%box, 0);
	%this.chunkY1 = getWord(%box, 1);
	%this.chunkZ1 = getWord(%box, 2);
	%this.chunkX2 = getWord(%box, 3);
	%this.chunkY2 = getWord(%box, 4);
	%this.chunkZ2 = getWord(%box, 5);

	%this.chunkSize = $ND::CubeSelectChunkSize;

	%this.numChunksX = mCeil((%this.chunkX2 - %this.chunkX1) / %this.chunkSize);
	%this.numChunksY = mCeil((%this.chunkY2 - %this.chunkY1) / %this.chunkSize);
	%this.numChunksZ = mCeil((%this.chunkZ2 - %this.chunkZ1) / %this.chunkSize);
	%this.numChunks = %this.numChunksX * %this.numChunksY * %this.numChunksZ;

	%this.currChunkX = 0;
	%this.currChunkY = 0;
	%this.currChunkZ = 0;
	%this.currChunk = 0;

	%this.highlightSet = %highlightSet;
	%this.queueCount = 0;
	%this.brickCount = 0;

	%this.brickLimitReached = false;

	if(%this.client.isAdmin)
		%brickLimit = $ND::MaxBricksAdmin;
	else
		%brickLimit = $ND::MaxBricksPlayer;

	//Process first tick
	messageClient(%this.client, 'MsgUploadStart', "");
	%this.tickCubeSelectionChunk(%limited, %brickLimit);
}

//Queue all bricks in a chunk
function ND_Selection::tickCubeSelectionChunk(%this, %limited, %brickLimit)
{
	cancel(%this.cubeSelectSchedule);

	//Calculate position and size of chunk
	%x1 = %this.chunkX1 + (%this.currChunkX * %this.chunkSize) + 0.05;
	%y1 = %this.chunkY1 + (%this.currChunkY * %this.chunkSize) + 0.05;
	%z1 = %this.chunkZ1 + (%this.currChunkZ * %this.chunkSize) + 0.05;

	%x2 = getMin(%this.chunkX2, %this.chunkX1 + ((%this.currChunkX + 1) * %this.chunkSize)) - 0.05;
	%y2 = getMin(%this.chunkY2, %this.chunkY1 + ((%this.currChunkY + 1) * %this.chunkSize)) - 0.05;
	%z2 = getMin(%this.chunkZ2, %this.chunkZ1 + ((%this.currChunkZ + 1) * %this.chunkSize)) - 0.05;

	%size = %x2 - %x1 SPC %y2 - %y1 SPC %z2 - %z1;
	%pos = vectorAdd(%x1 SPC %y1 SPC %z1, vectorScale(%size, 0.5));

	//Figure out which sides need to be limited in this chunk
	%limit = false;

	if(%limited)
	{
		if(%this.currChunkX == 0)
			%limitX1 = true;

		if(%this.currChunkX + 1 == %this.numChunksX)
			%limitX2 = true;

		if(%this.currChunkY == 0)
			%limitY1 = true;

		if(%this.currChunkY + 1 == %this.numChunksY)
			%limitY2 = true;

		if(%this.currChunkZ == 0)
			%limitZ1 = true;

		if(%this.currChunkZ + 1 == %this.numChunksZ)
			%limitZ2 = true;

		if(%limitX1 || %limitX2 || %limitY1 || %limitY2 || %limitZ1 || %limitZ2)
			%limit = true;
	}

	//Queue all new bricks found in this chunk
	initContainerBoxSearch(%pos, %size, $TypeMasks::FxBrickAlwaysObjectType);

	%i = %this.queueCount;
	%_id = %this @ "_ID"; //Maximum optimization for loop
	%_br = %this @ "_BR";

	while(%obj = containerSearchNext())
	{
		if($NS[%_id, %obj] $= "")
		{
			if(%limit)
			{
				//Skip bricks that are outside the limit
				%box = %obj.getWorldBox();

				if(%limitX1 && getWord(%box, 0) < %x1 - 0.1)
					continue;

				if(%limitY1 && getWord(%box, 1) < %y1 - 0.1)
					continue;

				if(%limitZ1 && getWord(%box, 2) < %z1 - 0.1)
					continue;

				if(%limitX2 && getWord(%box, 3) > %x2 + 0.1)
					continue;

				if(%limitY2 && getWord(%box, 4) > %y2 + 0.1)
					continue;

				if(%limitZ2 && getWord(%box, 5) > %z2 + 0.1)
					continue;
			}

			$NS[%_id, %obj] = %i;
			$NS[%_br, %i] = %obj;
			%i++;

			if(%i >= %brickLimit)
			{
				%limitReached = true;
				break;
			}
		}
	}

	%this.queueCount = %i;
	
	//Tell the client which chunk we just processed
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//If the brick limit was reached, start processing
	if(%limitReached)
	{
		%this.brickLimitReached = true;
		%this.rootPosition = $NS[%this, "BR", 0].getPosition();
		%this.cubeSelectSchedule = %this.schedule($ND::CubeSelectTickDelay, tickCubeSelectionProcess);

		return;
	}

	//Set next chunk index or finish
	%this.currChunk++;

	if(%this.currChunkZ++ >= %this.numChunksZ)
	{
		%this.currChunkZ = 0;

		if(%this.currChunkY++ >= %this.numChunksY)
		{
			%this.currChunkY = 0;

			if(%this.currChunkX++ >= %this.numChunksX)
			{
				//All chunks have been searched, now process connections
				if(%this.queueCount > 0)
				{
					%this.rootPosition = $NS[%this, "BR", 0].getPosition();
					%this.cubeSelectSchedule = %this.schedule($ND::CubeSelectTickDelay, tickCubeSelectionProcess);
				}
				else
				{
					messageClient(%this.client, 'MsgError', "");
					commandToClient(%this.client, 'centerPrint', "<font:Verdana:20>\c6No bricks were found inside the selection!", 4);

					%this.cancelCubeSelection();
					%this.client.ndSetMode(NDDM_CubeSelect);
				}

				return;
			}
		}
	}

	//Schedule next chunk
	%this.cubeSelectSchedule = %this.schedule($ND::CubeSelectTickDelay, tickCubeSelectionChunk, %limited, %brickLimit);
}

//Save connections between bricks and highlight them
function ND_Selection::tickCubeSelectionProcess(%this)
{
	cancel(%this.cubeSelectSchedule);
	%highlightSet = %this.highlightSet;

	//Get bounds for this tick
	%start = %this.brickCount;
	%end = %start + $ND::CubeSelectPerTick;

	if(%end > %this.queueCount)
		%end = %this.queueCount;

	//Save connections for bricks in the list
	for(%i = %start; %i < %end; %i++)
	{
		//Record data for next brick in queue
		%brick = ND_Selection::recordBrickData(%this, %i);

		if(!%brick)
		{
			messageClient(%this.client, 'MsgError', "\c0Error: Queued brick does not exist anymore. Do not modify the build during selection!");

			%this.cancelCubeSelection();
			%this.client.ndSetMode(NDDM_CubeSelect);
			return;
		}

		ND_HighlightSet::addBrick(%highlightSet, %brick);

		//Save all up bricks
		%upCount = %brick.getNumUpBricks();
		%realUpCnt = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%conn = %brick.getUpBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "ID", %conn]) !$= "")
			{
				$NS[%this, "UpId", %i, %realUpCnt] = %nId;
				%realUpCnt++;
			}
		}

		$NS[%this, "UpCnt", %i] = %realUpCnt;

		//Save all down bricks
		%downCount = %brick.getNumDownBricks();
		%realDownCnt = 0;

		for(%j = 0; %j < %downCount; %j++)
		{
			%conn = %brick.getDownBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "ID", %conn]) !$= "")
			{
				$NS[%this, "DownId", %i, %realDownCnt] = %nId;
				%realDownCnt++;
			}
		}

		$NS[%this, "DownCnt", %i] = %realDownCnt;
	}

	//Save how far we got
	%this.brickCount = %i;

	//Tell the client how much we selected this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%i >= %this.queueCount)
		%this.finishCubeSelection();
	else
		%this.cubeSelectSchedule = %this.schedule($ND::CubeSelectTickDelay, tickCubeSelectionProcess);
}

//Finish cube selection
function ND_Selection::finishCubeSelection(%this)
{
	%this.updateSize();
	%this.updateHighlightBox();

	//De-highlight the bricks after a few seconds
	%this.highlightSet.deHighlightDelayed($ND::HighlightTime);

	messageClient(%this.client, 'MsgUploadEnd', "");

	%msg = "<font:Verdana:20>\c6Selected \c3" @ %this.brickCount @ "\c6 Bricks!";

	if(%this.brickLimitReached)
		%msg = %msg @ " (Limit Reached)";

	%msg = %msg @ "\n<font:Verdana:17>\c6Press [Plant Brick] again to copy.";
	commandToClient(%this.client, 'centerPrint', %msg, 5);

	%this.client.ndSelectionChanged = false;
	%this.client.ndSetMode(NDDM_CubeSelect);
}

//Cancel cube selection
function ND_Selection::cancelCubeSelection(%this)
{
	cancel(%this.cubeSelectSchedule);
	%this.deleteData();
}



//Recording Brick Data
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Record info about a queued brick
function ND_Selection::recordBrickData(%this, %i)
{
	//Return false if brick no longer exists
	if(!isObject(%brick = $NS[%this, "BR", %i]))
		return false;

	///////////////////////////////////////////////////////////
	//Variables required for every brick

	//Datablock
	%datablock = %brick.getDatablock();
	$NS[%this, "Data", %i] = %datablock;

	//Offset from base brick
	$NS[%this, "Pos", %i] = vectorSub(%brick.getPosition(), %this.rootPosition);

	//Rotation
	$NS[%this, "Rot", %i] = %brick.angleID;

	//Colors
	if(%brick.ndHighlightSet)
	{
		$NS[%this, "Color", %i] = %brick.ndColor;

		if(%brick.ndColorFx)
			$NS[%this, "ColorFx", %i] = %brick.ndColorFx;
	}
	else
	{
		$NS[%this, "Color", %i] = %brick.colorID;

		if(%brick.colorFxID)
			$NS[%this, "ColorFx", %i] = %brick.colorFxID;
	}

	///////////////////////////////////////////////////////////
	//Optional variables only required for few bricks

	if(%tmp = %brick.shapeFxID)
		$NS[%this, "ShapeFx", %i] = %tmp;

	//Wrench settings
	if((%tmp = %brick.getName()) !$= "")
		$NS[%this, "Name", %i] = getSubStr(%tmp, 1, 999);

	if(%tmp = %brick.light)
		$NS[%this, "Light", %i] = %tmp.getDatablock();

	if(%tmp = %brick.emitter)
	{
		$NS[%this, "Emitter", %i] = %tmp.getEmitterDatablock();
		$NS[%this, "EmitDir", %i] = %brick.emitterDirection;
	}

	if(%tmp = %brick.item)
	{
		$NS[%this, "Item", %i] = %tmp.getDatablock();
		$NS[%this, "ItemPos", %i] = %brick.itemPosition;
		$NS[%this, "ItemDir", %i] = %brick.itemDirection;
		$NS[%this, "ItemTime", %i] = %brick.itemRespawnTime;
	}

	if(%tmp = %brick.vehicleDataBlock)
	{
		$NS[%this, "Vehicle", %i] = %tmp;
		$NS[%this, "VehColor", %i] = %brick.reColorVehicle;
	}

	if(%tmp = %brick.AudioEmitter)
		$NS[%this, "Music", %i] = %tmp.profile.getID();

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
			$NS[%this, "EvClient", %i, %j] = %brick.eventAppendClient[%j];

			$NS[%this, "EvInput", %i, %j] = %brick.eventInput[%j];
			$NS[%this, "EvInputIdx", %i, %j] = %brick.eventInputIdx[%j];

			$NS[%this, "EvOutput", %i, %j] = %brick.eventOutput[%j];
			$NS[%this, "EvOutputIdx", %i, %j] = %brick.eventOutputIdx[%j];

			%target = %brick.eventTargetIdx[%j];

			if(%target == -1)
				$NS[%this, "EvNT", %i, %j] = %brick.eventNT[%j];

			$NS[%this, "EvTarget", %i, %j] = %brick.eventTarget[%j];
			$NS[%this, "EvTargetIdx", %i, %j] = %target;

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



//Highlighting bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Set the size variables after selecting bricks
function ND_Selection::updateSize(%this)
{
	%this.minSize = vectorSub($NS[%this, "MinX"] SPC $NS[%this, "MinY"] SPC $NS[%this, "MinZ"], %this.rootPosition);
	%this.maxSize = vectorSub($NS[%this, "MaxX"] SPC $NS[%this, "MaxY"] SPC $NS[%this, "MaxZ"], %this.rootPosition);

	%this.brickSizeX = mFloor(($NS[%this, "MaxX"] - $NS[%this, "MinX"]) * 2);
	%this.brickSizeY = mFloor(($NS[%this, "MaxY"] - $NS[%this, "MinY"]) * 2);
	%this.brickSizeZ = mFloor(($NS[%this, "MaxZ"] - $NS[%this, "MinZ"]) * 5);

	%this.rootToCenter = vectorAdd(%this.minSize, vectorScale(vectorSub(%this.maxSize, %this.minSize), 0.5));
}

//Create or update the highlight box
function ND_Selection::updateHighlightBox(%this)
{
	if(!isObject(%this.highlightBox))
		%this.highlightBox = ND_HighlightBox();

	if(!isObject(%this.ghostGroup))
	{
		%min = vectorAdd(%this.rootPosition, %this.minSize);
		%max = vectorAdd(%this.rootPosition, %this.maxSize);
		%this.highlightBox.resize(%min, %max);
	}
	else
		%this.highlightBox.resize(%this.getGhostWorldBox());
}

//Remove the highlight box
function ND_Selection::deleteHighlightBox(%this)
{
	if(isObject(%this.highlightBox))
		%this.highlightBox.delete();
}

//Start clearing the highlight set
function ND_Selection::deHighlight(%this)
{
	if(isObject(%this.highlightSet))
		%this.highlightSet.deHighlight();
}



//Ghost bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Spawn ghost bricks at a specific location
function ND_Selection::spawnGhostBricks(%this, %position, %angleID)
{	
	//Create group to hold the ghost bricks
	%this.ghostGroup = ND_GhostGroup();

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
	%max = %this.brickCount;
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
		%i = mFloor(%f);

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
	%this.ghostAngleID = %angleID;

	//Change highlightbox to blue
	%this.highlightBox.color = "0.2 0.2 1 1";
	%this.highlightBox.applyColors();
	%this.updateHighlightBox();
}

//Move ghost bricks to an offset position
function ND_Selection::shiftGhostBricks(%this, %offset)
{
	//Fit to grid
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
	%this.updateHighlightBox();
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

	%this.ghostAngleID = (%this.ghostAngleID + %direction) % 4;

	//Get vector from pivot to root brick
	%pOffset = vectorSub(%rootBrick.getPosition(), %pivot);

	//Rotate offset vector 90 degrees
	%pOffset = ndRotateVector(%pOffset, %direction);

	//Add shift correction
	%pOffset = vectorAdd(%pOffset, %shiftCorrect);

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
	%this.updateHighlightBox();
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
function ND_Selection::deleteGhostBricks(%this)
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
function ND_Selection::startPlant(%this, %position, %angleID)
{
	%this.plantSearchIndex = 0;
	%this.plantQueueIndex = 0;
	%this.plantQueueCount = 0;

	%this.plantSuccessCount = 0;
	%this.plantTrustFailCount = 0;
	%this.plantBlockedFailCount = 0;

	%this.undoGroup = new SimSet();
	ND_ServerGroup.add(%this.undoGroup);

	messageClient(%this.client, 'MsgUploadStart', "");

	%this.tickPlantSearch($ND::PlantBricksPerTick, %position, %angleID);
}

//Go through the list of bricks until we find one that plants successfully
function ND_Selection::tickPlantSearch(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantSearchIndex;
	%end = %start + %remainingPlants;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%group = %this.client.brickGroup;
	%bl_id = %this.client.bl_id;

	%qCount = %this.plantQueueCount;

	for(%i = %start; %i < %end; %i++)
	{
		//Brick already placed
		if($NP[%this, %i])
			continue;

		//Attempt to place brick
		%brick = ND_Selection::plantBrick(%this, %i, %position, %angleID, %group, %bl_id);

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %i] = true;

			%upCnt = $NS[%this, "UpCnt", %i];
			for(%j = 0; %j < %upCnt; %j++)
			{
				%id = $NS[%this, "UpId", %i, %j];

				if(!$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			%downCnt = $NS[%this, "DownCnt", %i];
			for(%j = 0; %j < %downCnt; %j++)
			{
				%id = $NS[%this, "DownId", %i, %j];

				if(!$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}			

			//If we added bricks to plant queue, switch to second loop
			if(%upCnt || %downCnt)
			{
				%this.plantSearchIndex = %i + 1;
				%this.plantQueueCount = %qCount;
				%this.tickPlantTree(%end - %i, %position, %angleID);
				return;
			}
		}
		else if(%brick == -1)
		{
			$NP[%this, %i] = true;
			%this.plantBlockedFailCount++;
		}
		else if(%brick == -2)
		{
			$NP[%this, %i] = true;
			%this.plantTrustFailCount++;
		}
	}

	%this.plantSearchIndex = %i;
	%this.plantQueueCount = %qCount;

	//Tell the client how far we got
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%end < %this.brickCount && %this.plantSuccessCount < %this.brickCount)
		%this.plantSchedule = %this.schedule($ND::PlantBricksTickDelay, tickPlantSearch, $ND::PlantBricksPerTick, %position, %angleID);
	else
		%this.finishPlant();
}

//Plant search has prepared a queue, plant all bricks in this queue and add their connected bricks aswell
function ND_Selection::tickPlantTree(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantQueueIndex;
	%end = %start + %remainingPlants;

	%group = %this.client.brickGroup;
	%bl_id = %this.client.bl_id;

	%qCount = %this.plantQueueCount;

	for(%i = %start; %i < %end; %i++)
	{
		//The queue is empty! Switch back to plant search.
		if(%i >= %qCount)
		{
			%this.plantQueueCount = %qCount;
			%this.plantQueueIndex = %i;
			%this.tickPlantSearch(%end - %i, %position, %angleID);
			return;
		}

		//Attempt to plant queued brick
		%bId = $NS[%this, "PQueue", %i];

		%brick = ND_Selection::plantBrick(%this, %bId, %position, %angleID, %group, %bl_id);

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %bId] = true;

			%upCnt = $NS[%this, "UpCnt", %bId];
			for(%j = 0; %j < %upCnt; %j++)
			{
				%id = $NS[%this, "UpId", %bId, %j];

				if(!$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			%downCnt = $NS[%this, "DownCnt", %bId];
			for(%j = 0; %j < %downCnt; %j++)
			{
				%id = $NS[%this, "DownId", %bId, %j];

				if(!$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}
		}
		else if(%brick == -1)
		{
			%this.plantBlockedFailCount++;
			$NP[%this, %bId] = true;
		}
		else if(%brick == -2)
		{
			%this.plantTrustFailCount++;
			$NP[%this, %bId] = true;
		}
	}

	//Tell the client how far we got
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	%this.plantQueueCount = %qCount;
	%this.plantQueueIndex = %i;

	%this.plantSchedule = %this.schedule($ND::PlantBricksTickDelay, tickPlantTree, $ND::PlantBricksPerTick, %position, %angleID);
}

//Attempt to plant brick with id %i
//Returns brick if planted, 0 if floating, -1 if blocked, -2 if trust failure
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

	%datablock = $NS[%this, "Data", %i];

	//Attempt to plant brick	
	%brick = new FxDTSBrick()
	{
		datablock = %datablock;
		isPlanted = true;

		position = %bPos;
		rotation = %bRot;
		angleID = %bAngle;

		colorID = $NS[%this, "Color", %i];
		colorFxID = $NS[%this, "ColorFx", %i];
		shapeFxID = $NS[%this, "ShapeFx", %i];

		printID = $NS[%this, "Print", %i];
	};

	if(%error = %brick.plant())
	{
		%brick.delete();

		if(%error == 2)
			return 0;
		
		return -1;
	}

	//Check for trust
	%downCnt = %brick.getNumDownBricks();

	for(%j = 0; %j < %downCnt; %j++)
	{
		%group = %brick.getDownBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] > 0)
			continue;

		if(%group.isPublicDomain)
			continue;

		%brick.delete();
		return -2;
	}

	%upCnt = %brick.getNumUpBricks();

	for(%j = 0; %j < %upCnt; %j++)
	{
		%group = %brick.getUpBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] > 0)
			continue;

		if(%group.isPublicDomain)
			continue;

		%brick.delete();
		return -2;
	}

	//Add to brickgroup
	%brickGroup.add(%brick);
	%brick.setTrusted(true);

	if(%downCnt)
		%brick.stackBL_ID = %brick.getDownBrick(0).stackBL_ID;
	else if(%upCnt)
		%brick.stackBL_ID = %brick.getUpBrick(0).stackBL_ID;
	else
		%brick.stackBL_ID = %bl_id;

	//Hole bots... I guess it works
	if(%datablock.isBotHole)
	{
		%brick.isBotHole = true;
		%brick.onHoleSpawnPlanted();
	}

	//Apply wrench settings
	if($NS[%this, "NoRender", %i])
		%brick.setRendering(false);

	if($NS[%this, "NoRay", %i])
		%brick.setRaycasting(false);

	if($NS[%this, "NoCol", %i])
		%brick.setColliding(false);

	if((%tmp = $NS[%this, "Name", %i]) !$= "")
		%brick.setNTObjectName(%tmp);

	if(%tmp = $NS[%this, "Light", %i])
		%brick.setLight(%tmp);

	if(%tmp = $NS[%this, "Emitter", %i])
	{
		if((%dir = $NS[%this, "EmitDir", %i]) > 1)
			%dir = 2 + ((%dir + %angleID - 2) % 4);

		%brick.emitterDirection = %dir;
		%brick.setEmitter(%tmp);
	}

	if(%tmp = $NS[%this, "Item", %i])
	{
		if((%pos = $NS[%this, "ItemPos", %i]) > 1)
			%pos = 2 + ((%pos + %angleID - 2) % 4);

		if((%dir = $NS[%this, "ItemDir", %i]) > 1)
			%dir = 2 + ((%dir + %angleID - 2) % 4);

		%brick.itemPosition = %pos;
		%brick.itemDirection = %dir;
		%brick.itemRespawnTime = $NS[%this, "ItemTime", %i];
		%brick.setItem(%tmp);
	}

	if(%tmp = $NS[%this, "Vehicle", %i])
	{
		%brick.reColorVehicle = $NS[%this, "VehColor", %i];
		%brick.setVehicle(%tmp);
	}

	if(%tmp = $NS[%this, "Music", %i])
		%brick.setSound(%tmp);

	//Events
	if(%numEvents = $NS[%this, "EvNum", %i])
	{
		%brick.numEvents = %numEvents;
		%brick.implicitCancelEvents = 0;

		for(%j = 0; %j < %numEvents; %j++)
		{
			%brick.eventEnabled[%j] = $NS[%this, "EvEnable", %i, %j];
			%brick.eventDelay[%j] = $NS[%this, "EvDelay", %i, %j];
			%brick.eventAppendClient[%j] = $NS[%this, "EvClient", %i, %j];

			%brick.eventInput[%j] = $NS[%this, "EvInput", %i, %j];
			%brick.eventInputIdx[%j] = $NS[%this, "EvInputIdx", %i, %j];

			%brick.eventOutput[%j] = $NS[%this, "EvOutput", %i, %j];
			%brick.eventOutputIdx[%j] = $NS[%this, "EvOutputIdx", %i, %j];

			%target = $NS[%this, "EvTargetIdx", %i, %j];

			if(%target == -1)
				%brick.eventNT[%j] = $NS[%this, "EvNT", %i, %j];
			
			%brick.eventTarget[%j] = $NS[%this, "EvTarget", %i, %j];
			%brick.eventTargetIdx[%j] = %target;

			%brick.eventOutputParameter[%j, 1] = $NS[%this, "EvPar1", %i, %j];
			%brick.eventOutputParameter[%j, 2] = $NS[%this, "EvPar2", %i, %j];
			%brick.eventOutputParameter[%j, 3] = $NS[%this, "EvPar3", %i, %j];
			%brick.eventOutputParameter[%j, 4] = $NS[%this, "EvPar4", %i, %j];
		}
	}

	return %brick;
}

//Finished planting all the bricks!
function ND_Selection::finishPlant(%this)
{
	messageClient(%this.client, 'MsgProcessComplete', "");

	%count = %this.brickCount;
	%planted = %this.plantSuccessCount;
	%blocked = %this.plantBlockedFailCount;
	%trusted = %this.plantTrustFailCount;
	%floating = %count - %planted - %blocked - %trusted;

	%message = "<font:Verdana:20>\c6Planted \c3" @ %this.plantSuccessCount @ "\c6 / \c3" @ %count @ "\c6 Bricks!";

	if(%trusted)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %trusted @ "\c6 missing trust.";

	if(%blocked)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %blocked @ "\c6 blocked.";

	if(%floating)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %floating @ "\c6 floating.";


	commandToClient(%this.client, 'centerPrint', %message, 4);

	deleteVariables("$NP" @ %this @ "_*");

	if(%planted)
		%this.client.undoStack.push(%this.undoGroup TAB "DUPLICATE");
	else
		%this.undoGroup.delete();

	%this.client.ndSetMode(NDDM_PlantCopy);
}

//Cancel planting bricks
function ND_Selection::cancelPlanting(%this)
{
	cancel(%this.plantSchedule);
	deleteVariables("$NP" @ %this @ "_*");

	if(%this.plantSuccessCount)
		%this.client.undoStack.push(%this.undoGroup TAB "DUPLICATE");
	else
		%this.undoGroup.delete();
}
