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

	%this.brickLimitReached = false;
	%this.trustFailCount = 0;

	if(%this.client.isAdmin)
		%brickLimit = $Pref::Server::ND::MaxBricksAdmin;
	else
		%brickLimit = $Pref::Server::ND::MaxBricksPlayer;

	//Root position is position of the first selected brick
	%this.rootPosition = %brick.getPosition();

	//Process first brick
	%queueCount = 1;
	%brickCount = 1;

	$NS[%this, "BR", 0] = %brick;
	$NS[%this, "ID", %brick] = 0;

	%this.recordBrickData(0);
	%highlightSet.addBrick(%brick);

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	//Add bricks connected to the first brick to queue
	%conns = 0;

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
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelection(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;

				%queueCount++;
			}

			$NS[%this, "Conn", 0, %conns] = %nId;
			%conns++;
		}
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
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelection(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;
				
				%queueCount++;
			}

			$NS[%this, "Conn", 0, %conns] = %nId;
			%conns++;
		}
	}

	//Save number of connections
	$NS[%this, "Conns", 0] = %conns;

	%this.trustFailCount += %trustFailCount;
	%this.highlightSet = %highlightSet;
	%this.queueCount = %queueCount;
	%this.brickCount = %brickCount;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//First selection tick
	if(%queueCount > %brickCount)
		%this.tickStackSelection(%direction, %limited, %heightLimit, %brickLimit);
	else
		%this.finishStackSelection();
}

//Tick stack selection
function ND_Selection::tickStackSelection(%this, %direction, %limited, %heightLimit, %brickLimit)
{
	cancel(%this.stackSelectSchedule);

	%highlightSet = %this.highlightSet;
	%queueCount = %this.queueCount;

	//Continue processing where we left off last tick
	%start = %this.brickCount;
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;
	
	for(%i = %start; %i < %end; %i++)
	{
		//If no more bricks are queued, we're done!
		if(%i >= %queueCount)
		{
			%this.queueCount = %queueCount;
			%this.brickCount = %i;

			if(%i >= %brickLimit)
				%this.brickLimitReached = true;

			%this.finishStackSelection();
			return;
		}

		//Record data for next brick in queue
		%brick = ND_Selection::recordBrickData(%this, %i);

		if(!%brick)
		{
			messageClient(%this.client, 'MsgError', "\c0Error: Queued brick does not exist anymore. Do not modify the build during selection!");

			%this.cancelStackSelection();
			%this.client.ndSetMode(NDM_StackSelect);
			return;
		}

		ND_HighlightSet::addBrick(%highlightSet, %brick);

		//Queue all up bricks
		%upCount = %brick.getNumUpBricks();
		%conns = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%nextBrick = %brick.getUpBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 0 && getWord(%nextBrick.getWorldBox(), 5) > %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an i
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelection(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;				
				%queueCount++;
			}

			$NS[%this, "Conn", %i, %conns] = %nId;
			%conns++;
		}

		//Queue all down bricks
		%downCount = %brick.getNumDownBricks();

		for(%j = 0; %j < %downCount; %j++)
		{
			%nextBrick = %brick.getDownBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 1 && getWord(%nextBrick.getWorldBox(), 2) < %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to give it an id
			%nId = $NS[%this, "ID", %nextBrick];

			if(%nId $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;
					
				//Check trust
				if(!ndTrustCheckSelection(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "BR", %queueCount] = %nextBrick;
				$NS[%this, "ID", %nextBrick] = %queueCount;
				%nId = %queueCount;				
				%queueCount++;
			}

			$NS[%this, "Conn", %i, %conns] = %nId;
			%conns++;
		}

		$NS[%this, "Conns", %i] = %conns;
	}

	%this.trustFailCount += %trustFailCount;
	%this.queueCount = %queueCount;
	%this.brickCount = %i;

	if(%i >= %brickLimit)
	{
		%this.brickLimitReached = true;
		%this.finishStackSelection();
		return;
	}

	//Tell the client how much we selected this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Schedule next tick
	%this.stackSelectSchedule = %this.schedule(30, tickStackSelection, %direction, %limited, %heightLimit, %brickLimit);
}

//Finish stack selection
function ND_Selection::finishStackSelection(%this)
{
	%this.updateSize();
	%this.updateHighlightBox();

	//De-highlight the bricks after a few seconds
	%this.highlightSet.deHighlightDelayed($Pref::Server::ND::HighlightDelay * 1000);

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadEnd', "");

	%s = %this.brickCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Selected \c3" @ %this.brickCount @ "\c6 Brick" @ %s @ "!";

	if(%this.brickLimitReached)
		%msg = %msg @ " (Limit Reached)";

	if(%this.trustFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

	commandToClient(%this.client, 'centerPrint', %msg, 5);

	%this.client.ndSetMode(NDM_StackSelect);
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

	%this.chunkSize = $Pref::Server::ND::CubeSelectChunkSize;

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

	%this.trustFailCount = 0;
	%this.brickLimitReached = false;

	if(%this.client.isAdmin)
		%brickLimit = $Pref::Server::ND::MaxBricksAdmin;
	else
		%brickLimit = $Pref::Server::ND::MaxBricksPlayer;

	//Process first tick
	if($Pref::Server::ND::PlayMenuSounds)
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

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

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

			//Check trust
			if(!ndTrustCheckSelection(%obj, %group, %bl_id, %admin))
			{
				%trustFailCount++;
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

	%this.trustFailCount += %trustFailCount;
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
		%this.cubeSelectSchedule = %this.schedule(30, tickCubeSelectionProcess);

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
					%this.cubeSelectSchedule = %this.schedule(30, tickCubeSelectionProcess);
				}
				else
				{
					messageClient(%this.client, 'MsgError', "");

					%m = "<font:Verdana:20>\c6No bricks were found inside the selection!";

					if(%this.trustFailCount > 0)
						%m = %m @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

					commandToClient(%this.client, 'centerPrint', %m, 5);

					%this.cancelCubeSelection();
					%this.client.ndSetMode(NDM_CubeSelect);
				}

				return;
			}
		}
	}

	//Schedule next chunk
	%this.cubeSelectSchedule = %this.schedule(30, tickCubeSelectionChunk, %limited, %brickLimit);
}

//Save connections between bricks and highlight them
function ND_Selection::tickCubeSelectionProcess(%this)
{
	cancel(%this.cubeSelectSchedule);
	%highlightSet = %this.highlightSet;

	//Get bounds for this tick
	%start = %this.brickCount;
	%end = %start + $Pref::Server::ND::ProcessPerTick;

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
			%this.client.ndSetMode(NDM_CubeSelect);
			return;
		}

		ND_HighlightSet::addBrick(%highlightSet, %brick);

		//Save all up bricks
		%upCount = %brick.getNumUpBricks();
		%conns = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%conn = %brick.getUpBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "ID", %conn]) !$= "")
			{
				$NS[%this, "Conn", %i, %conns] = %nId;
				%conns++;
			}
		}

		//Save all down bricks
		%downCount = %brick.getNumDownBricks();

		for(%j = 0; %j < %downCount; %j++)
		{
			%conn = %brick.getDownBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "ID", %conn]) !$= "")
			{
				$NS[%this, "Conn", %i, %conns] = %nId;
				%conns++;
			}
		}

		$NS[%this, "Conns", %i] = %conns;
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
		%this.cubeSelectSchedule = %this.schedule(30, tickCubeSelectionProcess);
}

//Finish cube selection
function ND_Selection::finishCubeSelection(%this)
{
	%this.updateSize();
	%this.updateHighlightBox();

	//De-highlight the bricks after a few seconds
	%this.highlightSet.deHighlightDelayed($Pref::Server::ND::HighlightDelay * 8000);

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadEnd', "");

	%s = %this.brickCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Selected \c3" @ %this.brickCount @ "\c6 Brick" @ %s @ "!";

	if(%this.brickLimitReached)
		%msg = %msg @ " (Limit Reached)";

	if(%this.trustFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

	%msg = %msg @ "\n<font:Verdana:17>\c6Press [Plant Brick] again to copy.";
	commandToClient(%this.client, 'centerPrint', %msg, 8);

	%this.client.ndSelectionChanged = false;
	%this.client.ndSetMode(NDM_CubeSelect);
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

	if(%tmp = %brick.light | 0)
		$NS[%this, "Light", %i] = %tmp.getDatablock();

	if(%tmp = %brick.emitter | 0)
	{
		$NS[%this, "Emitter", %i] = %tmp.getEmitterDatablock();
		$NS[%this, "EmitDir", %i] = %brick.emitterDirection;
	}

	if(%tmp = %brick.item | 0)
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

	if(%tmp = %brick.AudioEmitter | 0)
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

			$NS[%this, "EvPar", %i, %j, 0] = %brick.eventOutputParameter[%j, 1];
			$NS[%this, "EvPar", %i, %j, 1] = %brick.eventOutputParameter[%j, 2];
			$NS[%this, "EvPar", %i, %j, 2] = %brick.eventOutputParameter[%j, 3];
			$NS[%this, "EvPar", %i, %j, 3] = %brick.eventOutputParameter[%j, 4];
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



//Cutting bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//Begin cutting
function ND_Selection::startCutting(%this)
{
	//Process first tick
	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	%this.cutIndex = 0;
	%this.cutSuccessCount = 0;
	%this.cutFailCount = 0;

	%this.tickCutting();
}

//Cut some bricks
function ND_Selection::tickCutting(%this)
{
	cancel(%this.cutSchedule);

	//Get bounds for this tick
	%start = %this.cutIndex;
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%cutSuccessCount = %this.cutSuccessCount;
	%cutFailCount = %this.cutFailCount;

	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	//Cut bricks
	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NS[%this, "BR", %i];

		if(!isObject(%brick))
			continue;

		if(!ndTrustCheckCut(%brick, %group, %bl_id))
		{
			%cutFailCount++;
			continue;
		}

		%brick.delete();
		%cutSuccessCount++;
	}

	//Save how far we got
	%this.cutIndex = %i;

	%this.cutSuccessCount = %cutSuccessCount;
	%this.cutFailCount = %cutFailCount;

	//Tell the client how much we selected this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%i >= %this.brickCount)
		%this.finishCutting();
	else
		%this.cutSchedule = %this.schedule(30, tickCutting);
}

//Finish cutting
function ND_Selection::finishCutting(%this)
{
	%s = %this.cutSuccessCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Cut \c3" @ %this.cutSuccessCount @ "\c6 Brick" @ %s @ "!";

	if(%this.cutFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.cutFailCount @ "\c6 missing trust.";

	commandToClient(%this.client, 'centerPrint', %msg, 8);

	%this.client.ndSetMode(NDM_PlantCopy);
}

//Cancel cutting
function ND_Selection::cancelCutting(%this)
{
	cancel(%this.cutSchedule);
}



//Ghost bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Spawn ghost bricks at a specific location
function ND_Selection::spawnGhostBricks(%this, %position, %angleID)
{
	%this.ghostMirrorX = false;
	%this.ghostMirrorY = false;
	%this.ghostMirrorZ = false;

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

	if(%max > $Pref::Server::ND::MaxGhostBricks)
	{
		if($Pref::Server::ND::ScatterGhostBricks)
			%increment = %max / $Pref::Server::ND::MaxGhostBricks;
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
	%this.highlightBox.color = "0.2 0.2 1 0.99";
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

	//Update variables
	%this.ghostPosition = vectorAdd(%this.ghostPosition, %x SPC %y SPC %z);
	%this.updateHighlightBox();

	//Update ghost bricks
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 450);
}

//Rotate ghost bricks left/right
function ND_Selection::rotateGhostBricks(%this, %direction, %useSelectionCenter)
{
	cancel(%this.ghostMoveSchedule);
	%max = %this.ghostGroup.getCount();

	if(%max > $Pref::Server::ND::InstantGhostBricks)
	{
		%max = $Pref::Server::ND::InstantGhostBricks;

		//Start schedule to move remaining ghost bricks
		%this.ghostMoveSchedule = %this.schedule(450, updateGhostBricks, %max);
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

	//Update variables
	%this.ghostAngleID = (%this.ghostAngleID + %direction) % 4;
	%this.ghostPosition = vectorAdd(%pivot, %pOffset);
	%this.updateHighlightBox();

	//Update ghost bricks
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 450);
}

//Mirror ghost bricks on x,y,z axis
function ND_Selection::mirrorGhostBricks(%this, %axis)
{
	//Update variables
	if(%axis == 0)
	{
		%this.ghostMirrorX = !%this.ghostMirrorX;

		//Offset ghost so we end up in the same area
		if(%this.ghostMirrorX)
			%offset = (getWord(%this.rootToCenter, 0) * 2) @ " 0 0";
		else
			%offset = (getWord(%this.rootToCenter, 0) * -2) @ " 0 0";
	}
	else if(%axis == 1)
	{
		%this.ghostMirrorY = !%this.ghostMirrorY;

		//Offset ghost so we end up in the same area
		if(%this.ghostMirrorY)
			%offset = "0 " @ (getWord(%this.rootToCenter, 1) * 2) @ " 0";
		else
			%offset = "0 " @ (getWord(%this.rootToCenter, 1) * -2) @ " 0";
	}
	else
	{
		%this.ghostMirrorZ = !%this.ghostMirrorZ;

		//Offset ghost so we end up in the same area
		if(%this.ghostMirrorZ)
			%offset = "0 0 " @ getWord(%this.rootToCenter, 2) * 2;
		else
			%offset = "0 0 " @ getWord(%this.rootToCenter, 2) * -2;
	}

	//Double mirror is just a rotation
	if(%this.ghostMirrorX && %this.ghostMirrorY)
	{
		%this.ghostAngleID = (%this.ghostAngleID + 2) % 4;
		%this.ghostMirrorX = false;
		%this.ghostMirrorY = false;

		if(%axis == 0)
			%offset = (getWord(%this.rootToCenter, 0) * -2) @ " 0 0";
		else
			%offset = "0 " @ (getWord(%this.rootToCenter, 1) * -2) @ " 0";
	}

	//If pivot is whole selection, shift bricks to keep area
	if(%this.client.ndPivot)
		%this.ghostPosition = vectorAdd(%this.ghostPosition, ndRotateVector(%offset, %this.ghostAngleID));

	%this.updateHighlightBox();

	//Update ghost bricks
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 450);
}

//Update some of the ghost bricks to the latest position/rotation
function ND_Selection::updateGhostBricks(%this, %start, %count, %wait)
{
	cancel(%this.ghostMoveSchedule);
	%max = %this.ghostGroup.getCount();

	if(%max - %start > %count)
	{
		%max = %start + %count;

		//Start schedule to move remaining ghost bricks
		%this.ghostMoveSchedule = %this.schedule(%wait, updateGhostBricks,
			%max, $Pref::Server::ND::ProcessPerTick, 30);
	}

	%pos = %this.ghostPosition;
	%angle = %this.ghostAngleID;
	%ghostGroup = %this.ghostGroup;
	%mirrX = %this.ghostMirrorX;
	%mirrY = %this.ghostMirrorY;
	%mirrZ = %this.ghostMirrorZ;

	//Update the ghost bricks in this tick
	for(%i = %start; %i < %max; %i++)
	{
		%brick = %ghostGroup.getObject(%i);
		%j = %brick.selectionIndex;

		//Offset position
		%bPos = $NS[%this, "Pos", %j];

		//Rotated local angle id
		%bAngle = $NS[%this, "Rot", %j];

		//Apply mirror effects (ugh)
		%datablock = $NS[%this, "Data", %j];

		if(%mirrX)
		{
			//Mirror offset
			%bPos = -firstWord(%bPos) SPC restWords(%bPos);

			//Handle symmetries
			switch($ND::Symmetry[%datablock])
			{
				//Asymmetric
				case 0:
					if(%db = $ND::SymmetryXDatablock[%datablock])
					{
						%datablock = %db;
						%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

						//Pair is made on X, so apply mirror logic for X afterwards
						if(%bAngle % 2 == 1)
							%bAngle = (%bAngle + 2) % 4;
					}

				//Do nothing for fully symmetric

				//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
				case 2:
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 2) % 4;

				//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
				case 3:
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 2) % 4;

				//X+Y symmetric - rotate 90 degrees
				case 4:
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 1) % 4;
					else						
						%bAngle = (%bAngle + 3) % 4;

				//X-Y symmetric - rotate -90 degrees
				case 5:
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 3) % 4;
					else						
						%bAngle = (%bAngle + 1) % 4;
			}
		}
		else if(%mirrY)
		{
			//Mirror offset
			%bPos = getWord(%bPos, 0) SPC -getWord(%bPos, 1) SPC getWord(%bPos, 2);

			//Handle symmetries
			switch($ND::Symmetry[%datablock])
			{
				//Asymmetric
				case 0:
					if(%db = $ND::SymmetryXDatablock[%datablock])
					{
						%datablock = %db;
						%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

						//Pair is made on X, so apply mirror logic for X afterwards
						if(%bAngle % 2 == 0)
							%bAngle = (%bAngle + 2) % 4;
					}

				//Do nothing for fully symmetric

				//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
				case 2:
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 2) % 4;

				//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
				case 3:
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 2) % 4;

				//X+Y symmetric - rotate 90 degrees
				case 4:
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 1) % 4;
					else						
						%bAngle = (%bAngle + 3) % 4;

				//X-Y symmetric - rotate -90 degrees
				case 5:
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 3) % 4;
					else						
						%bAngle = (%bAngle + 1) % 4;
			}
		}

		if(%mirrZ)
		{
			//Mirror offset
			%bPos = getWords(%bPos, 0, 1) SPC -getWord(%bPos, 2);

			//Change datablock if asymmetric
			if(!$ND::SymmetryZ[%datablock])
			{
				if(%db = $ND::SymmetryZDatablock[%datablock])
				{
					%datablock = %db;
					%bAngle = (%bAngle + $ND::SymmetryZOffset[%datablock]) % 4;
				}
			}
		}

		//Apply datablock
		if(%brick.getDatablock() != %datablock)
			%brick.setDatablock(%datablock);

		//Rotate and add offset		
		%bAngle = (%bAngle + %angle) % 4;
		%bPos = vectorAdd(%pos, ndRotateVector(%bPos, %angle));

		switch(%bAngle)
		{
			case 0: %bRot = "1 0 0 0";
			case 1: %bRot = "0 0 1 1.5708";
			case 2: %bRot = "0 0 1 3.14150";
			case 3: %bRot = "0 0 -1 1.5708";
		}

		//Apply transform
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

	%min = %this.minSize;
	%max = %this.maxSize;

	//Handle mirrors
	if(%this.ghostMirrorX)
	{
		%min = -firstWord(%min) SPC restWords(%min);
		%max = -firstWord(%max) SPC restWords(%max);
	}
	else if(%this.ghostMirrorY)
	{
		%min = getWord(%min, 0) SPC -getWord(%min, 1) SPC getWord(%min, 2);
		%max = getWord(%max, 0) SPC -getWord(%max, 1) SPC getWord(%max, 2);
	}

	if(%this.ghostMirrorZ)
	{
		%min = getWords(%min, 0, 1) SPC -getWord(%min, 2);
		%max = getWords(%max, 0, 1) SPC -getWord(%max, 2);
	}

	//Handle rotation
	%min = ndRotateVector(%min, %this.ghostAngleID);
	%max = ndRotateVector(%max, %this.ghostAngleID);

	//Get max values
	%minX = getMin(getWord(%min, 0), getWord(%max, 0));
	%minY = getMin(getWord(%min, 1), getWord(%max, 1));
	%minZ = getMin(getWord(%min, 2), getWord(%max, 2));

	%maxX = getMax(getWord(%min, 0), getWord(%max, 0));
	%maxY = getMax(getWord(%min, 1), getWord(%max, 1));
	%maxZ = getMax(getWord(%min, 2), getWord(%max, 2));

	%pos = %this.ghostPosition;
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

	//Reset mirror error list
	%client = %this.client;

	%countX = $NS[%client, "MirErrorsX"];
	%countZ = $NS[%client, "MirErrorsZ"];

	for(%i = 0; %i < %countX; %i++)
		$NS[%client, "MirKnownX", $NS[%client, "MirErrorX", %i]] = "";

	for(%i = 0; %i < %countZ; %i++)
		$NS[%client, "MirKnownZ", $NS[%client, "MirErrorZ", %i]] = "";

	$NS[%client, "MirErrorsZ"] = 0;
	$NS[%client, "MirErrorsX"] = 0;

	//Make list of spawned clients to scope bricks
	%this.numClients = 0;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.hasSpawnedOnce
		&& isObject(%obj = %cl.getControlObject())
		&& vectorDist(%this.ghostPosition, %obj.getTransform()) < 10000)
		{
			$NS[%this, "CL", %this.numClients] = %cl;
			%this.numClients++;
		}
	}

	if($Pref::Server::ND::PlayMenuSounds && %this.brickCount > $Pref::Server::ND::ProcessPerTick * 10)
		messageClient(%this.client, 'MsgUploadStart', "");

	%this.tickPlantSearch($Pref::Server::ND::ProcessPerTick, %position, %angleID);
}

//Go through the list of bricks until we find one that plants successfully
function ND_Selection::tickPlantSearch(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantSearchIndex;
	%end = %start + %remainingPlants;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%client = %this.client;
	%group = %client.brickGroup.getId();
	%bl_id = %client.bl_id;

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;

	for(%i = %start; %i < %end; %i++)
	{
		//Brick already placed
		if($NP[%this, %i])
			continue;

		//Attempt to place brick
		%brick = ND_Selection::plantBrick(%this, %i, %position, %angleID, %group, %client, %bl_id);
		%plants++;

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %i] = true;

			%conns = $NS[%this, "Conns", %i];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "Conn", %i, %j];

				if(!$NP[%this, %id])
				{
					%found = true;

					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			//Instantly ghost the brick to all spawned clients (wow hacks)
			for(%j = 0; %j < %numClients; %j++)
			{
				%cl = $NS[%this, "CL", %j];
				%brick.scopeToClient(%cl);
				%brick.clearScopeToClient(%cl);
			}

			//If we added bricks to plant queue, switch to second loop
			if(%found)
			{
				%this.plantSearchIndex = %i + 1;
				%this.plantQueueCount = %qCount;
				%this.tickPlantTree(%remainingPlants - %plants, %position, %angleID);
				return;
			}

			%lastPos = %brick.position;
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

	if(strLen(%lastPos))
		serverPlay3D(BrickPlantSound, %lastPos);

	//Tell the client how far we got
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%end < %this.brickCount && %this.plantSuccessCount < %this.brickCount)
		%this.plantSchedule = %this.schedule(30, tickPlantSearch, $Pref::Server::ND::ProcessPerTick, %position, %angleID);
	else
		%this.finishPlant();
}

//Plant search has prepared a queue, plant all bricks in this queue and add their connected bricks aswell
function ND_Selection::tickPlantTree(%this, %remainingPlants, %position, %angleID)
{
	%start = %this.plantQueueIndex;
	%end = %start + %remainingPlants;

	%client = %this.client;
	%group = %client.brickGroup.getId();
	%bl_id = %client.bl_id;

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;

	for(%i = %start; %i < %end; %i++)
	{
		//The queue is empty! Switch back to plant search.
		if(%i >= %qCount)
		{
			if(strLen(%lastPos))
				serverPlay3D(BrickPlantSound, %lastPos);

			%this.plantQueueCount = %qCount;
			%this.plantQueueIndex = %i;
			%this.tickPlantSearch(%end - %i, %position, %angleID);
			return;
		}

		//Attempt to plant queued brick
		%bId = $NS[%this, "PQueue", %i];

		%brick = ND_Selection::plantBrick(%this, %bId, %position, %angleID, %group, %client, %bl_id);

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %bId] = true;

			%conns = $NS[%this, "Conns", %bId];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "Conn", %bId, %j];

				if(!$NP[%this, %id])
				{
					$NS[%this, "PQueue", %qCount] = %id;
					$NP[%this, %id] = true;
					%qCount++;
				}
			}

			%lastPos = %brick.position;

			//Instantly ghost the brick to all spawned clients (wow hacks)
			for(%j = 0; %j < %numClients; %j++)
			{
				%cl = $NS[%this, "CL", %j];
				%brick.scopeToClient(%cl);
				%brick.clearScopeToClient(%cl);
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

	if(strLen(%lastPos))
		serverPlay3D(BrickPlantSound, %lastPos);

	//Tell the client how far we got
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	%this.plantQueueCount = %qCount;
	%this.plantQueueIndex = %i;

	%this.plantSchedule = %this.schedule(30, tickPlantTree, $Pref::Server::ND::ProcessPerTick, %position, %angleID);
}

//Attempt to plant brick with id %i
//Returns brick if planted, 0 if floating, -1 if blocked, -2 if trust failure
function ND_Selection::plantBrick(%this, %i, %position, %angleID, %brickGroup, %client, %bl_id)
{
	//Offset position
	%bPos = $NS[%this, "Pos", %i];

	//Local angle id
	%bAngle = $NS[%this, "Rot", %i];

	//Apply mirror effects (ugh)
	%datablock = $NS[%this, "Data", %i];

	%mirrX = %this.ghostMirrorX;
	%mirrY = %this.ghostMirrorY;
	%mirrZ = %this.ghostMirrorZ;

	if(%mirrX)
	{
		//Mirror offset
		%bPos = -firstWord(%bPos) SPC restWords(%bPos);

		//Handle symmetries
		switch($ND::Symmetry[%datablock])
		{
			//Asymmetric
			case 0:
				if(%db = $ND::SymmetryXDatablock[%datablock])
				{
					%datablock = %db;
					%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

					//Pair is made on X, so apply mirror logic for X afterwards
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 2) % 4;
				}
				else
				{
					//Add datablock to list of mirror problems
					if(!$NS[%client, "MirKnownX", %datablock])
					{
						%id = $NS[%client, "MirErrorsX"];
						$NS[%client, "MirErrorsX"]++;

						$NS[%client, "MirErrorX", %id]= %datablock;
						$NS[%client, "MirKnownX", %datablock] = true;
					}
				}

			//Do nothing for fully symmetric

			//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
			case 2:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 2) % 4;

			//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
			case 3:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 2) % 4;

			//X+Y symmetric - rotate 90 degrees
			case 4:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 1) % 4;
				else						
					%bAngle = (%bAngle + 3) % 4;

			//X-Y symmetric - rotate -90 degrees
			case 5:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 3) % 4;
				else						
					%bAngle = (%bAngle + 1) % 4;
		}
	}
	else if(%mirrY)
	{
		//Mirror offset
		%bPos = getWord(%bPos, 0) SPC -getWord(%bPos, 1) SPC getWord(%bPos, 2);

		//Handle symmetries
		switch($ND::Symmetry[%datablock])
		{
			//Asymmetric
			case 0:
				if(%db = $ND::SymmetryXDatablock[%datablock])
				{
					%datablock = %db;
					%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

					//Pair is made on X, so apply mirror logic for X afterwards
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 2) % 4;
				}
				else
				{
					//Add datablock to list of mirror problems
					if(!$NS[%client, "MirKnownX", %datablock])
					{
						%id = $NS[%client, "MirErrorsX"];
						$NS[%client, "MirErrorsX"]++;

						$NS[%client, "MirErrorX", %id]= %datablock;
						$NS[%client, "MirKnownX", %datablock] = true;
					}
				}

			//Do nothing for fully symmetric

			//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
			case 2:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 2) % 4;

			//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
			case 3:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 2) % 4;

			//X+Y symmetric - rotate 90 degrees
			case 4:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 1) % 4;
				else						
					%bAngle = (%bAngle + 3) % 4;

			//X-Y symmetric - rotate -90 degrees
			case 5:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 3) % 4;
				else						
					%bAngle = (%bAngle + 1) % 4;
		}
	}

	if(%mirrZ)
	{
		//Mirror offset
		%bPos = getWords(%bPos, 0, 1) SPC -getWord(%bPos, 2);

		//Change datablock if asymmetric
		if(!$ND::SymmetryZ[%datablock])
		{
			if(%db = $ND::SymmetryZDatablock[%datablock])
			{
				%datablock = %db;
				%bAngle = (%bAngle + $ND::SymmetryZOffset[%datablock]) % 4;
			}
			else
			{
				//Add datablock to list of mirror problems
				if(!$NS[%client, "MirKnownZ", %datablock])
				{
					%id = $NS[%client, "MirErrorsZ"];
					$NS[%client, "MirErrorsZ"]++;

					$NS[%client, "MirErrorZ", %id]= %datablock;
					$NS[%client, "MirKnownZ", %datablock] = true;
				}
			}
		}
	}

	//Rotate and add offset
	%bAngle = (%bAngle + %angleID) % 4;
	%bPos = vectorAdd(%position, ndRotateVector(%bPos, %angleID));

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
		datablock = %datablock;
		client = %client;

		position = %bPos;
		rotation = %bRot;
		angleID = %bAngle;

		colorID = $NS[%this, "Color", %i];
		colorFxID = $NS[%this, "ColorFx", %i];

		printID = $NS[%this, "Print", %i];
	};

	//Add to brickgroup
	%brickGroup.add(%brick);

	//Attempt plant
	if(%error = %brick.plant())
	{
		%brick.delete();

		if(%error == 2)
			return 0;
		
		return -1;
	}

	//Check for trust
	%downCount = %brick.getNumDownBricks();

	for(%j = 0; %j < %downCount; %j++)
	{
		%group = %brick.getDownBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] > 0)
			continue;

		if(%group.bl_id == 888888)
			continue;

		%brick.delete();
		return -2;
	}

	%upCount = %brick.getNumUpBricks();

	for(%j = 0; %j < %upCount; %j++)
	{
		%group = %brick.getUpBrick(%j).getGroup();

		if(%group == %brickGroup)
			continue;

		if(%group.Trust[%bl_id] > 0)
			continue;

		if(%group.bl_id == 888888)
			continue;

		%brick.delete();
		return -2;
	}

	//Finished trust check
	if(%downCount)
		%brick.stackBL_ID = %brick.getDownBrick(0).stackBL_ID;
	else if(%upCount)
		%brick.stackBL_ID = %brick.getUpBrick(0).stackBL_ID;
	else
		%brick.stackBL_ID = %bl_id;

	%brick.setTrusted(true);
	%datablock.onTrustCheckFinished(%brick);

	//Workaround - water cubes change this in onPlant
	//so we have to set it manually after planting.
	%brick.setShapeFx($NS[%this, "ShapeFx", %i]);

	//Apply events
	if(%numEvents = $NS[%this, "EvNum", %i])
	{
		%brick.numEvents = %numEvents;
		%brick.implicitCancelEvents = 0;

		for(%j = 0; %j < %numEvents; %j++)
		{
			%brick.eventEnabled[%j] = $NS[%this, "EvEnable", %i, %j];
			%brick.eventDelay[%j] = $NS[%this, "EvDelay", %i, %j];
			%brick.eventAppendClient[%j] = $NS[%this, "EvClient", %i, %j];

			%inputIdx = $NS[%this, "EvInputIdx", %i, %j];

			%brick.eventInput[%j] = $NS[%this, "EvInput", %i, %j];
			%brick.eventInputIdx[%j] = %inputIdx;

			%output = $NS[%this, "EvOutput", %i, %j];
			%outputIdx = $NS[%this, "EvOutputIdx", %i, %j];

			//Rotate fireRelay events
			switch$(%output)
			{
				case "fireRelayUp":    %dir = 0;
				case "fireRelayDown":  %dir = 1;
				case "fireRelayNorth": %dir = 2;
				case "fireRelayEast":  %dir = 3;
				case "fireRelaySouth": %dir = 4;
				case "fireRelayWest":  %dir = 5;
				default: %dir = -1;
			}

			if(%dir >= 0)
			{
				%rotated = %dir;

				//Apply mirror effects
				if(%rotated > 1)
				{
					if(%mirrX && %rotated % 2 == 1
					|| %mirrY && %rotated % 2 == 0)
						%rotated += 2;

					%rotated = (%rotated + %angleID - 2) % 4 + 2;
				}
				else if(%mirrZ)
					%rotated = !%rotated;

				%outputIdx += %rotated - %dir;

				switch(%rotated)
				{
					case 0: %output = "fireRelayUp";
					case 1: %output = "fireRelayDown";
					case 2: %output = "fireRelayNorth";
					case 3: %output = "fireRelayEast";
					case 4: %output = "fireRelaySouth";
					case 5: %output = "fireRelayWest";
				}
			}

			%brick.eventOutput[%j] = %output;
			%brick.eventOutputIdx[%j] = %outputIdx;

			%target = $NS[%this, "EvTarget", %i, %j];
			%targetIdx = $NS[%this, "EvTargetIdx", %i, %j];

			if(%targetIdx == -1)
				%brick.eventNT[%j] = $NS[%this, "EvNT", %i, %j];
			
			%brick.eventTarget[%j] = %target;
			%brick.eventTargetIdx[%j] = %targetIdx;

			//Why does this need to be so complicated?
			if(%targetIdx >= 0)
				%targetClass = getWord($InputEvent_TargetListfxDtsBrick_[%inputIdx], %targetIdx * 2 + 1);
			else
				%targetClass = "FxDTSBrick";
				
			%paramList = $OutputEvent_ParameterList[%targetClass, %outputIdx];
			%paramCount = getFieldCount(%paramList);

			for(%k = 0; %k < %paramCount; %k++)
			{
				%param = $NS[%this, "EvPar", %i, %j, %k];

				if(getWord(getField(%paramList, %k), 0) $= "vector")
				{
					//Apply mirror effects
					if(%mirrX)
						%param = -firstWord(%param) SPC restWords(%param);
					else if(%mirrY)
						%param = getWord(%param, 0) SPC -getWord(%param, 1) SPC getWord(%param, 2);

					if(%mirrZ)
						%param = getWord(%param, 0) SPC getWord(%param, 1) SPC -getWord(%param, 2);

					%param = ndRotateVector(%param, %angleID);
				}

				%brick.eventOutputParameter[%j, %k + 1] = %param;
			}
		}
	}

	//Hole bots... why don't you use the correct function?
	//Add-ons are supposed to package _ON_TrustCheckFinished...
	if(%datablock.isBotHole)
	{
		%brick.isBotHole = true;
		%brick.onHoleSpawnPlanted();
	}

	//Apply wrench settings
	%brick.setRendering(!$NS[%this, "NoRender", %i]);
	%brick.setRaycasting(!$NS[%this, "NoRay", %i]);
	%brick.setColliding(!$NS[%this, "NoCol", %i]);

	if((%tmp = $NS[%this, "Name", %i]) !$= "")
		%brick.setNTObjectName(%tmp);

	if(%tmp = $NS[%this, "Light", %i])
		%brick.setLight(%tmp);

	if(%tmp = $NS[%this, "Emitter", %i])
	{
		%dir = $NS[%this, "EmitDir", %i];

		//Apply mirror effects
		if(%dir > 1)
		{
			if(%mirrX && %dir % 2 == 1
			|| %mirrY && %dir % 2 == 0)
				%dir += 2;

			%dir = (%dir + %angleID - 2) % 4 + 2;
		}
		else if(%mirrZ)
			%dir = !%dir;

		%brick.emitterDirection = %dir;
		%brick.setEmitter(%tmp);
	}

	if(%tmp = $NS[%this, "Item", %i])
	{
		%pos = $NS[%this, "ItemPos", %i];
		%dir = $NS[%this, "ItemDir", %i];

		//Apply mirror effects
		if(%pos > 1)
		{
			if(%mirrX && %pos % 2 == 1
			|| %mirrY && %pos % 2 == 0)
				%pos += 2;

			%pos = (%pos + %angleID - 2) % 4 + 2;
		}
		else if(%mirrZ)
			%pos = !%pos;

		if(%dir > 1)
		{
			if(%mirrX && %dir % 2 == 1
			|| %mirrY && %dir % 2 == 0)
				%dir += 2;

			%dir = (%dir + %angleID - 2) % 4 + 2;
		}
		else if(%mirrZ)
			%dir = !%dir;

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

	return %brick;
}

//Finished planting all the bricks!
function ND_Selection::finishPlant(%this)
{
	//Report mirror errors
	if($NS[%this.client, "MirErrorsX"] > 0 || $NS[%this.client, "MirErrorsZ"] > 0)
		messageClient(%this.client, '', "\c6Some bricks were probably mirrored incorrectly. Say \c3/mirErrors\c6 to find out more.");

	%count = %this.brickCount;
	%planted = %this.plantSuccessCount;
	%blocked = %this.plantBlockedFailCount;
	%trusted = %this.plantTrustFailCount;
	%floating = %count - %planted - %blocked - %trusted;

	%s = %this.plantSuccessCount == 1 ? "" : "s";
	%message = "<font:Verdana:20>\c6Planted \c3" @ %this.plantSuccessCount @ "\c6 / \c3" @ %count @ "\c6 Brick" @ %s @ "!";

	if(%trusted)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %trusted @ "\c6 missing trust.";

	if(%blocked)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %blocked @ "\c6 blocked.";

	if(%floating)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %floating @ "\c6 floating.";

	commandToClient(%this.client, 'centerPrint', %message, 4);

	if($Pref::Server::ND::PlayMenuSounds && %planted && %this.brickCount > $Pref::Server::ND::ProcessPerTick * 10)
		messageClient(%this.client, 'MsgProcessComplete', "");

	deleteVariables("$NP" @ %this @ "_*");

	if(%planted)
		%this.client.undoStack.push(%this.undoGroup TAB "DUPLICATE");
	else
		%this.undoGroup.delete();

	%this.client.ndSetMode(NDM_PlantCopy);
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
