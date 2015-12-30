// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_UndoGroupPaint
// *
// *    -------------------------------------------------------------------
// *    Handles undoing painting selections
// *
// * ######################################################################

//Start undo paint
function ND_UndoGroupPaint::ndStartUndo(%this, %client)
{
	%client.ndUndoInProgress = true;
	%client.ndLastMessageTime = $Sim::Time;
	%this.ndTickUndo(%this.paintType, 0, %client);
}

//Tick undo paint
function ND_UndoGroupPaint::ndTickUndo(%this, %mode, %start, %client)
{
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = %this.brick[%i];

		if(isObject(%brick))
		{
			//De-highlight brick
			if(%brick.ndHighlightSet)
			{
				if(%brick.ndColor == $ND::BrickHighlightColor)
					%brick.setColorFx(%brick.ndColorFx);
				else
					%brick.setColor(%brick.ndColor);

				%brick.ndHighlightSet = false;
			}

			switch(%mode)
			{
				case 0: %brick.setColor(%this.value[%i]);
				case 1: %brick.setColorFx(%this.value[%i]);
				case 2: %brick.setShapeFx(%this.value[%i]);
			}
		}
	}

	//If undo is taking long, tell the client how far we get
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndLastMessageTime = $Sim::Time;

		%percent = mFloor(%end * 100 / %this.brickCount);
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo in progress...\n<font:Verdana:17>\c3" @ %percent @ "%\c6 finished.", 10);
	}

	if(%end >= %this.brickcount)
	{
		%this.delete();
		%client.ndUndoInProgress = false;

		if(%start != 0)
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo finished.", 4);

		return;
	}
	
	%this.schedule(30, ndTickUndo, %mode, %end, %client);
}
