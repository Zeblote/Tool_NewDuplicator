// This file is way too big. Fix later...
// -------------------------------------------------------------------

//Selection data arrays $NS[obj, type{, ...}]
// $NS[%s, "B", %i    ] - Brick object
// $NS[%s, "I", %b    ] - Index of brick in array
// $NS[%s, "N", %i    ] - Number of connected bricks
// $NS[%s, "C", %i, %j] - Index of connected brick

// $NS[%s, "D", %i] - Datablock
// $NS[%s, "P", %i] - Position
// $NS[%s, "R", %i] - Rotation

// $NS[%s, "NT", %i] - Brick name
// $NS[%s, "HN", %n] - Name exists in selection
// $NS[%s, "PR", %i] - Print

// $NS[%s, "CO", %i] - Color id
// $NS[%s, "CF", %i] - Color Fx id
// $NS[%s, "SF", %i] - Shape Fx id

// $NS[%s, "NRC", %i] - No ray casting
// $NS[%s, "NR",  %i] - No rendering
// $NS[%s, "NC",  %i] - No colliding

// $NS[%s, "LD", %i] - Light datablock

// $NS[%s, "ED", %i] - Emitter datablock
// $NS[%s, "ER", %i] - Emitter rotation

// $NS[%s, "ID", %i] - Item datablock
// $NS[%s, "IP", %i] - Item position
// $NS[%s, "IR", %i] - Item rotation
// $NS[%s, "IT", %i] - Item respawn time

// $NS[%s, "VD", %i] - Vehicle datablock
// $NS[%s, "VC", %i] - Vehicle color

// $NS[%s, "MD", %i] - Music datablock


// $NS[%s, "EN", %i] - Number of events on the brick

// $NS[%s, "EE", %i, %j] - Whether event is enabled
// $NS[%s, "ED", %i, %j] - Event delay

// $NS[%s, "EI",  %i, %j] - Event input name
// $NS[%s, "EII", %i, %j] - Event input idx
// $NS[%s, "EO",  %i, %j] - Event output name
// $NS[%s, "EOI", %i, %j] - Event output idx
// $NS[%s, "EOC", %i, %j] - Event output append client

// $NS[%s, "ET",  %i, %j] - Event target name
// $NS[%s, "ETI", %i, %j] - Event target idx
// $NS[%s, "ENT", %i, %j] - Event brick named target

// $NS[%s, "EP", %i, %j, %k] - Event output parameter


//Mirror error lists $NS[client, type{, ...}]
// $NS[%c, "MXC",   ] - Count of mirror errors on x
// $NS[%c, "MXE", %i] - Error datablock
// $NS[%c, "MXK", %d] - Index of datablock in list

// $NS[%c, "MZC",   ] - Count of mirror errors on z
// $NS[%c, "MZE", %i] - Error datablock
// $NS[%c, "MZK", %d] - Index of datablock in list



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
	if(%this.queueCount >= 1 || %this.brickCount >= 1)
	{
		//Variables follow the pattern $NS[object]_[type]_[...], allowing a single iteration to remove all
		deleteVariables("$NS" @ %this @ "_*");
	}

	%this.rootPosition = "0 0 0";
	%this.queueCount = 0;
	%this.brickCount = 0;

	%this.targetGroup = "";
	%this.targetBlid = "";

	%this.deHighlight();
	%this.deleteHighlightBox();
	%this.deleteGhostBricks();

	if(isObject(%this.saveFile))
		%this.saveFile.delete();
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
	//Clear previous selection
	%this.deleteData();

	//Create new highlight group
	%highlightGroup = ndNewHighlightGroup();

	%this.brickLimitReached = false;

	if(%this.client.isAdmin)
		%brickLimit = $Pref::Server::ND::MaxBricksAdmin;
	else
		%brickLimit = $Pref::Server::ND::MaxBricksPlayer;

	//Root position is position of the first selected brick
	%this.rootPosition = %brick.getPosition();

	//Process first brick
	%queueCount = 1;
	%brickCount = 1;

	$NS[%this, "B", 0] = %brick;
	$NS[%this, "I", %brick] = 0;

	%this.recordBrickData(0);
	ndHighlightBrick(%highlightGroup, %brick);

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	//Add bricks connected to the first brick to queue (do not register connections yet)
	if(%direction == 1)
	{
		//Set lower height limit
		%heightLimit = %this.minZ - 0.01;
		%upCount = %brick.getNumUpBricks();

		for(%i = 0; %i < %upCount; %i++)
		{
			%nextBrick = %brick.getUpBrick(%i);

			//If the brick is not in the list yet, add it to the queue
			if($NS[%this, "I", %nextBrick] $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "B", %queueCount] = %nextBrick;
				$NS[%this, "I", %nextBrick] = %queueCount;
				%queueCount++;
			}
		}
	}
	else
	{
		//Set upper height limit
		%heightLimit = %this.maxZ + 0.01;
		%downCount = %brick.getNumDownBricks();

		for(%i = 0; %i < %downCount; %i++)
		{
			%nextBrick = %brick.getDownBrick(%i);

			//If the brick is not in the list yet, add it to the queue
			if($NS[%this, "I", %nextBrick] $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "B", %queueCount] = %nextBrick;
				$NS[%this, "I", %nextBrick] = %queueCount;
				%queueCount++;
			}
		}
	}

	//Save number of connections
	%this.maxConnections = 0;
	%this.connectionCount = 0;

	%this.trustFailCount = %trustFailCount;
	%this.highlightGroup = %highlightGroup;
	%this.queueCount = %queueCount;
	%this.brickCount = %brickCount;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//First selection tick
	%this.selectionStart = 1;

	if(%queueCount > %brickCount)
		%this.tickStackSelection(%direction, %limited, %heightLimit, %brickLimit);
	else
		%this.finishStackSelection();
}

//Begin stack selection (multiselect)
function ND_Selection::startStackSelectionAdditive(%this, %brick, %direction, %limited)
{
	//If we have no bricks, start normal stack selection
	if(%this.brickCount < 1)
	{
		%this.startStackSelection(%brick, %direction, %limited);
		return;
	}

	//If we already reched the limit, don't even try
	if(%this.brickLimitReached)
	{
		%this.finishStackSelection();
		return;
	}

	%highlightGroup = %this.highlightGroup;

	if(%this.client.isAdmin)
		%brickLimit = $Pref::Server::ND::MaxBricksAdmin;
	else
		%brickLimit = $Pref::Server::ND::MaxBricksPlayer;

	%queueCount = %this.queueCount;
	%brickCount = %this.brickCount;

	//If the brick is not part of the selection yet, process it
	if($NS[%this, "I", %brick] $= "")
	{
		$NS[%this, "B", %queueCount] = %brick;
		$NS[%this, "I", %brick] = %queueCount;

		%this.recordBrickData(%queueCount);
		ndHighlightBrick(%highlightGroup, %brick);

		%brickIsNew = true;
		%brickIndex = %queueCount;
		%conns = 0;

		%queueCount++;
		%brickCount++;
	}

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	//Add bricks connected to the first brick to queue (do not register connections yet)
	if(%direction == 1)
	{
		//Set lower height limit
		%heightLimit = getWord(%brick.getWorldBox(), 2) - 0.01;
	}
	else
	{
		//Set upper height limit
		%heightLimit = getWord(%brick.getWorldBox(), 5) + 0.01;
	}

	//Process all up bricks
	%upCount = %brick.getNumUpBricks();

	for(%i = 0; %i < %upCount; %i++)
	{
		%nextBrick = %brick.getUpBrick(%i);

		//If the brick is not in the list yet, add it to the queue
		%nId = $NS[%this, "I", %nextBrick];

		if(%nId $= "")
		{
			//Don't add up bricks if we're searching down
			if(%direction != 1)
				continue;

			if(%queueCount >= %brickLimit)
				continue;

			//Check trust
			if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
			{
				%trustFailCount++;
				continue;
			}

			$NS[%this, "B", %queueCount] = %nextBrick;
			$NS[%this, "I", %nextBrick] = %queueCount;
			%queueCount++;
		}
		else if(%brickIsNew)
		{
			//If this brick already exists, we have to add the connection now
			//(Start brick won't be processed again unlike the others)
			$NS[%this, "C", %brickIndex, %conns] = %nId;
			%conns++;

			%ci = $NS[%this, "N", %nId]++;
			$NS[%this, "C", %nId, %ci - 1] = %brickIndex;

			if(%ci > %this.maxConnections)
				%this.maxConnections = %ci;

			%this.connectionCount++;
		}
	}

	//Process all down bricks
	%downCount = %brick.getNumDownBricks();

	for(%i = 0; %i < %downCount; %i++)
	{
		%nextBrick = %brick.getDownBrick(%i);

		//If the brick is not in the list yet, add it to the queue
		%nId = $NS[%this, "I", %nextBrick];

		if(%nId $= "")
		{
			//Don't add down bricks if we're searching up
			if(%direction == 1)
				continue;

			if(%queueCount >= %brickLimit)
				continue;

			//Check trust
			if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
			{
				%trustFailCount++;
				continue;
			}

			$NS[%this, "B", %queueCount] = %nextBrick;
			$NS[%this, "I", %nextBrick] = %queueCount;
			%queueCount++;
		}
		else if(%brickIsNew)
		{
			//If this brick already exists, we have to add the connection now
			//(Start brick won't be processed again unlike the others)
			$NS[%this, "C", %brickIndex, %conns] = %nId;
			%conns++;

			%ci = $NS[%this, "N", %nId]++;
			$NS[%this, "C", %nId, %ci - 1] = %brickIndex;

			if(%ci > %this.maxConnections)
				%this.maxConnections = %ci;

			%this.connectionCount++;
		}
	}

	$NS[%this, "N", %brickIndex] = %conns;

	//Inc number of connections
	%this.connectionCount += %conns;

	if(%conns > %this.maxConnections)
		%this.maxConnections = %conns;

	%this.trustFailCount += %trustFailCount;
	%this.queueCount = %queueCount;
	%this.brickCount = %brickCount;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//First selection tick
	%this.selectionStart = %queueCount;

	if(%queueCount > %brickCount)
		%this.tickStackSelection(%direction, %limited, %heightLimit, %brickLimit);
	else
		%this.finishStackSelection();
}

//Tick stack selection
function ND_Selection::tickStackSelection(%this, %direction, %limited, %heightLimit, %brickLimit)
{
	cancel(%this.stackSelectSchedule);

	%highlightGroup = %this.highlightGroup;
	%selectionStart = %this.selectionStart;
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
			messageClient(%this.client, 'MsgError', "\c0Error: \c6Queued brick does not exist anymore. Do not modify the build during selection!");

			%this.cancelStackSelection();
			%this.client.ndSetMode(NDM_StackSelect);
			return;
		}

		ndHighlightBrick(%highlightGroup, %brick);

		//Queue all up bricks
		%upCount = %brick.getNumUpBricks();
		%conns = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%nextBrick = %brick.getUpBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 0 && getWord(%nextBrick.getWorldBox(), 5) > %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to get an id
			%nId = $NS[%this, "I", %nextBrick];

			if(%nId $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "B", %queueCount] = %nextBrick;
				$NS[%this, "I", %nextBrick] = %queueCount;
				%nId = %queueCount;
				%queueCount++;
			}

			$NS[%this, "C", %i, %conns] = %nId;
			%conns++;

			//If this brick is from a previous stack selection,
			//we need to link the connection back as well
			if(%nId < %selectionStart)
			{
				%ci = $NS[%this, "N", %nId]++;
				$NS[%this, "C", %nId, %ci - 1] = %i;

				if(%ci > %this.maxConnections)
					%this.maxConnections = %ci;

				%this.connectionCount++;
			}
		}

		//Queue all down bricks
		%downCount = %brick.getNumDownBricks();

		for(%j = 0; %j < %downCount; %j++)
		{
			%nextBrick = %brick.getDownBrick(%j);

			//Skip bricks out of the limit
			if(%limited && %direction == 1 && getWord(%nextBrick.getWorldBox(), 2) < %heightLimit)
				continue;

			//If the brick is not in the selection yet, add it to the queue to get an id
			%nId = $NS[%this, "I", %nextBrick];

			if(%nId $= "")
			{
				if(%queueCount >= %brickLimit)
					continue;

				//Check trust
				if(!ndTrustCheckSelect(%nextBrick, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				$NS[%this, "B", %queueCount] = %nextBrick;
				$NS[%this, "I", %nextBrick] = %queueCount;
				%nId = %queueCount;
				%queueCount++;
			}

			$NS[%this, "C", %i, %conns] = %nId;
			%conns++;

			//If this brick is from a previous stack selection,
			//we need to link the connection back as well
			if(%nId < %selectionStart)
			{
				%ci = $NS[%this, "N", %nId]++;
				$NS[%this, "C", %nId, %ci - 1] = %i;

				if(%ci > %this.maxConnections)
					%this.maxConnections = %ci;

				%this.connectionCount++;
			}
		}

		$NS[%this, "N", %i] = %conns;

		//Inc number of connections
		%this.connectionCount += %conns;

		if(%conns > %this.maxConnections)
			%this.maxConnections = %conns;
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



//Box Selection
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin box selection
function ND_Selection::startBoxSelection(%this, %box, %limited)
{
	//Ensure there is no highlight group
	%this.deHighlight();

	//Save the chunk sizes
	%this.chunkX1 = getWord(%box, 0);
	%this.chunkY1 = getWord(%box, 1);
	%this.chunkZ1 = getWord(%box, 2);
	%this.chunkX2 = getWord(%box, 3);
	%this.chunkY2 = getWord(%box, 4);
	%this.chunkZ2 = getWord(%box, 5);

	%this.chunkSize = $Pref::Server::ND::BoxSelectChunkDim;

	%this.numChunksX = mCeil((%this.chunkX2 - %this.chunkX1) / %this.chunkSize);
	%this.numChunksY = mCeil((%this.chunkY2 - %this.chunkY1) / %this.chunkSize);
	%this.numChunksZ = mCeil((%this.chunkZ2 - %this.chunkZ1) / %this.chunkSize);
	%this.numChunks = %this.numChunksX * %this.numChunksY * %this.numChunksZ;

	%this.currChunkX = 0;
	%this.currChunkY = 0;
	%this.currChunkZ = 0;
	%this.currChunk = 0;

	%this.queueCount = 0;
	%this.brickCount = 0;

	%this.trustFailCount = 0;
	%this.brickLimitReached = false;

	%this.maxConnections = 0;
	%this.connectionCount = 0;

	if(%this.client.isAdmin)
		%brickLimit = $Pref::Server::ND::MaxBricksAdmin;
	else
		%brickLimit = $Pref::Server::ND::MaxBricksPlayer;

	//Process first tick
	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	%this.tickBoxSelectionChunk(%limited, %brickLimit);
}

//Queue all bricks in a chunk
function ND_Selection::tickBoxSelectionChunk(%this, %limited, %brickLimit)
{
	cancel(%this.boxSelectSchedule);

	//Restore chunk variables (scopes and slow object fields suck)
	%chunkSize = %this.chunkSize;
	%currChunk = %this.currChunk;

	%currChunkX = %this.currChunkX;
	%currChunkY = %this.currChunkY;
	%currChunkZ = %this.currChunkZ;

	%numChunksX = %this.numChunksX;
	%numChunksY = %this.numChunksY;
	%numChunksZ = %this.numChunksZ;

	%chunkX1 = %this.chunkX1;
	%chunkY1 = %this.chunkY1;
	%chunkZ1 = %this.chunkZ1;

	%chunkX2 = %this.chunkX2;
	%chunkY2 = %this.chunkY2;
	%chunkZ2 = %this.chunkZ2;

	//Where to insert bricks in the queue
	%queueIndex = %this.queueCount;

	//Variables for trust checks
	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	%chunksDone = 0;
	%bricksFound = 0;
	%trustFailCount = 0;

	//Process chunks until we reach the brick or chunk limit
	while(%chunksDone < 600 && %bricksFound < 1000)
	{
		%chunksDone++;

		//Calculate position and size of chunk
		%x1 = %chunkX1 + (%currChunkX * %chunkSize) + 0.05;
		%y1 = %chunkY1 + (%currChunkY * %chunkSize) + 0.05;
		%z1 = %chunkZ1 + (%currChunkZ * %chunkSize) + 0.05;

		%x2 = getMin(%chunkX2 - 0.05, %x1 + %chunkSize - 0.1);
		%y2 = getMin(%chunkY2 - 0.05, %y1 + %chunkSize - 0.1);
		%z2 = getMin(%chunkZ2 - 0.05, %z1 + %chunkSize - 0.1);

		%size = %x2 - %x1 SPC %y2 - %y1 SPC %z2 - %z1;
		%pos = vectorAdd(%x1 SPC %y1 SPC %z1, vectorScale(%size, 0.5));

		//Queue all new bricks found in this chunk
		initContainerBoxSearch(%pos, %size, $TypeMasks::FxBrickAlwaysObjectType);

		while(%obj = containerSearchNext())
		{
			%bricksFound++;

			if($NS[%this, "I", %obj] $= "")
			{
				if(%limited)
				{
					//Skip bricks that are outside the limit
					%box = %obj.getWorldBox();

					if(getWord(%box, 0) < %chunkX1 - 0.1)
						continue;

					if(getWord(%box, 1) < %chunkY1 - 0.1)
						continue;

					if(getWord(%box, 2) < %chunkZ1 - 0.1)
						continue;

					if(getWord(%box, 3) > %chunkX2 + 0.1)
						continue;

					if(getWord(%box, 4) > %chunkY2 + 0.1)
						continue;

					if(getWord(%box, 5) > %chunkZ2 + 0.1)
						continue;
				}

				//Check trust
				if(!ndTrustCheckSelect(%obj, %group, %bl_id, %admin))
				{
					%trustFailCount++;
					continue;
				}

				//Queue brick
				$NS[%this, "I", %obj] = %queueIndex;
				$NS[%this, "B", %queueIndex] = %obj;
				%queueIndex++;

				//Test brick limit
				if(%queueIndex >= %brickLimit)
				{
					%limitReached = true;
					break;
				}
			}
		}

		//Stop processing chunks if limit was reached
		if(%limitReached)
			break;

		//Set next chunk index or break
		%currChunk++;

		if(%currChunkX++ >= %numChunksX)
		{
			%currChunkX = 0;

			if(%currChunkY++ >= %numChunksY)
			{
				%currChunkY = 0;

				if(%currChunkZ++ >= %numChunksZ)
				{
					%searchComplete = true;
					break;
				}
			}
		}
	}

	//Save chunk variables (scopes and slow object fields suck)
	%this.currChunk = %currChunk;

	%this.currChunkX = %currChunkX;
	%this.currChunkY = %currChunkY;
	%this.currChunkZ = %currChunkZ;

	%this.numChunksX = %numChunksX;
	%this.numChunksY = %numChunksY;
	%this.numChunksZ = %numChunksZ;

	%this.trustFailCount += %trustFailCount;
	%this.queueCount = %queueIndex;

	//If the brick limit was reached, start processing
	if(%limitReached)
	{
		%this.brickLimitReached = true;
		%this.rootPosition = $NS[%this, "B", 0].getPosition();
		%this.boxSelectSchedule = %this.schedule(30, tickBoxSelectionProcess);

		return;
	}

	//If all chunks have been searched, start processing
	if(%searchComplete)
	{
		//Did we find any bricks at all?
		if(%queueIndex > 0)
		{
			//Create highlight group
			%this.highlightGroup = ndNewHighlightGroup();

			//Start processing bricks
			%this.rootPosition = $NS[%this, "B", 0].getPosition();
			%this.boxSelectSchedule = %this.schedule(30, tickBoxSelectionProcess);
		}
		else
		{
			messageClient(%this.client, 'MsgError', "");

			%m = "<font:Verdana:20>\c6No bricks were found inside the selection!";

			if(%this.trustFailCount > 0)
				%m = %m @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

			commandToClient(%this.client, 'centerPrint', %m, 5);

			%this.cancelBoxSelection();
			%this.client.ndSetMode(NDM_BoxSelect);
		}

		return;
	}

	//Tell the client which chunks we just processed
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Schedule next chunk
	%this.boxSelectSchedule = %this.schedule(30, tickBoxSelectionChunk, %limited, %brickLimit);
}

//Save connections between bricks and highlight them
function ND_Selection::tickBoxSelectionProcess(%this)
{
	cancel(%this.boxSelectSchedule);
	%highlightGroup = %this.highlightGroup;

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
			messageClient(%this.client, 'MsgError', "\c0Error: \c6Queued brick does not exist anymore. Do not modify the build during selection!");

			%this.cancelBoxSelection();
			%this.client.ndSetMode(NDM_BoxSelect);
			return;
		}

		ndHighlightBrick(%highlightGroup, %brick);

		//Save all up bricks
		%upCount = %brick.getNumUpBricks();
		%conns = 0;

		for(%j = 0; %j < %upCount; %j++)
		{
			%conn = %brick.getUpBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "I", %conn]) !$= "")
			{
				$NS[%this, "C", %i, %conns] = %nId;
				%conns++;
			}
		}

		//Save all down bricks
		%downCount = %brick.getNumDownBricks();

		for(%j = 0; %j < %downCount; %j++)
		{
			%conn = %brick.getDownBrick(%j);

			//If the brick is in the selection, save the connection
			if((%nId = $NS[%this, "I", %conn]) !$= "")
			{
				$NS[%this, "C", %i, %conns] = %nId;
				%conns++;
			}
		}

		$NS[%this, "N", %i] = %conns;

		//Inc number of connections
		%this.connectionCount += %conns;

		if(%conns > %this.maxConnections)
			%this.maxConnections = %conns;
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
		%this.finishBoxSelection();
	else
		%this.boxSelectSchedule = %this.schedule(30, tickBoxSelectionProcess);
}

//Finish box selection
function ND_Selection::finishBoxSelection(%this)
{
	%this.updateSize();
	%this.updateHighlightBox();

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadEnd', "");

	%s = %this.brickCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Selected \c3" @ %this.brickCount @ "\c6 Brick" @ %s @ "!";

	if(%this.brickLimitReached)
		%msg = %msg @ " (Limit Reached)";

	if(%this.trustFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

	%msg = %msg @ "\n<font:Verdana:17>\c6Press [Cancel Brick] to adjust the box.";
	commandToClient(%this.client, 'centerPrint', %msg, 8);

	%this.client.ndSetMode(NDM_BoxSelect);
}

//Cancel box selection
function ND_Selection::cancelBoxSelection(%this)
{
	cancel(%this.boxSelectSchedule);
	%this.deleteData();
}



//Recording Brick Data
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Record info about a queued brick
function ND_Selection::recordBrickData(%this, %i)
{
	//Return false if brick no longer exists
	if(!isObject(%brick = $NS[%this, "B", %i]))
		return false;

	///////////////////////////////////////////////////////////
	//Variables required for every brick

	//Datablock
	%datablock = %brick.getDatablock();
	$NS[%this, "D", %i] = %datablock;

	//Offset from base brick
	$NS[%this, "P", %i] = vectorSub(%brick.getPosition(), %this.rootPosition);

	//Rotation
	$NS[%this, "R", %i] = %brick.angleID;

	//Colors
	if($NDHN[%brick])
	{
		$NS[%this, "CO", %i] = %brick.colorID;
		$NS[%this, "CF", %i] = $NDHF[%brick];
	}
	else
	{
		$NS[%this, "CO", %i] = %brick.colorID;

		if(%brick.colorFxID)
			$NS[%this, "CF", %i] = %brick.colorFxID;
	}

	///////////////////////////////////////////////////////////
	//Optional variables only required for few bricks

	if(%tmp = %brick.shapeFxID)
		$NS[%this, "SF", %i] = %tmp;

	//Wrench settings
	if((%tmp = %brick.getName()) !$= "")
	{
		$NS[%this, "HN", %tmp] = true;
		$NS[%this, "NT", %i] = getSubStr(%tmp, 1, 254);
	}

	if(%tmp = %brick.light | 0)
		$NS[%this, "LD", %i] = %tmp.getDatablock();

	if(%tmp = %brick.emitter | 0)
	{
		$NS[%this, "ED", %i] = %tmp.getEmitterDatablock();
		$NS[%this, "ER", %i] = %brick.emitterDirection;
	}

	if(%tmp = %brick.item | 0)
	{
		$NS[%this, "ID", %i] = %tmp.getDatablock();
		$NS[%this, "IP", %i] = %brick.itemPosition;
		$NS[%this, "IR", %i] = %brick.itemDirection;
		$NS[%this, "IT", %i] = %brick.itemRespawnTime;
	}

	if(%tmp = %brick.vehicleDataBlock)
	{
		$NS[%this, "VD", %i] = %tmp;
		$NS[%this, "VC", %i] = %brick.reColorVehicle;
	}

	if(%tmp = %brick.AudioEmitter | 0)
		$NS[%this, "MD", %i] = %tmp.profile.getID();

	if(!%brick.isRaycasting())
		$NS[%this, "NRC", %i] = true;

	if(!%brick.isColliding())
		$NS[%this, "NC", %i] = true;

	if(!%brick.isRendering())
		$NS[%this, "NR", %i] = true;

	//Prints
	if(%datablock.hasPrint)
		$NS[%this, "PR", %i] = %brick.printID;

	//Events
	if(%numEvents = %brick.numEvents)
	{
		$NS[%this, "EN", %i] = %numEvents;

		for(%j = 0; %j < %numEvents; %j++)
		{
			$NS[%this, "EE", %i, %j] = %brick.eventEnabled[%j];
			$NS[%this, "ED", %i, %j] = %brick.eventDelay[%j];

			$NS[%this, "EI", %i, %j] = %brick.eventInput[%j];
			$NS[%this, "EII", %i, %j] = %brick.eventInputIdx[%j];

			$NS[%this, "EO", %i, %j] = %brick.eventOutput[%j];
			$NS[%this, "EOI", %i, %j] = %brick.eventOutputIdx[%j];
			$NS[%this, "EOC", %i, %j] = %brick.eventOutputAppendClient[%j];

			%target = %brick.eventTargetIdx[%j];

			if(%target == -1)
				$NS[%this, "ENT", %i, %j] = %brick.eventNT[%j];

			$NS[%this, "ET", %i, %j] = %brick.eventTarget[%j];
			$NS[%this, "ETI", %i, %j] = %target;

			$NS[%this, "EP", %i, %j, 0] = %brick.eventOutputParameter[%j, 1];
			$NS[%this, "EP", %i, %j, 1] = %brick.eventOutputParameter[%j, 2];
			$NS[%this, "EP", %i, %j, 2] = %brick.eventOutputParameter[%j, 3];
			$NS[%this, "EP", %i, %j, 3] = %brick.eventOutputParameter[%j, 4];
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
		if(%minX < %this.minX)
			%this.minX = %minX;

		if(%minY < %this.minY)
			%this.minY = %minY;

		if(%minZ < %this.minZ)
			%this.minZ = %minZ;

		if(%maxX > %this.maxX)
			%this.maxX = %maxX;

		if(%maxY > %this.maxY)
			%this.maxY = %maxY;

		if(%maxZ > %this.maxZ)
			%this.maxZ = %maxZ;
	}
	else
	{
		%this.minX = %minX;
		%this.minY = %minY;
		%this.minZ = %minZ;
		%this.maxX = %maxX;
		%this.maxY = %maxY;
		%this.maxZ = %maxZ;
	}

	return %brick;
}



//Highlighting bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Set the size variables after selecting bricks
function ND_Selection::updateSize(%this)
{
	%this.minSize = vectorSub(%this.minX SPC %this.minY SPC %this.minZ, %this.rootPosition);
	%this.maxSize = vectorSub(%this.maxX SPC %this.maxY SPC %this.maxZ, %this.rootPosition);

	%this.brickSizeX = mFloatLength((%this.maxX - %this.minX) * 2, 0);
	%this.brickSizeY = mFloatLength((%this.maxY - %this.minY) * 2, 0);
	%this.brickSizeZ = mFloatLength((%this.maxZ - %this.minZ) * 5, 0);

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
		%this.highlightBox.setSize(%min, %max);
	}
	else
		%this.highlightBox.setSize(%this.getGhostWorldBox());
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
	if(%this.highlightGroup)
	{
		ndStartDeHighlight(%this.highlightGroup);
		%this.highlightGroup = 0;
	}
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

	%admin = %this.client.isAdmin;
	%group = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	//Cut bricks
	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NS[%this, "B", %i];

		if(!isObject(%brick))
			continue;

		if(!ndTrustCheckModify(%brick, %group, %bl_id, %admin))
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

	//Tell the client how much we cut this tick
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



//Supercut
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin supercut
function ND_Selection::startSuperCut(%this, %box)
{
	//Ensure there is no highlight group
	%this.deHighlight();

	//Save the chunk sizes
	%this.chunkX1 = getWord(%box, 0);
	%this.chunkY1 = getWord(%box, 1);
	%this.chunkZ1 = getWord(%box, 2);
	%this.chunkX2 = getWord(%box, 3);
	%this.chunkY2 = getWord(%box, 4);
	%this.chunkZ2 = getWord(%box, 5);

	%this.chunkSize = $Pref::Server::ND::BoxSelectChunkDim;

	%this.numChunksX = mCeil((%this.chunkX2 - %this.chunkX1) / %this.chunkSize);
	%this.numChunksY = mCeil((%this.chunkY2 - %this.chunkY1) / %this.chunkSize);
	%this.numChunksZ = mCeil((%this.chunkZ2 - %this.chunkZ1) / %this.chunkSize);
	%this.numChunks = %this.numChunksX * %this.numChunksY * %this.numChunksZ;

	%this.currChunkX = 0;
	%this.currChunkY = 0;
	%this.currChunkZ = 0;
	%this.currChunk = 0;

	%this.trustFailCount = 0;
	%this.superCutCount = 0;
	%this.superCutPlacedCount = 0;

	//Process first tick
	if(%client && $Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	%this.tickSuperCutChunk();
}

//Process all bricks in a chunk
function ND_Selection::tickSuperCutChunk(%this)
{
	cancel(%this.superCutSchedule);

	//Restore chunk variables (scopes and slow object fields suck)
	%chunkSize = %this.chunkSize;
	%currChunk = %this.currChunk;

	%currChunkX = %this.currChunkX;
	%currChunkY = %this.currChunkY;
	%currChunkZ = %this.currChunkZ;

	%numChunksX = %this.numChunksX;
	%numChunksY = %this.numChunksY;
	%numChunksZ = %this.numChunksZ;

	%chunkX1 = %this.chunkX1;
	%chunkY1 = %this.chunkY1;
	%chunkZ1 = %this.chunkZ1;

	%chunkX2 = %this.chunkX2;
	%chunkY2 = %this.chunkY2;
	%chunkZ2 = %this.chunkZ2;

	//Variables for trust checks
	if(%this.client)
	{
		%admin = %this.client.isAdmin;
		%group = %this.client.brickGroup.getId();
		%bl_id = %this.client.bl_id;
	}

	%chunksDone = 0;
	%bricksFound = 0;
	%bricksPlanted = 0;
	%trustFailCount = 0;

	ndUpdateSpawnedClientList();

	//Process chunks until we reach the brick or chunk limit
	while(%chunksDone < 600 && %bricksFound < 1000 && %bricksPlanted < 300)
	{
		%chunksDone++;

		//Calculate position and size of chunk
		%x1 = %chunkX1 + (%currChunkX * %chunkSize) + 0.05;
		%y1 = %chunkY1 + (%currChunkY * %chunkSize) + 0.05;
		%z1 = %chunkZ1 + (%currChunkZ * %chunkSize) + 0.05;

		%x2 = getMin(%chunkX2 - 0.05, %x1 + %chunkSize - 0.1);
		%y2 = getMin(%chunkY2 - 0.05, %y1 + %chunkSize - 0.1);
		%z2 = getMin(%chunkZ2 - 0.05, %z1 + %chunkSize - 0.1);

		%size = %x2 - %x1 SPC %y2 - %y1 SPC %z2 - %z1;
		%pos = vectorAdd(%x1 SPC %y1 SPC %z1, vectorScale(%size, 0.5));

		//Process all new bricks found in this chunk
		initContainerBoxSearch(%pos, %size, $TypeMasks::FxBrickAlwaysObjectType);

		while(%obj = containerSearchNext())
		{
			%db = %obj.getDatablock();
			%bricksFound++;

			//Check trust
			if(%this.client && !ndTrustCheckModify(%obj, %group, %bl_id, %admin))
			{
				%trustFailCount++;
				continue;
			}

			//Skip zone bricks
			if(%db.isWaterBrick)
				continue;

			//Set variables for the fill brick function
			$ND::FillBrickGroup = %obj.getGroup();
			$ND::FillBrickClient = %obj.client;
			$ND::FillBrickBL_ID = %obj.getGroup().bl_id;

			$ND::FillBrickColorID = %obj.colorID;
			$ND::FillBrickColorFxID = %obj.colorFxID;
			$ND::FillBrickShapeFxID = %obj.shapeFxID;

			$ND::FillBrickRendering = %obj.isRendering();
			$ND::FillBrickColliding = %obj.isColliding();
			$ND::FillBrickRayCasting = %obj.isRayCasting();

			%box = %obj.getWorldBox();
			%boxX1 = getWord(%box, 0);
			%boxY1 = getWord(%box, 1);
			%boxZ1 = getWord(%box, 2);
			%boxX2 = getWord(%box, 3);
			%boxY2 = getWord(%box, 4);
			%boxZ2 = getWord(%box, 5);
			%obj.delete();

			%deleted = true;
			%cutCount++;

			$ND::FillBrickCount = 0;

			if((%boxX1 + 0.05) < %chunkX1)
			{
				ndFillAreaWithBricks(
					%boxX1 SPC %boxY1 SPC %boxZ1,
					%chunkX1 SPC %boxY2 SPC %boxZ2);
			}

			if((%boxX2 - 0.05) > %chunkX2)
			{
				ndFillAreaWithBricks(
					%chunkX2 SPC %boxY1 SPC %boxZ1,
					%boxX2 SPC %boxY2 SPC %boxZ2);
			}

			if((%boxY1 + 0.05) < %chunkY1)
			{
				ndFillAreaWithBricks(
					getMax(%boxX1, %chunkX1) SPC %boxY1 SPC %boxZ1,
					getMin(%boxX2, %chunkX2) SPC %chunkY1 SPC %boxZ2);
			}

			if((%boxY2 - 0.05) > %chunkY2)
			{
				ndFillAreaWithBricks(
					getMax(%boxX1, %chunkX1) SPC %chunkY2 SPC %boxZ1,
					getMin(%boxX2, %chunkX2) SPC %boxY2 SPC %boxZ2);
			}

			if((%boxZ1 + 0.05) < %chunkZ1)
			{
				ndFillAreaWithBricks(
					getMax(%boxX1, %chunkX1) SPC getMax(%boxY1, %chunkY1) SPC %boxZ1,
					getMin(%boxX2, %chunkX2) SPC getMin(%boxY2, %chunkY2) SPC %chunkZ1);
			}

			if((%boxZ2 - 0.05) > %chunkZ2)
			{
				ndFillAreaWithBricks(
					getMax(%boxX1, %chunkX1) SPC getMax(%boxY1, %chunkY1) SPC %chunkZ2,
					getMin(%boxX2, %chunkX2) SPC getMin(%boxY2, %chunkY2) SPC %boxZ2);
			}

			%bricksPlanted += $ND::FillBrickCount;
		}

		//Set next chunk index or break
		%currChunk++;

		if(%currChunkX++ >= %numChunksX)
		{
			%currChunkX = 0;

			if(%currChunkY++ >= %numChunksY)
			{
				%currChunkY = 0;

				if(%currChunkZ++ >= %numChunksZ)
				{
					%searchComplete = true;
					break;
				}
			}
		}
	}

	//Save chunk variables (scopes and slow object fields suck)
	%this.currChunk = %currChunk;

	%this.currChunkX = %currChunkX;
	%this.currChunkY = %currChunkY;
	%this.currChunkZ = %currChunkZ;

	%this.numChunksX = %numChunksX;
	%this.numChunksY = %numChunksY;
	%this.numChunksZ = %numChunksZ;

	%this.trustFailCount += %trustFailCount;
	%this.superCutCount += %cutCount;
	%this.superCutPlacedCount += %bricksPlanted;

	//If all chunks have been searched, start processing
	if(%searchComplete)
	{
		%this.finishSuperCut();
		return;
	}

	//Tell the client which chunks we just processed
	if(%this.client && %this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Schedule next chunk
	%this.superCutSchedule = %this.schedule(30, tickSuperCutChunk);
}

//Finish super cut
function ND_Selection::finishSuperCut(%this)
{
	if(!%this.client)
		return;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadEnd', "");

	%s = %this.superCutCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Deleted \c3" @ %this.superCutCount @ "\c6 Brick" @ %s @ "!";

	if(%this.superCutPlacedCount > 0)
	{
		%s = %this.superCutPlacedCount == 1 ? "" : "s";
		%msg = %msg @ "\n<font:Verdana:17>\c6Placed \c3" @ %this.superCutPlacedCount @ "\c6 new one" @ %s @ ".";
	}

	if(%this.trustFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.trustFailCount @ "\c6 missing trust.";

	commandToClient(%this.client, 'centerPrint', %msg, 12);
	%this.client.ndSetMode(NDM_BoxSelect);

	if(%this.client.fillBricksAfterSuperCut)
	{
		%this.client.fillBricksAfterSuperCut = false;

		if(%this.trustFailCount)
			messageClient(%this.client, '', "\c6Cannot run fill bricks, you do not have enough trust bricks already in the area.");
		else
			%this.client.doFillBricks();
	}
}

//Cancel super cut
function ND_Selection::cancelSuperCut(%this)
{
	cancel(%this.superCutSchedule);
	%this.deleteData();
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
	ndUpdateSpawnedClientList();

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

		//Skip missing bricks
		if($NS[%this, "D", %i] == 0)
			continue;

		//Offset position
		%bPos = vectorAdd(ndRotateVector($NS[%this, "P", %i], %angleID), %position);

		//Rotate local angle id and get correct rotation value
		%bAngle = ($NS[%this, "R", %i] + %angleID ) % 4;

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
			datablock = $NS[%this, "D", %i];
			isPlanted = false;

			position = %bPos;
			rotation = %bRot;
			angleID = %bAngle;

			colorID = $NS[%this, "CO", %i];
			printID = $NS[%this, "PR", %i];

			//Used in shiftGhostBricks
			selectionIndex = %i;
		};

		//Add ghost brick to ghost set
		%ghostGroup.add(%brick);

		//Scope ghost brick to all clients we found earlier
		for(%j = 0; %j < $ND::NumSpawnedClients; %j++)
			%brick.scopeToClient($ND::SpawnedClient[%j]);
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
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 230);
}

//Rotate ghost bricks left/right
function ND_Selection::rotateGhostBricks(%this, %direction, %useSelectionCenter)
{
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
	if(%direction % 2 != 0)
		%pOffset = vectorAdd(%pOffset, %shiftCorrect);

	//Update variables
	%this.ghostAngleID = (%this.ghostAngleID + %direction) % 4;
	%this.ghostPosition = vectorAdd(%pivot, %pOffset);
	%this.updateHighlightBox();

	//Update ghost bricks
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 230);
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
	%this.updateGhostBricks(0, $Pref::Server::ND::InstantGhostBricks, 230);
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
		%bPos = $NS[%this, "P", %j];

		//Rotated local angle id
		%bAngle = $NS[%this, "R", %j];

		//Apply mirror effects (ugh)
		%datablock = $NS[%this, "D", %j];

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

	%this.ghostGroup.tickDelete();
	%this.ghostGroup = false;
}

//World box center for ghosted selection
function ND_Selection::getGhostCenter(%this)
{
	if(!isObject(%this.ghostGroup))
		return "0 0 0";

	%offset = %this.rootToCenter;

	if(%this.ghostMirrorX)
		%offset = -getWord(%offset, 0) SPC getWord(%offset, 1) SPC getWord(%offset, 2);
	else if(%this.ghostMirrorY)
		%offset = getWord(%offset, 0) SPC -getWord(%offset, 1) SPC getWord(%offset, 2);

	if(%this.ghostMirrorZ)
		%offset = getWord(%offset, 0) SPC getWord(%offset, 1) SPC -getWord(%offset, 2);

	return vectorAdd(%this.ghostPosition, ndRotateVector(%offset, %this.ghostAngleID));
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
function ND_Selection::startPlant(%this, %position, %angleID, %forcePlant)
{
	%this.forcePlant = %forcePlant;

	%this.plantSearchIndex = 0;
	%this.plantQueueIndex = 0;
	%this.plantQueueCount = 0;

	%this.plantSuccessCount = 0;
	%this.plantTrustFailCount = 0;
	%this.plantBlockedFailCount = 0;
	%this.plantMissingFailCount = 0;

	%this.undoGroup = new SimSet();
	ND_ServerGroup.add(%this.undoGroup);

	//Reset mirror error list
	%client = %this.client;

	%countX = $NS[%client, "MXC"];
	%countZ = $NS[%client, "MZC"];

	for(%i = 0; %i < %countX; %i++)
		$NS[%client, "MXK", $NS[%client, "MXE", %i]] = "";

	for(%i = 0; %i < %countZ; %i++)
		$NS[%client, "MZK", $NS[%client, "MZE", %i]] = "";

	$NS[%client, "MZC"] = 0;
	$NS[%client, "MXC"] = 0;

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

	if(isObject(%this.targetGroup))
	{
		%group = %this.targetGroup;
		%bl_id = %this.targetBlid;
	}
	else
	{
		%group = %client.brickGroup.getId();
		%bl_id = %client.bl_id;
	}

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;

	for(%i = %start; %i < %end; %i++)
	{
		//Brick already placed
		if($NP[%this, %i])
			continue;

		//Skip nonexistant bricks
		if($NS[%this, "D", %i] == 0)
		{
			$NP[%this, %i] = true;
			%this.plantMissingFailCount++;
			continue;
		}

		//Attempt to place brick
		%brick = ND_Selection::plantBrick(%this, %i, %position, %angleID, %group, %client, %bl_id);
		%plants++;

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %i] = true;

			%conns = $NS[%this, "N", %i];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "C", %i, %j];

				if(%id < %i && !$NP[%this, %id])
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

	if(isObject(%this.targetGroup))
	{
		%group = %this.targetGroup;
		%bl_id = %this.targetBlid;
	}
	else
	{
		%group = %client.brickGroup.getId();
		%bl_id = %client.bl_id;
	}

	%qCount = %this.plantQueueCount;
	%numClients = %this.numClients;

	%searchIndex = %this.plantSearchIndex;

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

		//Skip nonexistant bricks
		if($NS[%this, "D", %i] == 0)
		{
			$NP[%this, %bId] = true;
			%this.plantMissingFailCount++;
			continue;
		}

		%brick = ND_Selection::plantBrick(%this, %bId, %position, %angleID, %group, %client, %bl_id);

		if(%brick > 0)
		{
			//Success! Add connected bricks to plant queue
			%this.plantSuccessCount++;
			%this.undoGroup.add(%brick);

			$NP[%this, %bId] = true;

			%conns = $NS[%this, "N", %bId];
			for(%j = 0; %j < %conns; %j++)
			{
				%id = $NS[%this, "C", %bId, %j];

				if(%id < %searchIndex && !$NP[%this, %id])
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
	%bPos = $NS[%this, "P", %i];

	//Local angle id
	%bAngle = $NS[%this, "R", %i];

	//Apply mirror effects (ugh)
	%datablock = $NS[%this, "D", %i];

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
					if(!$NS[%client, "MXK", %datablock])
					{
						%id = $NS[%client, "MXC"];
						$NS[%client, "MXC"]++;

						$NS[%client, "MXE", %id] = %datablock;
						$NS[%client, "MXK", %datablock] = true;
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
					if(!$NS[%client, "MXK", %datablock])
					{
						%id = $NS[%client, "MXC"];
						$NS[%client, "MXC"]++;

						$NS[%client, "MXE", %id]= %datablock;
						$NS[%client, "MXK", %datablock] = true;
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
				if(!$NS[%client, "MZK", %datablock])
				{
					%id = $NS[%client, "MZC"];
					$NS[%client, "MZC"]++;

					$NS[%client, "MZE", %id]= %datablock;
					$NS[%client, "MZK", %datablock] = true;
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
		isPlanted = true;
		client = %client;

		position = %bPos;
		rotation = %bRot;
		angleID = %bAngle;

		colorID = $NS[%this, "CO", %i];
		colorFxID = $NS[%this, "CF", %i];

		printID = $NS[%this, "PR", %i];
	};

	//This will call ::onLoadPlant instead of ::onPlant
	%prev1 = $Server_LoadFileObj;
	%prev2 = $LastLoadedBrick;
	$Server_LoadFileObj = %brick;
	$LastLoadedBrick = %brick;

	//Add to brickgroup
	%brickGroup.add(%brick);

	//Attempt plant
	%error = %brick.plant();

	//Restore variable
	$Server_LoadFileObj = %prev1;
	$LastLoadedBrick = %prev2;

	if(!isObject(%brick))
		return -1;

	if(%error == 2)
	{
		//Do we plant floating bricks?
		if(%this.forcePlant)
		{
			//Brick is floating. Pretend it is supported by terrain
			%brick.isBaseplate = true;

			//Make engine recompute distance from ground to apply it
			%brick.willCauseChainKill();
		}
		else
		{
			%brick.delete();
			return 0;
		}
	}
	else if(%error)
	{
		%brick.delete();
		return -1;
	}

	//Check for trust
	%downCount = %brick.getNumDownBricks();

	if(!%client.isAdmin || !$Pref::Server::ND::AdminTrustBypass2)
	{
		for(%j = 0; %j < %downCount; %j++)
		{
			if(!ndFastTrustCheck(%brick.getDownBrick(%j), %bl_id, %brickGroup))
			{
				%brick.delete();
				return -2;
			}
		}

		%upCount = %brick.getNumUpBricks();

		for(%j = 0; %j < %upCount; %j++)
		{
			if(!ndFastTrustCheck(%brick.getUpBrick(%j), %bl_id, %brickGroup))
			{
				%brick.delete();
				return -2;
			}
		}
	}
	else if(!%downCount)
		%upCount = %brick.getNumUpBricks();

	//Finished trust check
	if(%downCount)
		%brick.stackBL_ID = %brick.getDownBrick(0).stackBL_ID;
	else if(%upCount)
		%brick.stackBL_ID = %brick.getUpBrick(0).stackBL_ID;
	else
		%brick.stackBL_ID = %bl_id;

	%brick.trustCheckFinished();

	//Apply special settings
	%brick.setRendering(!$NS[%this, "NR", %i]);
	%brick.setRaycasting(!$NS[%this, "NRC", %i]);
	%brick.setColliding(!$NS[%this, "NC", %i]);
	%brick.setShapeFx($NS[%this, "SF", %i]);

	//Apply events
	if(%numEvents = $NS[%this, "EN", %i])
	{
		%brick.numEvents = %numEvents;
		%brick.implicitCancelEvents = 0;

		for(%j = 0; %j < %numEvents; %j++)
		{
			%brick.eventEnabled[%j] = $NS[%this, "EE", %i, %j];
			%brick.eventDelay[%j] = $NS[%this, "ED", %i, %j];

			%inputIdx = $NS[%this, "EII", %i, %j];

			%brick.eventInput[%j] = $NS[%this, "EI", %i, %j];
			%brick.eventInputIdx[%j] = %inputIdx;

			%target = $NS[%this, "ET", %i, %j];
			%targetIdx = $NS[%this, "ETI", %i, %j];

			if(%targetIdx == -1)
			{
				%nt = $NS[%this, "ENT", %i, %j];
				%brick.eventNT[%j] = %nt;
			}

			%brick.eventTarget[%j] = %target;
			%brick.eventTargetIdx[%j] = %targetIdx;

			%output = $NS[%this, "EO", %i, %j];
			%outputIdx = $NS[%this, "EOI", %i, %j];

			//Only rotate outputs for named bricks if they are selected
			if(%targetIdx >= 0 || $NS[%this, "HN", %nt])
			{
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
					%rotated = ndTransformDirection(%dir, %angleID, %mirrX, %mirrY, %mirrZ);
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
			}

			%brick.eventOutput[%j] = %output;
			%brick.eventOutputIdx[%j] = %outputIdx;
			%brick.eventOutputAppendClient[%j] = $NS[%this, "EOC", %i, %j];

			//Why does this need to be so complicated?
			if(%targetIdx >= 0)
				%targetClass = getWord($InputEvent_TargetListfxDtsBrick_[%inputIdx], %targetIdx * 2 + 1);
			else
				%targetClass = "FxDTSBrick";

			%paramList = $OutputEvent_ParameterList[%targetClass, %outputIdx];
			%paramCount = getFieldCount(%paramList);

			for(%k = 0; %k < %paramCount; %k++)
			{
				%param = $NS[%this, "EP", %i, %j, %k];

				//Only rotate outputs for named bricks if they are selected
				if(%targetIdx >= 0 || $NS[%this, "HN", %nt])
				{
					%paramType = getField(%paramList, %k);

					switch$(getWord(%paramType, 0))
					{
						case "vector":
							//Apply mirror effects
							if(%mirrX)
								%param = -firstWord(%param) SPC restWords(%param);
							else if(%mirrY)
								%param = getWord(%param, 0) SPC -getWord(%param, 1) SPC getWord(%param, 2);

							if(%mirrZ)
								%param = getWord(%param, 0) SPC getWord(%param, 1) SPC -getWord(%param, 2);

							%param = ndRotateVector(%param, %angleID);

						case "list":
							%value = getWord(%paramType, %param * 2 + 1);

							switch$(%value)
							{
								case "Up":    %dir = 0;
								case "Down":  %dir = 1;
								case "North": %dir = 2;
								case "East":  %dir = 3;
								case "South": %dir = 4;
								case "West":  %dir = 5;
								default: %dir = -1;
							}

							if(%dir >= 0)
							{
								switch(ndTransformDirection(%dir, %angleID, %mirrX, %mirrY, %mirrZ))
								{
									case 0: %value = "Up";
									case 1: %value = "Down";
									case 2: %value = "North";
									case 3: %value = "East";
									case 4: %value = "South";
									case 5: %value = "West";
								}

								for(%l = 1; %l < getWordCount(%paramType); %l += 2)
								{
									if(getWord(%paramType, %l) $= %value)
									{
										%param = getWord(%paramType, %l + 1);
										break;
									}
								}
							}
					}
				}

				%brick.eventOutputParameter[%j, %k + 1] = %param;
			}
		}
	}

	setCurrentQuotaObject(getQuotaObjectFromClient(%client));

	if((%tmp = $NS[%this, "NT", %i]) !$= "")
		%brick.setNTObjectName(%tmp);

	if(%tmp = $NS[%this, "LD", %i])
		%brick.setLight(%tmp, %client);

	if(%tmp = $NS[%this, "ED", %i])
	{
		%dir = ndTransformDirection($NS[%this, "ER", %i], %angleID, %mirrX, %mirrY, %mirrZ);

		%brick.emitterDirection = %dir;
		%brick.setEmitter(%tmp, %client);
	}

	if(%tmp = $NS[%this, "ID", %i])
	{
		%pos = ndTransformDirection($NS[%this, "IP", %i], %angleID, %mirrX, %mirrY, %mirrZ);
		%dir = ndTransformDirection($NS[%this, "IR", %i], %angleID, %mirrX, %mirrY, %mirrZ);

		%brick.itemPosition = %pos;
		%brick.itemDirection = %dir;
		%brick.itemRespawnTime = $NS[%this, "IT", %i];
		%brick.setItem(%tmp, %client);
	}

	if(%tmp = $NS[%this, "VD", %i])
	{
		%brick.reColorVehicle = $NS[%this, "VC", %i];
		%brick.setVehicle(%tmp, %client);
	}

	if(%tmp = $NS[%this, "MD", %i])
		%brick.setSound(%tmp, %client);

	return %brick;
}

//Finished planting all the bricks!
function ND_Selection::finishPlant(%this)
{
	//Report mirror errors
	if($NS[%this.client, "MXC"] > 0 || $NS[%this.client, "MZC"] > 0)
		messageClient(%this.client, '', "\c6Some bricks were probably mirrored incorrectly. Say \c3/mirErrors\c6 to find out more.");

	%count = %this.brickCount;
	%planted = %this.plantSuccessCount;
	%blocked = %this.plantBlockedFailCount;
	%trusted = %this.plantTrustFailCount;
	%missing = %this.plantMissingFailCount;
	%floating = %count - %planted - %blocked - %trusted - %missing;

	%s = %this.plantSuccessCount == 1 ? "" : "s";
	%message = "<font:Verdana:20>\c6Planted \c3" @ %this.plantSuccessCount @ "\c6 / \c3" @ %count @ "\c6 Brick" @ %s @ "!";

	if(%trusted)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %trusted @ "\c6 missing trust.";

	if(%blocked)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %blocked @ "\c6 blocked.";

	if(%floating)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %floating @ "\c6 floating.";

	if(%missing)
		%message = %message @ "\n<font:Verdana:17>\c3" @ %missing @ "\c6 missing Datablock.";

	commandToClient(%this.client, 'centerPrint', %message, 4);

	if($Pref::Server::ND::PlayMenuSounds && %planted && %this.brickCount > $Pref::Server::ND::ProcessPerTick * 10)
		messageClient(%this.client, 'MsgProcessComplete', "");

	deleteVariables("$NP" @ %this @ "_*");

	if(%planted)
	{
		%this.undoGroup.brickCount = %this.undoGroup.getCount();
		%this.client.undoStack.push(%this.undoGroup TAB "ND_PLANT");
	}
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
	{
		%this.undoGroup.brickCount = %this.undoGroup.getCount();
		%this.client.undoStack.push(%this.undoGroup TAB "ND_PLANT");
	}
	else
		%this.undoGroup.delete();
}



//Fill Colors
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Start filling bricks with a specific color
function ND_Selection::startFillColor(%this, %mode, %colorID)
{
	%this.paintIndex = 0;
	%this.paintFailCount = 0;
	%this.paintSuccessCount = 0;

	//Create undo group
	%this.undoGroup = new ScriptObject(ND_UndoGroupPaint)
	{
		paintType = %mode;
		brickCount = 0;
		client = %this.client;
	};

	ND_ServerGroup.add(%this.undoGroup);

	%this.tickFillColor(%mode, %colorID);
}

//Tick filling bricks with a specific color
function ND_Selection::tickFillColor(%this, %mode, %colorID)
{
	cancel(%this.fillColorSchedule);

	%start = %this.paintIndex;
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%admin = %this.client.isAdmin;
	%group2 = %this.client.brickGroup.getId();
	%bl_id = %this.client.bl_id;

	%paintCount = %this.paintSuccessCount;
	%failCount = %this.paintFailCount;

	%undoCount = %this.undoGroup.brickCount;

	%clientId = %this.client;
	%undoId = %this.undoGroup;

	for(%i = %start; %i < %end; %i++)
	{
		if(isObject(%brick = $NS[%this, "B", %i]))
		{
			if(ndTrustCheckModify(%brick, %group2, %bl_id, %admin))
			{
				//Color brick
				switch(%mode)
				{
					case 0:
						//Don't change to same value
						if(%brick.colorID == %colorID)
							continue;

						//Write previous value to undo array
						$NU[%clientId, %undoId, "V", %paintCount] = %brick.colorID;

						%brick.setColor(%colorID);

						//Update selection data
						$NS[%this, "CO", $NS[%this, "I", %brick]] = %colorID;

					case 1:
						//Check whether brick is highlighted
						if($NDHN[%brick])
						{
							//Don't change to same value
							if($NDHF[%brick] == %colorID)
								continue;

							//Write previous value to undo array
							$NU[%clientId, %undoId, "V", %paintCount] = $NDHF[%brick];

							//If we're highlighted, change the original color instead
							$NDHF[%brick] = %colorID;
						}
						else
						{
							//Don't change to same value
							if(%brick.colorFxID == %colorID)
								continue;

							//Write previous value to undo array
							$NU[%clientId, %undoId, "V", %paintCount] = %brick.colorFxID;

							%brick.setColorFx(%colorID);
						}

						//Update selection data
						$NS[%this, "CF", $NS[%this, "I", %brick]] = %colorID;

					case 2:
						//Don't change to same value
						if(%brick.shapeFxID == %colorID)
							continue;

						//Write previous value to undo array
						$NU[%clientId, %undoId, "V", %paintCount] = %brick.shapeFxID;

						%brick.setShapeFx(%colorID);

						//Update selection data
						$NS[%this, "SF", $NS[%this, "I", %brick]] = %colorID;
				}

				$NU[%clientId, %undoId, "B", %paintCount] = %brick;
				%paintCount++;
			}
			else
				%failCount++;
		}
	}

	%this.paintIndex = %i;
	%this.paintSuccessCount = %paintCount;
	%this.paintFailCount = %failCount;

	%this.undoGroup.brickCount = %paintCount;

	//Tell the client how much we painted this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%i >= %this.brickCount)
		%this.finishFillColor();
	else
		%this.fillColorSchedule = %this.schedule(30, tickFillColor, %mode, %colorID);
}

//Finish filling color
function ND_Selection::finishFillColor(%this)
{
	%s = %this.undoGroup.brickCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Painted \c3" @ %this.undoGroup.brickCount @ "\c6 Brick" @ %s @ "!";

	if(%this.paintFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.paintFailCount @ "\c6 missing trust.";

	commandToClient(%this.client, 'centerPrint', %msg, 8);

	if(%this.undoGroup.brickCount)
		%this.client.undoStack.push(%this.undoGroup TAB "ND_PAINT");
	else
		%this.undoGroup.delete();

	%this.client.ndSetMode(NDM_FillColor);
}

//Cancel filling color
function ND_Selection::cancelFillColor(%this)
{
	cancel(%this.fillColorSchedule);

	if(%this.undoGroup.brickCount)
		%this.client.undoStack.push(%this.undoGroup TAB "ND_PAINT");
	else
		%this.undoGroup.delete();
}



//Fill Wrench
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Start applying wrench settings to all bricks
function ND_Selection::startFillWrench(%this, %data)
{
	%valid = false;
	%this.fillWrenchName       = false;
	%this.fillWrenchLight      = false;
	%this.fillWrenchEmitter    = false;
	%this.fillWrenchEmitterDir = false;
	%this.fillWrenchItem       = false;
	%this.fillWrenchItemPos    = false;
	%this.fillWrenchItemDir    = false;
	%this.fillWrenchItemTime   = false;
	%this.fillWrenchRaycasting = false;
	%this.fillWrenchCollision  = false;
	%this.fillWrenchRendering  = false;

	//Verify and save data
	%fieldCount = getFieldCount(%data);

	for(%i = 0; %i < %fieldCount; %i++)
	{
		%field = getField(%data, %i);

		%type = getWord(%field, 0);
		%value = trim(restWords(%field));

		switch$(%type)
		{
			case "N":
				%this.fillWrenchName = true;
				%this.fillWrenchNameValue = getSafeVariableName(%value);
				%valid = true;

			case "LDB":
				if((isObject(%value) && %value.getClassName() $= "FxLightData" && %value.uiName !$= "") || %value == 0)
				{
					%this.fillWrenchLight = true;
					%this.fillWrenchLightValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid light datablock " @ %value);

			case "EDB":
				if((isObject(%value) && %value.getClassName() $= "ParticleEmitterData" && %value.uiName !$= "") || %value == 0)
				{
					%this.fillWrenchEmitter = true;
					%this.fillWrenchEmitterValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid emitter datablock " @ %value);

			case "EDIR":
				if(%value >= 0 && %value <= 5)
				{
					%this.fillWrenchEmitterDir = true;
					%this.fillWrenchEmitterDirValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid emitter direction " @ %value);

			case "IDB":
				if((isObject(%value) && %value.getClassName() $= "ItemData" && %value.uiName !$= "") || %value == 0)
				{
					%this.fillWrenchItem = true;
					%this.fillWrenchItemValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid item datablock " @ %value);

			case "IPOS":
				if(%value >= 0 && %value <= 5)
				{
					%this.fillWrenchItemPos = true;
					%this.fillWrenchItemPosValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid item position " @ %value);

			case "IDIR":
				if(%value >= 2 && %value <= 5)
				{
					%this.fillWrenchItemDir = true;
					%this.fillWrenchItemDirValue = %value;
					%valid = true;
				}
				else
					messageClient(%this.client, '', "\c6Fill wrench error - Invalid item direction " @ %value);

			case "IRT":
				%this.fillWrenchItemTime = true;
				%this.fillWrenchItemTimeValue = mFloor(%value) * 1000;
				%valid = true;

			case "RC":
				%this.fillWrenchRaycasting = true;
				%this.fillWrenchRaycastingValue = %value;
				%valid = true;

			case "C":
				%this.fillWrenchCollision = true;
				%this.fillWrenchCollisionValue = %value;
				%valid = true;

			case "R":
				%this.fillWrenchRendering = true;
				%this.fillWrenchRenderingValue = %value;
				%valid = true;

			default:
				messageClient(%this.client, '', "\c6Fill wrench error - Invalid field " @ %type);
		}
	}

	if(!%valid)
	{
		messageClient(%this.client, '', "\c6Fill wrench error - No data to apply?");
		%this.cancelFillWrench();
		%this.client.ndSetMode(%this.client.ndLastSelectMode);
		return;
	}

	%this.wrenchIndex = 0;
	%this.wrenchFailCount = 0;
	%this.wrenchSuccessCount = 0;

	//Create undo group
	%this.undoGroup = new ScriptObject(ND_UndoGroupWrench)
	{
		fillWrenchName       = %this.fillWrenchName;
		fillWrenchLight      = %this.fillWrenchLight;
		fillWrenchEmitter    = %this.fillWrenchEmitter;
		fillWrenchEmitterDir = %this.fillWrenchEmitterDir;
		fillWrenchItem       = %this.fillWrenchItem;
		fillWrenchItemPos    = %this.fillWrenchItemPos;
		fillWrenchItemDir    = %this.fillWrenchItemDir;
		fillWrenchItemTime   = %this.fillWrenchItemTime;
		fillWrenchRaycasting = %this.fillWrenchRaycasting;
		fillWrenchCollision  = %this.fillWrenchCollision;
		fillWrenchRendering  = %this.fillWrenchRendering;

		brickCount = 0;
		client = %this.client;
	};

	ND_ServerGroup.add(%this.undoGroup);

	%this.tickFillWrench();
}

//Tick applying wrench settings to all bricks
function ND_Selection::tickFillWrench(%this)
{
	cancel(%this.fillWrenchSchedule);

	%start = %this.wrenchIndex;
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%client = %this.client;

	%admin = %this.client.isAdmin;
	%group2 = %client.brickGroup.getId();
	%bl_id = %client.bl_id;

	%wrenchCount = %this.wrenchSuccessCount;
	%failCount = %this.wrenchFailCount;

	%undoCount = %this.undoGroup.brickCount;

	%clientId = %this.client;
	%undoId = %this.undoGroup;

	setCurrentQuotaObject(getQuotaObjectFromClient(%client));

	%fillWrenchName       = %this.fillWrenchName;
	%fillWrenchLight      = %this.fillWrenchLight;
	%fillWrenchEmitter    = %this.fillWrenchEmitter;
	%fillWrenchEmitterDir = %this.fillWrenchEmitterDir;
	%fillWrenchItem       = %this.fillWrenchItem;
	%fillWrenchItemPos    = %this.fillWrenchItemPos;
	%fillWrenchItemDir    = %this.fillWrenchItemDir;
	%fillWrenchItemTime   = %this.fillWrenchItemTime;
	%fillWrenchRaycasting = %this.fillWrenchRaycasting;
	%fillWrenchCollision  = %this.fillWrenchCollision;
	%fillWrenchRendering  = %this.fillWrenchRendering;

	%fillWrenchNameValue       = %this.fillWrenchNameValue;
	%fillWrenchLightValue      = %this.fillWrenchLightValue;
	%fillWrenchEmitterValue    = %this.fillWrenchEmitterValue;
	%fillWrenchEmitterDirValue = %this.fillWrenchEmitterDirValue;
	%fillWrenchItemValue       = %this.fillWrenchItemValue;
	%fillWrenchItemPosValue    = %this.fillWrenchItemPosValue;
	%fillWrenchItemDirValue    = %this.fillWrenchItemDirValue;
	%fillWrenchItemTimeValue   = %this.fillWrenchItemTimeValue;
	%fillWrenchRaycastingValue = %this.fillWrenchRaycastingValue;
	%fillWrenchCollisionValue  = %this.fillWrenchCollisionValue;
	%fillWrenchRenderingValue  = %this.fillWrenchRenderingValue;

	for(%i = %start; %i < %end; %i++)
	{
		if(isObject(%brick = $NS[%this, "B", %i]))
		{
			if(ndTrustCheckModify(%brick, %group2, %bl_id, %admin))
			{
				%undoRequired = false;

				//Apply wrench settings
				if(%fillWrenchName)
				{
					%curr = getSubStr(%brick.getName(), 1, 254);
					$NU[%clientId, %undoId, "N", %undoCount] = %curr;

					if(%curr !$= %fillWrenchNameValue)
					{
						%brick.setNTObjectName(%fillWrenchNameValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchLight)
				{
					if(%tmp = %brick.light | 0)
						%curr = %tmp.getDatablock();
					else
						%curr = 0;

					$NU[%clientId, %undoId, "LDB", %undoCount] = %curr;

					if(%curr != %fillWrenchLightValue)
					{
						%brick.setLight(%fillWrenchLightValue, %client);
						%undoRequired = true;
					}
				}

				if(%fillWrenchEmitter)
				{
					if(%tmp = %brick.emitter | 0)
						%curr = %tmp.getEmitterDatablock();
					else if(%tmp = %brick.oldEmitterDB | 0)
						%curr = %tmp;
					else
						%curr = 0;

					$NU[%clientId, %undoId, "EDB", %undoCount] = %curr;

					if(%curr != %fillWrenchEmitterValue)
					{
						%brick.setEmitter(%fillWrenchEmitterValue, %client);
						%undoRequired = true;
					}
				}

				if(%fillWrenchEmitterDir)
				{
					%curr = %brick.emitterDirection;
					$NU[%clientId, %undoId, "EDIR", %undoCount] = %curr;

					if(%curr != %fillWrenchEmitterDirValue)
					{
						%brick.setEmitterDirection(%fillWrenchEmitterDirValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchItem)
				{
					if(%tmp = %brick.item | 0)
						%curr = %tmp.getDatablock();
					else
						%curr = 0;

					$NU[%clientId, %undoId, "IDB", %undoCount] = %curr;

					if(%curr != %fillWrenchItemValue)
					{
						%brick.setItem(%fillWrenchItemValue, %client);
						%undoRequired = true;
					}
				}

				if(%fillWrenchItemPos)
				{
					%curr = %brick.itemPosition;
					$NU[%clientId, %undoId, "IPOS", %undoCount] = %curr;

					if(%curr != %fillWrenchItemPosValue)
					{
						%brick.setItemPosition(%fillWrenchItemPosValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchItemDir)
				{
					%curr = %brick.itemPosition;
					$NU[%clientId, %undoId, "IDIR", %undoCount] = %curr;

					if(%curr != %fillWrenchItemDirValue)
					{
						%brick.setItemDirection(%fillWrenchItemDirValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchItemTime)
				{
					%curr = %brick.itemRespawnTime;
					$NU[%clientId, %undoId, "IRT", %undoCount] = %curr;

					if(%curr != %fillWrenchItemTimeValue)
					{
						%brick.setItemRespawnTime(%fillWrenchItemTimeValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchRaycasting)
				{
					%curr = %brick.isRaycasting();
					$NU[%clientId, %undoId, "RC", %undoCount] = %curr;

					if(%curr != %fillWrenchRaycastingValue)
					{
						%brick.setRaycasting(%fillWrenchRaycastingValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchCollision)
				{
					%curr = %brick.isColliding();
					$NU[%clientId, %undoId, "C", %undoCount] = %curr;

					if(%curr != %fillWrenchCollisionValue)
					{
						%brick.setColliding(%fillWrenchCollisionValue);
						%undoRequired = true;
					}
				}

				if(%fillWrenchRendering)
				{
					%curr = %brick.isRendering();
					$NU[%clientId, %undoId, "R", %undoCount] = %curr;

					if(%curr != %fillWrenchRenderingValue)
					{
						//Copy emitter ...?
						if(!%fillWrenchRenderingValue && (%tmp = %brick.emitter | 0))
							%emitter = %tmp.getEmitterDatablock();
						else
							%emitter = 0;

						%brick.setRendering(%fillWrenchRenderingValue);
						%undoRequired = true;

						if(!%fillWrenchRenderingValue && %emitter)
							%brick.setEmitter(%emitter);
					}
				}

				if(%undoRequired)
				{
					$NU[%clientId, %undoId, "B", %undoCount] = %brick;
					%undoCount++;
				}

				%wrenchCount++;
			}
			else
				%failCount++;
		}
	}

	clearCurrentQuotaObject();

	%this.wrenchIndex = %i;
	%this.wrenchSuccessCount = %wrenchCount;
	%this.wrenchFailCount = %failCount;

	%this.undoGroup.brickCount = %undoCount;

	//Tell the client how much we wrenched this tick
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndUpdateBottomPrint();
		%client.ndLastMessageTime = $Sim::Time;
	}

	if(%i >= %this.brickCount)
		%this.finishFillWrench();
	else
		%this.fillWrenchSchedule = %this.schedule(30, tickFillWrench);
}

//Finish wrenching
function ND_Selection::finishFillWrench(%this)
{
	%s = %this.undoGroup.brickCount == 1 ? "" : "s";
	%msg = "<font:Verdana:20>\c6Applied changes to \c3" @ %this.undoGroup.brickCount @ "\c6 Brick" @ %s @ "!";

	if(%this.wrenchFailCount > 0)
		%msg = %msg @ "\n<font:Verdana:17>\c3" @ %this.wrenchFailCount @ "\c6 missing trust.";

	commandToClient(%this.client, 'centerPrint', %msg, 8);

	if(%this.undoGroup.brickCount)
		%this.client.undoStack.push(%this.undoGroup TAB "ND_WRENCH");
	else
		%this.undoGroup.delete();

	%this.client.ndSetMode(%this.client.ndLastSelectMode);
}

//Cancel wrenching
function ND_Selection::cancelFillWrench(%this)
{
	cancel(%this.fillWrenchSchedule);

	if(%this.undoGroup.brickCount)
		%this.client.undoStack.push(%this.undoGroup TAB "ND_WRENCH");
	else
		%this.undoGroup.delete();
}



//Saving bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin saving
function ND_Selection::startSaving(%this, %filePath)
{
	//Open file
	%this.saveFilePath = %filePath;
	%this.saveFile = new FileObject();

	if(!%this.saveFile.openForWrite(%filePath))
		return false;

	//Write file header
	%this.saveFile.writeLine("Do not modify this file at all. You will break it.");
	%this.saveFile.writeLine("1");
	%this.saveFile.writeLine("Saved by " @ %this.client.name @ " (" @ %this.client.bl_id @ ") at " @ getDateTime());

	//Write colorset
	for(%i = 0; %i < 64; %i++)
		%this.saveFile.writeLine(getColorIDTable(%i));

	//Write line count
	%this.saveFile.writeLine("Linecount " @ %this.brickCount);

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//Schedule first tick
	%this.saveStage = 0;
	%this.saveIndex = 0;
	%this.saveSchedule = %this.schedule(30, tickSaveBricks);

	return true;
}

//Save some bricks
function ND_Selection::tickSaveBricks(%this)
{
	cancel(%this.saveSchedule);

	//Get bounds for this tick
	%start = %this.saveIndex;
	%end = %start + $Pref::Server::ND::ProcessPerTick * 2;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%file = %this.saveFile;

	//Save bricks
	for(%i = %start; %i < %end; %i++)
	{
		%data = $NS[%this, "D", %i];

		//Get correct print texture
		if(%data.hasPrint)
		{
			%fileName = getPrintTexture($NS[%this, "PR", %i]);
			%fileBase = fileBase(%fileName);
			%path = filePath(%fileName);

			if(%path !$= "" && %fileName !$= "base/data/shapes/bricks/brickTop.png")
			{
				%dirName = getSubStr(%path, 8, 999);

				%posA = strStr(%dirName, "_");
				%posB = strPos(%dirName, "_", %posA + 1);

				%aspectRatio = getSubStr(%dirName, %posA + 1, %posB - %posA - 1);
				%printTexture = %aspectRatio @ "/" @ %fileBase;
			}
			else
				%printTexture = "/";
		}
		else
			%printTexture = "";

		//Write brick data
		%file.writeLine(%data.uiName @ "\""
			SPC vectorAdd($NS[%this, "P", %i], %this.rootPosition)
			SPC $NS[%this, "R", %i]
			SPC 0
			SPC $NS[%this, "CO", %i]
			SPC %printTexture
			SPC $NS[%this, "CF", %i] * 1
			SPC $NS[%this, "SF", %i] * 1
			SPC !$NS[%this, "NRC", %i]
			SPC !$NS[%this, "NC", %i]
			SPC !$NS[%this, "NR", %i]
		);

		//Write brick name
		if((%tmp = $NS[%this, "NT", %i]) !$= "")
			%file.writeLine("+-NTOBJECTNAME " @ %tmp);

		//Write events
		%cnt = $NS[%this, "EN", %i];

		for(%j = 0; %j < %cnt; %j++)
		{
			//Basic event parameters
			%enabled = $NS[%this, "EE", %i, %j];
			%inputName = $NS[%this, "EI", %i, %j];
			%delay = $NS[%this, "ED", %i, %j];
			%targetIdx = $NS[%this, "ETI", %i, %j];

			if(%targetIdx == -1)
			{
				%targetName = "-1";
				%NT = $NS[%this, "ENT", %i, %j];
			}
			else
			{
				%targetName = $NS[%this, "ET", %i, %j];
				%NT = "";
			}

			%outputName = $NS[%this, "EO", %i, %j];

			//Temp line (without output parameters)
			%line = "+-EVENT" TAB %j TAB %enabled TAB %inputName TAB %delay TAB %targetName TAB %NT TAB %outputName;

			//Output event parameters
			if(%targetIdx >= 0)
				%targetClass = getWord(getField($InputEvent_TargetListfxDtsBrick_[$NS[%this, "EII", %i, %j]], %targetIdx), 1);
			else
				%targetClass = "FxDTSBrick";

			for(%k = 0; %k < 4; %k++)
			{
				%param = $NS[%this, "EP", %i, %j, %k];
				%dataType = getWord(getField($OutputEvent_parameterList[%targetClass, $NS[%this, "EOI", %i, %j]], %k), 0);

				if(%dataType $= "Datablock")
				{
					if(isObject(%param))
						%line = %line TAB %param.getName();
					else
						%line = %line TAB "-1";
				}
				else
					%line = %line TAB %param;
			}

			%file.writeLine(%line);
		}

		//Write emitter
		%edb = $NS[%this, "ED", %i];
		%edir = $NS[%this, "ER", %i];

		if(isObject(%edb))
			%file.writeLine("+-EMITTER" SPC %edb.uiName @ "\" " @ %edir);
		else if(%edir != 0)
			%file.writeLine("+-EMITTER NONE\" " @ %edir);

		//Write light
		%ldb = $NS[%this, "LD", %i];

		if(isObject(%ldb))
			%file.writeLine("+-LIGHT" SPC %ldb.uiName @ "\" 1");

		//Write item
		%idb = $NS[%this, "ID", %i];
		%ipos = $NS[%this, "IP", %i];
		%idir = $NS[%this, "IR", %i];
		%irt = $NS[%this, "IT", %i];

		if(isObject(%idb))
			%file.writeLine("+-ITEM" SPC %idb.uiName @ "\" " @ %ipos SPC %idir SPC %irt);
		else if(%ipos != 0 || (%idir !$= "" && %idir != 2) || (%irt != 4000 && %irt != 0))
			%file.writeLine("+-ITEM NONE\" " @ %ipos SPC %idir SPC %irt);

		//Write music
		%mdb = $NS[%this, "MD", %i];

		if(isObject(%mdb))
			%file.writeLine("+-AUDIOEMITTER" SPC %mdb.uiName @ "\"");

		//Write vehicle
		%vdb = $NS[%this, "VD", %i];
		%vcol = $NS[%this, "VC", %i];

		if(isObject(%vdb))
			%file.writeLine("+-VEHICLE" SPC %vdb.uiName @ "\" " @ %vcol);
	}

	//Save how far we got
	%this.saveIndex = %i;

	//Tell the client how much we saved this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Finished saving all bricks?
	if(%i >= %this.brickCount)
	{
		//Find width of connection numbers
		if(%this.maxConnections >= 241 * 241)
			%numberSize = 3;
		else if(%this.maxConnections >= 241)
			%numberSize = 2;
		else
			%numberSize = 1;

		//Find width of connection indices
		if(%this.brickCount > 241 * 241)
			%indexSize = 3;
		else if(%this.brickCount > 241)
			%indexSize = 2;
		else
			%indexSize = 1;

		//Save the sizes
		%file.writeLine("ND_SIZE\" 1 " @ %this.connectionCount SPC %numberSize SPC %indexSize);

		%this.saveStage = 1;
		%this.saveIndex = 0;
		%this.saveLineBuffer = "ND_TREE\" ";

		//Create byte table
		if(!$ND::Byte241TableCreated)
			ndCreateByte241Table();

		//Start saving connections
		%this.connectionCount = 0;
		%this.saveSchedule = %this.schedule(30, tickSaveConnections, %numberSize, %indexSize);
	}
	else
		%this.saveSchedule = %this.schedule(30, tickSaveBricks);
}

//Save some connections
function ND_Selection::tickSaveConnections(%this, %numberSize, %indexSize)
{
	cancel(%this.saveSchedule);

	//Get bounds for this tick
	%start = %this.saveIndex;
	%end = %start + $Pref::Server::ND::ProcessPerTick * 4;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	%file = %this.saveFile;
	%connections = %this.connectionCount;

	%lineBuffer = %this.saveLineBuffer;
	%len = strLen(%lineBuffer);

	//Save connections
	for(%i = %start; %i < %end; %i++)
	{
		//Save number of connections of this brick
		%cnt = $NS[%this, "N", %i];
		%connections += %cnt;

		//Write compressed connection number
		if(%numberSize == 1)
		{
			%lineBuffer = %lineBuffer @ $ND::Byte241ToChar[%cnt];

			%len++;
		}
		else if(%numberSize == 2)
		{
			%lineBuffer = %lineBuffer @
			    $ND::Byte241ToChar[(%cnt / 241) | 0] @
			    $ND::Byte241ToChar[%cnt % 241];

			%len += 2;
		}
		else
		{
			%lineBuffer = %lineBuffer @
			    $ND::Byte241ToChar[(((%cnt / 241) | 0) / 241) | 0] @
			    $ND::Byte241ToChar[((%cnt / 241) | 0) % 241] @
			    $ND::Byte241ToChar[%cnt % 241];

			%len += 3;
		}

		//If buffer is full, save to file
		if(%len > 254)
		{
			%file.writeLine(%lineBuffer);
			%lineBuffer = "ND_TREE\" ";
			%len = 9;
		}

		for(%j = 0; %j < %cnt; %j++)
		{
			//Write compressed connection index
			if(%indexSize == 1)
			{
				%lineBuffer = %lineBuffer @ $ND::Byte241ToChar[$NS[%this, "C", %i, %j]];

				%len++;
			}
			else if(%indexSize == 2)
			{
				%conn = $NS[%this, "C", %i, %j];

				%lineBuffer = %lineBuffer @
				    $ND::Byte241ToChar[(%conn / 241) | 0] @
				    $ND::Byte241ToChar[%conn % 241];

				%len += 2;
			}
			else
			{
				%conn = $NS[%this, "C", %i, %j];

				%lineBuffer = %lineBuffer @
				    $ND::Byte241ToChar[(((%conn / 241) | 0) / 241) | 0] @
				    $ND::Byte241ToChar[((%conn / 241) | 0) % 241] @
				    $ND::Byte241ToChar[%conn % 241];

				%len += 3;
			}

			//If buffer is full, save to file
			if(%len > 254)
			{
				%file.writeLine(%lineBuffer);
				%lineBuffer = "ND_TREE\" ";
				%len = 9;
			}
		}
	}

	//Save how far we got
	%this.saveIndex = %i;
	%this.saveLineBuffer = %lineBuffer;
	%this.connectionCount = %connections;

	//Tell the client how much we cut this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	if(%i >= %this.brickCount)
	{
		if(strLen(%lineBuffer) != 9)
			%file.writeLine(%lineBuffer);

		%this.saveLineBuffer = "";
		%this.finishSaving();
	}
	else
		%this.saveSchedule = %this.schedule(30, tickSaveConnections, %numberSize, %indexSize);
}

//Finish saving
function ND_Selection::finishSaving(%this)
{
	%this.saveFile.close();
	%this.saveFile.delete();

	%s1 = %this.brickCount == 1 ? "" : "s";
	%s2 = %this.connectionCount == 1 ? "" : "s";

	messageClient(%this.client, 'MsgProcessComplete', "\c6Finished saving selection, wrote \c3"
		@ %this.brickCount @ "\c6 Brick" @ %s1 @ " with \c3" @ %this.connectionCount @ "\c6 Connection" @ %s2 @ "!");

	%this.client.ndLastSaveTime = $Sim::Time;
	%this.client.ndSetMode(NDM_PlantCopy);
}

//Cancel saving
function ND_Selection::cancelSaving(%this)
{
	cancel(%this.saveSchedule);

	%this.saveFile.close();
	%this.saveFile.delete();

	if(isFile(%this.saveFilePath))
		fileDelete(%this.saveFilePath);

	%this.client.ndLastSaveTime = $Sim::Time;
}



//Loading bricks
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Begin loading
function ND_Selection::startLoading(%this, %filePath)
{
	//Open file
	%this.loadFile = new FileObject();

	if(!%this.loadFile.openForRead(%filePath))
		return false;

	//Skip file header
	%this.loadFile.readLine();
	%cnt = %this.loadFile.readLine();

	for(%i = 0; %i < %cnt; %i++)
		%this.loadFile.readLine();

	//Read colorset
	for(%i = 0; %i < 64; %i++)
		$NS[%this, "CT", %i] = ndGetClosestColorID2(getColorI(%this.loadFile.readLine()));

	//Read line count (temporary, allows displaying percentage)
	%this.loadExpectedBrickCount = getWord(%this.loadFile.readLine(), 1) * 1;

	if($Pref::Server::ND::PlayMenuSounds)
		messageClient(%this.client, 'MsgUploadStart', "");

	//Schedule first tick
	%this.connectionCount = 0;
	%this.brickCount = 0;
	%this.loadCount = 0;

	%this.loadStage = 0;
	%this.loadIndex = -1;
	%this.loadSchedule = %this.schedule(30, tickLoadBricks);

	return true;
}

//Load some bricks
function ND_Selection::tickLoadBricks(%this)
{
	cancel(%this.loadSchedule);

	%file = %this.loadFile;
	%index = %this.loadIndex;

	%loadCount = %this.loadCount;

	//Process lines
	while(!%file.isEOF())
	{
		%line = %file.readLine();

		//Skip empty lines
		if(trim(%line $= ""))
			continue;

		//Figure out what to do with the line
		switch$(getWord(%line, 0))
		{
			//Line is brick name
			case "+-NTOBJECTNAME":

				$NS[%this, "NT", %index] = getWord(%line, 1);

			//Line is event
			case "+-EVENT":

				//Mostly copied from default loading code
				%idx = $NS[%this, "EN", %index];

				if(!%idx)
					%idx = 0;

				%enabled = getField(%line, 2);
				%inputName = getField(%line, 3);
				%delay = getField(%line, 4);
				%targetName = getField(%line, 5);
				%NT = getField(%line, 6);
				%outputName = getField(%line, 7);
				%par1 = getField(%line, 8);
				%par2 = getField(%line, 9);
				%par3 = getField(%line, 10);
				%par4 = getField(%line, 11);

				%inputIdx = inputEvent_GetInputEventIdx(%inputName);

				if(%inputIdx == -1)
					warn("LOAD DUP: Input Event not found for name \"" @ %inputName @ "\"");

				%targetIdx = inputEvent_GetTargetIndex("FxDTSBrick", %inputIdx, %targetName);

				if(%targetName == -1)
					%targetClass = "FxDTSBrick";
				else
				{
					%field = getField($InputEvent_TargetList["FxDTSBrick", %inputIdx], %targetIdx);
					%targetClass = getWord(%field, 1);
				}

				%outputIdx = outputEvent_GetOutputEventIdx(%targetClass, %outputName);

				if(%outputIdx == -1)
					warn("LOAD DUP: Output Event not found for name \"" @ %outputName @ "\"");

				for(%j = 1; %j < 5; %j++)
				{
					%field = getField($OutputEvent_ParameterList[%targetClass, %outputIdx], %j - 1);
					%dataType = getWord(%field, 0);

					if(%dataType $= "Datablock" && %par[%j] !$= "-1")
					{
						%par[%j] = nameToId(%par[%j]);

						if(!isObject(%par[%j]))
						{
							warn("LOAD DUP: Datablock not found for event " @ %outputName @ " -> " @ %par[%j]);
							%par[%j] = 0;
						}
					}
				}

				//Save event
				$NS[%this, "EE", %index, %idx] = %enabled;
				$NS[%this, "ED", %index, %idx] = %delay;

				$NS[%this, "EI", %index, %idx] = %inputName;
				$NS[%this, "EII", %index, %idx] = %inputIdx;

				$NS[%this, "EO", %index, %idx] = %outputName;
				$NS[%this, "EOI", %index, %idx] = %outputIdx;
				$NS[%this, "EOC", %index, %idx] = $OutputEvent_AppendClient["FxDTSBrick", %outputIdx];

				$NS[%this, "ET", %index, %idx] = %targetName;
				$NS[%this, "ETI", %index, %idx] = %targetIdx;
				$NS[%this, "ENT", %index, %idx] = %NT;

				$NS[%this, "EP", %index, %idx, 0] = %par1;
				$NS[%this, "EP", %index, %idx, 1] = %par2;
				$NS[%this, "EP", %index, %idx, 2] = %par3;
				$NS[%this, "EP", %index, %idx, 3] = %par4;

				$NS[%this, "EN", %index] = %idx + 1;

			//Line is emitter
			case "+-EMITTER":

				%line = getSubStr(%line, 10, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Emitters[%dbName];

					//Ensure emitter exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Emitter datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				$NS[%this, "ED", %index] = %db;
				$NS[%this, "ER", %index] = mFLoor(getSubStr(%line, %pos + 2, 9999));

			//Line is light
			case "+-LIGHT":

				%line = getSubStr(%line, 8, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				%db = $UINameTable_Lights[%dbName];

				//Ensure light exists
				if(!isObject(%db))
				{
					warn("LOAD DUP: Light datablock no found for uiName \"" @ %dbName @ "\"");
					%db = 0;
				}
				else
					$NS[%this, "LD", %index] = %db;

			//Line is item
			case "+-ITEM":

				%line = getSubStr(%line, 7, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Items[%dbName];

					//Ensure item exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Item datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				%line = getSubStr(%line, %pos + 2, 9999);

				$NS[%this, "ID", %index] = %db;
				$NS[%this, "IP", %index] = getWord(%line, 0);
				$NS[%this, "IR", %index] = getWord(%line, 1);
				$NS[%this, "IT", %index] = getWord(%line, 2);

			//Line is music
			case "+-AUDIOEMITTER":

				%line = getSubStr(%line, 15, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				%db = $UINameTable_Music[%dbName];

				//Ensure music exists
				if(!isObject(%db))
				{
					warn("LOAD DUP: Music datablock no found for uiName \"" @ %dbName @ "\"");
					%db = 0;
				}
				else
					$NS[%this, "MD", %index] = %db;

			//Line is vehicle
			case "+-VEHICLE":

				%line = getSubStr(%line, 10, 9999);

				%pos = strStr(%line, "\"");
				%dbName = getSubStr(%line, 0, %pos);

				if(%dbName !$= "NONE")
				{
					%db = $UINameTable_Vehicle[%dbName];

					//Ensure vehicle exists
					if(!isObject(%db))
					{
						warn("LOAD DUP: Vehicle datablock no found for uiName \"" @ %dbName @ "\"");
						%db = 0;
					}
				}
				else
					%db = 0;

				$NS[%this, "VD", %index] = %db;
				$NS[%this, "VC", %index] = mFLoor(getSubStr(%line, %pos + 2, 9999));

			//Start reading connections
			case "ND_SIZE\"":

				%version = getWord(%line, 1);
				%this.loadExpectedConnectionCount = getWord(%line, 2);
				%numberSize = getWord(%line, 3);
				%indexSize = getWord(%line, 4);
				%connections = true;
				break;

			//Error
			case "ND_TREE\"":

				warn("LOAD DUP: Got connection data before connection sizes");

			//Line is irrelevant
			case "+-OWNER":

				%nothing = "";

			//Line is brick
			default:

				//Increment selection index
				%index++;
				%quotePos = strstr(%line, "\"");

				if(%quotePos >= 0)
				{
					//Get datablock
					%uiName = getSubStr(%line, 0, %quotePos);
					%db = $uiNameTable[%uiName];

					if(isObject(%db))
					{
						$NS[%this, "D", %index] = %db;

						//Load all the info from brick line
						%line = getSubStr(%line, %quotePos + 2, 9999);
						%pos = getWords(%line, 0, 2);
						%angId = getWord(%line, 3);

						if(%loadCount == 0)
							%this.rootPosition = %pos;

						$NS[%this, "P", %index] = vectorSub(%pos, %this.rootPosition);
						$NS[%this, "R", %index] = %angId;

						$NS[%this, "CO", %index] = $NS[%this, "CT", getWord(%line, 5)];
						$NS[%this, "CF", %index] = getWord(%line, 7);
						$NS[%this, "SF", %index] = getWord(%line, 8);

						if(%db.hasPrint)
						{
							if((%print = $printNameTable[getWord(%line, 6)]) $= "")
								warn("LOAD DUP: Print texture not found for path \"" @ getWord(%line, 6) @ "\"");

							$NS[%this, "PR", %index] = %print;
						}

						if(!getWord(%line, 9))
							$NS[%this, "NRC", %index] = true;

						if(!getWord(%line, 10))
							$NS[%this, "NC", %index] = true;

						if(!getWord(%line, 11))
							$NS[%this, "NR", %index] = true;

						//Update selection size with brick datablock
						if(%angId % 2 == 0)
						{
							%sx = %db.brickSizeX / 4;
							%sy = %db.brickSizeY / 4;
						}
						else
						{
							%sy = %db.brickSizeX / 4;
							%sx = %db.brickSizeY / 4;
						}

						%sz = %db.brickSizeZ / 10;

						%minX = getWord(%pos, 0) - %sx;
						%minY = getWord(%pos, 1) - %sy;
						%minZ = getWord(%pos, 2) - %sz;
						%maxX = getWord(%pos, 0) + %sx;
						%maxY = getWord(%pos, 1) + %sy;
						%maxZ = getWord(%pos, 2) + %sz;

						if(%loadCount)
						{
							if(%minX < %this.minX)
								%this.minX = %minX;

							if(%minY < %this.minY)
								%this.minY = %minY;

							if(%minZ < %this.minZ)
								%this.minZ = %minZ;

							if(%maxX > %this.maxX)
								%this.maxX = %maxX;

							if(%maxY > %this.maxY)
								%this.maxY = %maxY;

							if(%maxZ > %this.maxZ)
								%this.maxZ = %maxZ;
						}
						else
						{
							%this.minX = %minX;
							%this.minY = %minY;
							%this.minZ = %minZ;
							%this.maxX = %maxX;
							%this.maxY = %maxY;
							%this.maxZ = %maxZ;
						}

						%loadCount++;
					}
					else
					{
						warn("LOAD DUP: Brick datablock not found for uiName \"" @ %uiName @ "\"");
						$NS[%this, "D", %index] = 0;
					}
				}
				else
				{
					warn("LOAD DUP: Brick uiName missing on line \"" @ %line @ "\"");
					$NS[%this, "D", %index] = 0;
				}
		}

		if(%linesProcessed++ > $Pref::Server::ND::ProcessPerTick * 2)
			break;
	}

	//Save how far we got
	%this.loadIndex = %index;
	%this.brickCount = %index + 1;
	%this.loadCount = %loadCount;

	//Tell the client how much we loaded this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Switch over to connection mode if necessary
	if(%connections)
	{
		%this.loadStage = 1;
		%this.loadIndex = 0;
		%this.connectionCount = 0;
		%this.connectionIndex = -1;
		%this.connectionIndex2 = 0;
		%this.connectionsRemaining = 0;

		if((%numberSize != 1 && %numberSize != 2 && %numberSize != 3) ||
		    (%indexSize != 1 &&  %indexSize != 2 &&  %indexSize != 3))
		{
			messageClient(%this.client, '', "\c0Warning:\c6 The connection data is corrupted. Planting may not work as expected.");
			%this.finishLoading();
			return;
		}

		//Create byte table
		if(!$ND::Byte241TableCreated)
			ndCreateByte241Table();

		%this.loadSchedule = %this.schedule(30, tickLoadConnections, %numberSize, %indexSize);
		return;
	}

	//Reached end of file, means we got no connection data
	if(%file.isEOF())
	{
		messageClient(%this.client, '', "\c0Warning:\c6 The save was not written by the New Duplicator. Planting may not work as expected.");
		%this.finishLoading();
	}
	else
		%this.loadSchedule = %this.schedule(30, tickLoadBricks);
}

//Load connections
function ND_Selection::tickLoadConnections(%this, %numberSize, %indexSize)
{
	cancel(%this.loadSchedule);

	%connections = %this.connectionCount;
	%maxConnections = %this.maxConnections;
	%connectionIndex = %this.connectionIndex;
	%connectionIndex2 = %this.connectionIndex2;
	%connectionsRemaining = %this.connectionsRemaining;

	//Process 10 lines
	for(%i = 0; %i < 10 && !%this.loadFile.isEOF(); %i++)
	{
		%line = getSubStr(%this.loadFile.readLine(), 9, 9999);
		%len = strLen(%line);
		%pos = 0;

		while(%pos < %len)
		{
			if(%connectionsRemaining)
			{
				//Read a connection
				if(%indexSize == 1)
				{
					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    strStr($ND::Byte241Lookup, getSubStr(%line, %pos, 1));

					%pos++;
				}
				else if(%indexSize == 2)
				{
					%tmp = getSubStr(%line, %pos, 2);

					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 241 +
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1));

					%pos += 2;
				}
				else
				{
					%tmp = getSubStr(%line, %pos, 3);

					$NS[%this, "C", %connectionIndex, %connectionIndex2] =
					    ((strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 58081) | 0) +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1)) *   241       +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 2, 1));

					%pos += 3;
				}

				%connectionsRemaining--;
				%connectionIndex2++;
				%connections++;
			}
			else
			{
				//No connections remaining for active brick, increment index
				%connectionIndex++;
				%connectionIndex2 = 0;

				//Read a connection number
				if(%numberSize == 1)
				{
					%connectionsRemaining =
					    strStr($ND::Byte241Lookup, getSubStr(%line, %pos, 1));

					%pos++;
				}
				else if(%numberSize == 2)
				{
					%tmp = getSubStr(%line, %pos, 2);

					%connectionsRemaining =
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 241 +
					    strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1));

					%pos += 2;
				}
				else
				{
					%tmp = getSubStr(%line, %pos, 3);

					%connectionsRemaining =
					    ((strStr($ND::Byte241Lookup, getSubStr(%tmp, 0, 1)) * 58081) | 0) +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 1, 1)) *   241       +
					      strStr($ND::Byte241Lookup, getSubStr(%tmp, 2, 1));

					%pos += 3;
				}

				$NS[%this, "N", %connectionIndex] = %connectionsRemaining;

				if(%maxConnections < %connectionsRemaining)
					%maxConnections = %connectionsRemaining;
			}
		}
	}

	//Save how far we got
	%this.connectionCount = %connections;
	%this.maxConnections = %maxConnections;
	%this.connectionIndex = %connectionIndex;
	%this.connectionIndex2 = %connectionIndex2;
	%this.connectionsRemaining = %connectionsRemaining;

	//Tell the client how much we loaded this tick
	if(%this.client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%this.client.ndUpdateBottomPrint();
		%this.client.ndLastMessageTime = $Sim::Time;
	}

	//Check if we're done
	if(%this.loadFile.isEOF())
		%this.finishLoading();
	else
		%this.loadSchedule = %this.schedule(30, tickLoadConnections, %numberSize, %indexSize);
}

//Finish loading
function ND_Selection::finishLoading(%this)
{
	%this.loadFile.close();
	%this.loadFile.delete();

	//Align the build to the brick grid
	%this.updateSize();

	%pos = vectorAdd(%this.rootPosition, %this.rootToCenter);

	%shiftX = mCeil(getWord(%pos, 0) * 2 - %this.brickSizeX % 2) / 2 + (%this.brickSizeX % 2) / 4  - getWord(%pos, 0);
	%shiftY = mCeil(getWord(%pos, 1) * 2 - %this.brickSizeY % 2) / 2 + (%this.brickSizeY % 2) / 4  - getWord(%pos, 1);
	%shiftZ = mCeil(getWord(%pos, 2) * 5 - %this.brickSizeZ % 2) / 5 + (%this.brickSizeZ % 2) / 10 - getWord(%pos, 2);

	%this.rootPosition = vectorAdd(%shiftX SPC %shiftY SPC %shiftZ, %this.rootPosition);

	%this.minX = %this.minX + %shiftX;
	%this.maxX = %this.maxX + %shiftX;
	%this.minY = %this.minY + %shiftY;
	%this.maxY = %this.maxY + %shiftY;
	%this.minZ = %this.minZ + %shiftZ;
	%this.maxZ = %this.maxZ + %shiftZ;

	%this.updateSize();
	%this.updateHighlightBox();

	//Message client
	%s1 = %this.brickCount == 1 ? "" : "s";
	%s2 = %this.connectionCount == 1 ? "" : "s";

	messageClient(%this.client, 'MsgProcessComplete', "\c6Finished loading selection, got \c3"
		@ %this.brickCount @ "\c6 Brick" @ %s1 @ " with \c3" @ %this.connectionCount @ "\c6 Connection" @ %s2 @ "!");

	%this.client.ndLastLoadTime = $Sim::Time;
	%this.client.ndSetMode(NDM_PlantCopy);
}

//Cancel loading
function ND_Selection::cancelLoading(%this)
{
	cancel(%this.loadSchedule);

	%this.loadFile.close();
	%this.loadFile.delete();

	%this.client.ndLastLoadTime = $Sim::Time;
}
