// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NDAT_StackSelect
// *
// *    -------------------------------------------------------------------
// *    Async task used to recursively select brick stacks
// *
// * ######################################################################

//Create stack selection task
function NDAT_StackSelect(%client, %selection, %highlightSet, %startBrick, %direction, %limited)
{
	ND_TaskGroup.add(
		%this = new ScriptObject()
		{
			//Task variables
			class = NDAT_StackSelect;
			superClass = ND_AsyncTask;
			tickTime = $ND::StackSelectTickDelay;

			//Custom variables
			client = %client;
			selection = %selection;
			highlightSet = %highlightSet;
			startBrick = %startBrick;
			direction = %direction;
			limited = %limited;
		}
	);

	return %this;
}

//Start stack selection task
function NDAT_StackSelect::start(%this)
{
	%selection = %this.selection;
	%brick = %this.startBrick;

	//Selection size should already be zero
	$NDS[%selection, "QueueCount"] = 0;

	//Set root position
	$NDS[%selection, "RootPos"] = %brick.getPosition();

	//Process first brick
	%selection.queueBrick(%brick);
	%selection.recordBrickData(0);
	%this.highlightSet.addBrick(%brick);

	$NDS[%selection, "Count"] = 1;

	if(%this.direction == 1)
	{
		//Set lower height limit
		%this.heightLimit = $NDS[%selection, "MinZ"] - 0.01;

		//Add all up bricks to queue
		%upCount = %brick.getNumUpBricks();

		for(%i = 0; %i < %upCount; %i++)
		{
			%nextBrick = %brick.getUpBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NDS[%selection, "ID", %nextBrick];

			if(%nId $= "")
				%nId = %selection.queueBrick(%nextBrick);

			$NDS[%selection, "UpId", 0, %i] = %nId;
		}

		//Start brick only has up bricks
		$NDS[%selection, "UpCnt", 0] = %upCount;
		$NDS[%selection, "DownCnt", 0] = 0;
	}
	else
	{
		//Set upper height limit
		%this.heightLimit = $NDS[%selection, "MaxZ"] + 0.01;

		//Add all down bricks to queue
		%downCount = %brick.getNumDownBricks();

		for(%i = 0; %i < %downCount; %i++)
		{
			%nextBrick = %brick.getDownBrick(%i);

			//If the brick is not in the list yet, add it to the queue to give it an id
			%nId = $NDS[%selection, "ID", %nextBrick];

			if(%nId $= "")
				%nId = %selection.queueBrick(%nextBrick);

			$NDS[%selection, "DownId", 0, %i] = %nId;
		}

		//Start brick only has down bricks
		$NDS[%selection, "UpCnt", 0] = 0;
		$NDS[%selection, "DownCnt", 0] = %downCount;
	}

	return parent::start(%this);
}

//Tick stack selection task
function NDAT_StackSelect::tick(%this)
{
	parent::tick(%this);

	//Local variables inside loop are faster than object ones
	%selection = %this.selection;
	%highlightSet = %this.highlightSet;

	%limited = %this.limited;
	%direction = %this.direction;
	%heightLimit = %this.heightLimit;

	//Continue processing where we left off last tick
	%i = $NDS[%selection, "Count"];
	
	//Process exactly 200 bricks
	for(%p = 0; %p < $ND::StackSelectPerTick; %p++)
	{
		//If no more bricks are queued, we're done!
		if(%i >= $NDS[%selection, "QueueCount"])
		{
			%this.finish();
			return;
		}

		//Record data for next brick in queue
		%brick = %selection.recordBrickData(%i);

		if(!%brick)
		{
			messageClient(%this.client, '', "\c0Error: Queued brick does not exist anymore. Do not modify the build during selection!");
			NDDM_StackSelectProgress.onCancelBrick(%this.client);
		}

		//Highlight brick
		%highlightSet.addBrick(%brick);

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
			%nextIndex = $NDS[%selection, "ID", %nextBrick];

			if(%nextIndex $= "")
			{
				//Copy of ND_Selection::queueBrick
				%nextIndex = $NDS[%selection, "QueueCount"];
				$NDS[%selection, "QueueCount"]++;

				$NDS[%selection, "Brick", %nextIndex] = %nextBrick;
				$NDS[%selection, "ID", %nextBrick] = %i;
			}

			$NDS[%selection, "UpId", %i, %realUpCnt] = %nextIndex;
			%realUpCnt++;
		}

		$NDS[%selection, "UpCnt", %i] = %realUpCnt;

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
			%nextIndex = $NDS[%selection, "ID", %nextBrick];

			if(%nextIndex $= "")
			{
				//Copy of ND_Selection::queueBrick
				%nextIndex = $NDS[%selection, "QueueCount"];
				$NDS[%selection, "QueueCount"]++;

				$NDS[%selection, "Brick", %nextIndex] = %nextBrick;
				$NDS[%selection, "ID", %nextBrick] = %i;
			}

			$NDS[%selection, "DownId", %i, %realDownCnt] = %nextIndex;
			%realDownCnt++;
		}

		$NDS[%selection, "DownCnt", %i] = %realDownCnt;

		//Save how far we got
		$NDS[%selection, "Count"]++;

		%i++;
	}

	//Tell the client how much we selected this tick
	%this.client.ndUpdateBottomPrint();
}

//Finish stack selection
function NDAT_StackSelect::finish(%this)
{
	%this.selection.updateSize();

	return parent::finish(%this);
}
